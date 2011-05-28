using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.SupportingClasses;

namespace Exolutio.Model.OCL.Types
{
    public class NestedElemetCollection<V, OwnerType> : ActionDictionary<string, V>
        where V : ModelElement,IHasOwner<OwnerType>
        where OwnerType : ModelElement
    {
        protected OwnerType Owner
        {
            get;
            set;
        }

        public NestedElemetCollection(OwnerType owner)
        {
            Owner = owner;
        }

        protected override void OnAdded(string key, V value)
        {
            value.Owner = Owner;
        }

        protected override void OnPreDelete(string key)
        {
            throw new InvalidOperationException();

        }

        protected override void OnPreSet(string key, V value)
        {
            if(Data.ContainsKey(key))
                throw new InvalidOperationException();
        }

        public void Add(V value)
        {
            Add(value.Name, value);
        }

    }
}
