using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    public class TupleLiteralPart : TypedElement
    {
        public TupleLiteralPart(Property attributeType, OclExpression value) {
            this.Attribute = attributeType;
            this.Value = value;
        }


        //TODO TupleLiteralPart: Tohle nedava smysl podle spec
        public Property Attribute
        {
            get;
            protected set;
        }

        public OclExpression Value {
            get;
            protected set;
        }
    }
}
