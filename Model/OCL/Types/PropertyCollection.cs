﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.SupportingClasses;

namespace EvoX.Model.OCL.Types
{
    public class PropertyCollection :ActionDictionary<string,Property>
    {  
        protected Classifier owner;

        internal PropertyCollection(Classifier owner)
        {
            this.owner = owner;
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

        public override string ToString()
        {
            StringBuilder nameBuilder = new StringBuilder("(");
            bool isFirst = true;

            foreach (var property in Data.Values)
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

    //public class PropertyCollection:ActionList<Property>
    //{
    //    protected Classifier owner;

    //    internal PropertyCollection(Classifier owner)
    //    {
    //        this.owner = owner;
    //    }

    //    protected override void OnPreDelete(Property item)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    protected override void OnPreSet(Property item)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    protected override void OnPreAdd(Property item)
    //    {
    //        //typ neodlisuje dve property
    //        if (Data.Any(i => i.Name == item.Name))
    //            throw new Exception();
    //    }

    //    protected override void OnAdded(Property item)
    //    {
    //        item.Owner = owner;
    //    }


    //    public override string ToString()
    //    {
    //        StringBuilder nameBuilder = new StringBuilder("(");


    //        bool isFirst = true;
    //        foreach (var property in Data)
    //        {
    //            nameBuilder.AppendFormat("{0}:{1}", property.QualifiedName, property.Type.QualifiedName);

    //            if (isFirst == false)
    //                nameBuilder.Append(",");
    //            else
    //                isFirst = false;
    //        }

            

    //        nameBuilder.Append(")");

    //        return nameBuilder.ToString();
    //    }

    //}
}
