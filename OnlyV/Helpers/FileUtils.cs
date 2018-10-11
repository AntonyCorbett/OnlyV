namespace OnlyV.Helpers
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// General file / folder utilities
    /// </summary>
    public static class FileUtils
    {
        private static readonly string AppNamePathSegment = "OnlyV";
        private static readonly string OptionsFileName = "options.json";

        public static string CleanFileName(string filename)
        {
            return Path.GetInvalidFileNameChars().Aggregate(filename, (current, c) =>
                current.Replace(c.ToString(CultureInfo.InvariantCulture), string.Empty));
        }

        public static string GetDefaultSaveToFolder()
        {
            string folder = Path.Combine(GetOnlyVMyDocsFolder(), @"BibleTextImages");
            CreateDirectory(folder);
            return folder;
        }

        public static string GetEpubFolder()
        {
            string folder = Path.Combine(GetOnlyVMyDocsFolder(), @"SourceEpubFiles");
            CreateDirectory(folder);
            return folder;
        }

        public static string GetStandardThemeFolder()
        {
            var folder = Path.Combine(GetOnlyVCommonAppDataFolder(), @"ThemeFiles");
            return !Directory.Exists(folder) ? null : folder;
        }

        public static string GetPrivateThemeFolder()
        {
            string folder = Path.Combine(GetOnlyVMyDocsFolder(), @"ThemeFiles");
            CreateDirectory(folder);
            return folder;
        }

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
        /// Gets OnlyV temp folder
        /// </summary>
        /// <returns>Temp folder</returns>
        public static string GetTempOnlyVFolder()
        {
            return Path.Combine(Path.GetTempPath(), AppNamePathSegment);
        }

        /// <summary>
        /// Gets the log folder
        /// </summary>
        /// <returns>Log folder</returns>
        public static string GetLogFolder()
        {
            return Path.Combine(
                GetOnlyVMyDocsFolder(),
                "Logs");
        }

        /// <summary>
        /// Gets the application's MyDocs folder, e.g. "...MyDocuments\OnlyV"
        /// </summary>
        /// <returns>Folder path</returns>
        public static string GetOnlyVMyDocsFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppNamePathSegment);
        }

        /// <summary>
        /// Gets the application's common appData folder.
        /// </summary>
        /// <returns>Folder path</returns>
        public static string GetOnlyVCommonAppDataFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppNamePathSegment);
        }

        /// <summary>
        /// Gets the file path for storing the user options
        /// </summary>
        /// <param name="commandLineIdentifier">Optional command-line id</param>
        /// <param name="optionsVersion">The options schema version</param>
        /// <returns>User Options file path.</returns>
        public static string GetUserOptionsFilePath(string commandLineIdentifier, int optionsVersion)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppNamePathSegment,
                commandLineIdentifier ?? string.Empty,
                optionsVersion.ToString(),
                OptionsFileName);
        }

        /// <summary>
        /// Gets the OnlyV application data folder.
        /// </summary>
        /// <returns>AppData folder.</returns>
        public static string GetAppDataFolder()
        {
            // NB - user-specific folder
            // e.g. C:\Users\Antony\AppData\Roaming\OnlyV
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppNamePathSegment);
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
    }
}
