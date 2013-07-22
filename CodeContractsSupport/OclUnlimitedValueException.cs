using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// Invalid use of unlimited value (for example adding, converting to int)
    /// </summary>
    public class OclUnlimitedValueException : ArithmeticException
    {
        public OclUnlimitedValueException()
            : base("Unlimited value")
        {
        }
    }
}
