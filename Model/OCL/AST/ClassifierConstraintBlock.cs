using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST {
    public interface IConstraintsContext
    {
        VariableDeclaration Self { get; }
        Classifier Context { get; }
    }

    /// <summary>
    /// Block of constraint definitions. Composed of a classifier (constrained type/class
    /// definition of the context variable (named 'self' by default) and a block of invariants 
    /// about the classifier.
    /// </summary>
    public class ClassifierConstraintBlock : IConstraintsContext
    {

        public ClassifierConstraintBlock(Classifier context, List<InvariantWithMessage> constraints, VariableDeclaration self) {
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

        public OclExpression Constraint {
            get;
            private set;
        }

        public OclExpression Message {
            get;
            private set;
        }

        public bool MessageIsString
        {
            get { return Message is LiteralExp; }
        }

        public List<SubExpressionInfo> MessageSubExpressions
        {
            get { return messageSubExpressions; }
            set { messageSubExpressions = value; }
        }

        private List<SubExpressionInfo> messageSubExpressions = new List<SubExpressionInfo>();

        public InvariantWithMessage(OclExpression constraint, OclExpression message) {
            Constraint = constraint;
            Message = message;
        }

        public InvariantWithMessage(OclExpression constraint) {
            Constraint = constraint;
        }

        public override string ToString() {
            return Constraint != null ? Constraint.ToString() : base.ToString();
        }
    }

    public struct SubExpressionInfo
    {
        public OclExpression SubExpression { get; set; }
        public int MessageStartIndex { get; set; }
        public int MessageEndIndex { get; set; }

        public bool Parsed { get; set; }

        public string PartAsString { get; set; }
    }
}
