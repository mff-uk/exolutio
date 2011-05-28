using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.SupportingClasses;

namespace Exolutio.Model.OCL.Types
{
    public class OperationCollection:ActionList<Operation>
    {
        protected Classifier owner;

        internal OperationCollection(Classifier owner)
        {
            this.owner = owner;
        }

        protected override void OnPreDelete(Operation item)
        {
            throw new NotSupportedException();
        }

        protected override void OnPreSet(Operation item)
        {
            throw new NotSupportedException();
        }

        protected override void OnPreAdd(Operation item)
        {
            if (Data.Any(i => i.Name == item.Name))
                throw new Exception();//Todo Exception
        }

        protected override void OnAdded(Operation item)
        {
            item.Owner = owner;
        }
    }
}
