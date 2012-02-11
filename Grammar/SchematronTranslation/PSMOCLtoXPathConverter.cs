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
    public class PSMOCLtoXPathConverter : IAstVisitor<ConvertedExp>
    {
        protected readonly Stack<LoopExp> loopStack
            = new Stack<LoopExp>();

        private bool variablesDefinedExplicitly { get; set; }

        private bool insideDynamicEvaluation = false;

        public SchematronSchemaGenerator.TranslationSettings Settings { get; set; }

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

        public ConvertedExp Visit(ErrorExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public ConvertedExp Visit(IterateExp node)
        {
            loopStack.Push(node);

            loopStack.Pop();
            return null;
        }

        public ConvertedExp Visit(IteratorExp node)
        {
            loopStack.Push(node);

            string sourceExpression = node.Source.Accept(this).GetString();
            var prevInsideDynamicEvaluation = insideDynamicEvaluation;            
            insideDynamicEvaluation = true; 
            string bodyExpression = node.Body.Accept(this).GetString();            
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
                variablesDef = "$variables";
            }

            string apostrophe = insideDynamicEvaluation ? "''" : "'";
            string result;
            if (node.Iterator.Count == 1)
            {

                result = string.Format("oclX:{0}({1}, {5}{2}{5}, {5}{3}{5}, {4})",
                                       iteratorName, sourceExpression, node.Iterator[0].Name, bodyExpression,
                                       variablesDef, apostrophe);
            }
            else
            {
                result = string.Format("oclX:{0}N({1}, {5}{2}{5}, {5}{3}{5}, {4})",
                                       iteratorName, sourceExpression, node.Iterator.ConcatWithSeparator(", "), bodyExpression,
                                       variablesDef, apostrophe);
            }
            loopStack.Pop();
            
            return new ConvertedExp(result);
        }

        public ConvertedExp Visit(OperationCallExp node)
        {
            ConvertedExp[] argumentsStrings = new ConvertedExp[node.Arguments.Count + 1];
            OclExpression[] arguments = new OclExpression[node.Arguments.Count + 1];
            ConvertedExp argTran = node.Source.Accept(this);
            arguments[0] = node.Source;
            argumentsStrings[0] = argTran;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                argTran = node.Arguments[i].Accept(this);
                argumentsStrings[i + 1] = argTran;
                arguments[i + 1] = node.Arguments[i];
            }
            string result = OperationHelper.ToStringWithArgs(node, arguments, argumentsStrings);
            return new ConvertedExp(result);
        }

        public ConvertedExp Visit(PropertyCallExp node)
        {
            if (!(node.ReferredProperty.Tag is PSMAttribute))
            {
                Log.AddWarningTaggedFormat(XPathTranslationLogMessages.UNSAFE_PROPERTY_CALL_EXP, node);
            }
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer);
            
            string xpath = psmPath.ToXPath(!insideDynamicEvaluation);
            return new ConvertedExp(xpath, OperationHelper.IsXPathAtomic(node.Type));
        }

        public ConvertedExp Visit(VariableExp node)
        {
            if (node.Type is Class)
            {
                Log.AddWarningTaggedFormat(XPathTranslationLogMessages.UNSAFE_VARIABLE_EXP, 
                    node, node.referredVariable.Name);
            }

            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer);
            string xpath = psmPath.ToXPath(!insideDynamicEvaluation);
            ConvertedExp convertedExp = new ConvertedExp(xpath, OperationHelper.IsXPathAtomic(node.Type));
            return convertedExp;
        }

        #region not supported yet 

        public ConvertedExp Visit(LetExp node)
        {
            // TODO: PSM2XPath: there should be some suport for let expression in the future
            throw new ExpressionNotSupportedInXPath(node);
        }

        public ConvertedExp Visit(TypeExp node)
        {
            // TODO: PSM2XPath: there should be some suport for types in the future
            throw new ExpressionNotSupportedInXPath(node);
        }

        #endregion


        #region structural

        public ConvertedExp Visit(IfExp node)
        {
            string cond = node.Condition.Accept(this).GetString();
            string thenExpr = node.ThenExpression.Accept(this).GetString();
            string elseExpr = node.ElseExpression.Accept(this).GetString();
            return new ConvertedExp(string.Format("if ({0}) then {1} else {2}", cond, thenExpr, elseExpr));
        }

        #endregion 

        #region literals

        public ConvertedExp Visit(TupleLiteralExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public ConvertedExp Visit(InvalidLiteralExp node)
        {
            return null;
        }

        public ConvertedExp Visit(BooleanLiteralExp node)
        {
            // instead boolean literals, functions are used
            return new ConvertedExp(node.Value ? "true()" : "false()");
        }

        public ConvertedExp Visit(CollectionLiteralExp node)
        {
            // TODO: PSM2XPath: some support for collection linterals
            throw new ExpressionNotSupportedInXPath(node);
        }

        public ConvertedExp Visit(EnumLiteralExp node)
        {
            return null;
        }

        public ConvertedExp Visit(NullLiteralExp node)
        {
            return new ConvertedExp("()");
        }

        public ConvertedExp Visit(UnlimitedNaturalLiteralExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public ConvertedExp Visit(IntegerLiteralExp node)
        {
            return new ConvertedExp(node.ToString());
        }

        public ConvertedExp Visit(RealLiteralExp node)
        {
            return new ConvertedExp(node.ToString());
        }

        public ConvertedExp Visit(StringLiteralExp node)
        {
            return new ConvertedExp(string.Format("'{0}'", node.Value));
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
            OperationHelper.ExplicitCastAtomicOperands = !Settings.SchemaAware;
            OperationHelper.PSMBridge = Bridge;
            TranslatedOclExpression = expression;
            VariableNamer = new VariableNamer();
            return expression.Accept(this).GetString();
        }
    }

    public class ExpressionNotSupportedInXPath: Exception
    {
        public ExpressionNotSupportedInXPath(OclExpression node)
        {
            
        }
    }

    public class ConvertedExp
    {
        public string RawString { get; set; }

        public bool AddDataCall { get; set; }
    
        public string GetString()
        {
            if (AddDataCall)
            {
                return string.Format("xs:data({0})", RawString);
            }
            else
            {
                return RawString;
            }
        }

        public ConvertedExp(string rawString, bool addDataCall = false)
        {
            this.RawString = rawString;
            AddDataCall = addDataCall;
        }

        public override string ToString()
        {
            //throw new Exception("TO STRING IN CONVERTEDEXP");
            return null;
        }
    }
}