using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    public class ClassifierConstraint
    {

        public ClassifierConstraint(Classifier context, List<OclExpression> constraints, VariableDeclaration self)
        {
            this.Context = context;
            this.Invariants = constraints;
            this.Self = self;
        }

        private VariableDeclaration self;

        public VariableDeclaration Self
        {
            get { return self; }
            private set
            {
                self = value;
                if (self != null)
                {
                    self.IsContextVariable = true;
                }
            }
        }

        public List<OclExpression> Invariants
        {
            get;
            protected set;
        }

        public Classifier Context
        {
            get;
            protected set;
        }
    }
}
