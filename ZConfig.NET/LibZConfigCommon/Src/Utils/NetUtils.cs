using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
    }
}
