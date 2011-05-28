using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    public class OclExpression:TypedElement
    {
        public OclExpression(Classifier type)
        {
            Type = type;
        }

        public OclExpression() { }
    }
}
