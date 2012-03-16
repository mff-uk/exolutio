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
    internal class PSMConstraintSuitabilityChecker : ConvertToPSMVisitorBase<bool>
    {
        private bool isSuitable;
        private OclExpression violatingExpression;

        #region constructions not supported

        public override bool Visit(CollectionLiteralExp node)
        {
            /* For simplicity we do not allow this yet */
            violatingExpression = node;
            isSuitable = false;
            return false;
        }

        public override bool Visit(EnumLiteralExp node)
        {
            /* For simplicity we do not allow this yet */
            violatingExpression = node;
            isSuitable = false;
            return false;
        }

        public override bool Visit(ErrorExp node)
        {
            violatingExpression = node;
            isSuitable = false;
            return false;
        }

        public override bool Visit(LetExp node)
        {
            violatingExpression = node;
            isSuitable = false;
            return false;
        }

        public override bool Visit(TupleLiteralExp node)
        {
            /* Tuples we can't handle yet */
            isSuitable = false;
            violatingExpression = node;
            return false;
        }

        public override bool Visit(TypeExp node)
        {
            /* Types we can't handle yet */
            isSuitable = false;
            violatingExpression = node;
            return false;
        }

        #endregion

        public override bool Visit(IteratorExp node)
        {
            // forAll apod. 
            bool sourceAccept = node.Source.Accept(this);
            OclExpression source = node.Source;

            while (source is IteratorExp)
            {
                if (collectionIteratorsPreservingType.Contains(((IteratorExp)source).IteratorName))
                {
                    source = ((IteratorExp)source).Source;
                }
                else
                    break;
            }
            
            if (source is PropertyCallExp)
            {
                // find path to source 
                PIMPath sourcePath = PIMPathBuilder.BuildPIMPath((PropertyCallExp)source);
                List<PSMPath> navigations = FindNavigationsForPIMNavigation(sourcePath);
                foreach (VariableDeclaration vd in node.Iterator)
                {
                    VariableClassMappings.CreateSubCollectionIfNeeded(vd);
                    foreach (PSMPath psmNavigation in navigations)
                    {                        
                        VariableClassMappings[vd].Add(psmNavigation.LastClass);
                    }
                }
            }
            else if (source is IteratorExp)
            {
                foreach (VariableDeclaration vd in node.Iterator)
                {
                    if (vd.PropertyType.Tag != null)
                    {
                        VariableClassMappings.CreateSubCollectionIfNeeded(vd);
                        VariableClassMappings[vd].AddRange(GetInterpretations((PIMClass)vd.PropertyType.Tag));   
                    }                    
                }
            }
            loopStacks.Push(node);
            bool bodyAccept = node.Body.Accept(this);
            loopStacks.Pop();
            return sourceAccept && bodyAccept;
        }

        public override bool Visit(IterateExp node)
        {
            // opravdu iterate
            // TODO: opravit jako iterator
            loopStacks.Push(node);
            bool bodypart = node.Body.Accept(this);
            loopStacks.Pop();
            return bodypart;
        }

        public override bool Visit(OperationCallExp node)
        {
            /* decide, which operations can be translated, because some can, 
             * we can create a library of operations on type, such as string.concat.
             * Also, arithmethic operations fit in here and many other standard (such 
             * as operators +,-,=,> etc.)
             */

            if (node.ReferredOperation.Tag == null) // standard operation 
            {
                bool sourceAccept = node.Source.Accept(this);
                foreach (OclExpression argument in node.Arguments)
                {
                    argument.Accept(this);
                }
                return sourceAccept;
            }
            else // model operation
            {
                violatingExpression = node;
                isSuitable = false;
                return false;
            }


            /*
            if (standardOperations.ContainsKey(node.Source.Type.Name) &&
                standardOperations[node.Source.Type.Name].Contains(node.ReferredOperation.Name))
            {
                node.Source.Accept(this);
                foreach (OclExpression argument in node.Arguments)
                {
                    argument.Accept(this);
                }
            }
            else
            {
                violatingExpression = node;
                isSuitable = false;
            }
            */
        }

        public override bool Visit(PropertyCallExp node)
        {
            /* It is necessary to find out, whether the property (attribute) 
             * is represented in PSM schema by a PSM attribute */

            // check is perfomed by building a PIM path
            PIMPath path = PIMPathBuilder.BuildPIMPath(node);
            // and testing suitability of the path
            List<PSMPath> psmNavigations
                = FindNavigationsForPIMNavigation(path);
            return psmNavigations.Count > 0;
        }

        public override bool Visit(VariableExp node)
        {
            /* hopefully no problem here */
            return true; 
        }

        #region structural expressions - decision is delegated

        public override bool Visit(IfExp node)
        {
            bool condition = node.Condition.Accept(this);
            bool thenpart = false;
            bool elsepart = false;
            if (node.ThenExpression != null)
            {
                thenpart = node.ThenExpression.Accept(this);
            }
            if (node.ElseExpression != null)
            {
                elsepart = node.ElseExpression.Accept(this);
            }
            return condition && thenpart && elsepart;
        }

        #endregion

        #region trivial expression that can be translated allways

        public override bool Visit(BooleanLiteralExp node) { return true; }

        public override bool Visit(IntegerLiteralExp node) { return true; }

        public override bool Visit(NullLiteralExp node) { return true; }

        public override bool Visit(RealLiteralExp node) { return true; }

        public override bool Visit(StringLiteralExp node) { return true; }

        public override bool Visit(UnlimitedNaturalLiteralExp node) { return true; }

        public override bool Visit(InvalidLiteralExp node) { return true; }

        #endregion

        public bool CheckConstraintSuitability(ClassifierConstraint classifierConstraint, OclExpression oclExpression)
        {
            Clear();
            psmBridge = new PSMBridge(TargetPSMSchema);
            PIMClass contextClass = (PIMClass) classifierConstraint.Context.Tag;
            VariableClassMappings.CreateSubCollectionIfNeeded(classifierConstraint.Self);
            VariableClassMappings[classifierConstraint.Self].AddRange(GetInterpretations(contextClass));
            
            oclExpression.Accept(this);

            IEnumerable<VariableDeclaration> f = from m in VariableClassMappings
                    where m.Value.Count == 0
                    select m.Key;

            if (f.Count() > 0)
            {
                isSuitable = false;
            }

            return isSuitable;
        }

        public override void Clear()
        {
            base.Clear();
            isSuitable = true;
            violatingExpression = null;
        }
    }
}