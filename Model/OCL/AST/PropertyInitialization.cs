using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{

    /// <summary>
    /// Block of constraint definitions. Composed of a classifier (constrained type/class
    /// definition of the context variable (named 'self' by default) and a block of invariants 
    /// about the classifier.
    /// </summary>
    public class PropertyInitializationBlock
    {

        public PropertyInitializationBlock(Classifier context, List<PropertyInitialization> propertyInitialization)
        {
            this.Context = context;
            this.PropertyInitializations = propertyInitialization;
        }

        public List<PropertyInitialization> PropertyInitializations
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

    public class PropertyInitialization
    {

        public OclExpression InitializationExpression
        {
            get;
            private set;
        }

        public Property Property
        {
            get;
            set;
        }

        public PropertyInitialization(OclExpression initializationExpression)
        {
            InitializationExpression = initializationExpression;
        }

        public override string ToString()
        {
            return string.Format(@"{0} := {1}", Property.Name, InitializationExpression);
        }
    }
}
