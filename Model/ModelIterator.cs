using System;
using System.Collections.Generic;
using System.Diagnostics;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using System.Linq;

namespace Exolutio.Model
{
    public static class ModelIterator
    {
        public static IEnumerable<ExolutioObject> GetAllModelItems(ProjectVersion projectVersion)
        {
            IEnumerable<ExolutioObject> result = projectVersion.AttributeTypes.Cast<ExolutioObject>();
                
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

        public static IEnumerable<PSMComponent> GetPSMChildren(PSMComponent component, bool returnAttributesForClass = false)
        {
            IEnumerable<PSMComponent> result = new PSMComponent[0];
            if (component is PSMClass && ((PSMClass)component).PSMAttributes.Count > 0 && returnAttributesForClass)
            {
                result = result.Concat(((PSMClass)component).PSMAttributes.Cast<PSMComponent>());
            }
            if (component is PSMAssociationMember && ((PSMAssociationMember)component).ChildPSMAssociations.Count > 0)
            {
                result = result.Concat(((PSMAssociationMember)component).ChildPSMAssociations.Select(association => association.Child).Cast<PSMComponent>());
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

        public static PSMClass NearestInterpretedClass(this PSMClass child)
        {
            if (child.Interpretation != null) return child;
            return NearestInterpretedParentClass(child);
        }

        public static PSMClass NearestInterpretedClass(this PSMAttribute attribute)
        {
            if (attribute.PSMClass == null) return null;
            return attribute.PSMClass.NearestInterpretedClass();
        }

        private static IEnumerable<PSMClass> InterpretedSubClasses(this PSMAssociationMember parent)
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
        
        public static IEnumerable<PSMClass> InterpretedSubClasses(this PSMClass parent)
        {
            List<PSMClass> list = new List<PSMClass>();
            foreach (PSMAssociation a in parent.ChildPSMAssociations)
            {
                if (a.Child != null) list.AddRange(a.Child.InterpretedSubClasses());
            }
            return list;
        }

        public static IEnumerable<PSMClass> UnInterpretedSubClasses(this PSMAssociationMember parent, bool includeThis = false)
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

        public static IEnumerable<PSMClass> UnInterpretedSubClasses(this PSMClass parent, bool includeThis = false)
        {
            List<PSMClass> list = new List<PSMClass>();
            if (includeThis && parent.Interpretation == null) list.Add(parent as PSMClass);
            foreach (PSMAssociation a in parent.ChildPSMAssociations)
            {
                if (a.Child != null) list.AddRange(a.Child.UnInterpretedSubClasses(true));
            }
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
    }
}