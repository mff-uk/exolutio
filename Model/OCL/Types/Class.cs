using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// A class describes a set of objects that share the same specifications of features, constraints, and semantics.
    /// Matches UML.Classes.Kernel.Class from UML SuperStructure
    /// </summary>
    public class Class:Classifier
    {
       
        /// <summary>
        /// This gives the superclasses of a class.
        /// </summary>
		public virtual List<Classifier> SuperClass
		{
            get;
            private set;
        }


        public Class(TypesTable.TypesTable tt, Namespace ns, string name)
            : base(tt,ns,name,tt.Library.Any)
        {
			SuperClass = new List<Classifier>();
        }

        public override bool ConformsToRegister(Classifier other)
        {
            if (other.GetType().IsSubclassOf(typeof(Class))||other.GetType()==this.GetType())
                return ConformsToRegisterClass((Class)other);
            else
                return base.ConformsToRegister(other);
        }

        public virtual bool ConformsToRegisterClass(Class other)
        {
            return this.QualifiedName == other.QualifiedName ||
                SuperClass.Exists(c => c.QualifiedName == other.QualifiedName);
        }

        //Ignore operation inherite

    }
}
