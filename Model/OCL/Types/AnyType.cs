using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// AnyType is the metaclass of the special type OclAny, which is the type to which all other types conform. OclAny is the
    /// sole instance of AnyType. This metaclass allows defining the special property of being the generalization of all other
    /// Classifiers, including Classes, DataTypes, and PrimitiveTypes.
    /// </summary>
    public class AnyType : Classifier
    {
        public AnyType()
            : base("OCLAny")
        { }
    }
}
