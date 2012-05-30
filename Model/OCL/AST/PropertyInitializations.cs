using System.Collections.Generic;

namespace Exolutio.Model.OCL.AST
{
    public class PropertyInitializations
    {
        public List<PropertyInitializationBlock> PropertyInitializationBlocks { get; set; }

        public PropertyInitializations()
        {
            PropertyInitializationBlocks = new List<PropertyInitializationBlock>();
        }
    }
}