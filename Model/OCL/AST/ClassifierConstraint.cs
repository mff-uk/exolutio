using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST {
    public class ClassifierConstraint {

        public ClassifierConstraint(Classifier context, List<OclExpression> constraints) {
            this.Context = context;
            this.Constraints = constraints;
        }

        public List<OclExpression> Constraints {
            get;
            protected set;
        }

        public Classifier Context {
            get;
            protected set;
        }
    }
}
