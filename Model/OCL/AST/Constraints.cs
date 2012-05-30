using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.AST
{
    public class Constraints
    {
        public List<ClassifierConstraintBlock> ClassifierConstraintBlocks { get; private set; }

        public Constraints()
        {
            ClassifierConstraintBlocks = new List<ClassifierConstraintBlock>();
        }
    }
}