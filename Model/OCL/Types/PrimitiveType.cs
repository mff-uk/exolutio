using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.Model.OCL.Types
{
    /// <summary>
    /// Matches UML.Classes.Kernel.PrimitiveType from UML SuperStructure
    /// </summary>
    public class PrimitiveType:DataType
    {
        protected static PrimitiveType integer;

        public static PrimitiveType Integer
        {
            get
            {
                return integer;
            }
        }

        public PrimitiveType(string name)
            : base(name)
        { }

        
    }
}
