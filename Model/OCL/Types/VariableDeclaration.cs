using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.AST;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// VariableDeclaration encapsulates tuple property.
    /// VariableDeclaration isn't part of OCL or UML superstructure specification. It's solving problem with TupleType.
    /// </summary>
    public class VariableDeclaration: ModelElement
    {
        public VariableDeclaration(string name, Classifier propertyType,OclExpression value):base(name) {
            this.Name = name;
            this.PropertyType = propertyType;
            this.Value = value;
        }


        ///// <summary>
        ///// Tuple property name
        ///// </summary>
        //public string Name
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Tuple promerty type.
        /// </summary>
        public Classifier PropertyType
        {
            get;
            set;
        }

        public OclExpression Value {
            get;
            set;
        }

        public override string QualifiedName {
            get { return Name; }
        }
    }
}
