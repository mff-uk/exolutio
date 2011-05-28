using System;
using System.Collections.Generic;
using System.Diagnostics;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Revalidation.Changes;
using System.Reflection;
using Exolutio.SupportingClasses;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Revalidation
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
                            changeSet.CreateSubCollectionIfNeeded(type);
                            ChangeInstance instance = (ChangeInstance)createInstanceMethods[type].Invoke(null, testParams);
                            changeSet[type].Add(instance);
                        }
                    }
                }
            }
        }
    }
}