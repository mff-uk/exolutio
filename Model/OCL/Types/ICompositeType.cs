using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    public interface ICompositeType:IConformsToComposite
    {
        bool ConformsToSimple(Classifier other);
        void RegistredComposite(TypesTable.TypesTable table);
        NonCompositeType SimpleRepresentation
        {
            get;
        }
    }
}
