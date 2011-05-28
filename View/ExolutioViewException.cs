using System;
using Exolutio.SupportingClasses;

namespace Exolutio.View
{
    public class ExolutioViewException: ExolutioException 
    {
        public ExolutioViewException(string message) : base(message)
        {
        }

        public ExolutioViewException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ExolutioViewException()
        {
        }
    }
}