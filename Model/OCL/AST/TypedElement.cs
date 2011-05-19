using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model.OCL.Types;

namespace EvoX.Model.OCL.AST
{
    /// <summary>
    /// Matches UML.Classes.Kernel.TypedElement from UML SuperStructure
    /// </summary>
    public class TypedElement
    {
        public virtual Classifier Type { get;protected set; }
    }
}
