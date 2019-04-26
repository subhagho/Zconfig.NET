using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// Network Related Utility functions.
    /// </summary>
    public static class NetUtils
    {
        /// <summary>
        /// Get an IP address string (first IP that is not loopback)
        /// </summary>
        /// <param name="family">IP Address Type</param>
        /// <returns>IP Address</returns>
        public static string GetIpAddress(AddressFamily family)
        {
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            if (entry != null)
            {
                foreach (var ip in entry.AddressList)
                {
                    if (ip.AddressFamily == family)
                    {
                        if (IPAddress.IsLoopback(ip))
                        {
                            continue;
                        }
                        return ip.ToString();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get an v4 IP address string (first IP that is not loopback)
        /// </summary>
        /// <returns>IP Address</returns>
        public static string GetIpAddress()
        {
            return GetIpAddress(AddressFamily.InterNetwork);
        }

        /// <summary>
        /// Get the hostname of the current machine.
        /// </summary>
        /// <returns>Hostname</returns>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        /// <summary>
        /// Get the URI path for the specified file path.
        /// 
        /// https://stackoverflow.com/questions/1546419/convert-file-path-to-a-file-uri
        /// </summary>
        /// <param name="filePath">Local filepath</param>
        /// <returns>URI file path</returns>
        public static string FilePathToFileUrl(string filePath)
        {
            StringBuilder uri = new StringBuilder();
            foreach (char v in filePath)
            {
                if ((v >= 'a' && v <= 'z') || (v >= 'A' && v <= 'Z') || (v >= '0' && v <= '9') ||
                  v == '+' || v == '/' || v == ':' || v == '.' || v == '-' || v == '_' || v == '~' ||
                  v > '\xFF')
                {
                    uri.Append(v);
                }
                else if (v == Path.DirectorySeparatorChar || v == Path.AltDirectorySeparatorChar)
                {
                    uri.Append('/');
                }
                else
                {
                    uri.Append(String.Format("%{0:X2}", (int)v));
                }
            }
            if (uri.Length >= 2 && uri[0] == '/' && uri[1] == '/') // UNC path
                uri.Insert(0, "file:");
            else
                uri.Insert(0, "file:///");
            return uri.ToString();
        }
    }
}
