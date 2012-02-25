using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// TODO Operation class
    /// 
    /// </summary>
    public class Operation:IModelElement,IHasOwner<Classifier>
    {
        public virtual bool IsQuery
        {
            get;
            protected set;
        }

        public virtual Classifier ReturnType
        {
            get;
            protected set;
        }

        public virtual ParameterCollection Parametrs
        {
            get;
            protected set;
        }



        #region IModelElement Members

        public string Name {
            get;
            private set;
        }

        Lazy<string> _QualifiedName;
        public string QualifiedName {
            get { throw new NotImplementedException(); }
        }

        public object Tag {
            get;
            set;
        }

        #endregion

        public Classifier owner = null;
        public virtual Classifier Owner
        {
            get
            {
                return owner;
            }
            set
            {
                if (owner != null)
                    throw new NotSupportedException();
                value = owner;
            }
        }

        public Operation(string name, Classifier returnType) {
            this.Name = name;
            IsQuery = false;
            this.ReturnType = returnType;
            Parametrs = new ParameterCollection(this);
            _QualifiedName = new Lazy<string>(() => Owner.QualifiedName + "." + Name);
        }

        public Operation(string name, Classifier returnType, IEnumerable<Parameter> Paremetrs)
            : this(name, returnType) {
            this.Parametrs.AddRange(Parametrs);
        }

        public Operation(string name, bool isQuery, Classifier returnType)
            : this(name, returnType) {
            IsQuery = isQuery;
            this.ReturnType = returnType;
            Parametrs = new ParameterCollection(this);
        }

        public Operation(string name, bool isQuery, Classifier returnType, IEnumerable<Parameter> parametrs)
            : this(name, isQuery, returnType) {
            this.Parametrs.AddRange(parametrs);
        }

        public override bool Equals(object obj)
        {
            Operation other = obj as Operation;

            if (other == null)
                return false;

            return this.QualifiedName == other.QualifiedName && this.Parametrs == other.Parametrs;
        }

        public override int GetHashCode()
        {
            return this.QualifiedName.GetHashCode()+Parametrs.GetHashCode();
        }

        public override string ToString()
        {
            return QualifiedName + Parametrs.ToString();
        }

 
    }
}
