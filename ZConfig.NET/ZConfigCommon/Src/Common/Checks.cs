using System;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common
{
    /// <summary>
    /// Exception class to be used to propogate object state errors.
    /// </summary>
    public class InvalidStateException : Exception
    {
        private static readonly string __PREFIX = "Invalid State Error : {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public InvalidStateException(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public InvalidStateException(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public InvalidStateException(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }
    }

    public static class Preconditions
    {
        public static void CheckArgument(bool state)
        {
            if (!state)
            {
                throw new ArgumentException();
            }
        }

        public static void CheckArgument(bool state, string mesg)
        {
            if (!state)
            {
                throw new ArgumentException(mesg);
            }
        }

        public static void CheckArgument<T>(T value)
        {
            if (value == null || value.Equals(default(T)))
            {
                throw new ArgumentException(String.Format("Argument of type {0} is NULL", typeof(T).FullName));
            }
        }

        public static void CheckArgument(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Passed String value is NULL/Empty");
            }
        }

    }

    public static class Postconditions
    {

        public static void CheckCondition(bool state)
        {
            if (!state)
            {
                throw new InvalidStateException("Object in Invalid state");
            }
        }

        public static void CheckCondition(bool state, string mesg)
        {
            if (!state)
            {
                throw new InvalidStateException(mesg);
            }
        }

        public static void CheckCondition<T>(T value)
        {
            if (value == null || value.Equals(default(T)))
            {
                throw new InvalidStateException(String.Format("Argument of type {0} is NULL", typeof(T).FullName));
            }
        }

        public static void CheckCondition(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new InvalidStateException("Passed String value is NULL/Empty");
            }
        }
    }
}
