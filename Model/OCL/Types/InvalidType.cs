using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// InvalidType represents a type that conforms to all types except the VoidType type.
    /// </summary>
    public class InvalidType:Classifier,IConformsToComposite
    {
        public InvalidType(TypesTable.TypesTable tt, string name)
            : base(tt,name)
        { }

        public override bool ConformsTo(Classifier other) {
            return ConformsToRegister(other);
        }

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
