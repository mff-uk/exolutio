using System;
using System.Collections.Generic;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model
{
    public class ModelVerifier
    {
        public Log Log { get; set; }

        public PSMComponent LastViolatingComponent { get; private set; }

        public ModelVerifier()
        {
            Log = new Log();
        }

        private Dictionary<PSMSchema, LogMessage> messageDict = new Dictionary<PSMSchema, LogMessage>();

        /// <summary>
        /// Returns true if a PSM schema is normalized
        /// </summary>
        public bool TestSchemaNormalized(PSMSchema psmSchema)
        {
            LogMessage m = new LogMessage()
                               {
                                   Severity = ELogMessageSeverity.Error,
                                   MessageText = string.Format("PSM schema '{0}' is not normalized", psmSchema)
                               };
            messageDict[psmSchema] = m;

            bool c11 = TestSchemaClassChildrenCardinality(psmSchema); //(1)
            bool c12 = TestSchemaClassChildrenNames(psmSchema); //(1)
            bool c2 = TestSchemaClassChildrenAreClasses(psmSchema); //(2)
            bool c3 = TestContentModelsAssociationNames(psmSchema); //(3)
            bool c4 = TestRootsAreNotContentModels(psmSchema); //(4)
            bool c5 = TestRootsAreReferenced(psmSchema); //(5)

            bool result = c11 && c12 && c2 && c3 && c4 && c5;
            if (!result)
            {
                Log.AddLogMessage(m);
            }
            return result;
        }

        public bool TestRootsAreReferenced(PSMSchema psmSchema)
        {
            bool result = true;

            foreach (PSMAssociationMember psmAssociationMember in psmSchema.Roots)
            {
                if (psmAssociationMember is PSMClass 
                    && psmAssociationMember != psmSchema.PSMSchemaClass 
                    && !((PSMClass)psmAssociationMember).HasStructuralRepresentatives)
                {
                    ILogMessage message = Log.AddWarningFormat("Root classes other than schema class must be referenced by structural representatives. Violated by {0}.", psmAssociationMember);
                    if (messageDict.ContainsKey(psmSchema))
                    {
                        message.RelatedMessage = messageDict[psmSchema];
                    }
                    LastViolatingComponent = psmAssociationMember;
                    result = false;
                }
            }

            return result;
        }

        public bool TestRootsAreNotContentModels(PSMSchema psmSchema)
        {
            bool result = true;

            foreach (PSMAssociationMember psmAssociationMember in psmSchema.Roots)
            {
                if (psmAssociationMember is PSMContentModel)
                {
                    ILogMessage message = Log.AddWarningFormat("Content models are not allowed as roots. Violated by {0}.", psmAssociationMember);
                    if (messageDict.ContainsKey(psmSchema))
                    {
                        message.RelatedMessage = messageDict[psmSchema];
                    }
                    LastViolatingComponent = psmAssociationMember;
                    result = false;
                }
            }

            return result;
        }

        public bool TestContentModelsAssociationNames(PSMSchema psmSchema)
        {
            bool result = true;

            foreach (PSMAssociation psmAssociation in psmSchema.PSMAssociations)
            {
                if (psmAssociation.Child is PSMContentModel && psmAssociation.IsNamed)
                {
                    ILogMessage message = Log.AddWarningFormat("Parent associations of content models must not have a name. Violated by {0}.", psmAssociation);
                    if (messageDict.ContainsKey(psmSchema))
                    {
                        message.RelatedMessage = messageDict[psmSchema];
                    }
                    LastViolatingComponent = psmAssociation;
                    result = false;
                }
            }

            return result;
        }

        public bool TestSchemaClassChildrenAreClasses(PSMSchema psmSchema)
        {
            bool result = true;

            foreach (PSMAssociationMember psmAssociationMember in ModelIterator.GetChildNodes(psmSchema.PSMSchemaClass))
            {
                if (!(psmAssociationMember is PSMClass))
                {
                    ILogMessage message = Log.AddWarningFormat("Child nodes of a schema class must be classes. Violated by {0}.", psmAssociationMember);
                    if (messageDict.ContainsKey(psmSchema))
                    {
                        message.RelatedMessage = messageDict[psmSchema];
                    }
                    LastViolatingComponent = psmAssociationMember;
                    result = false;
                }
            }

            return result;
        }

        public bool TestSchemaClassChildrenNames(PSMSchema psmSchema)
        {
            bool result = true;

            foreach (PSMClass psmClass in psmSchema.TopClasses)
            {
                if (!(psmClass.ParentAssociation.IsNamed))
                {
                    ILogMessage message = Log.AddWarningFormat("Associations in the content of a schema class must have a name. Violated by {0}.", psmClass.ParentAssociation);
                    if (messageDict.ContainsKey(psmSchema))
                    {
                        message.RelatedMessage = messageDict[psmSchema];
                    }
                    LastViolatingComponent = psmClass.ParentAssociation;
                    result = false;
                }
            }

            return result;
        }

        public bool TestSchemaClassChildrenCardinality(PSMSchema psmSchema)
        {
            bool result = true;

            foreach (PSMClass psmClass in psmSchema.TopClasses)
            {
                if (!(psmClass.ParentAssociation.Lower == 1 && psmClass.ParentAssociation.Upper == 1))
                {
                    ILogMessage message = Log.AddWarningFormat("Associations in the content of a schema class must have cardinality 1..1. Violated by {0}.", psmClass.ParentAssociation);
                    if (messageDict.ContainsKey(psmSchema))
                    {
                        message.RelatedMessage = messageDict[psmSchema];
                    }
                    LastViolatingComponent = psmClass.ParentAssociation;
                    result = false;
                }
            }

            return result;
        }
    }
}