using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public class Namespace : ModelElement,IHasOwner<Namespace>
    {
        public override string QualifiedName
        {
            get
            {
                if (Owner != null)
                    return Owner.QualifiedName + "::" + Name;
                else
                    return Name;
            }
        }
        

        public Namespace(string name)
            : base(name)
        {
            NestedNamespace = new NestedElemetCollection<Namespace, Namespace>(this);
            NestedClassifier = new NestedElemetCollection<Classifier, ModelElement>(this);
        }

        public NestedElemetCollection<Namespace, Namespace> NestedNamespace
        {
            get;
            private set;
        }

        public NestedElemetCollection<Classifier, ModelElement> NestedClassifier
        {
            get;
            private set;
        }

        #region IHasOwner<Namespace> Members

        public virtual Namespace Owner
        {
            get;
             set;
        }

        #endregion
    }
}
