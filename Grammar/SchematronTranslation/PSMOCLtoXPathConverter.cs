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
    /// <summary>
    /// Translats valid OCL expression (invariant) into an XPath expression 
    /// </summary>
    public class PSMOCLtoXPathConverter: IAstVisitor<string>
    {
        protected readonly Stack<LoopExp> loopStack
            = new Stack<LoopExp>();

        private bool variablesDefinedExplicitly { get; set; }

        private bool insideDynamicEvaluation = false; 

        private OperationHelper OperationHelper { get; set; }

        public OCLScript OCLScript { get; set; }

        public PSMBridge Bridge { get; set; }

        public ClassifierConstraint OclContext { get; set; }
        
        public Log<OclExpression> Log { get; set; }
        protected VariableNamer VariableNamer { get; set; }

        protected OclExpression TranslatedOclExpression { get; set; }


        private LoopExp GetLoopExpForVariable(VariableExp v)
        {
            return loopStack.LastOrDefault(l => l.Iterator.Any(vd => vd.Name == v.referredVariable.Name));
        }

        public string Visit(ErrorExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public string Visit(IterateExp node)
        {
            loopStack.Push(node);

            loopStack.Pop();
            return null;
        }

        public string Visit(IteratorExp node)
        {
            loopStack.Push(node);

            string sourceExpression = node.Source.Accept(this);
            var prevInsideDynamicEvaluation = insideDynamicEvaluation;            
            insideDynamicEvaluation = true; 
            string bodyExpression = node.Body.Accept(this);            
            insideDynamicEvaluation = prevInsideDynamicEvaluation;

            string iteratorName;
            // HACK: WFJT 
            if (node.IteratorName != "Collect")
            {
                iteratorName = node.IteratorName;
            }
            else
            {
                iteratorName = "collect";
            }
            
            string variablesDef;
            if (loopStack.Count > 1 || variablesDefinedExplicitly)
            {
                variablesDef = "$variables";
            }
            else
            {
                variablesDef = "oclX:vars(.)";
            }

            string apostrophe = insideDynamicEvaluation ? "''" : "'";
            string result = string.Format("oclX:{0}({1}, {5}{2}{5}, {5}{3}{5}, {4})", 
                iteratorName, sourceExpression, node.Iterator[0].Name, bodyExpression, variablesDef, apostrophe);

            loopStack.Pop();
            
            return result;
        }

        public string Visit(OperationCallExp node)
        {
            string[] argumentsStrings = new string[node.Arguments.Count + 1];
            OclExpression[] arguments = new OclExpression[node.Arguments.Count + 1];
            string argTran = node.Source.Accept(this);
            arguments[0] = node.Source;
            argumentsStrings[0] = argTran;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                argTran = node.Arguments[i].Accept(this);
                argumentsStrings[i + 1] = argTran;
                arguments[i + 1] = node.Arguments[i];
            }
            string result = OperationHelper.ToStringWithArgs(node, arguments, argumentsStrings);
            return result;
        }

        public string Visit(PropertyCallExp node)
        {
            if (!(node.ReferredProperty.Tag is PSMAttribute))
            {
                Log.AddWarningTaggedFormat(XPathTranslationLogMessages.UNSAFE_PROPERTY_CALL_EXP, node);
            }
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer);
            
            string xpath = psmPath.ToXPath(!insideDynamicEvaluation);
            return xpath;
        }

        public string Visit(VariableExp node)
        {
            if (node.Type is Class)
            {
                Log.AddWarningTaggedFormat(XPathTranslationLogMessages.UNSAFE_VARIABLE_EXP, 
                    node, node.referredVariable.Name);
            }

            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer);
            string xpath = psmPath.ToXPath(!insideDynamicEvaluation);
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
            loopStack.Clear();
            Log.Clear();
        }

        public string TranslateExpression(OclExpression expression)
        {
            OperationHelper = new OperationHelper();
            OperationHelper.InitStandard();
            OperationHelper.PSMSchema = (PSMSchema) this.OCLScript.Schema;
            OperationHelper.Log = Log;
            TranslatedOclExpression = expression;
            VariableNamer = new VariableNamer();
            return expression.Accept(this);
            //if (!(expression is LoopExp))
            //{
            //    variablesDefinedExplicitly = true;
            //    string expressionT = expression.Accept(this);
            //    insideDynamicEvaluation = true; 
            //    return string.Format("oclX:holds('{0}', oclX:vars(.))", expressionT);
            //}
            //else
            //{
            //    variablesDefinedExplicitly = false;
            //    insideDynamicEvaluation = false; 
            //    return expression.Accept(this);
            //}
        }
    }

    public class ExpressionNotSupportedInXPath: Exception
    {
        public ExpressionNotSupportedInXPath(OclExpression node)
        {
            
        }
    }

   
}