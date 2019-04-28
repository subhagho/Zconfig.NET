using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LibZConfig.Common
{
    /// <summary>
    /// Exception class to be used to propogate state errors.
    /// </summary>
    public class StateException : Exception
    {
        private static readonly string __PREFIX = "State Error : {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public StateException(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public StateException(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public StateException(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }
    }

    /// <summary>
    /// Abstract base class used to define state of objects/instances.
    /// </summary>
    /// <typeparam name="T">State enum to be managed.</typeparam>
    public abstract class AbstractState<T>
    {
        /// <summary>
        /// State of the instance type.
        /// </summary>
        public T State { get; set; }
        /// <summary>
        /// Error handle in case of error state.
        /// </summary>
        private Exception error = null;

        /// <summary>
        /// Get the exception associated with this state. Exception handle will be returned
        /// only if the current state is error.
        /// </summary>
        /// <returns>Exception handle, null if state is not error.</returns>
        public Exception GetError()
        {
            if (HasError())
            {
                return error;
            }
            return null;
        }

        /// <summary>
        /// Set the exception handle for this state instance. Will also set the current state to error state.
        /// </summary>
        /// <param name="state">Error state to set</param>
        /// <param name="error">Exception handle.</param>
        public void SetError(T state, Exception error)
        {
            Preconditions.CheckArgument(state);
            Preconditions.CheckArgument(error);
            
            State = state;
            this.error = error;
        }

        /// <summary>
        /// Set the exception handle for this state instance. Will also set the current state to error state.
        /// </summary>
        /// <param name="error">Exception handle.</param>
        public void SetError(Exception error)
        {
            Preconditions.CheckArgument(error);
           
            State = GetDefaultErrorState();
            this.error = error;
        }

        /// <summary>
        /// Check if this node has errors.
        /// </summary>
        /// <returns>Has Error?</returns>
        public bool HasError()
        {
            T[] states = GetErrorStates();
            if (states != null)
            {
                foreach (T state in states)
                {
                    if (State.Equals(state))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get the state that represents an error state.
        /// </summary>
        /// <returns>Array Of Error states</returns>
        public abstract T[] GetErrorStates();
        /// <summary>
        /// Get the default error state for this type.
        /// </summary>
        /// <returns>Error State</returns>
        public abstract T GetDefaultErrorState();
    }
}
