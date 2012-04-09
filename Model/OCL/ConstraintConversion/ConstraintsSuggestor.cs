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
        public IList<ClassifierConstraintBlock> FindSuitableConstraints(PIMSchema pimSchema, PSMSchema psmSchema)
        {
            List<ClassifierConstraintBlock> result = new List<ClassifierConstraintBlock>();
            foreach (OCLScript oclScript in pimSchema.OCLScripts)
            {
                CompilerResult compilerResult = oclScript.CompileToAst();
                if (!compilerResult.Errors.HasError)
                {
                    ConstraintSuitabilityChecker constraintSuitabilityChecker = new ConstraintSuitabilityChecker
                                                                                       {
                                                                                           TargetPSMSchema = psmSchema,
                                                                                           Bridge = compilerResult.Bridge
                                                                                       };
                    ConstraintConvertor constraintConvertor = new ConstraintConvertor
                                                                  {
                                                                      TargetPSMSchema = psmSchema,
                                                                      Bridge = compilerResult.Bridge
                                                                  };


                    foreach (ClassifierConstraintBlock classifierConstraint in compilerResult.Constraints.ClassifierConstraintBlocks)
                    {
                        /* constraints from one PIM context can be distributed amont several 
                         * PSM contexts (different PSM classes with identical interpretation) */
                        Dictionary<Classifier, ClassifierConstraintBlock> translatedInvariants =
                            new Dictionary<Classifier, ClassifierConstraintBlock>();

                        foreach (InvariantWithMessage pimInvariant in classifierConstraint.Invariants)
                        {
                            constraintSuitabilityChecker.Clear();
                            bool suitable = constraintSuitabilityChecker.CheckConstraintSuitability(
                                classifierConstraint, pimInvariant.Constraint);
                            if (suitable)
                            {
                                Classifier psmContextSuggestion = null; 
                                constraintConvertor.Clear();
                                try
                                {
                                    OclExpression psmInvariant = constraintConvertor.TranslateConstraint(classifierConstraint, pimInvariant.Constraint, constraintSuitabilityChecker.VariableClassMappings,
                                         constraintSuitabilityChecker.PathMappings, constraintSuitabilityChecker.VariableTranslations, out psmContextSuggestion);

                                    if (!translatedInvariants.ContainsKey(psmContextSuggestion))
                                    {
                                        translatedInvariants[psmContextSuggestion] = new ClassifierConstraintBlock(psmContextSuggestion,
                                            new List<InvariantWithMessage>(), constraintConvertor.SelfVariableDeclaration);
                                    }
                                    translatedInvariants[psmContextSuggestion].Invariants.Add(new InvariantWithMessage(psmInvariant));
                                }
                                catch
                                {

                                }
                            }
                        }

                        foreach (KeyValuePair<Classifier, ClassifierConstraintBlock> kvp in translatedInvariants)
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