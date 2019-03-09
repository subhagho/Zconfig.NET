using System;

namespace LibZConfig.Common.Config
{
    /// <summary>
    /// Exception class to be used to propogate configuration errors.
    /// </summary>
    public class ConfigurationException : Exception
    {
        private static readonly string __PREFIX = "Configuration Error : {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public ConfigurationException(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public ConfigurationException(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public ConfigurationException(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }

        /// <summary>
        /// Create a property missing exception.
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns>Configuration Exception</returns>
        public static ConfigurationException PropertyMissingException(string property)
        {
            return new ConfigurationException(String.Format("Required property missing : [name={0}]", property));
        }
    }

    public class Configuration
    {
        public ConfigurationSettings Settings { get; set; }
    }
}
