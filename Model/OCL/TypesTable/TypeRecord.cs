using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.TypesTable
{
    class TypeRecord
    {
        public int MatrixIndex
        {
            get;
            internal set;
        }

        public Classifier Type
        {
            get;
            internal set;
        }

        private List<int> edgesIndex = new List<int>();

        public List<int> EdgesIndex
        {
            get{return edgesIndex;}
            
        }
    }
}
