using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    public abstract class OclExpression:TypedElement
    {
        public OclExpression(Classifier type)
        {
            Type = type;
        }

        public virtual T Accept<T>(IAstVisitor<T> visitor) {
            throw new InvalidOperationException();
        }

        public virtual void Accept(IAstVisitor visitor) {
            throw new InvalidOperationException();
        }

     //   public OclExpression() { }
    }
}
