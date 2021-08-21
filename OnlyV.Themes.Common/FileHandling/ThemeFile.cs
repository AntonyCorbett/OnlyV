using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using OnlyV.Themes.Common.Cache;
using OnlyV.Themes.Common.Services;
using Serilog;

namespace OnlyV.Themes.Common.FileHandling
{
    public class ThemeFile
    {
        public const string ThemeFileExtension = ".onlyv";
        private const string ThemeEntryName = "theme.json";
        private const string ImageEntryName = "image.png";
        
        private static readonly ThemeCache Cache = new ThemeCache();

        public static IReadOnlyCollection<string> GetAll(string folder)
        {
            return Directory.GetFiles(folder, $"*.{ThemeFileExtension}");
        }
        
        public void Create(string themePath, OnlyVTheme theme, string backgroundImagePath, bool overwrite)
        {
            if (string.IsNullOrEmpty(themePath) ||
                !Path.GetExtension(themePath).Equals(ThemeFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(nameof(themePath));
            }

            if (theme == null)
            {
                throw new ArgumentException(nameof(theme));
            }

            if (File.Exists(themePath) && !overwrite)
            {
                throw new Exception(Properties.Resources.FILE_EXISTS);
            }

            var validBackgroundImage = false;
            if (!string.IsNullOrEmpty(backgroundImagePath))
            {
                var ext = Path.GetExtension(backgroundImagePath);
                if (string.IsNullOrEmpty(ext) ||
                    !ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    !File.Exists(backgroundImagePath))
                {
                    throw new ArgumentException(nameof(backgroundImagePath));
                }
                
                validBackgroundImage = true;
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var themeEntry = zip.CreateEntry(ThemeEntryName);

                    var serializer = new JsonSerializer { Formatting = Formatting.Indented };

                    using (var entryStream = themeEntry.Open())
                    using (var entryStreamWriter = new StreamWriter(entryStream))
                    using (var jsonTextWriter = new JsonTextWriter(entryStreamWriter))
                    {
                        serializer.Serialize(jsonTextWriter, theme);
                    }

                    if (validBackgroundImage)
                    {
                        zip.CreateEntryFromFile(backgroundImagePath, ImageEntryName);
                    }
                }

                if (overwrite)
                {
                    File.Delete(themePath);
                }
             
                using (var fileStream = new FileStream(themePath, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        public void Create(
            string themePath, 
            OnlyVTheme theme, 
            BitmapImage backgroundImage, 
            bool overwrite)
        {
            string backgroundImagePath = null;

            if (backgroundImage != null)
            {
                backgroundImagePath = Path.GetRandomFileName();
                backgroundImagePath = Path.ChangeExtension(backgroundImagePath, ".png");
                backgroundImagePath = Path.Combine(Path.GetTempPath(), backgroundImagePath);
                
                BitmapWriter.WritePng(backgroundImagePath, backgroundImage);
            }
            
            Create(themePath, theme, backgroundImagePath, overwrite);

            if (backgroundImagePath != null)
            {
                File.Delete(backgroundImagePath);
            }
        }

        public static ThemeCacheEntry Read(string themePath)
        {
            if (string.IsNullOrEmpty(themePath))
            {
                return null;
            }

            var result = Cache.Get(themePath);
            if (result == null)
            {
                result = ReadInternal(themePath);
                if (result != null)
                {
                    Cache.Add(themePath, result);
                }
            }

            return result;
        }

        private static ThemeCacheEntry ReadInternal(string themePath)
        {
            try
            {
                using (var zip = ZipFile.OpenRead(themePath))
                {
                    var entry = zip.GetEntry(ThemeEntryName);
                    if (entry != null)
                    {
                        using (var stream = entry.Open())
                        {
                            var serializer = new JsonSerializer();

                            using (var sr = new StreamReader(stream))
                            using (var jsonTextReader = new JsonTextReader(sr))
                            {
                                var theme = serializer.Deserialize<OnlyVTheme>(jsonTextReader);
                                if (theme != null)
                                {
                                    return new ThemeCacheEntry
                                    {
                                        ThemePath = themePath,
                                        Theme = theme,
                                        BackgroundImage = ReadBackgroundImage(zip)
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not read theme file");
            }

            return null;
        }

        private static ImageSource ReadBackgroundImage(ZipArchive zip)
        {
            try
            {
                var entry = zip.Entries.SingleOrDefault(x => x.Name.Equals(ImageEntryName, StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    using (var stream = entry.Open())
                    {
                        var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        return decoder.Frames[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not read background image");
            }

            return null;
        }
    }
}
