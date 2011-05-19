using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.Types;

namespace EvoX.Model.OCL.AST
{

    /// <summary>
    /// A NavigationCallExp is a reference to a Property or an AssociationClass defined in a UML model. It is used to determine
    /// objects linked to a target object by an association, whether explicitly modeled as an Association or implicit. If there is a
    /// qualifier attached to the source end of the association, then additional qualifier expressions may be used to specify the
    /// values of the qualifying attributes.
    /// </summary>
    public class NavigationCallExp : FeatureCallExp
    {
        /// <summary>
        /// The values for the qualifier attributes if applicable.
        /// </summary>
        public OclExpression Qualifier
        {
            get;
            set;
        }

        /// <summary>
        /// The source denotes the association end Property at the end of the object itself. This is used to
        /// resolve ambiguities when the same Classifier is at more than one end (plays more than one
        /// role) in the same association. In other cases it can be derived.
        /// </summary>
        public Property NavigationSource
        {
            get;
            set;
        }
    }
}
