using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// An AssociationClassCallExp is a reference to an AssociationClass defined in a UML model. It is used to determine
    /// objects linked to a target object by an association class. The expression refers to these target objects by the name of the
    /// target associationclass.
    /// </summary>
    public class AssociationClassCallExp //: NavigationCallExp
    {
        /// <summary>
        /// The AssociationClass to which this AssociationClassCallExp is a reference. This refers to an
        /// AssociationClass that is defined in the UML model.
        /// </summary>
        public AssociationClass referredAssociationClass
        {
            get;
            set;
        }
    }
}
