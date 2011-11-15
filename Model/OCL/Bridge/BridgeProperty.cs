using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;

namespace Exolutio.Model.OCL.Bridge {
    public class BridgeProperty : Property {
        /// <summary>
        /// Logic source type of property. (Association, Attribute)
        /// </summary>
        public BridgePropertyType SourceType {
            get;
            protected set;
        }

        public BridgeProperty(string name, PropertyType propertyType, Classifier type, BridgePropertyType propType)
            : base(name, propertyType, type) {
            SourceType = propType;
        }
    }
}
