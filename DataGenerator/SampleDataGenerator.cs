using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;
using Exolutio.Translation;

namespace Exolutio.DataGenerator
{
    public class SampleDataGenerator : SchemaTranslator<DataGeneratorContext, object, XDocument>
    {
        public uint InfinityBound { get; set; }

        public bool MinimalTree { get; set; }

		public bool EmptyValues { get; set; }

        public bool UseAttributesDefaultValues { get; set; }

        public bool GenerateComments { get; set; }

		public PSMAssociationMember RootForGeneration { get; set; }

        private readonly DataTypeValuesGenerator valuesGenerator;

        public readonly NamingSupport namingSupport;

        public XNamespace ProjectNamespace
        {
            get { return null; }
        }

        public SampleDataGenerator()
        {
            UseAttributesDefaultValues = true;
            InfinityBound = 10;
            GenerateComments = true;
            namingSupport = new NamingSupport { Log = Log };
            valuesGenerator = new DataTypeValuesGenerator(true);
        }

        public override XDocument Translate(PSMSchema schema, string schemaLocation = null)
        {
            Schema = schema;

			if (RootForGeneration == null)
			{
				IEnumerable<PSMAssociationMember> rootCandidates = null;

				if (Schema.PSMSchemaClass != null)
				{
					rootCandidates = ModelIterator.GetLabeledChildNodes(Schema.PSMSchemaClass);
				}

				if (rootCandidates == null || rootCandidates.Count() == 0)
				{
					Log.AddError("No possible root element. Schema class containing labeled association needed. ");
					return null;
				}

				RootForGeneration = rootCandidates.ChooseOneRandomly();
			}

	        DataGeneratorContext context = new DataGeneratorContext();
            context.RootCreated = false;
            TranslateComments(null, context);
			TranslateAssociation(RootForGeneration.ParentAssociation, context);

            if (!String.IsNullOrEmpty(schemaLocation))
            {
                XNamespace schemaInstance = "http://www.w3.org/2001/XMLSchema-instance";
                context.Document.Root.Add(new XAttribute(XNamespace.Xmlns + "xsi", schemaInstance.NamespaceName));
                context.Document.Root.Add(new XAttribute(schemaInstance + "noNamespaceSchemaLocation", schemaLocation));
            }
            return context.Document;
        }

        protected override void TranslateAssociation(PSMAssociation association, DataGeneratorContext context)
        {
            TranslateComments(association, context);
            PathElements.Enqueue(association);

            uint count = association.Parent is PSMSchemaClass ? 1 : ChooseCardinality(association);
            if (count > association.Upper)
            {
                throw new Exception();
            }
            if (association.IsNamed && !(association.Child is PSMContentModel))
            {                
                string elementName = namingSupport.NormalizeTypeName(association);

                for (int i = 0; i < count; i++)
                {
                    //XElement xmlElement = new XElement(ProjectNamespace + elementName);
                    XElement xmlElement = new XElement(elementName);
                    if (context.RootCreated)
                    {
                        context.CurrentElement.Add(xmlElement);
                    }
                    else
                    {
                        if (association.Lower != 1 || association.Upper != 1)
                        {
                            Log.AddErrorFormat(LogMessages.Cardinality_of_association__0__treated_as__1__1__because_it_is_a_root_association_, association);
                        }
                        context.RootCreated = true;
                        context.Document.Add(xmlElement);
                        context.CurrentElement = xmlElement;
                        AddSchemaArguments(xmlElement, context);
                    }

                    // set new current element
                    XElement prevCurrentElement = context.CurrentElement;
                    context.CurrentElement = xmlElement;
                    TranslateAssociationChild(association.Child, context);
                    // return current element to previous value
                    context.CurrentElement = prevCurrentElement;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    TranslateAssociationChild(association.Child, context);
                }
            }
            PathElements.Dequeue();
        }

        public override void TranslateSchemaClass(PSMSchemaClass psmSchemaClass, DataGeneratorContext context)
        {
            throw new InvalidOperationException(LogMessages.EX_TranslateSchemaClass_should_never_be_called);
        }

        protected override object TranslateClass(PSMClass psmClass, DataGeneratorContext context)
        {
            TranslateComments(psmClass, context);

            if (!context.TranslatingRepresentedClass && !context.TranslatingAncestors)
            {

            }
            else
            {
                context.TranslatingRepresentedClass = false;
                context.TranslatingAncestors = false;
            }

            //if (psmClass.Generalizations.Count > 0 && !ignoreAncestors)
            //{
            //    TranslateAncestors(psmClass, context);
            //}
                       
            if (psmClass.IsStructuralRepresentative)
            {
                context.TranslatingRepresentedClass = true;
                TranslateClass(psmClass.RepresentedClass, context);
                context.TranslatingRepresentedClass = false;
            }

            TranslateAttributes(psmClass.PSMAttributes, context);
            TranslateContent(psmClass, context);

            return null;
        }

        private void TranslateContent(PSMAssociationMember parentComponent, DataGeneratorContext context)
        {
            foreach (PSMAssociation childAssociation in parentComponent.ChildPSMAssociations)
            {
                TranslateAssociation(childAssociation, context);
            }
        }

        protected override void TranslateChoiceContentModel(PSMContentModel choiceModel, DataGeneratorContext context)
        {
            TranslateComments(choiceModel, context);
            PSMAssociation chosenAssociation = choiceModel.ChildPSMAssociations.ChooseOneRandomly();
            TranslateAssociation(chosenAssociation, context);
        }

        protected override void TranslateSetContentModel(PSMContentModel setModel, DataGeneratorContext context)
        {
            TranslateComments(setModel, context);
            IEnumerable<PSMAssociation> shuffled = setModel.ChildPSMAssociations.Shuffle();
            IEnumerable<PSMAssociation> followedAssociations;

            uint lower = setModel.ParentAssociation.Lower;
            if (lower > 1)
            {
                Log.AddErrorFormat(LogMessages.Lower_cardinality_of_association__0__treated_as__1__because_the_child_element_is__set__content_model_, setModel.ParentAssociation);
            }
            if (setModel.ParentAssociation.Upper > 1)
            {
                Log.AddErrorFormat(LogMessages.Upper_cardinality_of_association__0__treated_as__1__because_the_child_element_is__set__content_model_, setModel.ParentAssociation);
            }

            if (setModel.ParentAssociation.Lower == 0)
            {
                followedAssociations = shuffled.RandomDelete();
            }
            else
            {
                followedAssociations = shuffled;
            }
            foreach (PSMAssociation followedAssociation in followedAssociations)
            {
                TranslateAssociation(followedAssociation, context);
            }
        }

        protected override void TranslateSequenceContentModel(PSMContentModel sequenceModel, DataGeneratorContext context)
        {
            TranslateComments(sequenceModel, context);
            TranslateContent(sequenceModel, context);
        }

        //#region old generator
        //protected override void TranslateAssociation(PSMAssociation association, DataGeneratorContext context)
        //{
        //    PathElements.Enqueue(association);
        //    TranslateComments(association, context);
        //    base.TranslateAssociation(association, context);
        //    PathElements.Dequeue();
        //}
        //public string GenerateSubtree(PSMElement element)
        //{
        //    DataGeneratorContext c = new DataGeneratorContext();
        //    if (element is PSMClass)
        //    {
        //        TranslateClass((PSMClass) element, c);
        //    }
        //    else if (element is PSMAssociation)
        //    {
        //        TranslateAssociation((PSMAssociation) element, c);
        //    }
        //    else if (element is PSMClassUnion)
        //    {
        //        TranslateClassUnion((PSMClassUnion) element, c);
        //    }
        //    else if (element is PSMContentChoice)
        //    {
        //        TranslateContentChoice((PSMContentChoice) element, c);
        //    }
        //    else if (element is PSMContentContainer)
        //    {
        //        TranslateContentContainer((PSMContentContainer) element, c);
        //    }
        //    else if (element is PSMAttributeContainer)
        //    {
        //        TranslateAttributeContainer((PSMAttributeContainer) element, c);
        //    }
        //    else if (element is PSMAttribute)
        //    {
        //        TranslateAttributeAsElement((PSMAttribute) element, c);
        //    }
        //    return c.Document.InnerXml;
        //}

        //public string GenerateAttribute(PSMAttribute attribute)
        //{
        //    if (attribute.Upper > 1 || attribute.Lower > 1)
        //        Log.AddErrorFormat(string.Format("Attribute {0}.{1} has upper multiplicity {2} that is not valid for XML documents. ", attribute.Class.Name, attribute.Name, attribute.MultiplicityString));

        //    if (attribute.Lower == 1)
        //    {
        //        string value;
        //        if (attribute.Default != null && UseAttributesDefaultValues)
        //            value = attribute.Default;
        //        else
        //            value = valuesGenerator.GenerateValue(attribute.Type);

        //        string attributeName = namingSupport.NormalizeTypeName(attribute, at => at.AliasOrName);
        //        return string.Format("{0}={1}{2}{1}", attributeName, AttributeQuotation, value);
        //    }
        //    else // Lower == 0
        //        return string.Empty;
        //}

        //public override string Translate(PSMDiagram diagram)
        //{
        //    Diagram = diagram;

        //    //oldTODO: This is only fixed, it needs to take into account the Content Containers!
        //    IEnumerable<PSMClass> rootCandidates = diagram.Roots.Where(c => c is PSMClass && (c as PSMClass).HasElementLabel).Cast<PSMClass>();

        //    if (rootCandidates.Count() == 0)
        //    {
        //        Log.AddErrorFormat("No possible root element. Consider assigning an element label to one of the root classes.");
        //        return String.Empty;
        //    }

        //    try
        //    {
        //        PSMClass root = rootCandidates.ChooseOneRandomly();
        //        DataGeneratorContext context = new DataGeneratorContext();

        //        TranslateComments(null, context);
        //        TranslateClass(root, context);
        //        AddSchemaArguments((XmlElement)context.ClassNodes[root], context);

        //        // write with indentation
        //        StringBuilder sb = new StringBuilder();
        //        XmlWriterSettings settings = new XmlWriterSettings();
        //        settings.Indent = true;
        //        settings.IndentChars = "  ";
        //        settings.NewLineChars = "\r\n";
        //        settings.NewLineHandling = NewLineHandling.Replace;
        //        XmlWriter writer = XmlWriter.Create(sb, settings);
        //        // ReSharper disable AssignNullToNotNullAttribute
        //        context.Document.Save(writer);
        //        // ReSharper restore AssignNullToNotNullAttribute
        //        // ReSharper disable PossibleNullReferenceException
        //        writer.Close();
        //        // ReSharper restore PossibleNullReferenceException

        //        return sb.ToString();
        //    }
        //    catch (XmlSchemaException e)
        //    {
        //        if (e.Message.Contains("is recursive and causes infinite nesting"))
        //            return null;
        //        else
        //            throw;
        //    }
        //}

        private void TranslateComments(Component component, DataGeneratorContext context)
        {
            if (!GenerateComments)
                return;

            //IEnumerable<Comment> comments;
            //if (component != null)
            //    comments = component.Comments;
            //else
            //    comments = Diagram.DiagramElements.Keys.OfType<Comment>().Where(comment => comment.AnnotatedElement is Model.Model);

            //foreach (Comment comment in comments)
            //{
            //    XmlComment xmlComment = context.Document.CreateComment(comment.Body);
            //    if (context.CurrentElement != null)
            //        context.CurrentElement.AppendChild(xmlComment);
            //    else
            //        context.Document.AppendChild(xmlComment);
            //}
        }

        //protected override XmlNode TranslateClass(PSMClass psmClass, DataGeneratorContext context)
        //{
        //    if (psmClass.Specifications.Count > 0)
        //    {
        //        // identify the set of possible instances
        //        List<PSMClass> candidates = new List<PSMClass>();
        //        FindSpecificationsRecursive(ref candidates, psmClass);
        //        PSMClass selectedClass = candidates.Where(candidate => !candidate.IsAbstract && candidate.HasElementLabel).ChooseOneRandomly();
        //        return TranslateInstantiatedClass(selectedClass, false, context);
        //    }
        //    else
        //    {
        //        return TranslateInstantiatedClass(psmClass, false, context);
        //    }
        //}

        //private static void FindSpecificationsRecursive(ref List<PSMClass> result, PSMClass psmClass)
        //{
        //    result.Add(psmClass);

        //    foreach (Generalization generalization in psmClass.Specifications)
        //    {
        //        FindSpecificationsRecursive(ref result, (PSMClass)generalization.Specific);
        //    }
        //}

        //private XmlNode TranslateInstantiatedClass(PSMClass psmClass, bool ignoreAncestors, DataGeneratorContext context)
        //{
        //    XmlElement xmlElement = null;
        //    XmlElement prevCurrentElement = context.CurrentElement;

        //    TranslateComments(psmClass, context);
        //    PathElements.Enqueue(psmClass);
        //    if (!context.TranslatingRepresentedClass && !context.TranslatingAncestors)
        //    {
        //        if (psmClass.HasElementLabel)
        //        {
        //            string elementName = namingSupport.NormalizeTypeName(psmClass, c => c.ElementName);
        //            if (context.CurrentElement == null)
        //            {
        //                xmlElement = context.Document.CreateElement(elementName, ProjectNamespace);
        //                context.Document.AppendChild(xmlElement);
        //            }
        //            else
        //            {
        //                xmlElement = context.Document.CreateElement(elementName, ProjectNamespace);
        //                context.CurrentElement.AppendChild(xmlElement);
        //            }
        //            context.CurrentElement = xmlElement;
        //            context.ClassNodes[psmClass] = xmlElement;
        //        }
        //    }
        //    else
        //    {
        //        context.TranslatingRepresentedClass = false;
        //        context.TranslatingAncestors = false;
        //    }

        //    if (psmClass.Generalizations.Count > 0 && !ignoreAncestors)
        //    {
        //        TranslateAncestors(psmClass, context);
        //    }

        //    TranslateAttributes(psmClass.Attributes, context);

        //    if (psmClass.IsStructuralRepresentative)
        //    {
        //        context.TranslatingRepresentedClass = true;
        //        TranslateClass(psmClass.RepresentedPSMClass, context);
        //        context.TranslatingRepresentedClass = false;
        //    }


        //    TranslateComponents(psmClass, context);

        //    PathElements.Dequeue();

        //    context.CurrentElement = prevCurrentElement;
        //    return xmlElement;
        //}

        private static void AddSchemaArguments(XElement rootElement, DataGeneratorContext context)
        {
            //XmlAttribute schemaInstance = context.Document.CreateAttribute("xmlns:xsi", "http://www.w3.org/2000/xmlns/");
            //schemaInstance.Value = "http://www.w3.org/2001/XMLSchema-instance";
            //rootElement.Attributes.Append(schemaInstance);
        }

        public Queue<Component> PathElements = new Queue<Component>();

        //private void TranslateAncestors(PSMClass psmClass, DataGeneratorContext context)
        //{
        //    List<PSMClass> ancestors = FindClassAncestors(psmClass);

        //    foreach (PSMClass ancestor in ancestors)
        //    {
        //        context.TranslatingAncestors = true;
        //        TranslateInstantiatedClass(ancestor, true, context);
        //    }
        //    context.TranslatingAncestors = false;
        //}

        //private static List<PSMClass> FindClassAncestors(PSMClass psmClass)
        //{
        //    PSMClass level = psmClass;
        //    List<PSMClass> ancestors = new List<PSMClass>();
        //    while (level != null)
        //    {
        //        if (level.Generalizations.Count > 1)
        //            throw new Exception("PSM class can have only one generalization, multiple inheritance is forbidden. ");
        //        else if (level.Generalizations.Count == 1)
        //        {
        //            PSMClass generalClass = (PSMClass)level.Generalizations[0].General;
        //            ancestors.Add(generalClass);
        //            level = generalClass;
        //        }
        //        else
        //        {
        //            level = null;
        //        }
        //    }
        //    ancestors.Reverse();
        //    return ancestors;
        //}

        //private void TranslateComponents(PSMSuperordinateComponent psmSuperordinateComponent, DataGeneratorContext context)
        //{
        //    foreach (PSMSubordinateComponent component in psmSuperordinateComponent.Components)
        //    {
        //        TranslateSubordinateComponent(component, context);
        //    }
        //}

        protected void TranslateAttributes(IEnumerable<PSMAttribute> attributes, DataGeneratorContext context)
        {
            foreach (PSMAttribute attribute in attributes)
            {
                TranslateAttribute(attribute, context);
            }
        }

        protected override void TranslateAttribute(PSMAttribute attribute, DataGeneratorContext context)
        {
            if (!attribute.Element && (attribute.Upper > 1 || attribute.Lower > 1))
                Log.AddError(string.Format("Attribute {0}.{1} has upper multiplicity {2} that is not valid for XML documents. ", attribute.PSMClass.Name, attribute.Name, attribute.CardinalityString));

            bool appears = false;

            // mandatory attribute
            if (attribute.Lower >= 1)
            {
                appears = true;
            }
            else if (attribute.Lower == 0)
            {
                appears = RandomGenerator.Toss(2, 1);
            }

            uint upper = attribute.Element ? ChooseCardinality(attribute) : 1;

            if (appears)
            {
                for (int i = 0; i < upper; i++)
                {
                    string value = string.Empty;
					if (!EmptyValues)
					{
						if (attribute.DefaultValue != null && UseAttributesDefaultValues)
							value = attribute.DefaultValue;
						else
							value = valuesGenerator.GenerateValue(attribute.AttributeType);
					}
	                string attributeName = namingSupport.NormalizeTypeName(attribute);

                    
                    if (attribute.Element)
                    {
                        //XElement element = new XElement(ProjectNamespace + attributeName);
                        XElement element = new XElement(attributeName);
                        XText text = new XText(value);
                        context.CurrentElement.Add(element);
                        element.Add(text);
                    }
                    else
                    {
                        XAttribute a = new XAttribute(attributeName, value);
                        context.CurrentElement.Add(a);
                    }
                }
            }
        }

        //private void TranslateAttributeAsElement(PSMAttribute attribute, DataGeneratorContext context)
        //{
        //    int count = ChooseMultiplicity(attribute);

        //    for (int i = 0; i < count; i++)
        //    {
        //        string attributeName = namingSupport.NormalizeTypeName(attribute, a => a.AliasOrName);
        //        XmlElement element = context.Document.CreateElement(attributeName, ProjectNamespace);
        //        string value;
        //        if (attribute.Default != null && UseAttributesDefaultValues)
        //            value = attribute.Default;
        //        else
        //            value = valuesGenerator.GenerateValue(attribute.Type);
        //        XmlText text = context.Document.CreateTextNode(value);
        //        context.CurrentElement.AppendChild(element);
        //        element.AppendChild(text);
        //    }
        //}

        //protected override void TranslateContentChoice(PSMContentChoice contentChoice, DataGeneratorContext context)
        //{
        //    TranslateComments(contentChoice, context);
        //    PSMSubordinateComponent selectedPath = contentChoice.Components.ChooseOneRandomly();
        //    TranslateSubordinateComponent(selectedPath, context);
        //}

        //protected override void TranslateContentContainer(PSMContentContainer contentContainer, DataGeneratorContext context)
        //{
        //    XmlElement prevCurrentElement = context.CurrentElement;

        //    TranslateComments(contentContainer, context);
        //    string elementName = namingSupport.NormalizeTypeName(contentContainer, c => contentContainer.Name);
        //    XmlElement xmlElement = context.Document.CreateElement(elementName, ProjectNamespace);
        //    context.CurrentElement.AppendChild(xmlElement);
        //    // set new current element
        //    context.CurrentElement = xmlElement;
        //    TranslateComponents(contentContainer, context);
        //    // return current element to previous value
        //    context.CurrentElement = prevCurrentElement;
        //}

        public uint GetComponentOccurrences(Component component)
        {
            return (uint) PathElements.Count(pathElement => pathElement == component);
        }

        //protected override void TranslateAssociationChild(PSMAssociationChild associationChild, DataGeneratorContext context)
        //{
        //    int count = 1;
        //    if (associationChild.ParentAssociation != null)
        //    {
        //        count = ChooseMultiplicity(associationChild.ParentAssociation);
        //    }

        //    for (int i = 0; i < count; i++)
        //    {
        //        if (associationChild is PSMClass)
        //        {
        //            PSMClass psmClass = (PSMClass)associationChild;
        //            if (!psmClass.HasElementLabel)
        //            {
        //                if (GenerateComments)
        //                {
        //                    XmlComment xmlComment = context.Document.CreateComment(string.Format("Content group {0} {1}", psmClass.Name, i));
        //                    context.CurrentElement.AppendChild(xmlComment);
        //                }
        //            }
        //            TranslateClass(psmClass, context);
        //        }
        //        else
        //        {
        //            TranslateClassUnion((PSMClassUnion)associationChild, context);
        //        }
        //    }
        //}

        //private void TranslateClassUnion(PSMClassUnion classUnion, DataGeneratorContext context)
        //{
        //    PSMAssociationChild selectedPath = classUnion.Components.ChooseOneRandomly();
        //    TranslateComments(classUnion, context);
        //    TranslateAssociationChild(selectedPath, context);
        //}

        //protected override void TranslateAttributeContainer(PSMAttributeContainer attributeContainer, DataGeneratorContext context)
        //{
        //    foreach (PSMAttribute attribute in attributeContainer.PSMAttributes)
        //    {
        //        TranslateComments(attributeContainer, context);
        //        TranslateAttributeAsElement(attribute, context);
        //    }
        //}

        private uint ChooseCardinality(IHasCardinality cardinalityComponent)
        {
            uint lower = cardinalityComponent.Lower;
            uint upper = !cardinalityComponent.Upper.IsInfinity ? cardinalityComponent.Upper.Value : InfinityBound;
            if (upper > 2 && PathElements.Sum(e => e == cardinalityComponent ? 1 : 0) > 2)
            {
                upper = 2;
            }

            if (cardinalityComponent is PSMAssociation)
            {
                PSMAssociation association = (PSMAssociation)cardinalityComponent;
                uint occurrences = GetComponentOccurrences(association);
                if (occurrences > 10 && association.Lower > 0)
                {
                    Log.AddError(string.Format("Association {0} is recursive and causes infinite nesting.", association));
                    throw new ExolutioException(string.Format("Association {0} is recursive and causes infinite nesting.", association));
                }
                if (MinimalTree)
                    return lower;
                else
                {
                    int result = RandomGenerator.Next((int)lower, (int)upper + 1, (int) (occurrences - 1));
                    return (uint) result;
                }
            }
            else
            {
                if (MinimalTree)
                    return 1;
                else
                    return (uint) RandomGenerator.Next(Math.Max((int) lower, 1), (int) (upper + 1));
            }
        }

        //#endregion
    }

    public class DataGeneratorContext
    {
        private const string UTF8 = "utf-8";
        private const string XML_VERSION = @"1.0";

        public XDocument Document { get; private set; }

        public XElement CurrentElement { get; set; }

        public bool TranslatingRepresentedClass { get; set; }

        public bool TranslatingAncestors { get; set; }

        public bool RootCreated { get; set; }

        public DataGeneratorContext()
        {
            Document = new XDocument();
            Document.Declaration = new XDeclaration(XML_VERSION, UTF8, null);
        }
    }
}
