using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// TODO Parameter class
    /// </summary>
    public class Parameter:IHasOwner<Operation>
    {
        public Parameter([Localizable(false)] string name, Classifier type)
        {
            _Name = name;
            _Type = type;
        }


        protected readonly string _Name;
        public virtual string Name
        {
            get
            {
                return _Name;
            }
            
        }

        protected Classifier _Type;
        public virtual Classifier Type
        {
            get
            {
                return _Type;
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
