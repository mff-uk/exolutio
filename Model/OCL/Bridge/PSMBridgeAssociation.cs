using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;

namespace Exolutio.Model.OCL.Bridge {
    public class PSMBridgeAssociation : BridgeProperty {
        public string DefaultName {
            get;
            private set;
        }

        public List<string> Aliases {
            get;
            private set;
        }

        public PSMAssociation SourceAsscociation {
            get;
            private set;
        }

        public AssociationDirection Direction{
            get;
            private set;
        }


        public PSMBridgeAssociation(string defaultName, List<string> aliases, PSMAssociation sourceAss, AssociationDirection direction, PropertyType propertyType, Classifier type) :
            base(defaultName, propertyType, type, BridgePropertyType.Association) {
            this.DefaultName = defaultName;
            this.Aliases = aliases;
            this.SourceAsscociation = sourceAss;
            this.Direction = direction;
        }

        public enum AssociationDirection{
            Down, Up
        }

        /// <summary>
        /// Returns a constant string `parent` which can be used for navigation in PSM OCL
        /// </summary>
        public const string PARENT_STEP = @"parent";
        public const string CHILD_N_STEP = @"child_{0}";
    }
}
