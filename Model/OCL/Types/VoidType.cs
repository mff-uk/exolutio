using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// VoidType is the metaclass of the OclVoid type that conforms to all types except the OclInvalid type.
    /// </summary>
    public class VoidType: Classifier,IConformsToComposite
    {
        public VoidType(TypesTable.TypesTable tt)
            : base(tt,"Void")
        { }

        public override bool ConformsTo(Classifier other) {
            return ConformsToRegister(other);
        }

        public override bool ConformsToRegister(Classifier other)
        {
            return other.GetType() != typeof(InvalidType);
        }

        #region IConformsToComposite Members

        public bool ConformsToComposite(Classifier other)
        {
            return true;
        }

        #endregion

        public override Classifier CommonSuperType(Classifier other)
        {
            return other;
        }
    }
}
