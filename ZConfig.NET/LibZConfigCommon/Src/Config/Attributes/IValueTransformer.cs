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
using System.Reflection;
using System.Globalization;
using LibZConfig.Common.Utils;

namespace LibZConfig.Common.Config.Attributes
{
    /// <summary>
    /// Exception class to be used to propogate transformation errors.
    /// </summary>
    public class TransformationException : Exception
    {
        private static readonly string __PREFIX = "Transformation Failed: {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public TransformationException(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public TransformationException(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public TransformationException(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }
    }

    /// <summary>
    /// Field transformation interface: to be implemented for providing field value transformations.
    /// </summary>
    /// <typeparam name="S">Source Type</typeparam>
    /// <typeparam name="T">Target Type</typeparam>
    public interface IValueTransformer<S, T>
    {
        /// <summary>
        /// Transform the source value to the target type.
        /// </summary>
        /// <param name="data">Input Data</param>
        /// <returns>Transformed Value</returns>
        T Transform(S data);

        /// <summary>
        /// Transform the target value to the soruce type.
        /// </summary>
        /// <param name="data">Input Data</param>
        /// <returns>Transformed Value</returns>
        S Reverse(T data);
    }

    /// <summary>
    /// Transformer interface to translate string values to the target type.
    /// </summary>
    /// <typeparam name="T">Target Type</typeparam>
    public interface IStringValueTransformer<T> : IValueTransformer<string, T>
    {

    }

    /// <summary>
    /// String to date transformer.
    /// 
    /// NOTE: Uses default Date/Time format "MM.dd.yyyy HH:mm:ss"
    /// </summary>
    public class StringToDateTransformer : IStringValueTransformer<DateTime>
    {
        private const string DEFAULT_DATETIME_FORMAT = "MM.dd.yyyy HH:mm:ss";

        /// <summary>
        /// Transform the target value to the soruce type.
        /// </summary>
        /// <param name="data">Input Date/time</param>
        /// <returns>String Value</returns>
        public string Reverse(DateTime data)
        {
            if (data != null)
            {
                return data.ToString(DEFAULT_DATETIME_FORMAT);
            }
            return null;
        }

        /// <summary>
        /// Transform the source value to the target type.
        /// </summary>
        /// <param name="data">Input String</param>
        /// <returns>Date/Time Value</returns>
        public DateTime Transform(string data)
        {
            if (!String.IsNullOrWhiteSpace(data))
            {
                return DateTime.ParseExact(data, DEFAULT_DATETIME_FORMAT, CultureInfo.CurrentCulture);
            }
            return default(DateTime);
        }
    }

    /// <summary>
    /// Helper class to invoke the transformer methods.
    /// </summary>
    public static class TransformerHelper
    {
        /// <summary>
        /// Call the transform method on the specified transformer type.
        /// </summary>
        /// <param name="transformerType">Transformer type</param>
        /// <param name="source">Input source data</param>
        /// <returns>Transfromed data</returns>
        public static object Transform(Type transformerType, object source)
        {
            if (!ReflectionUtils.ImplementsGenericInterface(transformerType, typeof(IValueTransformer<,>)))
            {
                throw new TransformationException(String.Format("Invalid Tranfsormer Type: [type={0}]", transformerType.FullName));
            }
            object transformer = Activator.CreateInstance(transformerType);
            Conditions.NotNull(transformer);
            MethodInfo method = transformerType.GetMethod("Transform");
            Conditions.NotNull(method);
            return method.Invoke(transformer, new[] { source });
        }

        /// <summary>
        /// Call the reverse method on the specified transformer type.
        /// </summary>
        /// <param name="transformerType">Transformer type</param>
        /// <param name="source">Input source data</param>
        /// <returns>Transfromed data</returns>
        public static object Reverse(Type transformerType, object source)
        {
            if (!ReflectionUtils.ImplementsGenericInterface(transformerType, typeof(IValueTransformer<,>)))
            {
                throw new TransformationException(String.Format("Invalid Tranfsormer Type: [type={0}]", transformerType.FullName));
            }
            object transformer = Activator.CreateInstance(transformerType);
            Conditions.NotNull(transformer);
            MethodInfo method = transformerType.GetMethod("Reverse");
            Conditions.NotNull(method);
            return method.Invoke(transformer, new[] { source });
        }
    }
}
