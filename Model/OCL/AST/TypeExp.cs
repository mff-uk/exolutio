using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A TypeExp is an expression used to refer to an existing meta type within an expression. It is used in particular to pass the
    /// reference of the meta type when invoking the operations oclIsKindOf, oclIsTypeOf, and oclAsType.
    /// </summary>
    public class TypeExp : OclExpression
    {
        public Classifier ReferredType
        {
            get;
            set;
        }
    }
}
