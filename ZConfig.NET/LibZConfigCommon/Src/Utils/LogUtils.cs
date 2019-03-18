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
                __LOG.Debug(GetStackTrace(exception));
            }
        }

        /// <summary>
        /// Get the nested stacktrace as string for the specified exception instance.
        /// </summary>
        /// <param name="exception">Exception instance</param>
        /// <returns>Stacktrace as String</returns>
        private static string GetStackTrace(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            Exception err = exception;
            while (err != null)
            {
                builder.Append(String.Format("{0}BEGIN EXCEPTION{0}", new string('*', 12))).Append("\n");
                builder.Append(err.ToString()).Append("\n");
                builder.Append(String.Format("{0}END EXCEPTION{0}", new string('*', 13))).Append("\n");
                if (err.InnerException != null)
                {
                    err = err.InnerException;
                }
                else
                {
                    break;
                }
            }
            return builder.ToString();
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
                __LOG.Debug(GetStackTrace(exception));
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
                string json = JsonConvert.SerializeObject(data);
                string mesg = String.Format("[message={0}] {1}", message, json);
                __LOG.Trace(mesg);
            }
        }
    }
}