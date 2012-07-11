using System;
using System.Collections.Generic;
using System.ComponentModel;
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



        internal Lazy<Classifier> _ReturnType; 
        
        public virtual Classifier ReturnType
        {
            get { return _ReturnType.Value; }
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
            this._ReturnType = new Lazy<Classifier>(() => returnType);
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
            this._ReturnType = new Lazy<Classifier>(() => returnType);
            Parametrs = new ParameterCollection(this);
        }

        public Operation([Localizable(false)] string name, bool isQuery, Classifier returnType, IEnumerable<Parameter> parametrs)
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

        internal Classifier oclAsTypeReturnType()
        {
            return this.Owner.TypeTable.Library.Invalid;
        }

        public bool ReturnTypeDependsOnArguments { get; set; }

        internal Classifier GetReturnType(List<AST.OclExpression> args)
        {
            if (ReturnTypeDependsOnArguments)
            {
                if (Name == @"oclAsType")
                {
                    System.Diagnostics.Debug.Assert(args.First() is AST.TypeExp);
                    return (((AST.TypeExp)args.First()).ReferredType);
                }
                else
                {
                    throw new NotImplementedException(string.Format(CompilerErrors.Operation_GetReturnType_1, this));
                }
            }
            else
                return ReturnType;
        }
    }
}
