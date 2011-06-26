using System;
using System.Diagnostics;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;
using Exolutio.Model.Versioning;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation.Changes
{
    public abstract class ChangeInstance
    {
        public abstract EChangePredicateScope Scope { get; }

        public abstract EChangeCategory Category { get; }

        public string Type { get { return this.GetType().Name; } }

        protected internal static readonly DispNullFormatProvider DispNullFormatProvider = new DispNullFormatProvider();

        public Version OldVersion { get; set; }

        public Version NewVersion { get; set; }

        [ChangePredicateParameter]
        public PSMComponent Component { get; protected set; }

        protected ChangeInstance(PSMComponent component, Version oldVersion, Version newVersion)
        {
            OldVersion = oldVersion;
            NewVersion = newVersion;
            Component = component;
        }

        public override string ToString()
        {
            throw new InvalidOperationException(string.Format("Class {0} must override ToString!", base.ToString()));
        }

        public string ChangeType
        {
            get
            {
                return GetType().Name.Replace("Change", string.Empty);
            }
        }

        public virtual void Verify()
        {
            ExolutioVersionedObject verifiedElement = Component;

            //if (this is ISubelementAditionChange)
            //{
            //    verifiedElement = ((ISubelementAditionChange)this).ChangedSubelement;
            //}
            //else if (this is ISubelementRemovalChange)
            //{
            //    verifiedElement = ((ISubelementRemovalChange)this).ChangedSubelement;
            //}
            //else
            //{
            //    verifiedElement = Element;
            //}

            if (Category.IsAmong(EChangeCategory.Addition, EChangeCategory.Migratory, EChangeCategory.Sedentary))
            {
                Debug.Assert(NewVersion != null);
                Debug.Assert(verifiedElement.GetInVersion(NewVersion) != null);
            }

            if (Category.IsAmong(EChangeCategory.Removal, EChangeCategory.Migratory, EChangeCategory.Sedentary))
            {
                Debug.Assert(OldVersion != null);
                Debug.Assert(verifiedElement.ExistsInVersion(OldVersion));
            }

            if (Category == EChangeCategory.Addition)
            {
                Debug.Assert(NewVersion != null);
                Debug.Assert(verifiedElement.GetInVersion(OldVersion) == null);
            }

            if (Category == EChangeCategory.Removal)
            {
                Debug.Assert(OldVersion != null);
                Debug.Assert(verifiedElement.GetInVersion(NewVersion) == null);
            }
        }

        /// <summary>
        /// Implementation of the so-called NI-predicate from XSEM-Evo, returns true if this change instance is 
        /// backwards compatible. 
        /// </summary>
        public virtual bool IsNotInvalidatingChange { get { return true; } }
        
        protected static bool AdditionTest(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return candidate.Version == newVersion && candidate.ExistsInVersion(newVersion) && !candidate.ExistsInVersion(oldVersion);
        }

        protected static bool RemovalTest(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return candidate.Version == oldVersion && candidate.ExistsInVersion(oldVersion) && !candidate.ExistsInVersion(newVersion);
        }

        protected static bool ExistingTest(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            return candidate.Version == newVersion && candidate.ExistsInVersion(newVersion) && candidate.ExistsInVersion(oldVersion);
        }

        protected static bool AreLinked(PSMComponent componentOldVersion, PSMComponent componentNewVersion)
        {
            return componentOldVersion != null && componentNewVersion != null &&
                componentOldVersion.GetInVersion(componentNewVersion.Version) == componentNewVersion;
        }

        public static bool TestCandidate(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            throw new InvalidOperationException();
        }

        public static ChangeInstance CreateInstance(PSMComponent candidate, Version oldVersion, Version newVersion)
        {
            throw new InvalidOperationException();
        }
    }

    public interface IChangeInstance<out TComponent>
        where TComponent : PSMComponent
    {
        Version OldVersion { get; set; }

        Version NewVersion { get; set; }

        TComponent Component { get; }
    }

    public interface IAdditionChange
    {
        Component ComponentNewVersion { get; }
    }

    public interface IRemovalChange
    {
        Component ComponentOldVersion { get; }
    }

    public interface IExistingComponentChange
    {
        Component ComponentOldVersion { get; }
        Component ComponentNewVersion { get; }
    }

    public interface ISedentaryChange : IExistingComponentChange 
    {
    }

    public interface IRenameChange: ISedentaryChange
    {
        
    }

    public interface ICardinalityChange: ISedentaryChange
    {
        
    }

    public interface IMigratoryChange : IExistingComponentChange
    {
       
    }
}