using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.SupportingClasses;

namespace Exolutio.Model.OCL.Types
{
    public sealed class ParameterCollection : ActionList<Parameter>
    {
        private Operation operation;

        internal ParameterCollection(Operation owner)
        {
            operation = owner;
        }

        protected override void OnPreDelete(Parameter item)
        {
            throw new NotSupportedException();
        }

        protected override void OnPreSet(Parameter item)
        {
            throw new NotSupportedException();
        }

        protected override void OnAdded(Parameter item)
        {
            item.Owner = operation;
        }

        public override bool Equals(object obj)
        {
            //Dve kolekce parametru jsou schodne pokud se schoduji v typech
            ParameterCollection other = obj as ParameterCollection;
            if (other == null || other.Count != this.Count)
                return false;
            //schoduji se typy jednotlivych parametru
            return Data.Zip(other.Data, (a, b) => a.Type == b.Type).All(a => a);
        }

        public bool HasMatchingSignature(IEnumerable<Classifier> parTypes){
            if (parTypes.Count() != Count) {
                return false;
            }
            return Data.Zip(parTypes, (a, b) => b.ConformsTo(a.Type)).All(a => a);
        }

        public override int GetHashCode()
        {
            return Data.Sum(a => a.Type.GetHashCode());
        }

        public override string ToString()
        {
            StringBuilder nameBuilder = new StringBuilder("(");
            bool isFirst = true;
            foreach (var parameter in Data)
            {
                if (isFirst == false)
                    nameBuilder.Append(",");
                else
                    isFirst = false;
                nameBuilder.AppendFormat("{0}:{1}", parameter.Name, parameter.Type.QualifiedName);


            }
            nameBuilder.Append(")");

            return nameBuilder.ToString();
        }

        public static bool operator ==(ParameterCollection left, ParameterCollection right)
        {
            if (object.ReferenceEquals(left, null) && object.ReferenceEquals(right, null))
                return true;
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(ParameterCollection left, ParameterCollection right)
        {
            return !(left == right);
        }


    }
}
