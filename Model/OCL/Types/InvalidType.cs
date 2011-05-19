using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    /// <summary>
    /// InvalidType represents a type that conforms to all types except the VoidType type.
    /// </summary>
    public class InvalidType:Classifier,IConformsToComposite
    {
        public InvalidType()
            : base("Invalid")
        { }

        public override bool ConformsToRegister(Classifier other)
        {
            return true;
        }

        #region IConformsToComposite Members

        public bool ConformsToComposite(Classifier other)
        {
            return true;
        }

        #endregion
    }
}
