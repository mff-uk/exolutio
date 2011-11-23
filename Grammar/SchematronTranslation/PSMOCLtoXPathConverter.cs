using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.OCL.Types;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class PSMOCLtoXPathConverter: IAstVisitor<string>
    {

        protected readonly Stack<LoopExp> loopStacks
            = new Stack<LoopExp>();

        public OCLScript OCLScript { get; set; }

        public PSMBridge Bridge { get; set; }

        public ClassifierConstraint Constraint { get; set; }
        
        public Log<OclExpression> Log { get; set; }

        private LoopExp GetLoopExpForVariable(VariableExp v)
        {
            return loopStacks.LastOrDefault(l => l.Iterator.Any(vd => vd.Name == v.referredVariable.Name));
        }

        public string Visit(ErrorExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public string Visit(IterateExp node)
        {
            loopStacks.Push(node);

            loopStacks.Pop();
            return null;
        }

        public string Visit(IteratorExp node)
        {
            loopStacks.Push(node);

            loopStacks.Pop();
            return null;
        }

        public string Visit(OperationCallExp node)
        {
            return null;
        }

        public string Visit(PropertyCallExp node)
        {
            if (!(node.ReferredProperty.Tag is PSMAttribute))
            {
                Log.AddWarningTaggedFormat(XPathTranslationLogMessages.UNSAFE_PROPERTY_CALL_EXP, node);
            }
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node);
            string xpath = psmPath.ToXPath();
            return xpath;
        }

        public string Visit(VariableExp node)
        {
            if (node.Type is Class)
            {
                Log.AddWarningTaggedFormat(XPathTranslationLogMessages.UNSAFE_VARIABLE_EXP, 
                    node, node.referredVariable.Name);
            }

            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node);
            string xpath = psmPath.ToXPath();
            return xpath;
        }

        #region not supported yet 

        public string Visit(LetExp node)
        {
            // TODO: PSM2XPath: there should be some suport for let expression in the future
            throw new ExpressionNotSupportedInXPath(node);
        }

        public string Visit(TypeExp node)
        {
            // TODO: PSM2XPath: there should be some suport for types in the future
            throw new ExpressionNotSupportedInXPath(node);
        }

        #endregion


        #region structural

        public string Visit(IfExp node)
        {
            string cond = node.Condition.Accept(this);
            string thenExpr = node.ThenExpression.Accept(this);
            string elseExpr = node.ElseExpression.Accept(this);
            return string.Format("if ({0}) then {1} else {2}", cond, thenExpr, elseExpr);
        }

        #endregion 

        #region literals

        public string Visit(TupleLiteralExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public string Visit(InvalidLiteralExp node)
        {
            return null;
        }

        public string Visit(BooleanLiteralExp node)
        {
            // instead boolean literals, functions are used
            return node.Value ? "true()" : "false()";
        }

        public string Visit(CollectionLiteralExp node)
        {
            // TODO: PSM2XPath: some support for collection linterals
            throw new ExpressionNotSupportedInXPath(node);
        }

        public string Visit(EnumLiteralExp node)
        {
            return null;
        }

        public string Visit(NullLiteralExp node)
        {
            return null;
        }

        public string Visit(UnlimitedNaturalLiteralExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public string Visit(IntegerLiteralExp node)
        {
            return node.ToString();
        }

        public string Visit(RealLiteralExp node)
        {
            return node.ToString();
        }

        public string Visit(StringLiteralExp node)
        {
            return string.Format("'{0}'", node.Value);
        }

        #endregion 

        public void Clear()
        {
            loopStacks.Clear();
            Log.Clear();
        }

        public string TranslateInvariant(OclExpression invariant)
        {
            return invariant.Accept(this);
        }
    }

    public class ExpressionNotSupportedInXPath: Exception
    {
        public ExpressionNotSupportedInXPath(OclExpression node)
        {
            
        }
    }
}