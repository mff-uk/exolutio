using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public class Namespace : IModelElement,IHasOwner<Namespace>
    {
        public Namespace(string name, Namespace owner):this(name) {
            NestedNamespace = new NestedElemetCollection<Namespace, Namespace>(this);
            NestedClassifier = new NestedElemetCollection<Classifier, IModelElement>(this);
            this.Name = name;
            this.Owner = owner;
            this.QualifiedName = GetQualifiedName(name, owner);
        }

        public Namespace(string name)
        {
            NestedNamespace = new NestedElemetCollection<Namespace, Namespace>(this);
            NestedClassifier = new NestedElemetCollection<Classifier, IModelElement>(this);
            this.Name = name;
            this.QualifiedName = GetQualifiedName(name, null);
        }

        public string GetQualifiedName(string name, Namespace owner) {
            if (owner != null)
                return owner.QualifiedName + "::" + name;
            else
                return name;
        }

        public NestedElemetCollection<Namespace, Namespace> NestedNamespace
        {
            get;
            private set;
        }

        public NestedElemetCollection<Classifier, IModelElement> NestedClassifier
        {
            get;
            private set;
        }

        #region IModelElement Members

        public string Name {
            get;
            private set;
        }

        public string QualifiedName {
            get;
            private set;
        }

        public virtual object Tag {
            get;
            set;
        }

        #endregion

        #region IHasOwner<Namespace> Members

        public virtual Namespace Owner
        {
            get;
            set;
        }

        #endregion


    }
}
