﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.RNGTranslation
{
    public class RngSchemaGenerator
    {
        public PSMSchema PSMSchema { get; private set; }

        public Log Log { get; private set; }

        public NamingSupport NamingSupport { get; private set; }

        public void Initialize(PSMSchema psmSchema)
        {
            PSMSchema = psmSchema;
            nodeInfos.Clear();
            Log = new Log();
            NamingSupport = new NamingSupport { Log = Log };
            RelaxNGXmlSyntaxWriter = new RelaxNGXmlSyntaxWriter();
        }

        public RelaxNGXmlSyntaxWriter RelaxNGXmlSyntaxWriter { get; private set; }

        public void GenerateSchemaStructure()
        {
            foreach (PSMAssociationMember node in PSMSchema.PSMNodes)
            {
                if (node is PSMSchemaClass)
                    continue;

                RngNodeTranslationInfo nodeInfo = new RngNodeTranslationInfo();
                nodeInfo.Node = node;
                nodeInfos[node] = nodeInfo;

                nodeInfo.ContentPatternName = NamingSupport.SuggestName(node, topLevelPattern: true);
            }
        }

        readonly Dictionary<PSMComponent, RngNodeTranslationInfo> nodeInfos = new Dictionary<PSMComponent, RngNodeTranslationInfo>();

        private readonly List<AttributeType> usedNonDefaultAttribuXElements = new List<AttributeType>();

        public XDocument GetRelaxNgSchema()
        {
            XElement rngSchema;
            RelaxNGXmlSyntaxWriter.CreateInitialDeclarations(out rngSchema);
            string comment = string.Format(" generated by eXolutio on {0} {1} from {2}/{3}. ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), PSMSchema.Project.Name, PSMSchema.Caption);
            RelaxNGXmlSyntaxWriter.AddComment(rngSchema, comment);

            var orderedNodes = ModelIterator.OrderBFS(PSMSchema.PSMNodes.Cast<PSMComponent>().ToList());

            AddStartElements(rngSchema);

            foreach (PSMAssociationMember node in orderedNodes)
            {
                if (node is PSMClass)
                {
                    TranslateClass(rngSchema, (PSMClass)node);
                }
            }

            ReportUsedCustomSimpleTypes();

            XDocument doc = RelaxNGXmlSyntaxWriter.GetFinalResult();
            return doc;
        }

        private void ReportUsedCustomSimpleTypes()
        {
            foreach (AttributeType usedNonDefaultAttribuXElement in usedNonDefaultAttribuXElements)
            {
                Log.AddWarningFormat("Type '{0}' used in the schema is not a built-in type. Use translation to XML schema and reference the type from the XSD. ", usedNonDefaultAttribuXElement.Name);
            }
        }

        private void AddStartElements(XElement parentElement)
        {
            XElement startElement = RelaxNGXmlSyntaxWriter.RngStart(parentElement);
            if (PSMSchema.TopClasses.Count(p => p.ParentAssociation.IsNamed) > 1)
            {
                startElement = RelaxNGXmlSyntaxWriter.RngGroup(parentElement);
            }
            foreach (PSMClass psmClass in PSMSchema.TopClasses)
            {
                if (psmClass.ParentAssociation.IsNamed)
                {
                    XElement rootElement = RelaxNGXmlSyntaxWriter.RngElement(startElement, psmClass.ParentAssociation.Name);
                    RelaxNGXmlSyntaxWriter.RngRef(rootElement, nodeInfos[psmClass].ContentPatternName);
                }
            }
        }

        private void TranslateClass(XElement parentElement, PSMClass psmClass)
        {
            RngNodeTranslationInfo nodeInfo = nodeInfos[psmClass];

            XElement classPatternElement = RelaxNGXmlSyntaxWriter.RngDefine(parentElement, nodeInfo.ContentPatternName);

            //XElement complexTypeContent;
            if (psmClass.SuperClass != null)
            {
                RngNodeTranslationInfo superClassNodeInfo = nodeInfos[psmClass.SuperClass];
                RelaxNGXmlSyntaxWriter.RngRef(classPatternElement, superClassNodeInfo.ContentPatternName);
            }

            AddReferences(classPatternElement, nodeInfo);
        }

        private void AddReferences(XElement parentElement, RngNodeTranslationInfo nodeInfo)
        {
            #region SR
            if (nodeInfo.Node is PSMClass)
            {
                PSMClass psmClass = (PSMClass)nodeInfo.Node;
                // HACK: nodeInfos.ContainsKey(psmClass.RepresentedClass) -- this prevents references to cross-schema representatives,
                //       for now, but should be fixed in the future
                if (psmClass.IsStructuralRepresentative && nodeInfos.ContainsKey(psmClass.RepresentedClass))
                {
                    RngNodeTranslationInfo srInfo = nodeInfos[psmClass.RepresentedClass];
                    RelaxNGXmlSyntaxWriter.RngRef(parentElement, srInfo.ContentPatternName);
                }
            }

            #endregion

            #region attributes

            if (nodeInfo.Node is PSMClass)
            {
                PSMClass psmClass = (PSMClass)nodeInfo.Node;

                foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
                {
                    string attributeName = NamingSupport.SuggestName(psmAttribute, attribute: true);
                    if (psmAttribute.Element)
                    {
                        RelaxNGXmlSyntaxWriter.RngAttributeAsElement(parentElement, attributeName, psmAttribute);
                    }
                    if (!psmAttribute.Element)
                    {
                        RelaxNGXmlSyntaxWriter.RngAttribute(parentElement, attributeName, psmAttribute);
                    }
                    if (psmAttribute.AttributeType == null)
                    {
                        Log.AddWarningFormat("Type of attribute '{0}' is not specified. ", psmAttribute);
                    }
                    else if (!psmAttribute.AttributeType.IsSealed)
                    {
                        if (psmAttribute.AttributeType.BaseType == Guid.Empty || string.IsNullOrEmpty(psmAttribute.AttributeType.XSDDefinition))
                        {
                            Log.AddWarningFormat("Can not translate type of attribute '{0}' - '{1}'. Define base type and XSD definition via Data Type Manager.", psmAttribute, psmAttribute.AttributeType);
                        }
                        else
                        {
                            usedNonDefaultAttribuXElements.AddIfNotContained(psmAttribute.AttributeType);
                        }
                    }
                }
            }

            #endregion

            #region associations

            if (nodeInfo.Node is PSMAssociationMember)
            {
                PSMAssociationMember psmAssociationMember = (PSMAssociationMember)nodeInfo.Node;

                // associations 
                foreach (PSMAssociation psmAssociation in psmAssociationMember.ChildPSMAssociations)
                {
                    RngNodeTranslationInfo childInfo = nodeInfos[psmAssociation.Child];
                    XElement applyCardinalityInThisElement = null;

                    #region class
                    if (childInfo.Node is PSMClass)
                    {
                        PSMClass childClass = (PSMClass) childInfo.Node;
                        XElement elementContainingReference = parentElement;
                        if (psmAssociation.IsNamed)
                        {
                            XElement elementElement = RelaxNGXmlSyntaxWriter.RngElement(parentElement, psmAssociation.Name);
                            elementContainingReference = elementElement;
                            applyCardinalityInThisElement = elementElement;
                        }
                        
                        XElement referenceElement;
                        IEnumerable<PSMClass> allAllowedClasses = childClass.GetSpecificClasses(true).Where(c => !c.Abstract);
                        if (allAllowedClasses.Count() <= 1)
                        {
                            referenceElement = RelaxNGXmlSyntaxWriter.RngRef(elementContainingReference, childInfo.ContentPatternName);
                        }
                        else
                        {
                            referenceElement = RelaxNGXmlSyntaxWriter.RngChoice(elementContainingReference);
                            foreach (PSMClass allowedClass in allAllowedClasses)
                            {
                                RelaxNGXmlSyntaxWriter.RngRef(referenceElement, nodeInfos[allowedClass].ContentPatternName);
                            }
                            applyCardinalityInThisElement = referenceElement;
                        }

                        if (!psmAssociation.IsNamed)
                        {
                            applyCardinalityInThisElement = referenceElement;
                        }
                    }
                    #endregion 

                    #region content model
                    if (childInfo.Node is PSMContentModel)
                    {
                        /* 
                         * association name is ignored in this case,
                         * normalized schemas do not have content models 
                         * with named parent associations
                         */
                        if (psmAssociation.IsNamed)
                        {
                            Log.AddErrorFormat(
                                "Name of '{0}' is ignored. Associations leading to content models should not have names (this violates rules for normalized schemas).",
                                psmAssociation);
                        }
                        XElement contentModelElement;
                        switch (((PSMContentModel)childInfo.Node).Type)
                        {
                            case PSMContentModelType.Sequence:
                                contentModelElement = RelaxNGXmlSyntaxWriter.RngGroup(parentElement);
                                AddReferences(contentModelElement, childInfo);
                                applyCardinalityInThisElement = contentModelElement;
                                break;
                            case PSMContentModelType.Choice:
                                contentModelElement = RelaxNGXmlSyntaxWriter.RngChoice(parentElement);
                                AddReferences(contentModelElement, childInfo);
                                applyCardinalityInThisElement = contentModelElement;
                                break;
                            case PSMContentModelType.Set:
                                contentModelElement = RelaxNGXmlSyntaxWriter.RngInterleave(parentElement);
                                AddReferences(contentModelElement, childInfo);
                                applyCardinalityInThisElement = contentModelElement;
                                break;
                            default:
                                // never gets here
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    #endregion 

                    RelaxNGXmlSyntaxWriter.HandleCardinality(applyCardinalityInThisElement, psmAssociation);
                }
            }

            #endregion

            // todo: EMPTY content 
        }

        public void WriteInCompactSyntax(XDocument xmlSchemaDocument, TextWriter textWriter)
        {
            XslCompiledTransform transform = new XslCompiledTransform();
            XsltSettings settings = new XsltSettings();
            settings.EnableDocumentFunction = true;
            XmlResolver stylesheetResolver = null;
            string tempFileName = System.IO.Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFileName, Properties.Resources.RngToRncClassic);
                using (XmlReader xmlReader = xmlSchemaDocument.Root.CreateReader())
                {
                    transform.Load(tempFileName, settings, stylesheetResolver);
                    XsltArgumentList args = new XsltArgumentList();
                    transform.Transform(xmlReader, args, textWriter);
                }
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }

    public class RngNodeTranslationInfo
    {
        public PSMComponent Node { get; set; }

        public string ContentPatternName { get; set; }
    }
}