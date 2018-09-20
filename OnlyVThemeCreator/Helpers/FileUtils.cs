namespace OnlyVThemeCreator.Helpers
{
    using System;
    using System.IO;

    internal static class FileUtils
    {
        private static readonly string OnlyVAppNamePathSegment = "OnlyV";
        private static readonly string ThemeCreatorAppNamePathSegment = "OnlyVThemeCreator";
        private static readonly string OptionsFileName = "options.json";

        /// <summary>
        /// Creates directory if it doesn't exist. Throws if cannot be created
        /// </summary>
        /// <param name="folderPath">Directory to create</param>
        public static void CreateDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                if (!Directory.Exists(folderPath))
                {
                    // "Could not create folder {0}"
                    throw new Exception(string.Format(Properties.Resources.CREATE_FOLDER_ERROR, folderPath));
                }
            }
        }

        /// <summary>
        /// Gets system temp folder
        /// </summary>
        /// <returns>Temp folder</returns>
        public static string GetSystemTempFolder()
        {
            return Path.GetTempPath();
        }

        /// <summary>
        /// Gets the log folder
        /// </summary>
        /// <returns>Log folder</returns>
        public static string GetLogFolder()
        {
            return Path.Combine(
                GetAppMyDocsFolder(),
                "Logs");
        }

        public static string GetAppMyDocsFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ThemeCreatorAppNamePathSegment);
        }

        public static string GetEpubFolder()
        {
            string folder = Path.Combine(GetOnlyVMyDocsFolder(), @"SourceEpubFiles");
            CreateDirectory(folder);
            return folder;
        }

        public static string GetOnlyVMyDocsFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), OnlyVAppNamePathSegment);
        }

        public static string GetUserOptionsFilePath(int optionsVersion)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ThemeCreatorAppNamePathSegment,
                optionsVersion.ToString(),
                OptionsFileName);
        }

        public static string GetAppDataFolder()
        {
            // NB - user-specific folder
            // e.g. C:\Users\Antony\AppData\Roaming\OnlyVThemeCreator
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ThemeCreatorAppNamePathSegment);
            CreateDirectory(folder);
            return folder;
        }

        public static bool DirectoryIsAvailable(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return false;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                return Directory.Exists(dir);
            }

            return true;
        }

        public static string GetPrivateThemeFolder()
        {
            string folder = Path.Combine(GetOnlyVMyDocsFolder(), @"ThemeFiles");
            CreateDirectory(folder);
            return folder;
        }
    }
}
