using System;
using System.Reflection;
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
            if (transformer == null)
            {
                throw new TransformationException(String.Format("Error creating Tranfsormer instance: [type={0}]", transformerType.FullName));
            }
            MethodInfo method = transformerType.GetMethod("Transform");
            if (method == null)
            {
                throw new TransformationException(String.Format("Error getting Transform method from instance: [type={0}]", transformerType.FullName));
            }
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
            if (transformer == null)
            {
                throw new TransformationException(String.Format("Error creating Tranfsormer instance: [type={0}]", transformerType.FullName));
            }
            MethodInfo method = transformerType.GetMethod("Reverse");
            if (method == null)
            {
                throw new TransformationException(String.Format("Error getting Reverse method from instance: [type={0}]", transformerType.FullName));
            }
            return method.Invoke(transformer, new[] { source });
        }
    }
}
