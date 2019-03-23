using System;
using System.Collections.Generic;
using System.Text;
using LibZConfig.Common.Config;

namespace LibZConfig.Common.Config.Writers
{
    /// <summary>
    /// Abstract base class for defining configuration writers.
    ///Writers will serialize a configuration instance if to selected serialization format.
    /// </summary>
    public abstract class AbstractConfigWriter
    {
        /// <summary>
        /// Write this instance of the configuration to the specified output location.
        /// </summary>
        /// <param name="configuration">Configuration instance to write.</param>
        /// <param name="path">Output location to write to.</param>
        /// <returns>Return the path of the output file created.</returns>
        public abstract string Write(Configuration configuration, string path);

        /// <summary>
        /// Write this instance of the configuration to a string buffer.
        /// </summary>
        /// <param name="configuration">Configuration instance to write.</param>
        /// <returns>Return the string buffer</returns>
        public abstract string Write(Configuration configuration);
    }
}
