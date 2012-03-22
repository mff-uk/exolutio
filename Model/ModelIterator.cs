using System;
using System.Collections.Generic;
using System.Diagnostics;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using System.Linq;
using Exolutio.SupportingClasses;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Model
{
    public static class ModelIterator
    {
        public class MoveStep
        {
            public enum MoveStepType
            {
                StructuralRepresentant,
                Subtree,
                None
            }

            public MoveStepType StepType;
            public PSMAssociationMember StepTarget;
        }

        private static List<PSMClass> visited;
        private static List<PSMAssociationMember> visitedAM;
        
        public static IEnumerable<Tuple<PSMAttribute, IEnumerable<MoveStep>>> GetContextPSMAttributesWithPaths(this PSMClass psmClass)
        {
            visited = new List<PSMClass>();
            return psmClass.getContextPSMAttributesWithPaths();
        }

        private static List<Tuple<PSMAttribute, IEnumerable<MoveStep>>> getContextPSMAttributesWithPaths(this PSMClass psmClass)
        {
            List<Tuple<PSMAttribute, IEnumerable<MoveStep>>> list = new List<Tuple<PSMAttribute, IEnumerable<MoveStep>>>();
            if (visited.Contains(psmClass)) return list;
            else visited.Add(psmClass);

            list.AddRange(psmClass.PSMAttributes.Select(a => new Tuple<PSMAttribute, IEnumerable<MoveStep>>(a, Enumerable.Repeat(new MoveStep() { StepType = MoveStep.MoveStepType.None, StepTarget = psmClass }, 1))));
            foreach (PSMClass c in psmClass.GetSRs())
            {
                list.AddRange(c.getContextPSMAttributesWithPaths()
                    .Select(t => new Tuple<PSMAttribute, IEnumerable<MoveStep>>(t.Item1, new List<MoveStep>(t.Item2).Concat(Enumerable.Repeat(new MoveStep() { StepType = MoveStep.MoveStepType.StructuralRepresentant, StepTarget = psmClass }, 1))))
                    );
            }
            foreach (PSMClass c in psmClass.UnInterpretedSubClasses())
            {
                list.AddRange(c.getContextPSMAttributesWithPaths()
                    .Select(t => new Tuple<PSMAttribute, IEnumerable<MoveStep>>(t.Item1, new List<MoveStep>(t.Item2).Concat(Enumerable.Repeat(new MoveStep() { StepType = MoveStep.MoveStepType.Subtree, StepTarget = psmClass }, 1))))
                    );
            }
            return list;
        }

        public static IEnumerable<Tuple<PSMAssociation, IEnumerable<MoveStep>>> GetContextPSMAssociationsWithPaths(this PSMClass psmClass)
        {
            visited = new List<PSMClass>();
            return psmClass.getContextPSMAssociationsWithPaths();
        }

        private static List<Tuple<PSMAssociation, IEnumerable<MoveStep>>> getContextPSMAssociationsWithPaths(this PSMClass psmClass)
        {
            List<Tuple<PSMAssociation, IEnumerable<MoveStep>>> list = new List<Tuple<PSMAssociation, IEnumerable<MoveStep>>>();
            if (visited.Contains(psmClass)) return list;
            else visited.Add(psmClass);
            
            list.AddRange(psmClass.ChildPSMAssociations.Select(a => new Tuple<PSMAssociation, IEnumerable<MoveStep>>(a, Enumerable.Repeat(new MoveStep() { StepType = MoveStep.MoveStepType.None, StepTarget = psmClass }, 1))));
            foreach (PSMClass c in psmClass.GetSRs())
            {
                list.AddRange(c.getContextPSMAssociationsWithPaths()
                    .Select(t => new Tuple<PSMAssociation, IEnumerable<MoveStep>>(t.Item1, new List<MoveStep>(t.Item2).Concat(Enumerable.Repeat(new MoveStep() { StepType = MoveStep.MoveStepType.StructuralRepresentant, StepTarget = psmClass }, 1))))
                    );
            }
            foreach (PSMClass c in psmClass.UnInterpretedSubClasses())
            {
                list.AddRange(c.getContextPSMAssociationsWithPaths()
                    .Select(t => new Tuple<PSMAssociation, IEnumerable<MoveStep>>(t.Item1, new List<MoveStep>(t.Item2).Concat(Enumerable.Repeat(new MoveStep() { StepType = MoveStep.MoveStepType.Subtree, StepTarget = psmClass }, 1))))
                    );
            }
            return list;
        }

        public static IEnumerable<PSMAttribute> GetContextPSMAttributes(this PSMClass psmClass, bool leaveOutRepresentedClass = false)
        {
            visited = new List<PSMClass>();
            return psmClass.getContextPSMAttributes(leaveOutRepresentedClass);
        }

        private static IEnumerable<PSMAttribute> getContextPSMAttributes(this PSMClass psmClass, bool leaveOutRepresentedClass)
        {
            List<PSMAttribute> list = new List<PSMAttribute>();
            if (visited.Contains(psmClass)) return list;
            else visited.Add(psmClass);

            list.AddRange(psmClass.PSMAttributes);
            
            IEnumerable<PSMClass> classes;
            if (leaveOutRepresentedClass) classes = psmClass.UnInterpretedSubClasses();
            else classes = psmClass.GetSRs().Union(psmClass.UnInterpretedSubClasses());
            
            foreach (PSMClass c in classes.Where(c => !visited.Contains(c)))
            {
                list.AddRange(c.getContextPSMAttributes(false));
            }
            return list;
        }

        public static IEnumerable<PSMAssociation> GetContextPSMAssociations(this PSMAssociationMember psmAM, bool leaveOutRepresentedClass = false)
        {
            visitedAM = new List<PSMAssociationMember>();
            return psmAM.getContextPSMAssociations(leaveOutRepresentedClass);
        }

        private static IEnumerable<PSMAssociation> getContextPSMAssociations(this PSMAssociationMember psmAM, bool leaveOutRepresentedClass)
        {
            List<PSMAssociation> list = new List<PSMAssociation>();
            if (visitedAM.Contains(psmAM)) return list;
            else visitedAM.Add(psmAM);

            list.AddRange(psmAM.ChildPSMAssociations);

            IEnumerable<PSMAssociationMember> associationMembers;
            if (leaveOutRepresentedClass) associationMembers = psmAM.UnInterpretedSubAMs(false, true);
            else if (!(psmAM is PSMClass)) associationMembers = psmAM.UnInterpretedSubAMs(false, true);
            else associationMembers = (psmAM as PSMClass).GetSRs().Union(psmAM.UnInterpretedSubAMs(false, true));

            foreach (PSMAssociationMember c in associationMembers.Where(c => !visitedAM.Contains(c)))
            {
                list.AddRange(c.getContextPSMAssociations(false));
            }
            return list;
        }

        public static IEnumerable<PSMAttribute> GetActualPSMAttributes(this PSMClass psmClass)
        {
            List<PSMAttribute> list = new List<PSMAttribute>();

            list.AddRange(psmClass.PSMAttributes);
            foreach (PSMClass c in psmClass.GetSRs())
            {
                list.AddRange(c.PSMAttributes);
            }
            return list;
        }

        public static IEnumerable<PSMAttribute> GetActualPSMAttributesIncludingInherited(this PSMClass psmClass)
        {
            List<PSMAttribute> list = new List<PSMAttribute>();

            list.AddRange(psmClass.PSMAttributes);
            //TODO: Avoid double-cycle?
            if (psmClass.RepresentedClass != null) list.AddRange(psmClass.RepresentedClass.GetActualPSMAttributesIncludingInherited());
            if (psmClass.GeneralizationAsSpecific != null) list.AddRange(psmClass.GeneralizationAsSpecific.General.GetActualPSMAttributesIncludingInherited());
            return list;
        }

        public static IEnumerable<PSMAttribute> GetPSMAttributesOfRepresentedClasses(this PSMClass psmClass)
        {
            List<PSMAttribute> list = new List<PSMAttribute>();

            list.AddRange(psmClass.PSMAttributes);
            foreach (PSMClass c in psmClass.GetSRs())
            {
                list.AddRange(c.PSMAttributes);
            }
            return list;
        }

        public static IEnumerable<PSMAssociation> GetActualChildPSMAssociations(this PSMClass psmClass)
        {
            List<PSMAssociation> list = new List<PSMAssociation>();

            list.AddRange(psmClass.ChildPSMAssociations);
            foreach (PSMClass c in psmClass.GetSRs())
            {
                list.AddRange(c.ChildPSMAssociations);
            }
            return list;
        }

        public static IEnumerable<PSMAssociation> GetActualChildPSMAssociationsIncludingInherited(this PSMClass psmClass)
        {
            List<PSMAssociation> list = new List<PSMAssociation>();

            list.AddRange(psmClass.ChildPSMAssociations);
            //TODO: Avoid double-cycle?
            if (psmClass.RepresentedClass != null) list.AddRange(psmClass.RepresentedClass.GetActualChildPSMAssociationsIncludingInherited());
            if (psmClass.GeneralizationAsSpecific != null) list.AddRange(psmClass.GeneralizationAsSpecific.General.GetActualChildPSMAssociationsIncludingInherited());
            return list;
        }

        public static IEnumerable<ExolutioObject> GetAllModelItems(ProjectVersion projectVersion)
        {
            IEnumerable<ExolutioObject> result = projectVersion.PIMAttributeTypes.Cast<ExolutioObject>();
                
            result = result.Concat(new[] {projectVersion.PIMSchema});

            result = result.Concat(GetPIMComponents(projectVersion.PIMSchema).Cast<ExolutioObject>());

            foreach (PSMSchema psmSchema in projectVersion.PSMSchemas)
            {
                result = result.Concat(new ExolutioObject[] { psmSchema });
                result = result.Concat(GetPSMComponents(psmSchema).Cast<ExolutioObject>());
            }

            result = result.Concat(projectVersion.PIMDiagrams.Cast<ExolutioObject>());
            result = result.Concat(projectVersion.PSMDiagrams.Cast<ExolutioObject>());

            return result;
        }

        public static IEnumerable<PSMComponent> GetPSMComponents(PSMSchema psmSchema)
        {
            IEnumerable<PSMComponent> result;
            if (psmSchema.PSMSchemaClass != null)
                result = new[] { psmSchema.PSMSchemaClass };
            else
                result = new PSMComponent[0];
           
            result = result.Concat(psmSchema.PSMClasses.Cast<PSMComponent>());
            result = result.Concat(psmSchema.PSMContentModels.Cast<PSMComponent>());
            result = result.Concat(psmSchema.PSMAssociations.Cast<PSMComponent>());
            result = result.Concat(psmSchema.PSMAttributes.Cast<PSMComponent>());
            return result;
        }

        public static IEnumerable<PIMComponent> GetPIMComponents(PIMSchema pimSchema)
        {
            IEnumerable<PIMComponent> result;
            result = pimSchema.PIMClasses.Cast<PIMComponent>();
            result = result.Concat(pimSchema.PIMAttributes.Cast<PIMComponent>());
            result = result.Concat(pimSchema.PIMAssociations.Cast<PIMComponent>());
            result = result.Concat(pimSchema.PIMAssociationEnds.Cast<PIMComponent>());
            result = result.Concat(pimSchema.PIMGeneralizations.Cast<PIMComponent>());
            return result;
        }

        public static IEnumerable<PSMAssociationMember> GetChildNodes(PSMAssociationMember parentNode)
        {
            return parentNode.ChildPSMAssociations.Select(association => association.Child);
        }

        public static IEnumerable<PSMAssociationMember> GetLabeledChildNodes(PSMAssociationMember parentNode)
        {
            return parentNode.ChildPSMAssociations.Where(association => association.IsNamed && association.Child is PSMClass)
                .Select(association => association.Child);
        }
        
        public static PSMComponent GetPSMParent(PSMComponent psmComponent, bool returnClassForAttributes = false)
        {
            if (psmComponent is PSMAttribute && returnClassForAttributes)
            {
                return (((PSMAttribute)psmComponent).PSMClass);
            }
            if (psmComponent is PSMAssociationMember && ((PSMAssociationMember)psmComponent).ParentAssociation != null)
            {
                return (((PSMAssociationMember)psmComponent).ParentAssociation.Parent);
            }
            return null;
        }

        public static IEnumerable<PSMComponent> GetPSMChildren(PSMComponent component, bool returnAttributesForClass = false, bool returnChildAssociationsForAssociationMembers = false)
        {
            IEnumerable<PSMComponent> result = new PSMComponent[0];
            if (component is PSMClass && ((PSMClass)component).PSMAttributes.Count > 0 && returnAttributesForClass)
            {
                result = result.Concat(((PSMClass)component).PSMAttributes.Cast<PSMComponent>());
            }
            if (component is PSMAssociationMember && ((PSMAssociationMember)component).ChildPSMAssociations.Count > 0)
            {
                if (returnChildAssociationsForAssociationMembers)
                {
                    result = result.Concat(((PSMAssociationMember)component).ChildPSMAssociations.Cast<PSMComponent>());
                }
                else
                {
                    result = result.Concat(((PSMAssociationMember)component).ChildPSMAssociations.Select(association => association.Child).Cast<PSMComponent>());    
                }
            }
            if (component is PSMAssociation)
            {
                result = result.Concat(new PSMComponent[]{ ((PSMAssociation) component).Child });
            }
            return result;
        }

        public static IEnumerable<PSMComponent> GetPSMChildComponentsRecursive(this PSMComponent component, bool returnAttributesForClass = false, bool includeSelf = false)
        {
            IEnumerable<PSMComponent> result = new PSMComponent[0];
            if (includeSelf) result = result.Concat(Enumerable.Repeat(component, 1));
            if (component is PSMClass && ((PSMClass)component).PSMAttributes.Count > 0 && returnAttributesForClass)
            {
                result = result.Concat(((PSMClass)component).PSMAttributes.Cast<PSMComponent>());
            }
            if (component is PSMAssociationMember)
            {
                foreach (PSMAssociation a in (component as PSMAssociationMember).ChildPSMAssociations)
                {
                    result = result.Concat(GetPSMChildComponentsRecursive(a, returnAttributesForClass, true));
                }
            }
            if (component is PSMAssociation)
            {
                if ((component as PSMAssociation).Child != null)
                    result = result.Concat(GetPSMChildComponentsRecursive((component as PSMAssociation).Child, returnAttributesForClass, true));
            }
            return result;
        }

        public static bool HasPSMChildren(PSMComponent psmComponent, bool countAttributesAsClassChildren = false)
        {
            if (psmComponent is PSMClass && ((PSMClass)psmComponent).PSMAttributes.Count > 0 && countAttributesAsClassChildren)
            {
                return true;
            }
            if (psmComponent is PSMAssociationMember && ((PSMAssociationMember)psmComponent).ChildPSMAssociations.Count > 0)
            {
                return true;
            }

            else return false;   
        }

        public static bool IsDescendantFrom(this PSMComponent child, PSMComponent parent)
        {
            if (parent == null || child == null || child == parent) return false;
            PSMAssociation parentA;
            PSMAssociationMember parentM;
            if (child is PSMAssociation)
            {
                parentA = child as PSMAssociation;
            }
            else if (child is PSMAssociationMember)
            {
                parentA = (child as PSMAssociationMember).ParentAssociation;
            }
            else //PSMAttribute
            {
                if ((child as PSMAttribute).PSMClass == parent) return true;
                parentA = (child as PSMAttribute).PSMClass.ParentAssociation;
            }
            if (parentA == null) return false;
            if (parentA == parent) return true;
            parentM = parentA.Parent;
            if (parentM == parent) return true;

            while (parentA != parent && parentM != null && parentM != parent)
            {
                parentA = parentM.ParentAssociation;
                if (parentA != null) parentM = parentA.Parent;
                else return false;
            }

            if (parentA == parent) return true;
            if (parentM == null) return false;
            if (parentM == parent) return true;

            Debug.Assert(true, "Error, this should be unreachable");
            return false;
        }

        public static PSMAssociationMember GetNearestCommonAncestor(this PSMComponent component1, PSMComponent component2)
        {
            PSMAssociationMember am1;
            if (component1 is PSMAttribute)
                am1 = ((PSMAttribute)component1).PSMClass;
            else if (component1 is PSMAssociationMember)
                am1 = (PSMAssociationMember)component1;
            else
                throw new NotImplementedException();

            PSMAssociationMember am2;
            if (component2 is PSMAttribute)
                am2 = ((PSMAttribute)component2).PSMClass;
            else if (component2 is PSMAssociationMember)
                am2 = (PSMAssociationMember)component2;
            else
                throw new NotImplementedException();

            return GetNearestCommonAncestorAssociationMember(am1, am2);
        }

        public static PSMAssociationMember GetNearestCommonAncestorAssociationMember(this PSMAssociationMember component1, PSMAssociationMember component2)
        {
            IEnumerable<PSMAssociationMember> path1 = component1.GetPathToRoot();
            IEnumerable<PSMAssociationMember> path2 = component2.GetPathToRoot();

            IEnumerator<PSMAssociationMember> current = path1.GetEnumerator();
            while (!path2.Contains(current.Current))
            {
                if (!current.MoveNext()) return null;
            }
            return current.Current;
        }

        public static PSMClass GetNearestCommonAncestorClass(this PSMAssociationMember component1, PSMAssociationMember component2)
        {
            IEnumerable<PSMClass> path1 = component1.GetClassPathToRoot();
            IEnumerable<PSMClass> path2 = component2.GetClassPathToRoot();

            IEnumerator<PSMClass> current = path1.GetEnumerator();
            while (!path2.Contains(current.Current))
            {
                if (!current.MoveNext()) return null;
            }
            return current.Current;
        }

        /// <summary>
        /// Returns PSMAssociationMember path from start to root including start
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static List<PSMAssociationMember> GetPathToRoot(this PSMAssociationMember start)
        {
            List<PSMAssociationMember> path = new List<PSMAssociationMember>();
            if (start == null) return path;
            PSMAssociationMember current = start;
            path.Add(current);
            while (current.ParentAssociation != null)
            {
                if (current.ParentAssociation == null) break;
                else current = current.ParentAssociation.Parent;
                path.Add(current);
            }

            return path;
        }

        public static PSMComponent GetFirstAncestorOrSelfExistingInVersion(this PSMComponent start, Version version)
        {
            if (start.ExistsInVersion(version))
            {
                return start;
            }
            if (start is PSMAttribute)
            {
                start = ((PSMAttribute) start).PSMClass;
            }
            List<PSMAssociationMember> psmAssociationMembers = GetPathToRoot((PSMAssociationMember) start);
            foreach (PSMAssociationMember psmAssociationMember in psmAssociationMembers)
            {
                if (psmAssociationMember.ExistsInVersion(version))
                {
                    return psmAssociationMember;
                }
            }
            return null;
        }

        public static List<PSMClass> GetClassPathToRoot(this PSMAssociationMember start)
        {
            List<PSMClass> path = new List<PSMClass>();
            if (start == null) return path;

            if (start is PSMClass) path.Add(start as PSMClass);

            PSMClass current = start.NearestParentClass();
            while (current != null)
            {
                path.Add(current);
                current = current.NearestParentClass();
            }

            return path;
        }
        
        public static PSMClass NearestParentClass(this PSMAssociationMember child)
        {
            PSMAssociation currentaA = child.ParentAssociation;
            if (currentaA == null) return null;
            PSMAssociationMember current = currentaA.Parent;
            if (current == null) return null;
            while (!(current is PSMClass))
            {
                currentaA = current.ParentAssociation;
                if (currentaA == null) return null;
                current = currentaA.Parent;
                if (current == null) return null;
            }
            return current as PSMClass;
        }

        //Redo using NearestParentClass
        public static PSMClass NearestInterpretedParentClass(this PSMAssociationMember child)
        {
            PSMAssociation currentaA = child.ParentAssociation;
            if (currentaA == null) return null;
            PSMAssociationMember current = currentaA.Parent;
            if (current == null) return null;
            while (!(current is PSMClass && (current as PSMClass).Interpretation != null))
            {
                currentaA = current.ParentAssociation;
                if (currentaA == null) return null;
                current = currentaA.Parent;
                if (current == null) return null;
            }
            return current as PSMClass;
        }

        public static PSMClass NearestInterpretedClass(this PSMAssociation child)
        {
            if (child.Parent == null) return null;
            if (child.Parent is PSMClass && (child.Parent as PSMClass).Interpretation != null) return child.Parent as PSMClass;
            else return NearestInterpretedParentClass(child.Parent);
        }

        public static PSMClass NearestInterpretedClass(this PSMAssociationMember child)
        {
            if (child is PSMClass && child.Interpretation != null) return (child as PSMClass);
            return NearestInterpretedParentClass(child);
        }

        public static PSMClass NearestInterpretedClass(this PSMAttribute attribute)
        {
            if (attribute.PSMClass == null) return null;
            return attribute.PSMClass.NearestInterpretedClass();
        }

        private static List<PSMClass> InterpretedSubClasses(this PSMAssociationMember parent)
        {
            List<PSMClass> list = new List<PSMClass>();
            if (parent is PSMClass && (parent as PSMClass).Interpretation != null) list.Add(parent as PSMClass);
            else
                foreach (PSMAssociation a in parent.ChildPSMAssociations)
                {
                    if (a.Child != null) list.AddRange(a.Child.InterpretedSubClasses());
                }
            return list;
        }
        
        /// <summary>
        /// Returns a list of PSMClasses which form the content of psmClass. If psmClass is SR of A, A is SR of B, it returns A, B.
        /// </summary>
        /// <param name="psmClass"></param>
        /// <returns></returns>
        public static List<PSMClass> GetSRs(this PSMClass psmClass)
        {
            List<PSMClass> srs = new List<PSMClass>();

            if (psmClass.RepresentedClass == null) return srs;

            srs.Add(psmClass.RepresentedClass);
            srs.AddRange(psmClass.RepresentedClass.GetSRs());

            return srs;
        }

        /// <summary>
        /// Returns a list of PIMClasses more general than pimClass. If pimClass has a general class A, A has a general class B, it returns A, B.
        /// </summary>
        /// <param name="pimClass"></param>
        /// <returns></returns>
        public static List<PIMClass> GetGeneralClasses(this PIMClass pimClass)
        {
            List<PIMClass> general = new List<PIMClass>();

            if (pimClass.GeneralizationAsSpecific == null) return general;

            PIMClass generalClass = pimClass.GeneralizationAsSpecific.General;
            general.Add(generalClass);
            general.AddRange(generalClass.GetGeneralClasses());

            return general;
        }

        public static List<PIMGeneralization> GetGeneralizationPathTo(this PIMClass specific, PIMClass general)
        {
            List<PIMGeneralization> list = new List<PIMGeneralization>();

            if (specific.GeneralizationAsSpecific == null || specific == general) return list;

            list.Add(specific.GeneralizationAsSpecific);
            list.AddRange(specific.GeneralizationAsSpecific.General.GetGeneralizationPathTo(general));

            return list;
        }

        public static List<PIMClass> GetSpecificClasses(this PIMClass pimClass, bool includeThis = false)
        {
            List<PIMClass> specific = new List<PIMClass>();

            if (includeThis) specific.Add(pimClass);

            foreach (PIMGeneralization g in pimClass.GeneralizationsAsGeneral)
            {
                specific.AddRange(g.Specific.GetSpecificClasses(true));
            }

            return specific;
        }

        /// <summary>
        /// Returns a list of PSMClasses more general than psmClass. If psmClass has a general class A, A has a general class B, it returns A, B.
        /// </summary>
        /// <param name="psmClass"></param>
        /// <returns></returns>
        public static List<PSMClass> GetGeneralClasses(this PSMClass psmClass)
        {
            List<PSMClass> general = new List<PSMClass>();

            if (psmClass.GeneralizationAsSpecific == null) return general;

            PSMClass generalClass = psmClass.GeneralizationAsSpecific.General;
            general.Add(generalClass);
            general.AddRange(generalClass.GetGeneralClasses());

            return general;
        }

        public static List<PSMGeneralization> GetGeneralizationPathTo(this PSMClass specific, PSMClass general)
        {
            List<PSMGeneralization> list = new List<PSMGeneralization>();

            if (specific.GeneralizationAsSpecific == null || specific == general) return list;

            list.Add(specific.GeneralizationAsSpecific);
            list.AddRange(specific.GeneralizationAsSpecific.General.GetGeneralizationPathTo(general));

            return list;
        }

        public static List<PSMClass> InterpretedSubClasses(this PSMClass parent)
        {
            List<PSMClass> list = new List<PSMClass>();
            foreach (PSMAssociation a in parent.ChildPSMAssociations)
            {
                if (a.Child != null) list.AddRange(a.Child.InterpretedSubClasses());
            }
            return list;
        }

        public static List<PSMClass> UnInterpretedSubClasses(this PSMAssociationMember parent, bool includeThis = false)
        {
            List<PSMClass> list = new List<PSMClass>();
            if (parent is PSMClass && (parent as PSMClass).Interpretation != null) return list;
            else
            {
                if (parent is PSMClass && includeThis) list.Add(parent as PSMClass);
                foreach (PSMAssociation a in parent.ChildPSMAssociations)
                {
                    if (a.Child != null) list.AddRange(a.Child.UnInterpretedSubClasses(true));
                }
            }
            return list;
        }

        public static List<PSMAssociationMember> UnInterpretedSubAMs(this PSMAssociationMember parent, bool includeThis = false, bool first = false)
        {
            List<PSMAssociationMember> list = new List<PSMAssociationMember>();
            if (!first && parent is PSMClass && (parent as PSMClass).Interpretation != null) return list;
            else
            {
                if (includeThis) list.Add(parent as PSMAssociationMember);
                foreach (PSMAssociation a in parent.ChildPSMAssociations)
                {
                    if (a.Child != null) list.AddRange(a.Child.UnInterpretedSubAMs(true));
                }
            }
            return list;
        }

        public static List<PSMClass> UnInterpretedSubClasses(this PSMClass parent, bool includeThis = false)
        {
            List<PSMClass> list = new List<PSMClass>();
            if (includeThis && parent.Interpretation == null) list.Add(parent as PSMClass);
            foreach (PSMAssociation a in parent.ChildPSMAssociations)
            {
                if (a.Child != null) list.AddRange(a.Child.UnInterpretedSubClasses(true));
            }
            return list;
        }

        public static IEnumerable<PIMAssociation> GetAssociationsWithIncludeInherited(this PIMClass class1, PIMClass class2)
        {
            List<PIMAssociation> list1 = new List<PIMAssociation>();
            List<PIMAssociation> list2 = new List<PIMAssociation>();
            list1.AddRange(class1.PIMAssociationEnds.Select(ae => ae.PIMAssociation));
            list2.AddRange(class2.PIMAssociationEnds.Select(ae => ae.PIMAssociation));
            list1.AddRange(class1.GetGeneralClasses().SelectMany(gc => gc.PIMAssociationEnds.Select(ae => ae.PIMAssociation)));
            list2.AddRange(class2.GetGeneralClasses().SelectMany(gc => gc.PIMAssociationEnds.Select(ae => ae.PIMAssociation)));
            return list1.Intersect(list2);
        }

        public static List<PIMAttribute> GetInheritedAttributes(this PIMClass pimClass)
        {
            List<PIMAttribute> list = new List<PIMAttribute>();

            foreach (PIMClass c in pimClass.GetGeneralClasses())
                list.AddRange(c.PIMAttributes);
            
            return list;
        }

        public static List<PIMAssociationEnd> GetInheritedAssociationEnds(this PIMClass pimClass)
        {
            List<PIMAssociationEnd> list = new List<PIMAssociationEnd>();

            foreach (PIMClass c in pimClass.GetGeneralClasses())
                list.AddRange(c.PIMAssociationEnds);

            return list;
        }

        public static IEnumerable<PIMAssociation> GetAssociationsWith(this PIMClass class1, PIMClass class2)
        {
            if (class1 == class2)
            {
                return class1.PIMAssociationEnds.Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation).Where<PIMAssociation>(a => a.PIMClasses.Distinct<PIMClass>().Count<PIMClass>() == 1);
            }
            else return class1.PIMAssociationEnds.Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation)
                .Intersect<PIMAssociation>(
                class2.PIMAssociationEnds.Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation)
                );
        }

        public static Diagram GetDiagramForComponent(Component component)
        {
            ProjectVersion projectVersion;
            if (component.Project.UsesVersioning)
            {
                projectVersion = component.Project.GetProjectVersion(component.Version);                
            }
            else
            {
                projectVersion = component.Project.SingleVersion;
                
            }

            if (component is PIMAttribute)
                component = ((PIMAttribute) component).PIMClass;
            if (component is PSMAttribute)
                component = ((PSMAttribute) component).PSMClass;

            if (component is PIMComponent)
            {
                return projectVersion.PIMDiagrams.FirstOrDefault(d => d.PIMComponents.Contains((PIMComponent)component));
            }
            else
            {
                return projectVersion.PSMDiagrams.FirstOrDefault(d => d.PSMComponents.Contains((PSMComponent)component));
            }
        }

        public static IEnumerable<PSMAssociationMember> GetLeaves(PSMSchema psmSchema)
        {
            return psmSchema.SchemaComponents.OfType<PSMAssociationMember>().Where(m => m.ChildPSMAssociations.Count == 0);
        }

        public static IEnumerable<PSMComponent> OrderBFS(IList<PSMComponent> psmComponents)
        {
            IList<PSMComponent> result = new List<PSMComponent>();
            if (psmComponents.Count > 0)
            {
                foreach (PSMAssociationMember root in psmComponents.First().PSMSchema.Roots)
                {
                    Queue<PSMComponent> queue = new Queue<PSMComponent>();
                    queue.Enqueue(root);

                    while (queue.Count > 0)
                    {
                        PSMComponent psmComponent = queue.Dequeue();

                        if (psmComponents.Contains(psmComponent))
                        {
                            result.Add(psmComponent);
                        }

                        IEnumerable<PSMComponent> psmChildren = GetPSMChildren(psmComponent, true);
                        foreach (PSMComponent psmChild in psmChildren)                        
                        {
                            if (psmChild is PSMAssociationMember)
                            {
                                PSMAssociationMember psmChildAM = (PSMAssociationMember)psmChild;
                                if (psmChildAM.ParentAssociation != null && psmChildAM.ParentAssociation.Parent == psmComponent)
                                {
                                    queue.Enqueue(psmChild);
                                }
                            }
                        }
                    }
                }
            }
            return result; 
        }

        public static IList<PSMClass> GetReferencingStructuralRepresentatives(this PSMClass node, bool transitive)
        {
            if (!transitive)
            {
                return GetReferencingStructuralRepresentatives(node);
            }

            Queue<PSMClass> todo = new Queue<PSMClass>();
            List<PSMClass> result = new List<PSMClass>();

            todo.Enqueue(node);
            while (todo.Count > 0)
            {
                PSMClass member = todo.Dequeue();
                if (!result.Contains(member))
                {
                    result.Add(member);
                    foreach (PSMClass representative in GetReferencingStructuralRepresentatives(member))
                    {
                        todo.Enqueue(representative);    
                    }
                }
            }

            result.Remove(node);
            return result;  
        }

        private static IList<PSMClass> GetReferencingStructuralRepresentatives(PSMClass node)
        {
            PIMClass pim = (PIMClass) node.Interpretation;
            List<PSMClass> result = new List<PSMClass>();
            if (pim != null)
            {
                foreach (PSMSchema psmSchema in node.ProjectVersion.PSMSchemas)
                {
                    foreach (PSMClass psmClass in psmSchema.PSMClasses)
                    {
                        if (psmClass.IsStructuralRepresentative &&
                            psmClass.RepresentedClass == node)
                        {
                            result.Add(psmClass);
                        }
                    }
                }
            }
            return result; 
        }

        public static bool PropagatesToChoice(this PSMAssociationMember node)
        {
            if (node is PSMContentModel && ((PSMContentModel)node).Type.IsAmong(PSMContentModelType.Choice, PSMContentModelType.Set))
                return true; 

            if (node.ParentAssociation == null)
            {
                return false;
            }
            else
            {
                if (node.ParentAssociation.IsNamed)
                {
                    return false; 
                }
                else
                {
                    return PropagatesToChoice((PSMAssociationMember) GetPSMParent(node));
                }

            }
        }

        public static void ExpandInlinedNode(PSMComponent node, ref List<PSMComponent> result, Func<PSMComponent, bool> groupNodeTest, int ? maxRecursion = null)
        {
            if (groupNodeTest(node) && (!maxRecursion.HasValue || maxRecursion.Value > 0))
            {
                foreach (PSMComponent groupMember in GetPSMChildren(node, true, false))
                {
                    ExpandInlinedNode(groupMember, ref result, groupNodeTest, maxRecursion.HasValue ? maxRecursion.Value - 1 : (int?) null );
                }
            }
            else
            {
                result.Add(node);
            }
        }


        public static IEnumerable<PSMClass> GetAncestors(PSMClass psmClass)
        {
            if (psmClass.GeneralizationAsSpecific != null)
            {
                return psmClass.GeneralizationAsSpecific.General.Closure(c => c.GeneralizationAsSpecific != null ? c.GeneralizationAsSpecific.General : null);
            }
            else
            {
                return new PSMClass[0];
            }
        }

        public static IEnumerable<PSMClass> GetAncestorsWithSelf(PSMClass psmClass)
        {
            return psmClass.Closure(c => c.GeneralizationAsSpecific != null ? c.GeneralizationAsSpecific.General : null);
        }

        public static IEnumerable<PIMClass> GetPIMClassesInheritanceBFS(PIMSchema pimSchema)
        {
            IEnumerable<PIMClass> inheritanceRoots = pimSchema.PIMClasses.Where(c => c.GeneralizationAsSpecific == null);
            return inheritanceRoots.Closure(parent => parent.GeneralizationsAsGeneral.Select(gen => gen.Specific));
        }

        public static IEnumerable<PSMClass> GetPSMClassesInheritanceBFS(PSMSchema psmSchema)
        {
            IEnumerable<PSMClass> inheritanceRoots = psmSchema.PSMClasses.Where(c => c.GeneralizationAsSpecific == null);
            return inheritanceRoots.Closure(parent => parent.GeneralizationsAsGeneral.Select(gen => gen.Specific));
        }
    }
}