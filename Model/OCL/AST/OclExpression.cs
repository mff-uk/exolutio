using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.Types;

namespace EvoX.Model.OCL.AST
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
