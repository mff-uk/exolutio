using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// TODO Parameter class
    /// </summary>
    public class Parameter:IHasOwner<Operation>
    {
        public Parameter(string Name, Classifier Type)
        {
            name = Name;
            type = Type;
        }


        protected readonly string name;
        public virtual string Name
        {
            get
            {
                return name;
            }
            
        }

        protected readonly Classifier type;
        public virtual Classifier Type
        {
            get
            {
                return type;
            }
        }


        protected Operation owner;

        /// <summary>
        /// Parameter Owner
        /// </summary>
        public virtual Operation Owner
        {
            get
            {
                return owner;
            }
            set
            {
                if (owner != null)
                    throw new NotSupportedException();

                owner = value;
            }
        }

    }
}
