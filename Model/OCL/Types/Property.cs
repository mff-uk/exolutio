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
    public class Property:ModelElement,IHasOwner<Classifier>
    {
        public Property(string name, PropertyType propertyType,Classifier type)
            : base(name)
        {
            this.type = type;
            this.propertyType = propertyType;
        }

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

        public override string QualifiedName
        {
            get 
            {
                return Owner.QualifiedName + "." + Name;
            }
        }

        
    }
}
