using System;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.ConstraintConversion
{
    internal class ConstraintConvertor : ConvertToPSMVisitorBase<OclExpression>
    {
        private OclExpression notSupportedExpression;
        private OclExpression notConvertibleExpression; 
        private bool isSuitable;

        public VariableDeclaration SelfVariableDeclaration { get; set; }

        

        public override OclExpression Visit(IteratorExp node)
        {
            OclExpression sourceAccept = node.Source.Accept(this);
            loopStacks.Push(node);
            OclExpression bodyAccept = node.Body.Accept(this);
            loopStacks.Pop();
            List<VariableDeclaration> transVDs = new List<VariableDeclaration>();
            foreach (VariableDeclaration vd in node.Iterator)
            {                
                VariableDeclaration transVD;
                if (!VariableTranslations.ContainsKey(vd))
                {
                    transVD = new VariableDeclaration(vd.Name, null, null);
                    VariableTranslations[vd] = transVD;
                }
                else
                {
                    transVD = VariableTranslations[vd];
                }
                transVDs.Add(transVD);
            }
            return new IteratorExp(sourceAccept, bodyAccept, node.IteratorName, transVDs, node.Type);
        }

        public override OclExpression Visit(IterateExp node)
        {
            // opravdu iterate
            OclExpression sourceAccept = node.Source.Accept(this);
            loopStacks.Push(node);
            OclExpression bodyAccept = node.Body.Accept(this);
            loopStacks.Pop();
            return new IterateExp(sourceAccept, bodyAccept, new VariableDeclaration(node.Iterator.First().Name, null, null), new VariableDeclaration(node.Result.Name, null, null));
        }

        public override OclExpression Visit(OperationCallExp node)
        {
            if (node.ReferredOperation.Tag == null) // standard operation 
            {
                OclExpression source = node.Source.Accept(this);
                List<OclExpression> args = new List<OclExpression>();
                foreach (OclExpression argument in node.Arguments)
                {
                    OclExpression arg = argument.Accept(this);
                    args.Add(arg);
                }

                return new OperationCallExp(source, node.IsPre, node.ReferredOperation, args);
            }
            else // model operation
            {
                notSupportedExpression = node;
                isSuitable = false;
                throw new OclConstructNotAvailableInPSM(node);
            }
        }

        public override OclExpression Visit(PropertyCallExp node)
        {
            PIMPath pimPath = PIMPathBuilder.BuildPIMPath(node);
            #region failure check

            if (!PathMappings.ContainsKey(pimPath) || PathMappings[pimPath].Count == 0)
            {
                isSuitable = false;
                notConvertibleExpression = node;
                throw new OclSubexpressionNotConvertible(notConvertibleExpression);
            }

            #endregion
            PSMPath psmPath = PathMappings[pimPath].First();

            OclExpression result = CreateExpressionFromPath(psmPath);
            return result;
        }

        private OclExpression CreateExpressionFromPath(PSMPath psmPath)
        {
            VariableExp startVarExp = new VariableExp(psmPath.StartingVariableExp.referredVariable);
            OclExpression result = startVarExp;
            Classifier sourceClassifier = startVarExp.Type;
            foreach (PSMPathStep step in psmPath.Steps)
            {
                if (step is PSMPathVariableStep)
                {
                    continue;
                }
                Property referredProperty;
                if (step is PSMPathAssociationStep)
                {
                    PSMPathAssociationStep pathAssociationStep = ((PSMPathAssociationStep)step);
                    referredProperty = sourceClassifier.LookupProperty(pathAssociationStep.Association.Name);
                    if (referredProperty == null)
                    {
                        if (pathAssociationStep.IsUp)
                        {
                            referredProperty = sourceClassifier.LookupProperty(PSMBridgeAssociation.PARENT_STEP);
                        }
                        else
                        {
                            referredProperty = sourceClassifier.LookupProperty(string.Format(PSMBridgeAssociation.CHILD_N_STEP,
                                pathAssociationStep.From.ChildPSMAssociations.IndexOf(pathAssociationStep.Association)));
                        }
                    }
                }
                else if (step is PSMPathAttributeStep)
                    referredProperty = sourceClassifier.LookupProperty(((PSMPathAttributeStep)step).Attribute.Name);
                else
                    throw new NotImplementedException();

                Property navigationSource = null;
                OclExpression qualifier = null;
                PropertyCallExp propertyCallExp = new PropertyCallExp(result, false, navigationSource, qualifier, referredProperty);
                result = propertyCallExp;

                if (step is PSMPathAssociationStep)
                    sourceClassifier = psmBridge.Find(((PSMPathAssociationStep)step).To);
                // else => no other steps
            }
            return result;
        }

        public override OclExpression Visit(VariableExp node)
        {
            if (!VariableTranslations.ContainsKey(node.referredVariable))
            {
                OclExpression value = null;
                if (node.referredVariable.Value != null)
                {
                    value = node.referredVariable.Value.Accept(this);
                }
                PSMClass psmClass = VariableClassMappings[node.referredVariable].First();
                Classifier variableType = psmBridge.Find(psmClass);
                VariableDeclaration vd = new VariableDeclaration(node.referredVariable.Name, variableType, value);
                return new VariableExp(vd);
            }
            else
            {
                return new VariableExp(VariableTranslations[node.referredVariable]);
            }
        }

        #region structural, easily converted

        public override OclExpression Visit(IfExp node)
        {
            OclExpression condition = node.Condition.Accept(this);
            OclExpression thenExpression = node.ThenExpression.Accept(this);
            OclExpression elseExpression = node.ElseExpression.Accept(this);
            
            return new IfExp(node.Type /* bool? */, condition, thenExpression, elseExpression);
        }

        #endregion 

        #region constructions not supported

        public override OclExpression Visit(CollectionLiteralExp node)
        {
            /* For simplicity we do not allow this yet */
            notSupportedExpression = node;
            isSuitable = false;
            throw new OclConstructNotAvailableInPSM(node); 
        }

        public override OclExpression Visit(EnumLiteralExp node)
        {
            /* For simplicity we do not allow this yet */
            notSupportedExpression = node;
            isSuitable = false;
            throw new OclConstructNotAvailableInPSM(node); 
        }

        public override OclExpression Visit(ErrorExp node)
        {
            notSupportedExpression = node;
            isSuitable = false;
            throw new OclConstructNotAvailableInPSM(node); 
        }

        public override OclExpression Visit(LetExp node)
        {
            notSupportedExpression = node;
            isSuitable = false;
            throw new OclConstructNotAvailableInPSM(node); 
        }

        public override OclExpression Visit(TupleLiteralExp node)
        {
            /* Tuples we can't handle yet */
            isSuitable = false;
            notSupportedExpression = node;
            throw new OclConstructNotAvailableInPSM(node); 
        }

        public override OclExpression Visit(TypeExp node)
        {
            /* Types we can't handle yet */
            isSuitable = false;
            notSupportedExpression = node;
            throw new OclConstructNotAvailableInPSM(node); 
        }

        #endregion

        #region trivial expression that can be translated allways

        public override OclExpression Visit(BooleanLiteralExp node) { return new BooleanLiteralExp(node.Value, node.Type); }

        public override OclExpression Visit(IntegerLiteralExp node) { return new IntegerLiteralExp(node.Value, node.Type); }

        public override OclExpression Visit(NullLiteralExp node) { return new NullLiteralExp(node.Type); }

        public override OclExpression Visit(RealLiteralExp node) { return new RealLiteralExp(node.Value, node.Type); }

        public override OclExpression Visit(StringLiteralExp node) { return new StringLiteralExp(node.Value, node.Type); }

        public override OclExpression Visit(UnlimitedNaturalLiteralExp node) { return new UnlimitedNaturalLiteralExp(node.Type); }

        public override OclExpression Visit(InvalidLiteralExp node) { return new InvalidLiteralExp(node.Type); }

        #endregion

        public OclExpression TranslateConstraint(ClassifierConstraintBlock classifierConstraintBlock, OclExpression pimInvariant, Dictionary<VariableDeclaration, List<PSMClass>> initialVariableMappings, Dictionary<PIMPath, List<PSMPath>> initialPathMappings, Dictionary<VariableDeclaration, VariableDeclaration> variableTranslations, out Classifier psmContextSuggestion)
        {
            #region fill prepared translations

            foreach (KeyValuePair<VariableDeclaration, List<PSMClass>> kvp in initialVariableMappings)
            {
                VariableClassMappings.CreateSubCollectionIfNeeded(kvp.Key);
                VariableClassMappings[kvp.Key].AddRange(kvp.Value);
            }

            foreach (KeyValuePair<PIMPath, List<PSMPath>> kvp in initialPathMappings)
            {
                PathMappings.CreateSubCollectionIfNeeded(kvp.Key);
                PathMappings[kvp.Key].AddRange(kvp.Value);
            }

            foreach (KeyValuePair<VariableDeclaration, VariableDeclaration> kvp in variableTranslations)
            {
                VariableTranslations[kvp.Key] = kvp.Value;
            }

            #endregion

            psmBridge = new PSMBridge(TargetPSMSchema);

            #region prepare self variable 

            PSMClass psmClass = VariableClassMappings[classifierConstraintBlock.Self].First();
            Classifier variableType = psmBridge.Find(psmClass);
            VariableDeclaration vd = new VariableDeclaration(classifierConstraintBlock.Self.Name, variableType, null);
            this.SelfVariableDeclaration = vd;

            #endregion

            OclExpression translateConstraint = pimInvariant.Accept(this);
            psmContextSuggestion = psmBridge.Find(VariableClassMappings[classifierConstraintBlock.Self].First()); 
            if (isSuitable)
                return translateConstraint;
            else
                return null; 
        }

        public override void Clear()
        {
            base.Clear();
            SelfVariableDeclaration = null;
            notSupportedExpression = null;
            isSuitable = true;
        }
    }

    public class OclConstructNotAvailableInPSM: ExolutioModelException
    {
        public OclConstructNotAvailableInPSM(string message) : base(message)
        {
        }

        public OclConstructNotAvailableInPSM(string message, Exception innerException) : base(message, innerException)
        {
        }

        public OclConstructNotAvailableInPSM()
        {
        }

        public OclConstructNotAvailableInPSM(OclExpression oclExpression)
        {
        }
    }

    public class OclSubexpressionNotConvertible : ExolutioModelException
    {
        public OclSubexpressionNotConvertible(string message)
            : base(message)
        {
        }

        public OclSubexpressionNotConvertible(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public OclSubexpressionNotConvertible()
        {
        }

        public OclSubexpressionNotConvertible(OclExpression oclExpression)
        {
        }
    }
}