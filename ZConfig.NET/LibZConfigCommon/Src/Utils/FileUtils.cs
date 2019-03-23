#region copyright
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2019
// Date: 2019-3-23
// Project: LibZConfigCommon
// Subho Ghosh (subho dot ghosh at outlook.com)
//
//
#endregion
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
            return String.Format("{0}{1}{2}", path, Path.DirectorySeparatorChar, filename);
        }

        /// <summary>
        /// Get the temporary directory for this application.
        /// </summary>
        /// <returns>Temporary directory path (includes app name)</returns>
        public static string GetTempDirectory()
        {
            string app = AppDomain.CurrentDomain.FriendlyName;
            string path = String.Format("{0}{1}{2}", Path.GetTempPath(), Path.DirectorySeparatorChar, app);
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
            string dir = String.Format("{0}{1}{2}", path, Path.DirectorySeparatorChar, name);
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
            DeleteTempDirectory(GetTempDirectory());
        }

        /// <summary>
        /// Clean up the temporary folder for this app.
        /// </summary>
        /// <param name="path">Directory Path</param>
        public static void DeleteTempDirectory(string path)
        {
            var dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                dir.Delete(true);
            }
        }

        /// <summary>
        /// Check if directory exists or create the directory path.
        /// </summary>
        /// <param name="path">Directory Path</param>
        /// <returns>Available?</returns>
        public static bool CheckDirectory(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                if (fi.Attributes.HasFlag(FileAttributes.Directory))
                {
                    return true;
                }
                return false;
            }
            if (Directory.CreateDirectory(fi.FullName) != null)
                return true;
            return false;
        }

        /// <summary>
        /// Check if the parent directory for this file exists, else create.
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns>Created?</returns>
        public static bool CheckFilePath(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                if (fi.Attributes.HasFlag(FileAttributes.Directory))
                {
                    return false;
                }
                return true;
            }
            return CheckDirectory(fi.DirectoryName);
        }

        /// <summary>
        /// Create a local instance of the reader content.
        /// </summary>
        /// <param name="reader">Stream Reader</param>
        /// <param name="path">File name (path)</param>
        /// <param name="outdir">Output Directory</param>
        /// <returns>File Path</returns>
        public static string WriteLocalFile(StreamReader reader, string path, string outdir)
        {
            return WriteLocalFile(reader, path, outdir, true);
        }

        /// <summary>
        /// Create a local instance of the reader content.
        /// </summary>
        /// <param name="reader">Stream Reader</param>
        /// <param name="path">File name (path)</param>
        /// <param name="outdir">Output Directory</param>
        /// <param name="overwrite">Overwrite if exists</param>
        /// <returns>File Path</returns>
        public static string WriteLocalFile(StreamReader reader, string path, string outdir, bool overwrite)
        {
            string file = String.Format("{0}\\{1}", outdir, path);
            FileInfo fi = new FileInfo(file);
            if (fi.Exists)
            {
                if (overwrite)
                    fi.Delete();
                else
                    return fi.FullName;
            }
            LogUtils.Debug(String.Format("Writing output to file: {0}", fi.FullName));
            CheckFilePath(fi.FullName);

            using (StreamWriter writer = new StreamWriter(fi.FullName))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                }
            }
            return fi.FullName;
        }
    }

    /// <summary>
    /// Enum for supported URI types.
    /// </summary>
    public enum EUriScheme
    {
        /// <summary>
        /// No type defined.
        /// </summary>
        none,
        /// <summary>
        /// URI type file.
        /// </summary>
        file,
        /// <summary>
        /// URI type HTTP
        /// </summary>
        http,
        /// <summary>
        /// URI type HTTPS
        /// </summary>
        https,
        /// <summary>
        /// URI type FTP
        /// </summary>
        ftp
    }

    public static class UriUtils
    {
        /// <summary>
        /// Parse the reader type based on the URI.
        /// </summary>
        /// <param name="type">Reference Type</param>
        /// <param name="uri">URI Path</param>
        /// <returns>Reader Type.</returns>
        public static EUriScheme GetUriType(this EUriScheme type, Uri location)
        {
            if (location.Scheme == Uri.UriSchemeFile)
            {
                return EUriScheme.file;
            }
            else if (location.Scheme == Uri.UriSchemeFtp)
            {
                return EUriScheme.ftp;
            }
            else if (location.Scheme == Uri.UriSchemeHttp)
            {
                return EUriScheme.http;
            }
            else if (location.Scheme == Uri.UriSchemeHttps)
            {
                return EUriScheme.https;
            }

            return EUriScheme.none;
        }

        public static string GetUriScheme(EUriScheme type)
        {
            switch (type)
            {
                case EUriScheme.file:
                    return String.Format("{0}{1}", Uri.UriSchemeFile, Uri.SchemeDelimiter);
                case EUriScheme.ftp:
                    return String.Format("{0}{1}", Uri.UriSchemeFtp, Uri.SchemeDelimiter);
                case EUriScheme.http:
                    return String.Format("{0}{1}", Uri.UriSchemeHttp, Uri.SchemeDelimiter);
                case EUriScheme.https:
                    return String.Format("{0}{1}", Uri.UriSchemeHttps, Uri.SchemeDelimiter);
            }
            return null;
        }

        /// <summary>
        /// Parse the input string as the URI scheme enum.
        /// 
        /// </summary>
        /// <param name="type">URI Scheme Enum</param>
        /// <param name="value">String input</param>
        /// <returns>URI Scheme Enum</returns>
        public static EUriScheme ParseScheme(this EUriScheme type, string value)
        {
            value = value.ToLower();
            return Enum.Parse<EUriScheme>(value);
        }
    }
}