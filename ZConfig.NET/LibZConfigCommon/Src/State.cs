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
        /// <param name="state">Error state to set</param>
        /// <param name="exception">Exception instance.</param>
        public void SetError(T state, Exception exception)
        {
            State = state;
            this.exception = exception;
        }

        /// <summary>
        /// Get the exception for this instance. 
        /// Will return null if state is not in error state.
        /// </summary>
        /// <returns>Exception instance.</returns>
        public Exception GetError()
        {
            if (HasError())
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
            T[] states = GetErrorStates();
            if (states != null)
            {
                foreach(T state in states)
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
        /// Abstract method to be implemented for specifying the error state(s).
        /// </summary>
        /// <returns>Error state(s) enum</returns>
        public abstract T[] GetErrorStates();
    }
}
