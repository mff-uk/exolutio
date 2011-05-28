using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// VariableDeclaration encapsulates tuple property.
    /// VariableDeclaration isn't part of OCL or UML superstructure specification. It's solving problem with TupleType.
    /// </summary>
    public class VariableDeclaration
    {
        /// <summary>
        /// Tuple property name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Tuple promerty type.
        /// </summary>
        public Classifier PropertyType
        {
            get;
            set;
        }
    }
}
