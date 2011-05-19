using System;
using System.Collections;
using System.Collections.Generic;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.Model.Versioning;
using NUnit.Framework;
using Version = EvoX.Model.Versioning.Version;

namespace Tests.Versioning
{
    public class BranchTest
    {
        public static void VersionsEquivalent(ProjectVersion version1, ProjectVersion version2)
        {
            Assert.AreEqual(version1.Version.Items.Count, version2.Version.Items.Count);
            VersionItemComparer c = new VersionItemComparer();
            c.Project = version1.Project;
            CollectionAssert.AreEqual(version1.Version.Items, version2.Version.Items, c);

            foreach (IVersionedItem versionedItem in version1.Version.Items)
            {
                IVersionedItem other = versionedItem.GetInVersion(version2.Version);
                Assert.IsNotNull(other);

                RelationsCorrespond(versionedItem, other);
            }
        }

        private static void RelationsCorrespond(IVersionedItem versionedItem, IVersionedItem other)
        {
            #region PIMSchema
            {
                PIMSchema PIMSchema1 = versionedItem as PIMSchema;
                if (PIMSchema1 != null)
                {
                    SubcollectionCheck(PIMSchema1, other, owner => owner.PIMAttributes);
                    SubcollectionCheck(PIMSchema1, other, owner => owner.PIMAssociationEnds);
                    SubcollectionCheck(PIMSchema1, other, owner => owner.PIMClasses);
                    SubcollectionCheck(PIMSchema1, other, owner => owner.PIMAssociations);
                }
            }
            #endregion 

            #region PIMClass
            {
                PIMClass pimClass1 = versionedItem as PIMClass;
                if (pimClass1 != null)
                {
                    SubcollectionCheck(pimClass1, other, owner => owner.PIMAttributes);
                    SubcollectionCheck(pimClass1, other, owner => owner.PIMAssociationEnds);
                }
            }
            #endregion 

            #region PIMAttribute
            {
                PIMAttribute PIMAttribute1 = versionedItem as PIMAttribute;
                if (PIMAttribute1 != null)
                {
                    ReferenceCheck(PIMAttribute1, other, owner => owner.PIMClass);
                    ReferenceCheck(PIMAttribute1, other, owner => owner.AttributeType);
                }
            }
            #endregion 

            #region PIMAssociation
            {
                PIMAssociation PIMAssociation1 = versionedItem as PIMAssociation;
                if (PIMAssociation1 != null)
                {
                    SubcollectionCheck(PIMAssociation1, other, owner => owner.PIMClasses);
                    SubcollectionCheck(PIMAssociation1, other, owner => owner.PIMAssociationEnds);
                }
            }
            #endregion 

            #region PSMClass
            {
                PSMClass PSMClass1 = versionedItem as PSMClass;
                if (PSMClass1 != null)
                {
                    SubcollectionCheck(PSMClass1, other, owner => owner.PSMAttributes);
                    SubcollectionCheck(PSMClass1, other, owner => owner.ChildPSMAssociations);
                    ReferenceCheck(PSMClass1, other, owner => owner.RepresentedClass);
                    ReferenceCheck(PSMClass1, other, owner => owner.ParentAssociation);
                }
            }
            #endregion

            #region PSMAttribute
            {
                PSMAttribute PSMAttribute1 = versionedItem as PSMAttribute;
                if (PSMAttribute1 != null)
                {
                    ReferenceCheck(PSMAttribute1, other, owner => owner.PSMClass);
                    ReferenceCheck(PSMAttribute1, other, owner => owner.AttributeType);
                }
            }
            #endregion

            #region PSMAssociation
            {
                PSMAssociation PSMAssociation1 = versionedItem as PSMAssociation;
                if (PSMAssociation1 != null)
                {
                    ReferenceCheck(PSMAssociation1, other, owner => owner.Parent);
                    ReferenceCheck(PSMAssociation1, other, owner => owner.Child);
                }
            }
            #endregion 

            #region PSMContentModel
            {
                PSMContentModel PSMContentModel1 = versionedItem as PSMContentModel;
                if (PSMContentModel1 != null)
                {
                    SubcollectionCheck(PSMContentModel1, other, owner => owner.ChildPSMAssociations);
                    ReferenceCheck(PSMContentModel1, other, owner => owner.ParentAssociation);
                }
            }
            #endregion
            
            #region PSMSchema
            {
                PSMSchema PSMSchema1 = versionedItem as PSMSchema;
                if (PSMSchema1 != null)
                {
                    SubcollectionCheck(PSMSchema1, other, owner => owner.PSMAttributes);
                    SubcollectionCheck(PSMSchema1, other, owner => owner.PSMContentModels);
                    SubcollectionCheck(PSMSchema1, other, owner => owner.PSMClasses);
                    SubcollectionCheck(PSMSchema1, other, owner => owner.PSMAssociations);
                    SubcollectionCheck(PSMSchema1, other, owner => owner.PSMSchemaReferences);
                    SubcollectionCheck(PSMSchema1, other, owner => owner.Roots);
                    
                    ReferenceCheck(PSMSchema1, other, owner => owner.PSMSchemaClass);
                }
            }
            #endregion 
        }

        private delegate ICollection<TMember> GetSubcollectionHandler<in TOwner, TMember>(TOwner owner) where TMember : EvoXObject;
        private delegate TReferred GetReferredItemHandler<in TOwner, out TReferred>(TOwner owner) where TReferred : EvoXObject;

        private static void SubcollectionCheck<TOwner, TMember>(TOwner owner, IVersionedItem other, GetSubcollectionHandler<TOwner, TMember> getSubcollection)
            where TOwner : class, IVersionedItem
            where TMember : EvoXObject, IVersionedItem
        {
            Assert.IsTrue(other is TOwner);
            TOwner otherT = (TOwner) other;
            foreach (TMember member1 in getSubcollection(owner))
            {
                TMember member2 = member1.GetInVersion(other.Version) as TMember;
                Assert.IsNotNull(member2);
                Assert.AreEqual(member2.Version, other.Version);
                Assert.IsTrue(getSubcollection(otherT).Contains(member2));
            }
        }

        private static void ReferenceCheck<TOwner, TMember>(TOwner owner, IVersionedItem other, GetReferredItemHandler<TOwner, TMember> getReferredItem)
            where TOwner : class, IVersionedItem
            where TMember : EvoXObject, IVersionedItem
        {
            Assert.IsTrue(other is TOwner);
            TOwner otherT = (TOwner) other;
            TMember member1 = getReferredItem(owner);
            if (member1 == null)
            {
                TMember member2 = getReferredItem(otherT);
                Assert.IsNull(member2);
            }
            else
            {
                TMember member2 = member1.GetInVersion(other.Version) as TMember;
                Assert.IsNotNull(member2);
                Assert.AreEqual(member2.GetInVersion(owner.Version), member1);
                Assert.AreEqual(getReferredItem(otherT), member2);
            }
        }

        private class VersionItemComparer: IComparer, IComparer<IVersionedItem>
        {
            public int Compare(IVersionedItem x, IVersionedItem y)
            {
                return 0;
            }

            public int Compare(object x, object y)
            {
                return Compare((IVersionedItem) x, (IVersionedItem) y);
            }

            public Project Project { get; set; }
        }
    }
}