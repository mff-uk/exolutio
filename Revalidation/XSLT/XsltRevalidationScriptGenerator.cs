using System;
using System.Collections.Generic;
using Exolutio.Model.PSM;
using Exolutio.Revalidation.Changes;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.XSLT
{
    public class XsltRevalidationScriptGenerator
    {
        public XsltRevalidationScriptGenerator(PSMSchema psmSchema)
        {
            

        }

        public string Generate(DetectedChangesSet changes, Version version, Version version1)
        {
            return string.Empty;
        }
    }
}