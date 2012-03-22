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
    public class VariableDeclaration: IModelElement
    {
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

        #region IModelElement Members
        public string QualifiedName {
            get { return Name; }
        }


        public string Name {
            get;
            // Todo zmenit na private set - zmenono jenom kvuli ConstraintConversion/PSMPath
            set;
        }

        public object Tag {
            get;
            set;
        }
        #endregion

        public VariableDeclaration(string name, Classifier propertyType, OclExpression value) {
            this.Name = name;
            this.PropertyType = propertyType;
            this.Value = value;
        }

        public bool IsContextVariable { get; set; }

        public static string SELF = @"self";
    }
}
