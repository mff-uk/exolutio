using System;
using System.Runtime.Serialization;
using EvoX.SupportingClasses;

namespace EvoX.Model
{
    public class EvoXModelException : EvoXException 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvoXModelException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EvoXModelException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvoXModelException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public EvoXModelException(string message, Exception innerException) : base(message, innerException)
        {
        }

                /// <summary>
        /// Initializes a new instance of the <see cref="EvoXModelException"/> class.
        /// </summary>
        public EvoXModelException()
        {
        }
    }
}