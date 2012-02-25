using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// 
    /// It is immutable class(but owner will by able to changed)
    /// </summary>
    public class Property:IModelElement,IHasOwner<Classifier>
    {
        protected readonly PropertyType propertyType;
        virtual public PropertyType PropertyType
        {
            get{return propertyType;}
         
        }

        protected readonly Classifier type;
        virtual public Classifier Type
        {
            get{return type;}
          
        }

        protected Classifier owner = null;
        public virtual Classifier Owner
        {
            get
            {
                return owner;
            }
            set
            {
                if (owner != null && value != owner) {
                    throw new InvalidOperationException();
                }
                owner = value;
            }
        }

        #region IModelElement Members

        public string Name {
            get;
            protected set;
        }

        Lazy<string> _QualifiedName;
        public string QualifiedName {
            get { return _QualifiedName.Value; }
        }

        public object Tag {
            get;
            set;
        }
        #endregion

        public Property(string name, PropertyType propertyType, Classifier type) {
            this.type = type;
            this.propertyType = propertyType;
            this.Name = name;
            _QualifiedName = new Lazy<string>(() => GetQualifiedName(), true);
        }

        private string GetQualifiedName(){
            return String.Format("{0}.{1}",Owner.QualifiedName,Name);
        }
    }
}
