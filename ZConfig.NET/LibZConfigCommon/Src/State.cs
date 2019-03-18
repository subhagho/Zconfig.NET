using System;

namespace LibZConfig.Common
{
    /// <summary>
    /// Abstract base class to define an instance state.
    /// </summary>
    /// <typeparam name="T">Enum type for instance states.</typeparam>
    public abstract class AbstractState<T>
    {
        /// <summary>
        /// Instance state
        /// </summary>
        public T State { get; set; }
        private Exception exception;

        /// <summary>
        /// Set this instance in error state with the specified exception.
        /// </summary>
        /// <param name="exception">Exception instance.</param>
        public void SetError(Exception exception)
        {
            State = GetErrorState();
            this.exception = exception;
        }

        /// <summary>
        /// Get the exception for this instance. 
        /// Will return null if state is not in error state.
        /// </summary>
        /// <returns>Exception instance.</returns>
        public Exception GetError()
        {
            if (State.Equals(GetErrorState()))
            {
                return exception;
            }
            return null;
        }

        /// <summary>
        /// Check if this node has errors.
        /// </summary>
        /// <returns>Has Error?</returns>
        public bool HasError()
        {
            if (State.Equals(GetErrorState()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Abstract method to be implemented for specifying the error state.
        /// </summary>
        /// <returns>Error state enum</returns>
        public abstract T GetErrorState();
    }
}
