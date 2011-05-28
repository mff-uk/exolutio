using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    public class TupleLiteralPart : TypedElement
    {
        //TODO TupleLiteralPart: Tohle nedava smysl podle spec
        public Property Attribute
        {
            get;
            set;
        }
    }
}
