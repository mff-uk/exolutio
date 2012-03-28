using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST {
    public class ClassifierConstraint {

        public ClassifierConstraint(Classifier context, List<InvariantWithMessage> constraints, VariableDeclaration self) {
            this.Context = context;
            this.Invariants = constraints;
            this.Self = self;
        }

        private VariableDeclaration self;

        public VariableDeclaration Self {
            get { return self; }
            private set {
                self = value;
                if (self != null) {
                    self.IsContextVariable = true;
                }
            }
        }

        public List<InvariantWithMessage> Invariants {
            get;
            protected set;
        }

        public Classifier Context {
            get;
            protected set;
        }
    }

    public class InvariantWithMessage {

        public OclExpression Constarint {
            get;
            private set;
        }

        public StringLiteralExp Message {
            get;
            private set;
        }

        public InvariantWithMessage(OclExpression constraint, StringLiteralExp message) {
            Constarint = constraint;
            Message = message;
        }

        public InvariantWithMessage(OclExpression constraint) {
            Constarint = constraint;
        }

        public override string ToString() {
            return Constarint != null ? Constarint.ToString() : base.ToString();
        }
    }
}
