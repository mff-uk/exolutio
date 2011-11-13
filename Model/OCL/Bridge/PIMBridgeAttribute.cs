using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;

namespace Exolutio.Model.OCL.Bridge {
    public class PIMBridgeAttribute:BridgeProperty {
        public PIMAttribute SourceAttribute {
            get;
            private set;
        }

        public PIMBridgeAttribute(PIMAttribute source, PropertyType propertyType, Classifier type)
            : base(source.Name, propertyType, type, BridgePropertyType.Attribute) {
                SourceAttribute = source;
        }
    }
}
