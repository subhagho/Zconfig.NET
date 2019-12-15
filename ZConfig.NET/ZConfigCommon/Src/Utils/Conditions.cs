using System;
using System.Collections.Generic;
using System.Reflection;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// Exception class to be used to propogate configuration errors.
    /// </summary>
    public class ConditionError : Exception
    {
        private static readonly string __PREFIX = "Condition Error : [condition={0}] {1}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public ConditionError(string condition, string mesg) : base(String.Format(__PREFIX, condition, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public ConditionError(string condition, string mesg, Exception cause) : base(String.Format(__PREFIX, condition, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public ConditionError(string condition, Exception exception) : base(String.Format(__PREFIX, condition, exception.Message), exception)
        {

        }
    }

    /// <summary>
    /// Helper class to check for variable value conditions.
    /// </summary>
    public static class Conditions
    {
        /// <summary>
        /// Check value is not NULL/default.
        /// </summary>
        /// <typeparam name="T">Value Type</typeparam>
        /// <param name="value">Value</param>
        public static bool NotNull<T>(T value)
        {
            if (ReflectionUtils.IsNull(value))
            {
                throw new ConditionError(nameof(NotNull), String.Format("[type={0}][default={1}] : Value is NULL or default.", typeof(T).FullName, default(T)));
            }
            return true;
        }

        /// <summary>
        /// Check string is not NULL/empty.
        /// </summary>
        /// <param name="value">string value</param>
        public static bool EmptyString(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ConditionError(nameof(EmptyString), "String Value is NULL or empty.");
            }
            return false;
        }
    }
}
