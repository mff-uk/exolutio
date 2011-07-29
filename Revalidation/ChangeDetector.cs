using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Exolutio.Revalidation.Changes;
using System.Reflection;
using Exolutio.SupportingClasses;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation
{
    /// <summary>
    /// Holds instances of all change predicates
    /// </summary>
    public class DetectedChangeInstancesSet: Dictionary<Type, List<ChangeInstance>>
    {
        private readonly List<PSMComponent> redNodes = new List<PSMComponent>();

        public List<PSMComponent> RedNodes { get { return redNodes; } }

        private readonly List<PSMComponent> blueNodes = new List<PSMComponent>();

        public List<PSMComponent> BlueNodes { get { return blueNodes; } }

        private readonly List<PSMComponent> greenNodes = new List<PSMComponent>();

        public List<PSMComponent> GreenNodes { get { return greenNodes; } }

        private readonly List<PSMComponent> addedNodes = new List<PSMComponent>();

        public List<PSMComponent> AddedNodes { get { return addedNodes; } }

        private readonly List<PSMAttribute> convertedAttributesAE = new List<PSMAttribute>();

        public List<PSMAttribute> ConvertedAttributesAE { get { return convertedAttributesAE; } }

        private readonly List<PSMAttribute> convertedAttributesEA = new List<PSMAttribute>();

        public List<PSMAttribute> ConvertedAttributesEA { get { return convertedAttributesEA; } }

        public bool IsAddedNode(PSMComponent component)
        {
            return AddedNodes.Contains(component);
        }

        public bool IsGroupNode(PSMComponent psmComponent)
        {
            return psmComponent.DownCastSatisfies<PSMClass>(psmClass => 
                       psmClass.GetInVersion(OldVersion) != null 
                       && psmClass.GetInVersion(OldVersion).ParentAssociation != null
                       && !psmClass.GetInVersion(OldVersion).ParentAssociation.IsNamed) || 
                   psmComponent.DownCastSatisfies<PSMClass>(psmClass => 
                       IsAddedNode(psmClass) && psmClass.ParentAssociation != null && !psmClass.ParentAssociation.IsNamed);
        }

        //private readonly List<PSMComponent> removedNodes = new List<PSMComponent>();

        //public List<PSMComponent> RemovedNodes { get { return removedNodes; } }
        
        public IEnumerable<PSMComponent> AllNodes { get { return RedNodes.Union(BlueNodes).Union(GreenNodes); } }

        public Version OldVersion { get; set; }

        public Version NewVersion { get; set; }

        public bool ExistsCardinalityChange(PSMComponent component)
        {
            bool dummy;
            return ExistsCardinalityChange(component, out dummy, out dummy);
        }

        public bool ExistsCardinalityChangeRequiringCreation(PSMComponent component)
        {
            bool requiresCreation, dummy2;
            return ExistsCardinalityChange(component, out requiresCreation, out dummy2);
        }

        public bool ExistsCardinalityChangeRequiringDeletion(PSMComponent component)
        {
            bool dummy1, requiresDeletion;
            return ExistsCardinalityChange(component, out dummy1, out requiresDeletion);
        }

        private bool ExistsCardinalityChange(PSMComponent component, out bool requiresCreation, out bool requiresDeletion)
        {
            bool result = false;
            requiresCreation = false;
            requiresDeletion = false; 

            if (component is PSMAttribute)
            {
                requiresCreation = this.ContainsKey(typeof (AttributeCardinalityChangedInstance)) &&
                       this[typeof (AttributeCardinalityChangedInstance)].Cast<AttributeCardinalityChangedInstance>().
                           Any(ci => ci.PSMAttribute == component && ci.RequiresCreation);

                result |= this.ContainsKey(typeof (AttributeCardinalityChangedInstance)) &&
                       this[typeof (AttributeCardinalityChangedInstance)].Cast<AttributeCardinalityChangedInstance>().
                           Any(ci => ci.PSMAttribute == component);
            }

            if (component is PSMAssociationMember && ((PSMAssociationMember)component).ParentAssociation != null)
            {
                requiresCreation = this.ContainsKey(typeof(AssociationCardinalityChangedInstance)) &&
                       this[typeof(AssociationCardinalityChangedInstance)].Cast<AssociationCardinalityChangedInstance>().
                           Any(ci => ci.PSMAssociation == ((PSMAssociationMember)component).ParentAssociation && ci.RequiresCreation);

                result |= this.ContainsKey(typeof(AssociationCardinalityChangedInstance)) &&
                       this[typeof(AssociationCardinalityChangedInstance)].Cast<AssociationCardinalityChangedInstance>().
                           Any(ci => ci.PSMAssociation == ((PSMAssociationMember)component).ParentAssociation);
            }

            if (component is PSMAttribute)
            {
                requiresDeletion = this.ContainsKey(typeof(AttributeCardinalityChangedInstance)) &&
                       this[typeof(AttributeCardinalityChangedInstance)].Cast<AttributeCardinalityChangedInstance>().
                           Any(ci => ci.PSMAttribute == component && ci.RequiresDeletion);

                result |= this.ContainsKey(typeof(AttributeCardinalityChangedInstance)) &&
                       this[typeof(AttributeCardinalityChangedInstance)].Cast<AttributeCardinalityChangedInstance>().
                           Any(ci => ci.PSMAttribute == component);
            }

            if (component is PSMAssociationMember && ((PSMAssociationMember)component).ParentAssociation != null)
            {
                requiresDeletion = this.ContainsKey(typeof(AssociationCardinalityChangedInstance)) &&
                       this[typeof(AssociationCardinalityChangedInstance)].Cast<AssociationCardinalityChangedInstance>().
                           Any(ci => ci.PSMAssociation == ((PSMAssociationMember)component).ParentAssociation && ci.RequiresDeletion);
                
                result |= this.ContainsKey(typeof(AssociationCardinalityChangedInstance)) &&
                       this[typeof(AssociationCardinalityChangedInstance)].Cast<AssociationCardinalityChangedInstance>().
                           Any(ci => ci.PSMAssociation == ((PSMAssociationMember)component).ParentAssociation);
            }

            return result;
        }
    }

    public class ChangeDetector
    {
        private static Dictionary<Type, MethodInfo> testMethods = new Dictionary<Type, MethodInfo>();
        
        private static Dictionary<Type, MethodInfo> createInstanceMethods = new Dictionary<Type, MethodInfo>();

        private static Dictionary<EChangePredicateScope, List<Type>> _changePredicatesByScope;
        
        private static Dictionary<EChangePredicateScope, List<Type>> changePredicatesByScope
        {
            get
            {
                if (_changePredicatesByScope == null)
                {
                    _changePredicatesByScope = new Dictionary<EChangePredicateScope, List<Type>>();

                    foreach (Type type in typeof(ChangeInstance).Assembly.GetTypes())
                    {
                        if (!type.IsAbstract && type.IsSubclassOf(typeof(ChangeInstance)))
                        {
                            ChangePredicateScopeAttribute attribute =
                                (ChangePredicateScopeAttribute)type.GetCustomAttributes(typeof(ChangePredicateScopeAttribute), true)[0];

                            _changePredicatesByScope.CreateSubCollectionIfNeeded(attribute.Scope);
                            _changePredicatesByScope[attribute.Scope].Add(type);
                            testMethods[type] = type.GetMethod("TestCandidate", BindingFlags.Static | BindingFlags.Public);
                            createInstanceMethods[type] = type.GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public);
                        }
                    }
                }

                return _changePredicatesByScope;
            }
        }

        public DetectedChangeInstancesSet DetectChanges(PSMSchema schema1, PSMSchema schema2)
        {
            Version oldVersion = schema1.Version;
            Version newVersion = schema2.Version;
            Debug.Assert(oldVersion != newVersion);

            DetectedChangeInstancesSet changeInstancesSet = new DetectedChangeInstancesSet();
            changeInstancesSet.OldVersion = oldVersion;
            changeInstancesSet.NewVersion = newVersion;
            
            #region attributes 

            testConstructs(schema1.PSMAttributes, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMAttribute);
            testConstructs(schema2.PSMAttributes, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMAttribute);

            #endregion 

            #region associations 

            testConstructs(schema1.PSMAssociations, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMAssociation);
            testConstructs(schema2.PSMAssociations, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMAssociation);

            #endregion 

            #region classes

            testConstructs(schema1.PSMClasses, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMClass);
            testConstructs(schema2.PSMClasses, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMClass);

            #endregion 

            #region contentModels

            testConstructs(schema1.PSMContentModels, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMContentModel);
            testConstructs(schema2.PSMContentModels, oldVersion, newVersion, changeInstancesSet, EChangePredicateScope.PSMContentModel);

            #endregion 

            ClassifyNodes(changeInstancesSet, schema2);

            return changeInstancesSet;
        }

        private static void testConstructs<TPSMComponent>(IEnumerable<TPSMComponent> components, 
            Version oldVersion, Version newVersion, DetectedChangeInstancesSet changeInstancesSet, EChangePredicateScope scope) 
            where TPSMComponent : ExolutioObject
        {
            foreach (TPSMComponent component in components)
            {
                if (changePredicatesByScope.ContainsKey(scope))
                {
                    foreach (Type type in changePredicatesByScope[scope])
                    {
                        object[] testParams = new object[] { component, oldVersion, newVersion };
                        bool result = (bool)testMethods[type].Invoke(null, testParams);
                        if (result)
                        {
                            changeInstancesSet.CreateSubCollectionIfNeeded(type);
                            ChangeInstance instance = (ChangeInstance)createInstanceMethods[type].Invoke(null, testParams);
                            changeInstancesSet[type].Add(instance);
                        }
                    }
                }
            }
        }

        private void ClassifyNodes(DetectedChangeInstancesSet changeInstances, PSMSchema psmSchema)
        {
            FindRedNodes(changeInstances);

            #region blue and green nodes

            Queue<PSMAssociationMember> toDo = new Queue<PSMAssociationMember>();
            foreach (PSMAssociationMember m in ModelIterator.GetLeaves(psmSchema))
            {
                if (changeInstances.RedNodes.Contains(m))
                {
                    toDo.Enqueue(m);
                }
            }

            foreach (PSMAssociationMember redNode in changeInstances.RedNodes.OfType<PSMAssociationMember>())
            {
                toDo.Enqueue(redNode);
            }

            foreach (PSMAttribute psmAttribute in changeInstances.RedNodes.OfType<PSMAttribute>())
            {
                if (!changeInstances.RedNodes.Contains(psmAttribute.PSMClass))
                {
                    changeInstances.BlueNodes.AddIfNotContained(psmAttribute.PSMClass);
                    toDo.Enqueue(psmAttribute.PSMClass);
                }
            }

            while (!toDo.IsEmpty())
            {
                PSMAssociationMember m = toDo.Dequeue();
                if (m.ParentAssociation != null)
                {
                    if (!changeInstances.RedNodes.Contains(m.ParentAssociation.Parent)
                        && !changeInstances.BlueNodes.Contains(m.ParentAssociation.Parent))
                    {
                        changeInstances.BlueNodes.Add(m.ParentAssociation.Parent);
                        if (!(m.ParentAssociation.Parent is PSMSchemaClass))
                        {
                            toDo.Enqueue(m.ParentAssociation.Parent);
                        }
                    }
                }
            }

            changeInstances.GreenNodes.AddRange(psmSchema.SchemaComponents.Where(c => !(c is PSMAssociation) && !(c is PSMSchemaClass) &&
                !changeInstances.RedNodes.Contains(c) && !changeInstances.BlueNodes.Contains(c)).Cast<PSMComponent>());

            foreach (PSMComponent psmComponent in changeInstances.AllNodes)
            {
                if (!psmComponent.ExistsInVersion(changeInstances.OldVersion))
                {
                    changeInstances.AddedNodes.Add(psmComponent);
                }
            }

            #endregion
        }

        private void FindRedNodes(DetectedChangeInstancesSet changeInstances)
        {
            foreach (KeyValuePair<Type, List<ChangeInstance>> kvp in changeInstances)
            {
                foreach (ChangeInstance changeInstance in kvp.Value)
                {
                    PSMComponent node =
                        changeInstance.Component is PSMAssociation ?
                        ((PSMAssociation)changeInstance.Component).Parent :
                        changeInstance.Component;

                    if (changeInstance is IMigratoryChange)
                    {
                        FormerParentAsRedNode(changeInstances.RedNodes, changeInstance);
                        CurrentParentAsRedNode(changeInstances.RedNodes, changeInstance);
                    }
                    if (changeInstance is IAdditionChange)
                    {
                        changeInstances.RedNodes.AddIfNotContained(node);
                        CurrentParentAsRedNode(changeInstances.RedNodes, changeInstance);
                    }
                    if (changeInstance is IRemovalChange)
                    {
                        FormerParentAsRedNode(changeInstances.RedNodes, changeInstance);
                    }
                    if (changeInstance is ISedentaryChange)
                    {
                        if (changeInstance is IRenameChange)
                        {
                            
                            AssociationRenamedInstance associationRenamedInstance
                                = changeInstance as AssociationRenamedInstance;

                            AttributeRenamedInstance attributeRenamedInstance
                                = changeInstance as AttributeRenamedInstance;

                            if (attributeRenamedInstance != null)
                            {
                                changeInstances.RedNodes.AddIfNotContained(attributeRenamedInstance.PSMAttribute);
                            }

                            if (associationRenamedInstance != null)
                            {
                                changeInstances.RedNodes.AddIfNotContained(associationRenamedInstance.PSMAssociation.Child);
                                /*
                                 * again for technical reasons (in order to process the whole instance 
                                 * by the node template), the parent must be invalidated as well in
                                 * the case the name of the association was empty in the old version
                                 */
                                if (!associationRenamedInstance.ComponentOldVersion.IsNamed)
                                {
                                    FormerParentAsRedNode(changeInstances.RedNodes, changeInstance);
                                }
                            }                            
                        }
                        if (changeInstance is ICardinalityChange)
                        {
                            FormerParentAsRedNode(changeInstances.RedNodes, changeInstance);
                            {
                                /*
                                 * again for technical reasons (in order to process the whole instance 
                                 * by the node template), the parent must be invalidated as well 
                                 */
                                AssociationCardinalityChangedInstance associationRenamedInstance
                                    = changeInstance as AssociationCardinalityChangedInstance;
                                if (associationRenamedInstance != null
                                    && !associationRenamedInstance.ComponentOldVersion.IsNamed)
                                {
                                    FormerParentAsRedNode(changeInstances.RedNodes, changeInstance);
                                }
                            }
                        }
                        if (changeInstance is AttributeXFormChangedInstance)
                        {
                            FormerParentAsRedNode(changeInstances.RedNodes, changeInstance);
                            /*
                             * A to E and E to A convertor templates will be used for green nodes 
                             */ 
                            if (((AttributeXFormChangedInstance)changeInstance).NewXFormElement)
                            {
                                changeInstances.ConvertedAttributesAE.Add((PSMAttribute) changeInstance.Component);
                            }
                            else
                            {
                                changeInstances.ConvertedAttributesEA.Add((PSMAttribute) changeInstance.Component);
                            }
                        }
                        if (changeInstance is AttributeTypeChangedInstance)
                        {
                            changeInstances.RedNodes.AddIfNotContained(node);
                            FormerParentAsRedNode(changeInstances.RedNodes, changeInstance);
                        }
                        if (changeInstance is ContentModelTypeChangedInstance)
                        {
                            changeInstances.RedNodes.AddIfNotContained(node);
                            /* 
                             * technically this is required so the whole instance 
                             * of a content model is processed as a whole 
                             */
                            CurrentParentAsRedNode(changeInstances.RedNodes, changeInstance);
                        }
                    }
                }
            }

            /* parent node of every red group node is marked red */
            List<PSMComponent> redNodesCopy = changeInstances.RedNodes.ToList();
            foreach (PSMComponent c in redNodesCopy)
            {
                if (changeInstances.IsGroupNode(c))
                {
                    CurrentParentAsRedNode(changeInstances.RedNodes, c);
                }
            }
        }

        private void CurrentParentAsRedNode(List<PSMComponent> redNodes, PSMComponent componentNewVersion)
        {
            PSMComponent currentParent =
                componentNewVersion is PSMAssociation ? ((PSMAssociation)componentNewVersion).Parent : ModelIterator.GetPSMParent(componentNewVersion, true);

            if (currentParent != null)
            {
                redNodes.AddIfNotContained(currentParent);
            }
        }

        private void CurrentParentAsRedNode(List<PSMComponent> redNodes, ChangeInstance changeInstance)
        {
            PSMComponent componentNewVersion = changeInstance is IExistingComponentChange ?
                (PSMComponent)((IExistingComponentChange)changeInstance).ComponentNewVersion :
                (PSMComponent)((IAdditionChange)changeInstance).ComponentNewVersion;
            CurrentParentAsRedNode(redNodes, componentNewVersion);
        }

        private void FormerParentAsRedNode(List<PSMComponent> redNodes, ChangeInstance changeInstance)
        {
            PSMComponent componentOldVersion = changeInstance is IExistingComponentChange ?
                (PSMComponent)((IExistingComponentChange)changeInstance).ComponentOldVersion :
                (PSMComponent)((IRemovalChange)changeInstance).ComponentOldVersion;

            PSMComponent formerParent =
                componentOldVersion is PSMAssociation ? ((PSMAssociation)componentOldVersion).Parent : ModelIterator.GetPSMParent(componentOldVersion, true);

            PSMComponent formerParentNewVersion = formerParent.GetInVersion(changeInstance.NewVersion);
            if (formerParentNewVersion != null)
            {
                redNodes.AddIfNotContained(formerParentNewVersion);
            }
        }
    }
}