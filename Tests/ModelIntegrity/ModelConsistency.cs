#define SKIP

using System;
using System.Linq;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using NUnit.Framework;
using EvoX.Model.Versioning;

namespace Tests.ModelIntegrity
{
    public static class ModelConsistency
    {
        public static void CheckProject(Project project)
        {
            if (project.UsesVersioning)
            {
                Assert.IsNotNull(project.VersionManager);
                Assert.IsNotNull(project.ProjectVersions);

#if SKIP
#else 
                Assert.Throws(typeof(EvoXModelException), () =>
                {
                    if (project.SingleVersion != null)
                    {
                    }
                });
#endif

                foreach (ProjectVersion projectVersion in project.ProjectVersions)
                {
                    CheckProjectVersion(projectVersion);
                }

                CheckVersioningConsistency(project);
            }
            else
            {
#if SKIP
#else 
                Assert.Throws(typeof(EvoXModelException), () =>
                                                                     {
                                                                         if (project.ProjectVersions != null)
                                                                         {
                                                                         }
                                                                     });
#endif
                Assert.IsNotNull(project.SingleVersion);

                CheckProjectVersion(project.SingleVersion);
            }
        }

        private static void CheckComponent(Component component)
        {
            CheckEvoXObject(component);
            Assert.IsNotNull(component.Schema);
            Assert.AreEqual(component.Schema.ProjectVersion, component.ProjectVersion);
            Assert.IsNotNull(component.ProjectVersion);
            Assert.AreEqual(component.ProjectVersion.Project, component.Project);

            CheckVersionedItem(component);
        }

        private static void CheckVersionedItem(IVersionedItem component)
        {
            if (component.ProjectVersion.Project.UsesVersioning)
            {
                Assert.IsNotNull(component.Version);
                Assert.IsTrue(component.ProjectVersion.Project.VersionManager.Versions.Contains(component.Version));
                Assert.AreEqual(component.Version, component.ProjectVersion.Version);
            }
            else
            {
                Assert.IsNull(component.Version);
            }
        }

        private static void CheckEvoXObject(EvoXObject evoXObject)
        {
            Assert.AreNotEqual(evoXObject, Guid.Empty);
            Assert.IsNotNull(evoXObject.Project);
        }

        private static void CheckProjectVersion(ProjectVersion projectVersion)
        {
            foreach (AttributeType attributeType in projectVersion.AttributeTypes)
            {
                CheckAttributeType(attributeType);
            }

            CheckPIMSchema(projectVersion.PIMSchema);

            foreach (PSMSchema psmSchema in projectVersion.PSMSchemas)
            {
                CheckPSMSchema(psmSchema);
            }
        }

        private static void CheckSchema(Schema schema)
        {
            CheckEvoXObject(schema);

            Assert.IsNotNull(schema.ProjectVersion);
        }

        private static void CheckAttributeType(AttributeType attributeType)
        {
            CheckEvoXObject(attributeType);
            CheckVersionedItem(attributeType);
            Assert.IsTrue(attributeType.Schema == null || attributeType.Schema.ProjectVersion == attributeType.ProjectVersion);
        }

        #region PSM

        private static void CheckPSMSchema(PSMSchema psmSchema)
        {
            CheckSchema(psmSchema);

            CollectionAssert.AllItemsAreNotNull(psmSchema.Roots);
            foreach (PSMAssociationMember psmAssociationMember in psmSchema.Roots)
            {
                Assert.IsNull(psmAssociationMember.ParentAssociation);
            }

            foreach (PSMComponent psmComponent in ModelIterator.GetPSMComponents(psmSchema))
            {
                Assert.AreEqual(psmComponent.PSMSchema, psmSchema);
            }

            foreach (PSMSchemaReference psmSchemaReference in psmSchema.PSMSchemaReferences)
            {
                CheckPSMSchemaReference(psmSchemaReference);
            }

            foreach (PSMComponent psmComponent in ModelIterator.GetPSMComponents(psmSchema))
            {
                if (psmComponent is PSMAssociation)
                {
                    CheckPSMAssociation((PSMAssociation) psmComponent);
                }
                if (psmComponent is PSMClass)
                {
                    CheckPSMClass((PSMClass) psmComponent);
                }
                if (psmComponent is PSMAttribute)
                {
                    CheckPSMAttribute((PSMAttribute) psmComponent);
                }
                if (psmComponent is PSMContentModel)
                {
                    CheckPSMContentModel((PSMContentModel) psmComponent);
                }
                if (psmComponent is PSMSchemaClass)
                {
                    CheckPSMSchemaClass((PSMSchemaClass) psmComponent);
                }
            }
        }

        private static void CheckPSMSchemaReference(PSMSchemaReference psmSchemaReference)
        {
            CheckPSMComponent(psmSchemaReference);
            Assert.AreEqual(psmSchemaReference.PSMSchema.ProjectVersion, psmSchemaReference.ReferencedPSMSchema.ProjectVersion);
            Assert.IsNotNull(psmSchemaReference.ReferencedPSMSchema);
        }

        private static void CheckPSMAssociation(PSMAssociation psmAssociation)
        {
            CheckPSMComponent(psmAssociation);

            Assert.IsNotNull(psmAssociation.Child);
            Assert.IsNotNull(psmAssociation.Parent);
            Assert.IsTrue(psmAssociation.Parent.ChildPSMAssociations.Contains(psmAssociation));
            Assert.IsTrue(psmAssociation.Child.ParentAssociation == psmAssociation);

            CollectionAssert.Contains(psmAssociation.Schema.SchemaComponents, psmAssociation.Child);
        }

        private static void CheckPSMAssociationMember(PSMAssociationMember psmAssociationMember)
        {
            CheckPSMComponent(psmAssociationMember);
            Assert.IsTrue(psmAssociationMember.ParentAssociation == null || !psmAssociationMember.PSMSchema.Roots.Contains(psmAssociationMember));
            Assert.IsFalse(psmAssociationMember.ParentAssociation == null && !psmAssociationMember.PSMSchema.Roots.Contains(psmAssociationMember));
            if (psmAssociationMember.ParentAssociation != null)
            {
                Assert.IsTrue(psmAssociationMember.ParentAssociation.Child == psmAssociationMember);
            }
            foreach (PSMAssociation childPsmAssociation in psmAssociationMember.ChildPSMAssociations)
            {
                Assert.IsTrue(childPsmAssociation.Parent == psmAssociationMember);
            }

            foreach (PSMAssociation childPsmAssociation in psmAssociationMember.ChildPSMAssociations)
            {
                CollectionAssert.Contains(psmAssociationMember.Schema.SchemaComponents, childPsmAssociation);
            }
        }

        private static void CheckPSMAttribute(PSMAttribute psmAttribute)
        {
            CheckPSMComponent(psmAttribute);

            Assert.IsNotNull(psmAttribute.PSMClass);
            Assert.IsTrue(psmAttribute.PSMClass.PSMAttributes.Contains(psmAttribute));
        }

        private static void CheckPSMClass(PSMClass psmClass)
        {
            CheckPSMAssociationMember(psmClass);

            foreach (PSMAttribute psmAttribute in psmClass.PSMAttributes)
            {
                Assert.IsNotNull(psmAttribute.PSMClass);
                Assert.AreEqual(psmAttribute.PSMClass, psmClass);
                CollectionAssert.Contains(psmAttribute.Schema.SchemaComponents, psmAttribute);
            }
        }

        private static void CheckPSMComponent(PSMComponent psmComponent)
        {
            CheckComponent(psmComponent);
        }

        private static void CheckPSMContentModel(PSMContentModel psmContentModel)
        {
            CheckPSMAssociationMember(psmContentModel);
        }

        private static void CheckPSMSchemaClass(PSMSchemaClass psmSchemaClass)
        {
            CheckPSMAssociationMember(psmSchemaClass);
            CollectionAssert.Contains(psmSchemaClass.PSMSchema.Roots, psmSchemaClass);
            Assert.IsNull(psmSchemaClass.ParentAssociation);
        }

        #endregion

        #region PIM

        private static void CheckPIMSchema(PIMSchema pimSchema)
        {
            CheckSchema(pimSchema);

            foreach (PIMClass pimClass in pimSchema.PIMClasses)
            {
                CheckPIMClass(pimClass);
            }

            foreach (PIMAssociation pimAssociation in pimSchema.PIMAssociations)
            {
                CheckPIMAssociation(pimAssociation);
            }

            foreach (PIMComponent pimComponent in ModelIterator.GetPIMComponents(pimSchema))
            {
                Assert.AreEqual(pimComponent.PIMSchema, pimSchema);
            }
        }

        private static void CheckPIMComponent(PIMComponent pimComponent)
        {
            CheckComponent(pimComponent);
            Assert.IsNotNull(pimComponent.PIMSchema);

#if SKIP
#else 
	        foreach (PSMComponent interpretedComponent in pimComponent.InterpretedComponents)
	        {
	            Assert.AreEqual(interpretedComponent.Interpretation, pimComponent);
	        }
#endif
        }

        private static void CheckPIMAssociation(PIMAssociation pimAssociation)
        {
            CheckPIMComponent(pimAssociation);
            foreach (PIMAssociationEnd pimAssociationEnd in pimAssociation.PIMAssociationEnds)
            {
                CheckPIMAssociationEnd(pimAssociationEnd);
            }

            CollectionAssert.Contains(pimAssociation.PIMSchema.PIMAssociations, pimAssociation);

            foreach (PIMAssociationEnd pimAssociationEnd in pimAssociation.PIMAssociationEnds)
            {
                Assert.IsTrue(pimAssociationEnd.PIMClass.PIMAssociationEnds.Contains(pimAssociationEnd));
                Assert.IsTrue(pimAssociationEnd.PIMAssociation.PIMAssociationEnds.Contains(pimAssociationEnd));
            }
        }

        private static void CheckPIMAssociationEnd(PIMAssociationEnd pimAssociationEnd)
        {
            CheckPIMComponent(pimAssociationEnd);
            Assert.IsNotNull(pimAssociationEnd.PIMAssociation);
            Assert.IsNotNull(pimAssociationEnd.PIMClass);

            CollectionAssert.Contains(pimAssociationEnd.PIMSchema.PIMAssociationEnds, pimAssociationEnd);

            Assert.IsTrue(pimAssociationEnd.PIMClass.PIMAssociationEnds.Contains(pimAssociationEnd));
            Assert.IsTrue(pimAssociationEnd.PIMAssociation.PIMAssociationEnds.Contains(pimAssociationEnd));
        }

        private static void CheckPIMClass(PIMClass pimClass)
        {
            CheckPIMComponent(pimClass);
            CollectionAssert.Contains(pimClass.PIMSchema.PIMClasses, pimClass);

            foreach (PIMAttribute pimAttribute in pimClass.PIMAttributes)
            {
                CheckPIMAttribute(pimAttribute);
            }

            foreach (PIMAssociationEnd pimAssociationEnd in pimClass.PIMAssociationEnds)
            {
                CheckPIMAssociationEnd(pimAssociationEnd);
                Assert.IsTrue(pimAssociationEnd.PIMAssociation.PIMClasses.Contains(pimClass));
                Assert.IsTrue(pimAssociationEnd.PIMAssociation.PIMAssociationEnds.Contains(pimAssociationEnd));
            }
        }

        private static void CheckPIMAttribute(PIMAttribute pimAttribute)
        {
            CheckPIMComponent(pimAttribute);
            Assert.IsNotNull(pimAttribute.PIMClass);
            CollectionAssert.Contains(pimAttribute.PIMSchema.PIMAttributes, pimAttribute);
            CollectionAssert.Contains(pimAttribute.PIMClass.PIMAttributes, pimAttribute);
        }

        //private static void CheckPsmClassParent(PSMDiagram diagram)
        //{
        //    foreach (PSMClass psmClass in diagram.DiagramElements.Keys.OfType<PSMClass>())
        //    {
        //        if (psmClass.ParentAssociation != null)
        //        {
        //            if (psmClass.ParentAssociation.ChildEnd != psmClass)
        //            {
        //                throw new ModelConsistencyException(string.Format("Bad class parent association component {0}", psmClass));
        //            }
        //            if (psmClass.ParentUnion != null)
        //            {
        //                throw new ModelConsistencyException(string.Format("Bad class parent association component {0}", psmClass));
        //            }
        //        }
        //        else if (psmClass.ParentUnion != null)
        //        {
        //            if (!psmClass.ParentUnion.Components.Contains(psmClass))
        //            {
        //                throw new ModelConsistencyException(string.Format("Bad class parent union component {0}", psmClass));
        //            }
        //            if (psmClass.ParentAssociation != null)
        //            {
        //                throw new ModelConsistencyException(string.Format("Bad class parent association component {0}", psmClass));
        //            }
        //        }
        //        else
        //        {
        //            if (!diagram.Roots.Contains(psmClass) && psmClass.Generalizations.Count == 0)
        //            {
        //                throw new ModelConsistencyException(string.Format("Bad class {0}", psmClass));
        //            }
        //        }
        //    }
        //}

        //public static void CheckPsmParentsAndRoots(PSMDiagram diagram)
        //{
        //    foreach (Element element in diagram.DiagramElements.Keys)
        //    {
        //        PSMSubordinateComponent subordinateComponent = (element as PSMSubordinateComponent);
        //        if (subordinateComponent != null)
        //        {
        //            if (subordinateComponent.ParentEnd == null && !diagram.Roots.Contains((PSMClass)subordinateComponent))
        //            {
        //                throw new ModelConsistencyException(string.Format("Bad subordinate component {0}", subordinateComponent));
        //            }
        //            if (subordinateComponent.ParentEnd != null)
        //            {
        //                if (!subordinateComponent.ParentEnd.Components.Contains(subordinateComponent))
        //                {
        //                    throw new ModelConsistencyException(string.Format("Bad subordinate component {0}", subordinateComponent));
        //                }
        //            }
        //        }

        //        PSMSuperordinateComponent superordinateComponent = element as PSMSuperordinateComponent;

        //        if (superordinateComponent != null)
        //        {
        //            foreach (PSMSubordinateComponent component in superordinateComponent.Components)
        //            {
        //                if (component.ParentEnd != superordinateComponent)
        //                {
        //                    throw new ModelConsistencyException(string.Format("Bad superordinateComponent component {0}", superordinateComponent));
        //                }
        //            }
        //        }
        //    }
        //}

        //public static void CheckPsmElementsDiagram(PSMDiagram diagram)
        //{
        //    foreach (Element element in diagram.DiagramElements.Keys)
        //    {
        //        PSMElement psmElement = element as PSMElement;
        //        if (psmElement != null)
        //        {
        //            if (psmElement.Diagram != diagram)
        //            {
        //                throw new ModelConsistencyException(string.Format("Element {0}  has wrong diagram.", psmElement));
        //            }
        //        }
        //    }
        //}

        //public static void CheckViewHelpersDiagram(Diagram diagram)
        //{
        //    foreach (KeyValuePair<Element, ViewHelper> kvp in diagram.DiagramElements)
        //    {
        //        if (kvp.Value.Diagram != diagram)
        //        {
        //            throw new ModelConsistencyException(string.Format("ViewHelper {0} for element {1} has wrong diagram.", kvp.Key,
        //                                                              kvp.Value));
        //        }
        //    }
        //}

        //public static void CheckElementSchema(IEnumerable<Element> elements, Schema schema, List<Element> checkedAlready)
        //{
        //    if (checkedAlready == null)
        //        checkedAlready = new List<Element>();
        //    foreach (Element element in elements)
        //    {
        //        CheckElementSchema(element, schema, checkedAlready);
        //    }
        //}

        //public static void CheckElementSchema(Element element, Schema schema, List<Element> checkedAlready)
        //{
        //    if (element == null)
        //    {
        //        return;
        //    }

        //    if (checkedAlready != null)
        //    {
        //        if (checkedAlready.Contains(element))
        //            return;
        //        else
        //            checkedAlready.Add(element);
        //    }

        //    if (element.Schema != schema)
        //    {
        //        throw new ModelConsistencyException(string.Format("Schema of element {0} differs.", element));
        //    }

        //    // reiterate through properties
        //    Type type = element.GetType();
        //    Type elementInterfaceType = typeof(Element);
        //    Type elementCollectionType = typeof(IEnumerable);

        //    foreach (PropertyInfo propertyInfo in type.GetProperties())
        //    {
        //        if (elementInterfaceType.IsAssignableFrom(propertyInfo.PropertyType))
        //        {
        //            //System.Diagnostics.Debug.WriteLine(String.Format("Checking property {0}.{1}.", type.Name, propertyInfo.Name));
        //            Element value = propertyInfo.GetValue(element, null) as Element;
        //            if (value != null)
        //            {
        //                CheckElementSchema(value, schema, checkedAlready);
        //            }
        //        }

        //        if (elementCollectionType.IsAssignableFrom(propertyInfo.PropertyType))
        //        {
        //            IEnumerable theCollection = propertyInfo.GetValue(element, null) as IEnumerable;
        //            if (theCollection != null)
        //            {
        //                foreach (object item in theCollection)
        //                {
        //                    if (item is Element)
        //                    {
        //                        CheckElementSchema(element, schema, checkedAlready);
        //                    }
        //                }

        //            }
        //        }
        //    }

        //}

        #endregion

        #region Versioning

        private static void CheckVersioningConsistency(Project project)
        {
            VersionManager versionManager = project.VersionManager;

            Assert.AreEqual(versionManager.Versions.Count, project.ProjectVersions.Count);
            foreach (ProjectVersion projectVersion in project.ProjectVersions)
            {
                Assert.AreEqual(projectVersion, project.GetProjectVersion(projectVersion.Version));

            }

            foreach (ProjectVersion projectVersion in project.ProjectVersions)
            {
                foreach (EvoXObject o in ModelIterator.GetAllModelItems(projectVersion))
                {
                    IVersionedItem versionedItem = (o as IVersionedItem);
                    if (versionedItem != null)
                    {
                        Assert.AreEqual(versionedItem.Version,projectVersion.Version);
                        Assert.IsTrue(projectVersion.Version.Items.Contains(versionedItem));
                    }
                }
            }

            foreach (EvoX.Model.Versioning.Version version in project.VersionManager.Versions)
            {
                EvoX.Model.Versioning.Version _v = version;
                Assert.IsTrue(version.Items.All(i => i.Version == _v));
            }

#if DEBUG
            versionManager.VerifyConsistency();
#endif
        }

        #endregion
    }
}