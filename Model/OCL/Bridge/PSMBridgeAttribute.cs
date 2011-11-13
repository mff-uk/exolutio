using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;

namespace Exolutio.Model.OCL.Bridge {
    class PSMBridgeAttribute : BridgeProperty 
    {
        public PSMAttribute SourceAttribute {
            get;
            private set;
        }

        public PSMBridgeAttribute(PSMAttribute source, PropertyType propertyType, Classifier type)
            : base(source.Name, propertyType, type, BridgePropertyType.Attribute) {
                SourceAttribute = source;
        }
    }
}
