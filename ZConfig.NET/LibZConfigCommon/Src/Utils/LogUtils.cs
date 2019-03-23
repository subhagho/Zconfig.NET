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
using NLog;
using System;
using System.Text;
using Newtonsoft.Json;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// Utility class for logging interfaces.
    /// </summary>
    public static class LogUtils
    {
        private static readonly Logger __LOG = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Log an Error message.
        /// </summary>
        /// <param name="message">Error message</param>
        public static void Error(string message)
        {
            __LOG.Error(message);
        }

        /// <summary>
        /// Log the exception:
        ///     Message + Stacktrace (if in debug mode)
        /// </summary>
        /// <param name="exception">Exception instance</param>
        public static void Error(Exception exception)
        {
            __LOG.Error(exception.Message);
            if (__LOG.IsDebugEnabled)
            {
               GetStackTrace(exception);
            }
        }

        /// <summary>
        /// Get the nested stacktrace as string for the specified exception instance.
        /// </summary>
        /// <param name="exception">Exception instance</param>
        /// <returns>Stacktrace as String</returns>
        private static void GetStackTrace(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            Exception err = exception;
            while (err != null)
            {
                __LOG.Debug(String.Format("{0}BEGIN EXCEPTION{0}", new string('*', 12)));
                __LOG.Debug(err.ToString());
                __LOG.Debug(String.Format("{0}END EXCEPTION{0}", new string('*', 13)));
                if (err.InnerException != null)
                {
                    err = err.InnerException;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        /// <param name="message">Warning message</param>
        public static void Warn(string message)
        {
            __LOG.Warn(message);
        }

        /// <summary>
        /// Log the specified exception instance as a warning.
        ///     Message + Stacktrace (if in debug mode)
        /// </summary>
        /// <param name="exception">Exception instance.</param>
        public static void Warn(Exception exception)
        {
            __LOG.Warn(exception.Message);
            if (__LOG.IsDebugEnabled)
            {
                GetStackTrace(exception);
            }
        }

        /// <summary>
        /// Log an information message.
        /// </summary>
        /// <param name="message">Information message</param>
        public static void Info(string message)
        {
            __LOG.Info(message);
        }

        /// <summary>
        /// Log a debug message.
        /// </summary>
        /// <param name="message">Debug message.</param>
        public static void Debug(string message)
        {
            __LOG.Debug(message);
        }

        /// <summary>
        /// Log a debug message and the specified object handle as a JSON dump.
        /// JSON dump will only be logged in Trace mode.
        /// </summary>
        /// <param name="message">Debug message.</param>
        /// <param name="data">Object instance to dump</param>
        public static void Debug(string message, object data)
        {
            if (data == null || !__LOG.IsTraceEnabled)
            {
                __LOG.Debug(message);
            }
            else
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                string json = JsonConvert.SerializeObject(data, settings);
                string mesg = String.Format("[message={0}] {1}", message, json);
                __LOG.Trace(mesg);
            }
        }
    }
}