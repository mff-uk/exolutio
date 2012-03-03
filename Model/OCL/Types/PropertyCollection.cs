using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.SupportingClasses;

namespace Exolutio.Model.OCL.Types
{
    public class PropertyCollection :ActionDictionary<string,Property>
    {  
        protected Classifier owner;

        internal PropertyCollection(Classifier owner)
        {
            this.owner = owner;
        }

        protected override bool IsToAdd(string key, Property value) {
            // is exist in collection
            if (Data.ContainsKey(key) == false) {
                return true;
            }
            if (Data[key].IsAmbiguous) {
                return false; 
            }
            Data.Remove(key); // remove unambiguous property (one instance of Property class can be use more than one property - a property can have more than one name) 

            Property ambProp = new Property(key, value.PropertyType, value.Type);
            ambProp.MarkAsAmbigious();
          
            Add(ambProp); // add as ambigous
            return false;
        }

        protected override void OnAdded(string key, Property value)
        {
            value.Owner = owner;
        }

        protected override void OnPreDelete(string key)
        {
            throw new InvalidOperationException();

        }

        protected override void OnPreSet(string key, Property value)
        {
            if(Data.ContainsKey(key))
                throw new InvalidOperationException();
        }

        public void Add(Property property)
        {
            base.Add(property.Name, property);
        }

        public void Add(Property property, string alias) {
            base.Add(alias , property);
        }

       

        public override string ToString()
        {
            return PropertyCollectionHelpers.Print(Data.Values);
        }
    }

    internal static class PropertyCollectionHelpers{
         public static string Print(IEnumerable<Property> properties){
            StringBuilder nameBuilder = new StringBuilder("(");
            bool isFirst = true;

            foreach (var property in properties)
            {
                if (isFirst == false)
                    nameBuilder.Append(",");
                else
                    isFirst = false;
                nameBuilder.AppendFormat("{0}:{1}", property.Name, property.Type.QualifiedName);
            }

            nameBuilder.Append(")");
            return nameBuilder.ToString();
        }
    }
}
