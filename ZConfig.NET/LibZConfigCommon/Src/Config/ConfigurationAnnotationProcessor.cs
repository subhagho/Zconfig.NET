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
                if (!String.IsNullOrWhiteSpace(path.Path))
                {
                    node = configuration.Find(path.Path);
                }
                if ((node == null || node.GetType() != typeof(ConfigPathNode)) && path.Required)
                {
                    throw new AnnotationProcessorException(String.Format("Annotation not found: [path={0}][type={1}]", path.Path, type.FullName));
                }
                if (node != null && node.GetType() == typeof(ConfigPathNode))
                {
                    target = ReadValues((ConfigPathNode)node, target);
                    CallMethodInvokes((ConfigPathNode)node, target);
                }
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
                if (!String.IsNullOrWhiteSpace(path.Path))
                {
                    node = parent.Find(path.Path);
                }
                if ((node == null || node.GetType() != typeof(ConfigPathNode)) && path.Required)
                {
                    throw new AnnotationProcessorException(String.Format("Annotation not found: [path={0}][type={1}]", path.Path, type.FullName));
                }
                if (node != null && node.GetType() == typeof(ConfigPathNode))
                {
                    target = ReadValues((ConfigPathNode)node, target);
                    CallMethodInvokes((ConfigPathNode)node, target);
                }
            }
            return target;
        }

        /// <summary>
        /// Set the target instance values reading from the passed configuration.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="target">Target Type instance</param>
        /// <returns>Updated Target Type instance</returns>
        public static T Process<T>(Configuration configuration)
        {
            Contract.Requires(configuration != null);

            Type type = typeof(T);
            T target = default(T);

            ConfigPath path = (ConfigPath)Attribute.GetCustomAttribute(type, typeof(ConfigPath));
            if (path != null)
            {
                AbstractConfigNode node = null;
                if (!String.IsNullOrWhiteSpace(path.Path))
                {
                    node = configuration.Find(path.Path);
                }
                if ((node == null || node.GetType() != typeof(ConfigPathNode)) && path.Required)
                {
                    throw new AnnotationProcessorException(String.Format("Annotation not found: [path={0}][type={1}]", path.Path, type.FullName));
                }
                if (node != null && node.GetType() == typeof(ConfigPathNode))
                {
                    target = CreateInstance<T>(type, (ConfigPathNode)node);
                    if (target == null)
                    {
                        throw new AnnotationProcessorException(String.Format("Error creating instance of Type: [path={0}][type={1}]", path.Path, type.FullName));
                    }
                    target = ReadValues((ConfigPathNode)node, target);
                    CallMethodInvokes((ConfigPathNode)node, target);
                }
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
        public static T Process<T>(AbstractConfigNode parent)
        {
            Contract.Requires(parent != null);

            Type type = typeof(T);
            T target = default(T);

            ConfigPath path = (ConfigPath)Attribute.GetCustomAttribute(type, typeof(ConfigPath));
            if (path != null)
            {
                AbstractConfigNode node = null;
                if (!String.IsNullOrWhiteSpace(path.Path))
                {
                    node = parent.Find(path.Path);
                }
                if ((node == null || node.GetType() != typeof(ConfigPathNode)) && path.Required)
                {
                    throw new AnnotationProcessorException(String.Format("Annotation not found: [path={0}][type={1}]", path.Path, type.FullName));
                }
                if (node != null && node.GetType() == typeof(ConfigPathNode))
                {
                    target = CreateInstance<T>(type, (ConfigPathNode)node);
                    if (target == null)
                    {
                        throw new AnnotationProcessorException(String.Format("Error creating instance of Type: [path={0}][type={1}]", path.Path, type.FullName));
                    }
                    target = ReadValues((ConfigPathNode)node, target);
                    CallMethodInvokes((ConfigPathNode)node, target);
                }
            }
            return target;
        }

        /// <summary>
        /// Create a new Instance of the specified type.
        /// 
        /// Type should have an empty constructor or a constructor with annotation.
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="node">Configuration node.</param>
        /// <returns>Created Instance</returns>
        public static object CreateInstance(Type type, ConfigPathNode node)
        {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors != null && constructors.Length > 0)
            {
                foreach (ConstructorInfo ci in constructors)
                {
                    MethodInvoke mi = (MethodInvoke)Attribute.GetCustomAttribute(ci, typeof(MethodInvoke));
                    if (mi != null)
                    {
                        ParameterInfo[] parameters = ci.GetParameters();
                        ConfigPathNode nnode = node;
                        if (parameters != null && parameters.Length > 0)
                        {
                            if (!String.IsNullOrWhiteSpace(mi.Path))
                            {
                                AbstractConfigNode cnode = nnode.Find(mi.Path);
                                if (cnode != null && cnode.GetType() == typeof(ConfigPathNode))
                                {
                                    nnode = (ConfigPathNode)cnode;
                                }
                            }
                            if (nnode != null)
                            {
                                ConfigParametersNode pnode = nnode.GetParameters();
                                if (pnode != null)
                                {
                                    List<object> values = FindParameters(pnode, ci.Name, parameters);
                                    if (values != null && values.Count > 0)
                                    {
                                        return Activator.CreateInstance(type, values.ToArray());
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Activator.CreateInstance(type);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Create a new Instance of the specified type.
        /// 
        /// Type should have an empty constructor or a constructor with annotation.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="type">Type</param>
        /// <param name="node">Configuration node.</param>
        /// <returns>Created Instance</returns>
        public static T CreateInstance<T>(Type type, ConfigPathNode node)
        {
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors != null && constructors.Length > 0)
            {
                foreach (ConstructorInfo ci in constructors)
                {
                    MethodInvoke mi = (MethodInvoke)Attribute.GetCustomAttribute(ci, typeof(MethodInvoke));
                    if (mi != null)
                    {
                        ParameterInfo[] parameters = ci.GetParameters();
                        ConfigPathNode nnode = node;
                        if (parameters != null && parameters.Length > 0)
                        {
                            if (!String.IsNullOrWhiteSpace(mi.Path))
                            {
                                AbstractConfigNode cnode = nnode.Find(mi.Path);
                                if (cnode != null && cnode.GetType() == typeof(ConfigPathNode))
                                {
                                    nnode = (ConfigPathNode)cnode;
                                }
                            }
                            if (nnode != null)
                            {
                                ConfigParametersNode pnode = nnode.GetParameters();
                                if (pnode != null)
                                {
                                    List<object> values = FindParameters(pnode, ci.Name, parameters);
                                    if (values != null && values.Count > 0)
                                    {
                                        return (T)Activator.CreateInstance(type, values.ToArray());
                                    }
                                }
                            }
                        }
                        else
                        {
                            return Activator.CreateInstance<T>();
                        }
                    }
                }
            }
            return default(T);
        }

        /// <summary>
        /// Check and Invoke annotated methods for this type.
        /// </summary>
        /// <typeparam name="T">Target Instance type</typeparam>
        /// <param name="node">Configuration node.</param>
        /// <param name="target">Target Type instance</param>
        private static void CallMethodInvokes<T>(ConfigPathNode node, T target)
        {
            Type type = target.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            if (methods != null)
            {
                foreach (MethodInfo method in methods)
                {
                    MethodInvoke mi = (MethodInvoke)Attribute.GetCustomAttribute(method, typeof(MethodInvoke));
                    if (mi != null)
                    {
                        bool invoked = false;
                        ParameterInfo[] parameters = method.GetParameters();
                        ConfigPathNode nnode = node;
                        if (parameters != null && parameters.Length > 0)
                        {
                            if (!String.IsNullOrWhiteSpace(mi.Path))
                            {
                                AbstractConfigNode cnode = nnode.Find(mi.Path);
                                if (cnode != null && cnode.GetType() == typeof(ConfigPathNode))
                                {
                                    nnode = (ConfigPathNode)cnode;
                                }
                            }
                            if (nnode != null)
                            {
                                ConfigParametersNode pnode = nnode.GetParameters();
                                if (pnode != null)
                                {
                                    List<object> values = FindParameters(pnode, method.Name, parameters);
                                    if (values != null && values.Count > 0)
                                    {
                                        method.Invoke(target, values.ToArray());
                                        invoked = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            method.Invoke(target, null);
                            invoked = true;
                        }

                        if (!invoked)
                        {
                            throw new AnnotationProcessorException(String.Format("Error Invoking Method : [mehtod={0}][node={1}]",
                                method.Name, node.GetSearchPath()));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find the parameter values for the method.
        /// </summary>
        /// <param name="pnode">Parameters Node</param>
        /// <param name="method">Method Name</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>List of object values</returns>
        private static List<object> FindParameters(ConfigParametersNode pnode, string method, ParameterInfo[] parameters)
        {
            List<object> values = new List<object>();
            foreach (ParameterInfo pi in parameters)
            {
                ConfigParam param = (ConfigParam)Attribute.GetCustomAttribute(pi, typeof(ConfigParam));
                if (param != null)
                {
                    object v = null;
                    ConfigValueNode cv = pnode.GetValue(param.Name);
                    if (cv != null)
                    {
                        string value = cv.GetValue();
                        if (!String.IsNullOrWhiteSpace(value))
                        {
                            v = ReflectionUtils.ConvertFromString(pi.ParameterType, value);
                        }
                    }
                    if (v != null)
                    {
                        values.Add(v);
                    }
                    else
                    {
                        throw new AnnotationProcessorException(String.Format("Error Invoking Method: Value not found for parameter. [method={0}][parameter={1}]",
                            method, pi.Name));
                    }
                }
                else
                {
                    throw new AnnotationProcessorException(String.Format("Error Invoking Method: Annotation not defined for parameter. [method={0}][parameter={1}]",
                        method, pi.Name));
                }
            }
            if (values.Count > 0)
            {
                return values;
            }
            return null;
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
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public |
                                                            BindingFlags.Instance);
            if (properties != null)
            {
                foreach (PropertyInfo property in properties)
                {
                    ConfigParam param = (ConfigParam)Attribute.GetCustomAttribute(property, typeof(ConfigParam));
                    if (param != null)
                    {
                        target = ProcessProperty(node, target, property, param);
                    }
                    ConfigAttribute attr = (ConfigAttribute)Attribute.GetCustomAttribute(property, typeof(ConfigAttribute));
                    if (attr != null)
                    {
                        target = ProcessProperty(node, target, property, attr);
                    }
                    ConfigValue value = (ConfigValue)Attribute.GetCustomAttribute(property, typeof(ConfigValue));
                    if (value != null)
                    {
                        target = ProcessProperty(node, target, property, value);
                    }
                }
            }
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public |
                                                BindingFlags.Instance);
            if (fields != null)
            {
                foreach (FieldInfo field in fields)
                {
                    ConfigParam param = (ConfigParam)Attribute.GetCustomAttribute(field, typeof(ConfigParam));
                    if (param != null)
                    {
                        target = ProcessField(node, target, field, param);
                    }
                    ConfigAttribute attr = (ConfigAttribute)Attribute.GetCustomAttribute(field, typeof(ConfigAttribute));
                    if (attr != null)
                    {
                        target = ProcessField(node, target, field, attr);
                    }
                    ConfigValue value = (ConfigValue)Attribute.GetCustomAttribute(field, typeof(ConfigValue));
                    if (value != null)
                    {
                        target = ProcessField(node, target, field, value);
                    }
                }
            }
            return target;
        }

        private static object GetValue<T>(string field, string value, Type function, Type valueType, T target, bool required)
        {
            object v = null;
            if (function != null)
            {
                v = TransformerHelper.Transform(function, value);
            }
            else
            {
                v = ReflectionUtils.ConvertFromString(valueType, value);
            }
            if (v == null && required)
            {
                throw AnnotationProcessorException.Throw(target.GetType(), field);
            }
            return v;
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
            string pname = param.Name;
            if (String.IsNullOrWhiteSpace(pname))
            {
                pname = property.Name;
            }

            string value = null;
            if (!String.IsNullOrWhiteSpace(param.Path))
            {
                AbstractConfigNode nnode = node.Find(param.Path);
                if (nnode != null && nnode.GetType() == typeof(ConfigPathNode))
                {
                    node = (ConfigPathNode)nnode;
                }
                else
                {
                    node = null;
                }
            }
            if (node != null)
            {
                ConfigParametersNode pnode = node.GetParameters();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(value))
            {
                object v = GetValue<T>(pname, value, param.Function, property.PropertyType, target, param.Required);
                if (v != null)
                    property.SetValue(target, v);
            }
            else if (param.Required)
            {
                throw AnnotationProcessorException.Throw(target.GetType(), pname);
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
            string pname = attr.Name;
            if (String.IsNullOrWhiteSpace(pname))
            {
                pname = property.Name;
            }

            string value = null;
            if (!String.IsNullOrWhiteSpace(attr.Path))
            {
                AbstractConfigNode nnode = node.Find(attr.Path);
                if (nnode != null && nnode.GetType() == typeof(ConfigPathNode))
                {
                    node = (ConfigPathNode)nnode;
                }
                else
                {
                    node = null;
                }
            }
            if (node != null)
            {
                ConfigAttributesNode pnode = node.GetAttributes();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(value))
            {
                object v = GetValue<T>(pname, value, attr.Function, property.PropertyType, target, attr.Required);
                if (v != null)
                    property.SetValue(target, v);
            }
            else if (attr.Required)
            {
                throw AnnotationProcessorException.Throw(target.GetType(), pname);
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
            string pname = configValue.Name;
            if (String.IsNullOrWhiteSpace(pname))
            {
                pname = property.Name;
            }

            string value = null;
            if (!String.IsNullOrWhiteSpace(configValue.Path))
            {
                AbstractConfigNode nnode = node.Find(configValue.Path);
                if (nnode != null && nnode.GetType() == typeof(ConfigPathNode))
                {
                    node = (ConfigPathNode)nnode;
                }
                else
                {
                    node = null;
                }
            }

            if (node != null)
            {
                AbstractConfigNode cnode = node.GetChildNode(pname);
                if (cnode.GetType() == typeof(ConfigValueNode))
                {
                    ConfigValueNode vn = (ConfigValueNode)cnode;
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                    if (!String.IsNullOrWhiteSpace(value))
                    {
                        object v = GetValue<T>(pname, value, configValue.Function, property.PropertyType, target, configValue.Required);
                        if (v != null)
                            property.SetValue(target, v);
                    }
                }
                else
                {
                    if (ReflectionUtils.IsSubclassOfRawGeneric(property.PropertyType, typeof(List<>)))
                    {
                        if (cnode.GetType() == typeof(ConfigListValueNode))
                        {
                            ConfigListValueNode configList = (ConfigListValueNode)cnode;
                            List<string> values = configList.GetValueList();
                            if (values != null)
                            {
                                Type inner = property.PropertyType.GetGenericArguments()[0];
                                object v = ReflectionUtils.ConvertListFromStrings(inner, values);
                                if (v != null)
                                {
                                    property.SetValue(target, v);
                                }
                            }
                        }
                    }
                    else if (ReflectionUtils.IsSubclassOfRawGeneric(property.PropertyType, typeof(HashSet<>)))
                    {
                        if (cnode.GetType() == typeof(ConfigListValueNode))
                        {
                            ConfigListValueNode configList = (ConfigListValueNode)cnode;
                            List<string> values = configList.GetValueList();
                            if (values != null)
                            {
                                Type inner = property.PropertyType.GetGenericArguments()[0];
                                object v = ReflectionUtils.ConvertSetFromStrings(inner, values);
                                if (v != null)
                                {
                                    property.SetValue(target, v);
                                }
                            }
                        }
                    }
                }
            }
            object ov = property.GetValue(target);
            if (ov == null && configValue.Required)
            {
                throw AnnotationProcessorException.Throw(target.GetType(), pname);
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
            string pname = param.Name;
            if (String.IsNullOrWhiteSpace(pname))
            {
                pname = field.Name;
            }

            string value = null;
            if (!String.IsNullOrWhiteSpace(param.Path))
            {
                AbstractConfigNode nnode = node.Find(param.Path);
                if (nnode != null && nnode.GetType() == typeof(ConfigPathNode))
                {
                    node = (ConfigPathNode)nnode;
                }
                else
                {
                    node = null;
                }
            }
            if (node != null)
            {
                ConfigParametersNode pnode = node.GetParameters();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(value))
            {
                object v = GetValue<T>(pname, value, param.Function, field.FieldType, target, param.Required);
                if (v != null)
                    TypeUtils.CallSetter(field, target, v);
            }
            else if (param.Required)
            {
                throw AnnotationProcessorException.Throw(target.GetType(), pname);
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
            string pname = attr.Name;
            if (String.IsNullOrWhiteSpace(pname))
            {
                pname = field.Name;
            }

            string value = null;
            if (!String.IsNullOrWhiteSpace(attr.Path))
            {
                AbstractConfigNode nnode = node.Find(attr.Path);
                if (nnode != null && nnode.GetType() == typeof(ConfigPathNode))
                {
                    node = (ConfigPathNode)nnode;
                }
                else
                {
                    node = null;
                }
            }

            if (node != null)
            {
                ConfigAttributesNode pnode = node.GetAttributes();
                if (pnode != null)
                {
                    ConfigValueNode vn = pnode.GetValue(pname);
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(value))
            {
                object v = GetValue<T>(pname, value, attr.Function, field.FieldType, target, attr.Required);
                if (v != null)
                    TypeUtils.CallSetter(field, target, v);
            }
            else if (attr.Required)
            {
                throw AnnotationProcessorException.Throw(target.GetType(), pname);
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
            string pname = configValue.Name;
            if (String.IsNullOrWhiteSpace(pname))
            {
                pname = field.Name;
            }

            string value = null;
            if (!String.IsNullOrWhiteSpace(configValue.Path))
            {
                AbstractConfigNode nnode = node.Find(configValue.Path);
                if (nnode != null && nnode.GetType() == typeof(ConfigPathNode))
                {
                    node = (ConfigPathNode)nnode;
                }
                else
                {
                    node = null;
                }
            }

            if (node != null)
            {
                AbstractConfigNode cnode = node.GetChildNode(pname);
                if (cnode.GetType() == typeof(ConfigValueNode))
                {
                    ConfigValueNode vn = (ConfigValueNode)cnode;
                    if (vn != null)
                    {
                        value = vn.GetValue();
                    }
                    if (!String.IsNullOrWhiteSpace(value))
                    {
                        object v = GetValue<T>(pname, value, configValue.Function, field.FieldType, target, configValue.Required);
                        if (v != null)
                            TypeUtils.CallSetter(field, target, v);
                    }
                }
                else
                {
                    if (ReflectionUtils.IsSubclassOfRawGeneric(field.FieldType, typeof(List<>)))
                    {
                        if (cnode.GetType() == typeof(ConfigListValueNode))
                        {
                            ConfigListValueNode configList = (ConfigListValueNode)cnode;
                            List<string> values = configList.GetValueList();
                            if (values != null)
                            {
                                Type inner = field.FieldType.GetGenericArguments()[0];
                                object v = ReflectionUtils.ConvertListFromStrings(inner, values);
                                if (v != null)
                                    TypeUtils.CallSetter(field, target, v);
                            }
                        }
                    }
                    else if (ReflectionUtils.IsSubclassOfRawGeneric(field.FieldType, typeof(HashSet<>)))
                    {
                        if (cnode.GetType() == typeof(ConfigListValueNode))
                        {
                            ConfigListValueNode configList = (ConfigListValueNode)cnode;
                            List<string> values = configList.GetValueList();
                            if (values != null)
                            {
                                Type inner = field.FieldType.GetGenericArguments()[0];
                                object v = ReflectionUtils.ConvertSetFromStrings(inner, values);
                                if (v != null)
                                    TypeUtils.CallSetter(field, target, v);
                            }
                        }
                    }
                }
            }
            object ov = TypeUtils.CallGetter(field, target);
            if (ov == null && configValue.Required)
            {
                throw AnnotationProcessorException.Throw(target.GetType(), pname);
            }

            return target;
        }
    }
}
