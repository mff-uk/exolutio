using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public interface IModelElement
    {
       string Name
        {
            get;
        }

        string QualifiedName {
            get;
        }

        object Tag {
            get;
            set;
        }
    }
}
