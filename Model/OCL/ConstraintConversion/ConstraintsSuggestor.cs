using System.Collections.Generic;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.ConstraintConversion
{
    public class ConstraintsSuggestor
    {
        public IList<ClassifierConstraint> FindSuitableConstraints(PIMSchema pimSchema, PSMSchema psmSchema)
        {
            List<ClassifierConstraint> result = new List<ClassifierConstraint>();
            foreach (OCLScript oclScript in pimSchema.OCLScripts)
            {
                CompilerResult compilerResult = oclScript.CompileToAst();
                if (!compilerResult.Errors.HasError)
                {
                    PSMConstraintSuitabilityChecker constraintSuitabilityChecker = new PSMConstraintSuitabilityChecker
                                                                                       {
                                                                                           TargetPSMSchema = psmSchema,
                                                                                           Bridge = compilerResult.Bridge
                                                                                       };
                    ConstraintConvertor constraintConvertor = new ConstraintConvertor
                                                                  {
                                                                      TargetPSMSchema = psmSchema,
                                                                      Bridge = compilerResult.Bridge
                                                                  };


                    foreach (ClassifierConstraint classifierConstraint in compilerResult.Constraints.Classifiers)
                    {
                        /* constraints from one PIM context can be distributed amont several 
                         * PSM contexts (different PSM classes with identical interpretation) */
                        Dictionary<Classifier, ClassifierConstraint> translatedInvariants =
                            new Dictionary<Classifier, ClassifierConstraint>();

                        foreach (InvariantWithMessage pimInvariant in classifierConstraint.Invariants)
                        {
                            constraintSuitabilityChecker.Clear();
                            bool suitable = constraintSuitabilityChecker.CheckConstraintSuitability(
                                classifierConstraint, pimInvariant.Constarint);
                            if (suitable)
                            {
                                Classifier psmContextSuggestion = null; 
                                constraintConvertor.Clear();
                                OclExpression psmInvariant = constraintConvertor.TranslateConstraint(classifierConstraint, pimInvariant.Constarint, constraintSuitabilityChecker.VariableClassMappings, 
                                     constraintSuitabilityChecker.PathMappings, constraintSuitabilityChecker.VariableTranslations, out psmContextSuggestion);
                                if (!translatedInvariants.ContainsKey(psmContextSuggestion))
                                {
                                    translatedInvariants[psmContextSuggestion] = new ClassifierConstraint(psmContextSuggestion,
                                        new List<InvariantWithMessage>(), constraintConvertor.SelfVariableDeclaration);
                                }
                                translatedInvariants[psmContextSuggestion].Invariants.Add(new InvariantWithMessage(psmInvariant));
                            }
                        }

                        foreach (KeyValuePair<Classifier, ClassifierConstraint> kvp in translatedInvariants)
                        {
                            result.Add(kvp.Value);
                        }
                    }
                }
            }
            return result;
        }
    }
}