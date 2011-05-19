using System;
using EvoX.SupportingClasses;

namespace EvoX.View
{
    public class EvoXViewException: EvoXException 
    {
        public EvoXViewException(string message) : base(message)
        {
        }

        public EvoXViewException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EvoXViewException()
        {
        }
    }
}