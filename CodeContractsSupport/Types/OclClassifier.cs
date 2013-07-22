using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    public abstract class OclClassifier
    {
        public OclBoolean conformsTo(OclClassifier cls)
        {
            return (OclBoolean)ConformsToInternal(cls);
        }
        internal abstract bool ConformsToInternal(OclClassifier cls);
    }
}
