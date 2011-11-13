using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A PropertyCallExpression is a reference to an Attribute of a Classifier defined in a UML model. It evaluates to the value
    /// of the attribute.
    /// </summary>
    public class PropertyCallExp : NavigationCallExp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Source expression of iterotor. Itertor is calling on this expression.</param>
        /// <param name="isPre">Is marked by pre</param>
        /// <param name="navigationSource">Source of navigation. Example: expr.navigationSource::referedProperty</param>
        /// <param name="qualifier">Qulifier of property. Example: expr.referedProperty[qualifier]</param>
        /// <param name="referredProperty">Destination property</param>
        public PropertyCallExp(OclExpression source, bool isPre, Property navigationSource, OclExpression qualifier, Property referredProperty)
            : base(source, isPre, navigationSource, qualifier, referredProperty.Type) {
                this.ReferredProperty = referredProperty;
        }

        /// <summary>
        /// The Attribute to which this AttributeCallExp is a reference.
        /// </summary>
        public Property ReferredProperty
        {
            get;
            set;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }
    }
}
