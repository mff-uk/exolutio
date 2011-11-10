using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST {
    public class ClassifierConstraint {

        public ClassifierConstraint(Classifier context, List<OclExpression> constraints, VariableDeclaration self) {
            this.Context = context;
            this.Invariants = constraints;
            this.Self = self;
        }

        public VariableDeclaration Self {
            get;
            private set;
        }

        public List<OclExpression> Invariants {
            get;
            protected set;
        }

        public Classifier Context {
            get;
            protected set;
        }
    }
}
