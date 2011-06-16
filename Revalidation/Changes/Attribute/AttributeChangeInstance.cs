using System;
using System.Diagnostics;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.Changes
{
    [ChangePredicateScope(EChangePredicateScope.PSMAttribute)]
    public abstract class AttributeChangeInstance : ChangeInstance, IChangeInstance<PSMAttribute>
    {
        protected AttributeChangeInstance(PSMComponent component, Version oldVersion, Version newVersion) 
            : base(component, oldVersion, newVersion)
        {
        }

        public override EChangePredicateScope Scope
        {
            get { return EChangePredicateScope.PSMAttribute; }
        }

        PSMAttribute IChangeInstance<PSMAttribute>.Component
        {
            get { return (PSMAttribute) Component; }
        }

        public PSMAttribute PSMAttribute
        {
            get { return ((IChangeInstance<PSMAttribute>) this).Component; }
        }

        
    }

    public class AttributeAddedInstance : AttributeChangeInstance, IAdditionChange
    {
        public AttributeAddedInstance(PSMComponent component, Version oldVersion, Version newVersion) 
            : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentNewVersion
        {
            get { return PSMAttribute; }
        }

        [ChangePredicateParameter]
        public PSMClass Parent { get; private set; }

        [ChangePredicateParameter]
        public int Index { get; private set; }

        public override string ToString()
        {
            return string.Format("A new attribute '{0}' was added to class '{1}' at position '{2}'.", PSMAttribute, Parent, Index);
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Addition; }
        }   

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return AdditionTest(candidate, oldVersion, newVersion);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = ((PSMAttribute)candidate);
            return new AttributeAddedInstance(candidate, oldVersion, newVersion) { Parent = psmAttribute.PSMClass, Index = psmAttribute.Index };
        }
    }

    public class AttributeRemovedInstance : AttributeChangeInstance, IRemovalChange
    {
        public AttributeRemovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentOldVersion
        {
            get { return PSMAttribute; }
        }

        public override string ToString()
        {
            return string.Format("Attribute '{0}' was removed.", PSMAttribute);
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Removal; }
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return RemovalTest(candidate, oldVersion, newVersion);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return new AttributeRemovedInstance(candidate, oldVersion, newVersion);
        }
    }

    public class AttributeRenamedInstance: AttributeChangeInstance, ISedentaryChange
    {
        public AttributeRenamedInstance(PSMComponent component, Version oldVersion, Version newVersion) : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentOldVersion
        {
            get { return PSMAttribute.GetInVersion(OldVersion); }
        }

        public Component ComponentNewVersion
        {
            get { return PSMAttribute; }
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        public string OldName { get; set; }

        [ChangePredicateParameter]
        public string NewName { get; set; }

        public override string  ToString()
        {
 	        return string.Format("Attribute '{0}' was renamed from '{1}' to '{2}'.", PSMAttribute, OldName, NewName);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return ExistingTest(candidate, oldVersion, newVersion) && candidate.Name != candidate.GetInVersion(oldVersion).Name;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return new AttributeRenamedInstance(candidate, oldVersion, newVersion) { OldName = candidate.GetInVersion(oldVersion).Name, NewName = candidate.Name};
        }
    }

    public class AttributeMovedInstance : AttributeChangeInstance, IMigratoryChange
    {
        public AttributeMovedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentOldVersion
        {
            get { return PSMAttribute.GetInVersion(OldVersion); }
        }

        public Component ComponentNewVersion
        {
            get { return PSMAttribute; }
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Migratory; }
        }

        [ChangePredicateParameter]
        public PSMClass NewParent { get; set; }

        [ChangePredicateParameter]
        public int NewIndex { get; set; }

        public PSMClass OldParent { get; set; }

        public int OldIndex { get; set; }

        public override string ToString()
        {
            return string.Format("Attribute '{0}' was moved from class '{1}' to '{2}' to index '{3}'.", PSMAttribute, OldParent, NewParent, NewIndex);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute) candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) &&
                   !AreLinked(psmAttributeO.PSMClass, psmAttribute.PSMClass);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return new AttributeMovedInstance(candidate, oldVersion, newVersion)
                       {
                           NewParent = psmAttribute.PSMClass,
                           OldParent = psmAttributeO.PSMClass,
                           NewIndex = psmAttribute.Index,
                           OldIndex = psmAttributeO.Index
                       };
        }
    }

    public class AttributeXFormChangedInstance : AttributeChangeInstance, ISedentaryChange
    {
        public AttributeXFormChangedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentOldVersion
        {
            get { return PSMAttribute.GetInVersion(OldVersion); }
        }

        public Component ComponentNewVersion
        {
            get { return PSMAttribute; }
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        public bool OldXFormElement { get; set; }

        [ChangePredicateParameter]
        public bool NewXFormElement { get; set; }

        public override string ToString()
        {
            return string.Format("Attribute '{0}' xform changed from '{1}' to '{2}'.", PSMAttribute, 
                OldXFormElement ? "element" : "attribute",
                NewXFormElement ? "element" : "attribute");
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) && psmAttribute.Element != psmAttributeO.Element;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return new AttributeXFormChangedInstance(candidate, oldVersion, newVersion)
                       {
                           OldXFormElement = psmAttributeO.Element,
                           NewXFormElement = psmAttribute.Element
                       };
        }
    }

    public class AttributeIndexChangedInstance : AttributeChangeInstance, IMigratoryChange
    {
        public AttributeIndexChangedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentOldVersion
        {
            get { return PSMAttribute.GetInVersion(OldVersion); }
        }

        public Component ComponentNewVersion
        {
            get { return PSMAttribute; }
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Migratory; }
        }

        public int OldIndex { get; set; }

        [ChangePredicateParameter]
        public int NewIndex { get; set; }

        public override string ToString()
        {
            return string.Format("Attribute '{0}' index changed from '{1}' to '{2}'.", PSMAttribute,
                OldIndex,
                NewIndex);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) 
                && AreLinked(psmAttributeO.PSMClass, psmAttribute.PSMClass)
                && psmAttribute.Index != psmAttributeO.Index;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return new AttributeIndexChangedInstance(candidate, oldVersion, newVersion)
            {
                OldIndex = psmAttributeO.Index,
                NewIndex = psmAttribute.Index
            };
        }
    }

    public class AttributeCardinalityChangedInstance : AttributeChangeInstance, ISedentaryChange
    {
        public AttributeCardinalityChangedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentOldVersion
        {
            get { return PSMAttribute.GetInVersion(OldVersion); }
        }

        public Component ComponentNewVersion
        {
            get { return PSMAttribute; }
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Migratory; }
        }

        public uint OldLower { get; set; }

        [ChangePredicateParameter]
        public uint NewLower { get; set; }

        public UnlimitedInt OldUpper { get; set; }

        [ChangePredicateParameter]
        public UnlimitedInt NewUpper { get; set; }

        public override string ToString()
        {
            return string.Format("Attribute '{0}' cardinality changed from '{1}' to '{2}'.", PSMAttribute,
                IHasCardinalityExt.GetCardinalityString(OldLower, OldUpper),
                IHasCardinalityExt.GetCardinalityString(NewLower, NewUpper));
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) && (psmAttribute.Lower != psmAttributeO.Lower || psmAttribute.Upper != psmAttributeO.Upper);
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return new AttributeCardinalityChangedInstance(candidate, oldVersion, newVersion)
            {
                OldLower = psmAttributeO.Lower,
                OldUpper= psmAttributeO.Upper,
                NewLower = psmAttribute.Lower,
                NewUpper = psmAttribute.Upper
            };
        }
    }

    public class AttributeTypeChangedInstance : AttributeChangeInstance, ISedentaryChange
    {
        public AttributeTypeChangedInstance(PSMComponent component, Version oldVersion, Version newVersion)
            : base(component, oldVersion, newVersion)
        {
        }

        public Component ComponentOldVersion
        {
            get { return PSMAttribute.GetInVersion(OldVersion); }
        }

        public Component ComponentNewVersion
        {
            get { return PSMAttribute; }
        }

        public override EChangeCategory Category
        {
            get { return EChangeCategory.Sedentary; }
        }

        public AttributeType OldType { get; set; }

        [ChangePredicateParameter]
        public AttributeType NewType { get; set; }

        public override string ToString()
        {
            return string.Format("Attribute '{0}' type changed from '{1}' to '{2}'.", PSMAttribute,
                OldType,
                NewType);
        }

        public new static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return ExistingTest(candidate, oldVersion, newVersion) && psmAttribute.AttributeType != psmAttributeO.AttributeType;
        }

        public new static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            PSMAttribute psmAttribute = (PSMAttribute)candidate;
            PSMAttribute psmAttributeO = (PSMAttribute)candidate.GetInVersion(oldVersion);
            return new AttributeTypeChangedInstance(candidate, oldVersion, newVersion)
            {
                OldType = psmAttributeO.AttributeType,
                NewType = psmAttribute.AttributeType
            };
        }
    }
}