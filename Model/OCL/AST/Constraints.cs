using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST {
    public class Constraints {
        public List<ClassifierConstraint> Classifiers {
            get;
            private set;
        }

        public Constraints() {
            Classifiers = new List<ClassifierConstraint>();
        }
    }
}
