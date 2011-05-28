using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public interface IHasOwner<T> where T:ModelElement
    {
        T Owner
        {
            get;
             set;
        }
    }
}
