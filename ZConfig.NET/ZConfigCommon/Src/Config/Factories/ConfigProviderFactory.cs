using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Config.Parsers;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Writers;

namespace LibZConfig.Common.Config.Factories
{
    public enum EConfigType
    {
        /// <summary>
        /// Source type is Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// JSON configuration source.
        /// </summary>
        JSON,
        /// <summary>
        /// XML configuration source.
        /// </summary>
        XML
    }

    /// <summary>
    /// Helper class to parse the configuration source type.
    /// </summary>
    public static class ConfigTypeHelper
    {
        /// <summary>
        /// Parse the passed string as the config type.
        /// </summary>
        /// <param name="type">Type place holder</param>
        /// <param name="value">String value</param>
        /// <returns>Parsed Enum Value</returns>
        public static EConfigType Parse(this EConfigType type, string value)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                value = value.Trim().ToUpper();
                return (EConfigType)Enum.Parse(typeof(EConfigType), value);
            }
            return default(EConfigType);
        }
    }

    /// <summary>
    /// Enum specifies the type of defined readers.
    /// </summary>
    public enum EReaderType
    {
        /// <summary>
        /// Reader type is unkown
        /// </summary>
        Unknown,
        /// <summary>
        /// Reader reads from a local file path.
        /// </summary>
        FILE,
        /// <summary>
        /// Reader reads from a specified HTTP.
        /// </summary>
        HTTP,
        /// <summary>
        /// Reader reads from a specified HTTPS.
        /// </summary>
        HTTPS,
        /// <summary>
        /// Reader reads from a specified FTP location.
        /// </summary>
        FTP,
        /// <summary>
        /// Reader reads from a specified SFTP location.
        /// </summary>
        SFTP
    }

    /// <summary>
    /// Helper class to parse the configuration reader type.
    /// </summary>
    public static class ReaderTypeHelper
    {
        /// <summary>
        /// Parse the reader type from the specified string value.
        /// </summary>
        /// <param name="type">Type place holder</param>
        /// <param name="value">String value to parse</param>
        /// <returns>Parsed Enum value</returns>
        public static EReaderType Parse(this EReaderType type, string value)
        {
            Preconditions.CheckArgument(value);

            value = value.Trim().ToUpper();
            return (EReaderType)Enum.Parse(typeof(EReaderType), value);
        }

        /// <summary>
        ///  Get the configuration reader type based on the URI protocol.
        /// </summary>
        /// <param name="uri">Specified URI</param>
        /// <returns>Extracted Reader type</returns>
        public static EReaderType ParseFromUri(Uri uri)
        {
            Preconditions.CheckArgument(uri);
            return EReaderType.Unknown.Parse(uri.Scheme);
        }

        /// <summary>
        /// Get the URI Scheme for this Reader Type.
        /// </summary>
        /// <param name="type">Reader Type</param>
        /// <returns>URI Scheme</returns>
        public static string GetURIScheme(EReaderType type)
        {
            switch (type)
            {
                case EReaderType.FILE:
                    return Uri.UriSchemeFile;
                case EReaderType.FTP:
                    return Uri.UriSchemeFtp;
                case EReaderType.HTTP:
                    return Uri.UriSchemeHttp;
                case EReaderType.HTTPS:
                    return Uri.UriSchemeHttps;
                case EReaderType.SFTP:
                    return Uri.UriSchemeFtp;
            }
            return null;
        }
    }

    /// <summary>
    /// Factory class to provide config parser and config reader instances.
    /// </summary>
    public static class ConfigProviderFactory
    {
        /// <summary>
        /// Method will try to get the configuration parser based on the extension of the
        /// specified configuration file name.
        /// </summary>
        /// <param name="filename">Configuration filename</param>
        /// <returns>Configuration Parser instance</returns>
        public static AbstractConfigParser GetParser(string filename)
        {
            Preconditions.CheckArgument(filename);

            string ext = Path.GetExtension(filename);
            if (!String.IsNullOrWhiteSpace(ext))
            {
                EConfigType type = EConfigType.Unknown.Parse(ext);
                return GetParser(type);
            }
            return null;
        }

        /// <summary>
        /// Get a new configuration parser instance of the parser for the specified configuration source type.
        /// </summary>
        /// <param name="type">Configuration source type</param>
        /// <returns>Parser instance</returns>
        public static AbstractConfigParser GetParser(EConfigType type)
        {
            if (type != default(EConfigType))
            {
                switch (type)
                {
                    case EConfigType.JSON:
                        return new JSONConfigParser();
                    case EConfigType.XML:
                        return new XmlConfigParser();
                }
            }
            return null;
        }

        /// <summary>
        /// Get a new configuration writer instance of the parser for the specified configuration type.
        /// </summary>
        /// <param name="type">Configuration type</param>
        /// <returns>Writer instance</returns>
        public static AbstractConfigWriter GetWriter(EConfigType type)
        {
            switch (type)
            {
                case EConfigType.XML:
                    return new XmlConfigWriter();
            }
            return null;
        }

        /// <summary>
        /// Get a new instance of a Configuration reader. Type of reader is determined based on the
        /// SCHEME of the URI.
        /// SCHEME = "http", TYPE = HTTP
        /// SCHEME = "file", type = File
        /// </summary>
        /// <param name="source">URI to get the input from.</param>
        /// <returns>Configuration Reader instance</returns>
        public static AbstractReader GetReader(Uri source)
        {
            Preconditions.CheckArgument(source);

            EReaderType type = ReaderTypeHelper.ParseFromUri(source);
            if (type != EReaderType.Unknown)
            {
                if (type == EReaderType.HTTP || type == EReaderType.HTTPS)
                {
                    return new RemoteReader(source);
                }
                else if (type == EReaderType.FILE)
                {
                    return new FileReader(source);
                }
                else if (type == EReaderType.FTP)
                {
                    return new FtpRemoteReader(source);
                }
            }
            return null;
        }
    }
}
