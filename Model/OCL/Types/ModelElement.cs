using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public abstract class ModelElement
    {
        public virtual string Name
        {
            get;
            set;
        }

        public abstract string QualifiedName
        {
            get;

        }

        public ModelElement(string name)
        {
            Name = name;
        }

    

        public override string ToString()
        {
            return QualifiedName;
        }

        public object Tag {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            ModelElement other = obj as ModelElement;

            if (other == null)
                return false;
            return other.QualifiedName == this.QualifiedName;
        }

        public override int GetHashCode()
        {
            return QualifiedName.GetHashCode();
        }

        public static bool operator ==(ModelElement left, ModelElement right)
        {
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
                return true;

            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(ModelElement left, ModelElement right)
        {
            return !(left==right);
        }
    }
}
