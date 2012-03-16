using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.ConstraintConversion
{
    internal abstract class ConvertToPSMVisitorBase<TType> : IAstVisitor<TType>
    {
        protected PSMBridge psmBridge;

        protected readonly Stack<LoopExp> loopStacks
            = new Stack<LoopExp>();

        private readonly Dictionary<VariableDeclaration, List<PSMClass>> variableClassMappings
            = new Dictionary<VariableDeclaration, List<PSMClass>>();

        public Dictionary<VariableDeclaration, List<PSMClass>> VariableClassMappings
        {
            get { return variableClassMappings; }
        }

        private readonly Dictionary<PIMPath, List<PSMPath>> pathMappings
            = new Dictionary<PIMPath, List<PSMPath>>();

        public Dictionary<PIMPath, List<PSMPath>> PathMappings
        {
            get { return pathMappings; }
        }

        private readonly Dictionary<VariableDeclaration, VariableDeclaration> variableTranslations
            = new Dictionary<VariableDeclaration, VariableDeclaration>();

        internal Dictionary<VariableDeclaration, VariableDeclaration> VariableTranslations
        {
            get { return variableTranslations; }
        }

        protected List<string> collectionIteratorsPreservingType = new List<string> {"closure", "reject", "select", "sortedBy"};

        public PSMSchema TargetPSMSchema { get; set; }

        public IBridgeToOCL Bridge { get; set; }

        public abstract TType Visit(CollectionLiteralExp node);
        public abstract TType Visit(EnumLiteralExp node);
        public abstract TType Visit(ErrorExp node);
        public abstract TType Visit(LetExp node);
        public abstract TType Visit(TupleLiteralExp node);
        public abstract TType Visit(TypeExp node);

        private LoopExp GetLoopExpForVariable(VariableExp v)
        {
            return loopStacks.LastOrDefault(l => l.Iterator.Any(vd => vd.Name == v.referredVariable.Name));
        }

        public abstract TType Visit(IteratorExp node);
        public abstract TType Visit(IterateExp node);
        public abstract TType Visit(OperationCallExp node);
        public abstract TType Visit(PropertyCallExp node);
        public abstract TType Visit(VariableExp node);
        public abstract TType Visit(IfExp node);
        public abstract TType Visit(BooleanLiteralExp node);
        public abstract TType Visit(IntegerLiteralExp node);
        public abstract TType Visit(NullLiteralExp node);
        public abstract TType Visit(RealLiteralExp node);
        public abstract TType Visit(StringLiteralExp node);
        public abstract TType Visit(UnlimitedNaturalLiteralExp node);
        public abstract TType Visit(InvalidLiteralExp node);

        public virtual void Clear()
        {
            loopStacks.Clear();
            variableTranslations.Clear();
            variableClassMappings.Clear();
        }

        protected IEnumerable<PSMClass> GetInterpretations(PIMClass pimClass)
        {
            return TargetPSMSchema.PSMClasses.Where(c => c.Interpretation == pimClass);
        }

        protected List<PSMPath> FindNavigationsForPIMNavigation(PIMPath pimPath)
        {
            List<PSMPath> result = new List<PSMPath>();

            FindNavigationsForPIMNavigationRecursive(pimPath, 0, null, null, true, ref result, null);

            pathMappings.CreateSubCollectionIfNeeded(pimPath);
            pathMappings[pimPath].AddRange(result);

            return result;
        }

        private bool allowNonTree = true; 
        private bool FindNavigationsForPIMNavigationRecursive(PIMPath pimPath, int stepIndex, PSMAssociationMember currentMember, PSMPath builtPath, bool canGoToParent, ref List<PSMPath> result, PSMAssociation associationUsedAlready)
        {
            if (stepIndex == pimPath.Steps.Count)
            {
                result.Add(builtPath);
                return true;
            }

            PIMPathStep currentStep = pimPath.Steps[stepIndex];
            if (currentStep is PIMPathAssociationStep)
            {
                Debug.Assert(currentMember != null);
                PIMPathAssociationStep nextStep = (PIMPathAssociationStep)currentStep;
                List<PSMAssociationMember> candidates = currentMember.ChildPSMAssociations.Where(a => allowNonTree || !a.IsNonTreeAssociation).Select(a => a.Child).ToList();
                /* 
                 * we forbid non-tree associations for now,
                 * it certainly makes thinks easier and I am not sure 
                 * whether it is really a restriction
                 */ 
                List<PSMAssociation> candidatesAssociations = currentMember.ChildPSMAssociations.Where(a => allowNonTree || !a.IsNonTreeAssociation).ToList();
                PSMAssociationMember parent = currentMember.ParentAssociation.Parent;

                if (canGoToParent && !(parent is PSMSchemaClass))
                {
                    bool candidateParent = true;
                    if (currentMember.ParentAssociation.Interpretation != null)
                    {
                        PIMAssociation interpretedAssociation = (PIMAssociation) currentMember.ParentAssociation.Interpretation;
                        PIMAssociationEnd interpretedEnd = currentMember.ParentAssociation.InterpretedAssociationEnd;
                        PIMAssociationEnd oppositeEnd = interpretedAssociation.PIMAssociationEnds.Single(e => e != interpretedEnd);

                        // upwards navigation on ends with upper cardinality > 1 breaks semantics of the expression
                        if (oppositeEnd.Upper > 1)
                        {
                            candidateParent = false;
                        }
                    }

                    if (candidateParent)
                    {
                        candidates.Add(parent);
                        candidatesAssociations.Add(currentMember.ParentAssociation);    
                    }
                }
                bool found = false;
                for (int index = 0; index < candidates.Count; index++)
                {
                    PSMAssociationMember candidate = candidates[index];
                    PSMAssociation candidateAssociation = candidatesAssociations[index];
                    bool parentStep = candidate == parent && !candidateAssociation.IsNonTreeAssociation;
                    // forbid traversing the same association several times
                    if (associationUsedAlready == candidateAssociation)
                        continue;

                    int nextStepIndex = stepIndex;

                    bool interpretedClassSatisfies = candidateAssociation.Interpretation != null &&
                        candidate.DownCastSatisfies<PSMClass>(c => c.Interpretation == nextStep.Class);
                    if (candidate.Interpretation == null || interpretedClassSatisfies)
                    {
                        PSMPath nextBuiltPath = (PSMPath) builtPath.Clone();
                        nextBuiltPath.Steps.Add(new PSMPathAssociationStep(nextBuiltPath) {Association = candidateAssociation, To = candidate, From = currentMember});

                        if (interpretedClassSatisfies)
                        {
                            nextStepIndex++;
                        }
                        
                        found |= FindNavigationsForPIMNavigationRecursive(pimPath, nextStepIndex, candidate, nextBuiltPath,
                            canGoToParent && !parentStep, ref result, parentStep ? candidateAssociation : null);
                    }
                }
                return found; 
            }
            else if (currentStep is PIMPathVariableStep)
            {
                Debug.Assert(currentMember == null);
                PIMPathVariableStep pathVariableStep = (PIMPathVariableStep)currentStep;
                IEnumerable<PSMClass> candidates = TargetPSMSchema.PSMClasses.Where(c => c.Interpretation == pimPath.StartingClass);
                candidates = candidates.Intersect(VariableClassMappings[pathVariableStep.Variable]);
                bool found = false;
                List<PSMClass> eliminatedCandidates = new List<PSMClass>();
                eliminatedCandidates.AddRange(VariableClassMappings[pathVariableStep.Variable].Except(candidates));
                foreach (PSMClass candidate in candidates)
                {
                    PSMBridgeClass startType = psmBridge.Find(candidate);
                    builtPath = new PSMPath();
                    VariableDeclaration vd;
                    if (!variableTranslations.ContainsKey(pathVariableStep.Variable))
                    {
                        vd = new VariableDeclaration(pathVariableStep.Variable.Name, startType, null);
                        variableTranslations[pathVariableStep.Variable] = vd;
                    }
                    else
                    {
                        vd = variableTranslations[pathVariableStep.Variable];
                    }

                    builtPath.Steps.Add(new PSMPathVariableStep(builtPath) { VariableExp = new VariableExp(vd) });
                    bool candidateUsable = FindNavigationsForPIMNavigationRecursive(pimPath, stepIndex + 1, candidate, builtPath, true, ref result, null);
                    if (!candidateUsable)
                    {
                        eliminatedCandidates.Add(candidate);
                    }
                    found |= candidateUsable;
                }
                VariableClassMappings[pathVariableStep.Variable].RemoveAll(eliminatedCandidates.Contains);
                if (PathMappings.ContainsKey(pimPath))
                {
                    PathMappings[pimPath].RemoveAll(p => eliminatedCandidates.Contains(p.StartingClass));
                }
                return found;
            }
            else if (currentStep is PIMPathAttributeStep)
            {
                PIMPathAttributeStep pathAttributeStep = (PIMPathAttributeStep) currentStep;
                Debug.Assert(currentMember is PSMClass);

                bool found = false;
                foreach (PSMAttribute psmAttribute in ((PSMClass) currentMember).PSMAttributes)
                {
                    if (psmAttribute.Interpretation == pathAttributeStep.Attribute)
                    {
                        PSMPath nextBuiltPath = (PSMPath) builtPath.Clone();
                        nextBuiltPath.Steps.Add(new PSMPathAttributeStep(nextBuiltPath) {Attribute = psmAttribute});
                        result.Add(nextBuiltPath);
                        found |= true; 
                    }
                }
                return found; 
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}