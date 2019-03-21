using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Attributes;

namespace LibZConfig.Common.Config
{
    /// <summary>
    /// Exception class to be used to propogate annotation processor errors.
    /// </summary>
    public class AnnotationProcessorException : Exception
    {
        private static readonly string __PREFIX = "Transformation Failed: {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public AnnotationProcessorException(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public AnnotationProcessorException(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public AnnotationProcessorException(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }

        public static AnnotationProcessorException Throw(Type type, string property)
        {
            string mesg = String.Format("Required value not found: [type={0}][name={1}]", type.FullName, property);
            return new AnnotationProcessorException(mesg);
        }
    }

    /// <summary>
    /// Annotation processor class: Reads the defined Attribute annotations for a type and
    /// sets the values based on the passed configuration.
    /// </summary>
    public static class ConfigurationAnnotationProcessor
    {
        /// <summary>
        /// Set the target instance values reading from the passed configuration.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="target">Target Type instance</param>
        /// <returns>Updated Target Type instance</returns>
        public static T Process<T>(Configuration configuration, T target)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(target != null);

            Type type = target.GetType();
            ConfigPath path = (ConfigPath)Attribute.GetCustomAttribute(type, typeof(ConfigPath));
            if (path != null)
            {
                AbstractConfigNode node = null;
                if (!String.IsNullOrWhiteSpace(path.path))
                {
                    node = configuration.Find(path.path);
                }
                if ((node == null || node.GetType() != typeof(ConfigPathNode)) && path.required)
                {
                    throw new AnnotationProcessorException(String.Format("Annotation not found: [path={0}][type={1}]", path.path, type.FullName));
                }
                if (node != null && node.GetType() == typeof(ConfigPathNode))
                    return ReadValues((ConfigPathNode)node, target);
            }
            return target;
        }

        /// <summary>
        /// Set the target instance values reading from the passed configuration node.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="parent">Configuration node instance</param>
        /// <param name="target">Target Type instance</param>
        /// <returns>Updated Target Type instance</returns>
        public static T Process<T>(AbstractConfigNode parent, T target)
        {
            Contract.Requires(parent != null);
            Contract.Requires(target != null);

            Type type = target.GetType();
            ConfigPath path = (ConfigPath)Attribute.GetCustomAttribute(type, typeof(ConfigPath));
            if (path != null)
            {
                AbstractConfigNode node = null;
                if (!String.IsNullOrWhiteSpace(path.path))
                {
                    node = parent.Find(path.path);
                }
                if ((node == null || node.GetType() != typeof(ConfigPathNode)) && path.required)
                {
                    throw new AnnotationProcessorException(String.Format("Annotation not found: [path={0}][type={1}]", path.path, type.FullName));
                }
                if (node != null && node.GetType() == typeof(ConfigPathNode))
                    return ReadValues((ConfigPathNode)node, target);
            }
            return target;
        }

        /// <summary>
        /// Read the configuration values from the passed node and
        /// update the target instance.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration Node to read values from</param>
        /// <param name="target">Target Type instance</param>
        /// <returns>Updated Target Type instance</returns>
        private static T ReadValues<T>(ConfigPathNode node, T target)
        {
            Type type = target.GetType();
            PropertyInfo[] properties = type.GetProperties();
            if (properties != null)
            {
                foreach (PropertyInfo property in properties)
                {
                    ConfigParam param = (ConfigParam)Attribute.GetCustomAttribute(property, typeof(ConfigParam));
                    if (param != null)
                    {
                        return ProcessProperty(node, target, property, param);
                    }
                    ConfigAttribute attr = (ConfigAttribute)Attribute.GetCustomAttribute(property, typeof(ConfigAttribute));
                    if (attr != null)
                    {
                        return ProcessProperty(node, target, property, attr);
                    }
                    ConfigValue value = (ConfigValue)Attribute.GetCustomAttribute(property, typeof(ConfigValue));
                    if (value != null)
                    {
                        return ProcessProperty(node, target, property, value);
                    }
                }
            }
            FieldInfo[] fields = type.GetFields();
            if (fields != null)
            {
                foreach (FieldInfo field in fields)
                {
                    ConfigParam param = (ConfigParam)Attribute.GetCustomAttribute(field, typeof(ConfigParam));
                    if (param != null)
                    {
                        return ProcessField(node, target, field, param);
                    }
                    ConfigAttribute attr = (ConfigAttribute)Attribute.GetCustomAttribute(field, typeof(ConfigAttribute));
                    if (attr != null)
                    {
                        return ProcessField(node, target, field, attr);
                    }
                    ConfigValue value = (ConfigValue)Attribute.GetCustomAttribute(field, typeof(ConfigValue));
                    if (value != null)
                    {
                        return ProcessField(node, target, field, value);
                    }
                }
            }
            return target;
        }

        /// <summary>
        /// Process the annotated property and set the values from the configuration parameters.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration Node</param>
        /// <param name="target">Target Type instance.</param>
        /// <param name="property">Property to update</param>
        /// <param name="param">Config param annotation</param>
        /// <returns>Updated Target Type instance.</returns>
        private static T ProcessProperty<T>(ConfigPathNode node, T target, PropertyInfo property, ConfigParam param)
        {
            string pname = param.name;
            if (!String.IsNullOrWhiteSpace(pname))
            {
                string value = null;
                ConfigParametersNode pnode = node.GetParameters();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
                if (!String.IsNullOrWhiteSpace(value))
                {
                    object v = null;
                    if (param.transformer != null)
                    {
                        v = TransformerHelper.Transform(param.transformer, value);
                    }
                    else
                    {
                        v = ReflectionUtils.ConvertFromString(property.PropertyType, value);
                    }
                    if (v == null && param.required)
                    {
                        throw AnnotationProcessorException.Throw(target.GetType(), pname);
                    }
                    if (v != null)
                        property.SetValue(target, v);
                }
                else if (param.required)
                {
                    throw AnnotationProcessorException.Throw(target.GetType(), pname);
                }
            }
            return target;
        }
        /// <summary>
        /// Process the annotated property and set the values from the configuration attributes.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration Node</param>
        /// <param name="target">Target Type instance.</param>
        /// <param name="property">Property to update</param>
        /// <param name="attr">Config attribute annotation</param>
        /// <returns>Updated Target Type instance.</returns>

        private static T ProcessProperty<T>(ConfigPathNode node, T target, PropertyInfo property, ConfigAttribute attr)
        {
            string pname = attr.name;
            if (!String.IsNullOrWhiteSpace(pname))
            {
                string value = null;
                ConfigAttributesNode pnode = node.GetAttributes();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
                if (!String.IsNullOrWhiteSpace(value))
                {
                    object v = null;
                    if (attr.transformer != null)
                    {
                        v = TransformerHelper.Transform(attr.transformer, value);
                    }
                    else
                    {
                        v = ReflectionUtils.ConvertFromString(property.PropertyType, value);
                    }
                    if (v == null && attr.required)
                    {
                        throw AnnotationProcessorException.Throw(target.GetType(), pname);
                    }
                    if (v != null)
                        property.SetValue(target, v);
                }
                else if (attr.required)
                {
                    throw AnnotationProcessorException.Throw(target.GetType(), pname);
                }
            }
            return target;
        }

        /// <summary>
        /// Process the annotated property and set the values from the configuration value.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration Node</param>
        /// <param name="target">Target Type instance.</param>
        /// <param name="property">Property to update</param>
        /// <param name="configValue">Config value annotation</param>
        /// <returns>Updated Target Type instance.</returns>
        private static T ProcessProperty<T>(ConfigPathNode node, T target, PropertyInfo property, ConfigValue configValue)
        {
            string pname = configValue.name;
            if (!String.IsNullOrWhiteSpace(pname))
            {
                string value = null;

                AbstractConfigNode cnode = node.GetChildNode(pname);
                if (cnode.GetType() == typeof(ConfigValueNode))
                {
                    ConfigValueNode vn = (ConfigValueNode)cnode;
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
                if (!String.IsNullOrWhiteSpace(value))
                {
                    object v = null;
                    if (configValue.transformer != null)
                    {
                        v = TransformerHelper.Transform(configValue.transformer, value);
                    }
                    else
                    {
                        v = ReflectionUtils.ConvertFromString(property.PropertyType, value);
                    }
                    if (v == null && configValue.required)
                    {
                        throw AnnotationProcessorException.Throw(target.GetType(), pname);
                    }
                    if (v != null)
                        property.SetValue(target, v);
                }
                else if (configValue.required)
                {
                    throw AnnotationProcessorException.Throw(target.GetType(), pname);
                }
            }
            return target;
        }

        /// <summary>
        /// Process the annotated field and set the values from the configuration parameters.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration Node</param>
        /// <param name="target">Target Type instance.</param>
        /// <param name="field">Property to update</param>
        /// <param name="param">Config param annotation</param>
        /// <returns>Updated Target Type instance.</returns>
        private static T ProcessField<T>(ConfigPathNode node, T target, FieldInfo field, ConfigParam param)
        {
            string pname = param.name;
            if (!String.IsNullOrWhiteSpace(pname))
            {
                string value = null;
                ConfigParametersNode pnode = node.GetParameters();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
                if (!String.IsNullOrWhiteSpace(value))
                {
                    object v = null;
                    if (param.transformer != null)
                    {
                        v = TransformerHelper.Transform(param.transformer, value);
                    }
                    else
                    {
                        v = ReflectionUtils.ConvertFromString(field.FieldType, value);
                    }
                    if (v == null && param.required)
                    {
                        throw AnnotationProcessorException.Throw(target.GetType(), pname);
                    }
                    if (v != null)
                        TypeUtils.CallSetter(field, target, v);
                }
                else if (param.required)
                {
                    throw AnnotationProcessorException.Throw(target.GetType(), pname);
                }
            }
            return target;
        }

        /// <summary>
        /// Process the annotated field and set the values from the configuration attributes.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration Node</param>
        /// <param name="target">Target Type instance.</param>
        /// <param name="field">Property to update</param>
        /// <param name="attr">Config attribute annotation</param>
        /// <returns>Updated Target Type instance.</returns>
        private static T ProcessField<T>(ConfigPathNode node, T target, FieldInfo field, ConfigAttribute attr)
        {
            string pname = attr.name;
            if (!String.IsNullOrWhiteSpace(pname))
            {
                string value = null;
                ConfigAttributesNode pnode = node.GetAttributes();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
                if (!String.IsNullOrWhiteSpace(value))
                {
                    object v = null;
                    if (attr.transformer != null)
                    {
                        v = TransformerHelper.Transform(attr.transformer, value);
                    }
                    else
                    {
                        v = ReflectionUtils.ConvertFromString(field.FieldType, value);
                    }
                    if (v == null && attr.required)
                    {
                        throw AnnotationProcessorException.Throw(target.GetType(), pname);
                    }
                    if (v != null)
                        TypeUtils.CallSetter(field, target, v);
                }
                else if (attr.required)
                {
                    throw AnnotationProcessorException.Throw(target.GetType(), pname);
                }
            }
            return target;
        }

        /// <summary>
        /// Process the annotated field and set the values from the configuration value.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration Node</param>
        /// <param name="target">Target Type instance.</param>
        /// <param name="field">Property to update</param>
        /// <param name="configValue">Config value annotation</param>
        /// <returns>Updated Target Type instance.</returns>
        private static T ProcessField<T>(ConfigPathNode node, T target, FieldInfo field, ConfigValue configValue)
        {
            string pname = configValue.name;
            if (!String.IsNullOrWhiteSpace(pname))
            {
                string value = null;

                AbstractConfigNode cnode = node.GetChildNode(pname);
                if (cnode.GetType() == typeof(ConfigValueNode))
                {
                    ConfigValueNode vn = (ConfigValueNode)cnode;
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
                if (!String.IsNullOrWhiteSpace(value))
                {
                    object v = null;
                    if (configValue.transformer != null)
                    {
                        v = TransformerHelper.Transform(configValue.transformer, value);
                    }
                    else
                    {
                        v = ReflectionUtils.ConvertFromString(field.FieldType, value);
                    }
                    if (v == null && configValue.required)
                    {
                        throw AnnotationProcessorException.Throw(target.GetType(), pname);
                    }
                    if (v != null)
                        TypeUtils.CallSetter(field, target, v);
                }
                else if (configValue.required)
                {
                    throw AnnotationProcessorException.Throw(target.GetType(), pname);
                }
            }
            return target;
        }
    }
}
