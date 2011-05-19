using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.Types;

namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// A PropertyCallExpression is a reference to an Attribute of a Classifier defined in a UML model. It evaluates to the value
    /// of the attribute.
    /// </summary>
    public class PropertyCallExp : NavigationCallExp
    {
        /// <summary>
        /// The Attribute to which this AttributeCallExp is a reference.
        /// </summary>
        public Property ReferredProperty
        {
            get;
            set;
        }

        public override Classifier Type
        {
            get
            {
                return ReferredProperty.Type;
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
