using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST {
    class AssociationEndCall:NavigationCallExp {
        public AssociationEndCall(OclExpression source, bool isPre, Property navigationSource, OclExpression qualifier, Classifier returnType)
            : base(source, isPre, navigationSource, qualifier, returnType) {
           
        }
    }
}
