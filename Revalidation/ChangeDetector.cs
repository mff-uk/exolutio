using System;
using System.Collections.Generic;
using System.Diagnostics;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Revalidation.Changes;
using System.Reflection;
using EvoX.SupportingClasses;
using Version = EvoX.Model.Versioning.Version;

namespace EvoX.Revalidation
{
    /// <summary>
    /// Holds instances of all change predicates
    /// </summary>
    public class DetectedChangesSet: Dictionary<Type, List<ChangeInstance>>
    {
        
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
                        ChangePredicateScopeAttribute attribute = 
                            (ChangePredicateScopeAttribute) type.GetCustomAttributes(typeof (ChangePredicateScopeAttribute), true)[0];
                        if (!type.IsAbstract)
                        {
                            _changePredicatesByScope.CreateSubCollectionIfNeeded(attribute.Scope);
                            _changePredicatesByScope[attribute.Scope].Add(type);
                            testMethods[type] = type.GetMethod("TestCandidate", BindingFlags.Static);
                            testMethods[type] = type.GetMethod("CreateInstance", BindingFlags.Static);
                        }
                    }
                }

                return _changePredicatesByScope;
            }
        }

        public DetectedChangesSet DetectChanges(PSMSchema schema1, PSMSchema schema2)
        {
            Version oldVersion = schema1.Version;
            Version newVersion = schema2.Version;
            Debug.Assert(oldVersion != newVersion);

            DetectedChangesSet changeSet = new DetectedChangesSet();

            #region attributes 

            testConstructs(schema1.PSMAttributes, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMAttribute);
            testConstructs(schema2.PSMAttributes, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMAttribute);

            #endregion 

            #region associations 

            testConstructs(schema1.PSMAssociations, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMAssociation);
            testConstructs(schema2.PSMAssociations, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMAssociation);

            #endregion 

            #region classes

            testConstructs(schema1.PSMClasses, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMClass);
            testConstructs(schema2.PSMClasses, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMClass);

            #endregion 

            #region contentModels

            testConstructs(schema1.PSMContentModels, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMContentModel);
            testConstructs(schema2.PSMContentModels, oldVersion, newVersion, changeSet, EChangePredicateScope.PSMContentModel);

            #endregion 

            return changeSet;
        }

        private static void testConstructs<TPSMComponent>(IEnumerable<TPSMComponent> components, 
            Version oldVersion, Version newVersion, DetectedChangesSet changeSet, EChangePredicateScope scope) 
            where TPSMComponent : EvoXObject
        {
            foreach (TPSMComponent component in components)
            {
                foreach (Type type in changePredicatesByScope[scope])
                {
                    object[] testParams = new object[]{component, oldVersion, newVersion};
                    bool result = (bool) testMethods[type].Invoke(null, testParams);
                    if (result)
                    {
                        changeSet.CreateSubCollectionIfNeeded(type);
                        ChangeInstance instance = (ChangeInstance) createInstanceMethods[type].Invoke(null, testParams);
                        changeSet[type].Add(instance);
                    }
                }
            }
        }
    }
}