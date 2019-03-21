using System;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Config.Attributes
{
    /// <summary>
    /// Annotation to be used for auto-wiring configurations.
    /// Annotation specfies the search path for this type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConfigPath : Attribute
    {
        /// <summary>
        /// Search Path to find the configuration node.
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Is the search mandatory?
        /// </summary>
        public bool required { get; set; }
    }

    /// <summary>
    /// Annotation to be used to define configuration mapping for auto-wired configuration elements.
    /// Annotation reads values from config value elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ConfigValue : Attribute
    {
        /// <summary>
        /// Name of the node, if null will use the Field/Property name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Is the value mandatory?
        /// </summary>
        public bool required { get; set; }

        /// <summary>
        /// Transformer to handle the type conversion if required.
        /// </summary>
        public Type transformer { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ConfigValue()
        {
            name = null;
            required = true;
            transformer = null;
        }
    }

    /// <summary>
    /// Annotation to be used to define configuration mapping for auto-wired configuration elements.
    /// Annotation reads values from config attribute elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ConfigAttribute : Attribute
    {
        /// <summary>
        /// Name of the node, if null will use the Field/Property name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Is the value mandatory?
        /// </summary>
        public bool required { get; set; }

        /// <summary>
        /// Transformer to handle the type conversion if required.
        /// </summary>
        public Type transformer { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ConfigAttribute()
        {
            name = null;
            required = true;
            transformer = null;
        }
    }

    /// <summary>
    /// Annotation to be used to define configuration mapping for auto-wired configuration elements.
    /// Annotation reads values from config parameter elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ConfigParam : Attribute
    {
        /// <summary>
        /// Name of the node, if null will use the Field/Property name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Is the value mandatory?
        /// </summary>
        public bool required { get; set; }

        /// <summary>
        /// Transformer to handle the type conversion if required.
        /// </summary>
        public Type transformer { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ConfigParam()
        {
            name = null;
            required = true;
            transformer = null;
        }
    }
}
