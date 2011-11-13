using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;

namespace Exolutio.Model.OCL.Bridge {
    public class PIMBridgeAssociation : BridgeProperty {
        public PIMAssociation SourceAssociation {
            get;
            private set;
        }

        public PIMAssociationEnd SourceAssociationEnd {
            get;
            private set;
        }

        public PIMBridgeAssociation(string name, PIMAssociation sourceAss, PIMAssociationEnd sourceAssEnd, PropertyType propertyType, Classifier type)
            : base(name, propertyType, type, BridgePropertyType.Association) {
            this.SourceAssociation = sourceAss;
            this.SourceAssociationEnd = sourceAssEnd;
        }
    }
}
