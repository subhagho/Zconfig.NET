using System;
using System.IO;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// Utility methods for File/Directory
    /// </summary>
    public static class FileUtils
    {
        /// <summary>
        /// Extract the filename from the path. 
        /// Filename is extracted without the extension.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>File name</returns>
        public static string ExtractFileName(string path)
        {
            string filename = Path.GetFileName(path);
            if (!String.IsNullOrWhiteSpace(filename))
            {
                string[] parts = filename.Split('.');
                if (parts != null && parts.Length > 0)
                {
                    return parts[parts.Length - 1];
                }
            }
            return null;
        }

        /// <summary>
        /// Extract the extension for the specified file.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>File extension</returns>
        public static string ExtractExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        /// Create a new temporary file in the temp directory.
        /// </summary>
        /// <returns>Path to temp file</returns>
        public static string GetTempFile()
        {
            string path = GetTempDirectory();
            string filename = String.Format("{0}.tmp", Guid.NewGuid().ToString());
            return String.Format("{0}{1}{2}", path, Path.PathSeparator, filename);
        }

        /// <summary>
        /// Get the temporary directory for this application.
        /// </summary>
        /// <returns>Temporary directory path (includes app name)</returns>
        public static string GetTempDirectory()
        {
            string app = AppDomain.CurrentDomain.FriendlyName;
            string path = String.Format("{0}{1}{2}", Path.GetTempPath(), Path.PathSeparator, app);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// Create a new temporary directory in the App temp folder.
        /// </summary>
        /// <param name="name">Directory name.</param>
        /// <returns>Path to created directory</returns>
        public static string GetTempDirectory(string name)
        {
            string path = GetTempDirectory();
            string dir = String.Format("{0}{1}{2}", path, Path.PathSeparator, name);
            if (!File.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        /// <summary>
        /// Clean up the temporary folder for this app.
        /// </summary>
        public static void DeleteTempDirectory()
        {
            string path = GetTempDirectory();
            var dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                dir.Delete(true);
            }
        }
    }
}