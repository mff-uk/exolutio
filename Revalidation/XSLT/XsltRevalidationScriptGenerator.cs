using System;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.Revalidation.Changes;
using Exolutio.SupportingClasses;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.XSLT
{
    public class XsltRevalidationScriptGenerator
    {
        public XsltRevalidationScriptGenerator()
        {


        }

        public PSMSchema PSMSchema1 { get; private set; }

        public Version Version1 { get { return PSMSchema1.Version; } }

        public PSMSchema PSMSchema2 { get; private set; }

        public Version Version2 { get { return PSMSchema2.Version; } }

        public DetectedChangeInstancesSet DetectedChangeInstances { get; private set; }


        public string Generate(PSMSchema psmSchema1, PSMSchema psmSchema2, DetectedChangeInstancesSet changeInstances)
        {
            this.PSMSchema2 = psmSchema2;
            this.PSMSchema1 = psmSchema1;
            DetectedChangeInstances = changeInstances;
            return string.Empty;

        }

        

        private void CleanUp()
        {

        }
    }
}