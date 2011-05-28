using System;
using System.Runtime.Serialization;
using Exolutio.SupportingClasses;

namespace Exolutio.Model
{
    public class ExolutioModelException : ExolutioException 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExolutioModelException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ExolutioModelException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExolutioModelException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception. </param><param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified. </param>
        public ExolutioModelException(string message, Exception innerException) : base(message, innerException)
        {
        }

                /// <summary>
        /// Initializes a new instance of the <see cref="ExolutioModelException"/> class.
        /// </summary>
        public ExolutioModelException()
        {
        }
    }
}