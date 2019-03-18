using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// C# equivalent of a Java properties.
    /// </summary>
    public class Properties
    {
        private Dictionary<string, string> properties = new Dictionary<string, string>();

        /// <summary>
        /// Get a property value.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <returns>Value or NULL</returns>
        public string GetProperty(string name)
        {
            if (properties.ContainsKey(name))
            {
                return properties[name];
            }
            return null;
        }

        /// <summary>
        /// Add a new Property name/value.
        /// </summary>
        /// <param name="name">Property Name</param>
        /// <param name="value">Property Value</param>
        /// <returns>Self</returns>
        public Properties AddProperty(string name, string value)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(name));
            properties[name] = value;
            return this;
        }

        /// <summary>
        /// Load the properties from the specified file.
        /// </summary>
        /// <param name="filename">Properties file path.</param>
        /// <returns>Self</returns>
        public Properties Load(string filename)
        {
            Contract.Assert(!String.IsNullOrWhiteSpace(filename));
            using (StreamReader reader = new StreamReader(filename))
            {
                Load(reader);
            }
            return this;
        }

        /// <summary>
        /// Load the properties from the specified stream.
        /// </summary>
        /// <param name="reader">Input Stream reader</param>
        /// <returns>Self</returns>
        public Properties Load(StreamReader reader)
        {
            Contract.Assert(reader != null);
            string line;
            bool comment = false;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (comment)
                {
                    if (line.Contains("*/"))
                    {
                        comment = false;
                    }
                    continue;
                }
                if (line.StartsWith("//") || line.StartsWith("#"))
                {
                    continue;
                }
                if (line.StartsWith("/*"))
                {
                    comment = true;
                    continue;
                }
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string name = parts[0].Trim();
                    string value = parts[1];
                    AddProperty(name, value);
                }
            }
            return this;
        }
    }
}
