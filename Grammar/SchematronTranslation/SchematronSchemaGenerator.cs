﻿using System;
using System.Xml.Linq;
using Exolutio.Model.OCL;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class SchematronSchemaGenerator
    {
        private PSMSchema psmSchema;

        public PSMSchema PSMSchema
        {
            get { return psmSchema; }
        }

        public Log Log { get; private set; }

        public void Initialize(PSMSchema psmSchema)
        {
            this.psmSchema = psmSchema;
            Log = new Log();
        }

        public XDocument GetSchematronSchema()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement schSchema = doc.SchematronSchema();
            XComment comment = new XComment(string.Format(" generated by eXolutio on {0} {1} from {2}/{3}. ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), PSMSchema.Project.Name, PSMSchema.Caption));
            schSchema.Add(comment);

            foreach (OCLScript oclScript in PSMSchema.OCLScripts)
            {
                TranslateScript(schSchema, oclScript);
            }

            return doc;
        }

        private void TranslateScript(XElement schSchema, OCLScript oclScript)
        {
            schSchema.SchematronPattern(oclScript.Name);
        }
    }
}