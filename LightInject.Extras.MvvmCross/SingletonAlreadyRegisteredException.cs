using System;

namespace LightInject.Extras.MvvmCross
{
    /// <summary>
    /// 
    /// </summary>
    public class SingletonAlreadyRegisteredException : Exception
    {
        /// <inheritdoc/>
        public SingletonAlreadyRegisteredException()
            : base()
        {

        }

        /// <inheritdoc/>
        public SingletonAlreadyRegisteredException(string message)
            : base(message)
        {

        }

        /// <inheritdoc/>
        public SingletonAlreadyRegisteredException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
