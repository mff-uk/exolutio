using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvoX.Model.OCL.Types
{
    /// <summary>
    /// TODO Operation class
    /// 
    /// </summary>
    public class Operation:ModelElement,IHasOwner<Classifier>
    {
   

        public Operation(string name, Classifier returnType):base(name)
        {
            IsQuery = false;
            this.ReturnType = returnType;
            Parametrs = new ParameterCollection(this);
        }

        public Operation(string name, Classifier returnType, IEnumerable<Parameter> Paremetrs)
            : this(name,returnType)
        {
            this.Parametrs.AddRange(Parametrs);
        }

        public Operation(string name, bool isQuery, Classifier returnType)
            : this(name,returnType)
        {
            IsQuery = isQuery;
            this.ReturnType = returnType;
            Parametrs = new ParameterCollection(this);
        }

        public Operation(string name, bool isQuery, Classifier returnType, IEnumerable<Parameter> Paremetrs)
            : this(name, isQuery, returnType)
        {
            this.Parametrs.AddRange(Parametrs);
        }

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

        public override string Name
        {
            get
            {
                return base.Name;
            }
            protected set
            {
                base.Name = value;
            }
        }

        public override string QualifiedName
        {
            get 
            {
                return owner.QualifiedName + "." + Name;
            }
        }

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
