using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.Utils;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    /// <summary>
    /// Translats valid OCL expression (invariant) into an XPath expression 
    /// </summary>
    public abstract class PSMOCLtoXPathConverter : IAstVisitor
    {
        protected readonly Stack<LoopExp> loopStack
            = new Stack<LoopExp>();

        public TranslationSettings Settings { get; set; }

        protected OperationHelper OperationHelper { get; set; }

        public SubexpressionTranslations SubexpressionTranslations { get; private set; }

        public OCLScript OCLScript { get; set; }

        public PSMBridge Bridge { get; set; }

        public ClassifierConstraint OclContext { get; set; }
        
        public Log<OclExpression> Log { get; set; }
        
        protected VariableNamer VariableNamer { get; set; }

        protected OclExpression TranslatedOclExpression { get; set; }
       
        public virtual void Visit(ErrorExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public abstract void Visit(IterateExp node);

        public abstract void Visit(IteratorExp node);

        public virtual void Visit(OperationCallExp node)
        {
            OclExpression[] arguments = new OclExpression[node.Arguments.Count + 1];
            if (node.Source is PropertyCallExp)
                this.Visit((PropertyCallExp)node.Source, true);
            else
                node.Source.Accept(this);
            
            arguments[0] = node.Source;
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                if (node.Arguments[i] is PropertyCallExp)
                    this.Visit((PropertyCallExp)node.Arguments[i], true);
                else
                    node.Arguments[i].Accept(this);
                arguments[i + 1] = node.Arguments[i];
            }

            TranslationOption option = new TranslationOption();
            option.FormatString = OperationHelper.CreateBasicFormatString(node, arguments);
            this.SubexpressionTranslations.AddTranslationOption(node, option, arguments);
        }

        public virtual void Visit(PropertyCallExp node)
        {
            Visit(node, false);
        }

        public virtual void Visit(PropertyCallExp node, bool isOperationArgument)
        {
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer, TupleLiteralToXPath, GenericExpressionLiteralToXPath);
            string xpath = psmPath.ToXPath(delayFirstVariableStep:true);
            if (!isOperationArgument)
            {
                string wrapString = OperationHelper.WrapAtomicOperand(node, null, 0);
                xpath = string.Format(wrapString, xpath);
            }

            var steps = psmPath.Steps.OfType<IPSMPathStepWithCardinality>().Where(s => s.Lower == 0);
            if (steps.Any())
            {
                Log.AddWarningTaggedFormat("Navigation using '{0}' may result in 'null', which will be an empty sequence in XPath. ", node, steps.ConcatWithSeparator(s => s.ToXPath(), ",") );
            }

            TranslationOption option = new TranslationOption();
            if (psmPath.StartingVariableExp != null)
            {
                option.ContextVariableSubstitution = true;
                option.StartingVariable = psmPath.StartingVariableExp.referredVariable;                
            }
            option.FormatString = xpath;
            SubexpressionTranslations.AddTranslationOption(node, option, psmPath.SubExpressions.ToArray());
        }

        public virtual void Visit(VariableExp node)
        {
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer, TupleLiteralToXPath, GenericExpressionLiteralToXPath);
            string xpath = psmPath.ToXPath(delayFirstVariableStep:true);

            TranslationOption option = new TranslationOption();
            if (psmPath.StartingVariableExp != null)
            {
                option.ContextVariableSubstitution = true;
                option.StartingVariable = psmPath.StartingVariableExp.referredVariable;
            }
            option.FormatString = xpath;
            SubexpressionTranslations.AddTranslationOption(node, option, psmPath.SubExpressions.ToArray());
        }

        #region not supported yet 

        public abstract void Visit(LetExp node);
        
        public virtual void Visit(TypeExp node)
        {
            // TODO: PSM2XPath: there should be some suport for types in the future
            throw new ExpressionNotSupportedInXPath(node);
        }

        #endregion
        
        #region structural

        public virtual void Visit(IfExp node)
        {
            node.Condition.Accept(this);
            node.ThenExpression.Accept(this);
            node.ElseExpression.Accept(this);
            TranslationOption option = new TranslationOption();
            option.FormatString = "if ({0}) then {1} else {2}";
            SubexpressionTranslations.AddTranslationOption(node, option, node.Condition, node.ThenExpression, node.ElseExpression);
        }

        #endregion 

        #region literals

        public abstract void Visit(TupleLiteralExp node);

        public virtual void Visit(InvalidLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, "invalid()");
        }

        public virtual void Visit(BooleanLiteralExp node)
        {
            // instead boolean literals, functions are used
            SubexpressionTranslations.AddTrivialTranslation(node, node.Value ? "true()" : "false()");
        }

        public virtual void Visit(CollectionLiteralExp node)
        {
            StringBuilder formatBuilder = new StringBuilder();
            List<OclExpression> subExpressions = new List<OclExpression>();

            // opening parenthesis of xpath sequence literal
            formatBuilder.Append("(");
            foreach (CollectionLiteralPart clp in node.Parts)
            {
                #region helper function
                Action<OclExpression> appendOne = delegate(OclExpression exp)
                                                      {
                                                          if (exp is LiteralExp)
                                                          {
                                                              formatBuilder.Append("{");
                                                              formatBuilder.Append(subExpressions.Count);
                                                              formatBuilder.Append("}");
                                                          }
                                                          else
                                                          {
                                                              formatBuilder.Append("{(");
                                                              formatBuilder.Append(subExpressions.Count);
                                                              formatBuilder.Append(")}");
                                                          }
                                                      };
                #endregion

                if (clp is CollectionRange)
                {
                    CollectionRange range = ((CollectionRange) clp);
                    range.First.Accept(this);
                    range.Last.Accept(this);

                    appendOne(range.First);
                    subExpressions.Add(range.First);
                    formatBuilder.Append(" to ");
                    appendOne(range.Last);
                    subExpressions.Add(range.Last);
                }

                if (clp is CollectionItem)
                {
                    CollectionItem collectionItem = (CollectionItem) clp;
                    collectionItem.Item.Accept(this);
                    appendOne(collectionItem.Item);
                    subExpressions.Add(collectionItem.Item);
                }
                formatBuilder.Append(", ");
            }

            if (formatBuilder.Length > 0) // remove last comma
            {
                formatBuilder.Length = formatBuilder.Length - 2;
            }
            
            // closing parenthesis of xpath sequence literal
            formatBuilder.Append(")");

            TranslationOption option = new TranslationOption();
            option.FormatString = formatBuilder.ToString();
            SubexpressionTranslations.AddTranslationOption(node, option, subExpressions.ToArray());
        }

        public virtual void Visit(EnumLiteralExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public virtual void Visit(NullLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, "()");
        }

        public virtual void Visit(UnlimitedNaturalLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, node.ToString());
        }

        public virtual void Visit(IntegerLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, node.Value.ToString());
        }

        public virtual void Visit(RealLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, node.Value.ToString());
        }

        public virtual void Visit(StringLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, string.Format("'{0}'", node.Value));
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
            OperationHelper.PSMSchema = (PSMSchema) this.OCLScript.Schema;
            OperationHelper.Log = Log;
            OperationHelper.ExplicitCastAtomicOperands = !Settings.SchemaAware;
            OperationHelper.PSMBridge = Bridge;
            OperationHelper.InitStandard();
            TranslatedOclExpression = expression;
            VariableNamer = new VariableNamer();
            SubexpressionTranslations = new SubexpressionTranslations();
            SubexpressionTranslations.Log = Log; 
            expression.Accept(this);
            //SubexpressionTranslations.ContextVariableStack.Push(OclContext.Self);
            SubexpressionTranslations.SelfVariableDeclaration = OclContext.Self;
            string result = SubexpressionTranslations.GetSubexpressionTranslation(expression).GetString(true);
            //SubexpressionTranslations.ContextVariableStack.Pop();
            return result;
        }

        protected abstract string TupleLiteralToXPath(TupleLiteralExp tupleLiteral, List<OclExpression> subExpressions);

        protected virtual string GenericExpressionLiteralToXPath(OclExpression expression, List<OclExpression> subExpressions)
        {
            expression.Accept(this);
            //TranslationOption option = SubexpressionTranslations.GetSubexpressionTranslation(expression);
            subExpressions.Add(expression);
            return "{0}";

            //StringBuilder formatBuilder = new StringBuilder();
            //formatBuilder.Append("(map{{");
            //foreach (KeyValuePair<string, TupleLiteralPart> kvp in tupleLiteral.Parts)
            //{
            //    kvp.Value.Value.Accept(this);
            //    formatBuilder.AppendFormat("'{0}' := {{{1}}}, ", kvp.Value.Attribute.Name, subExpressions.Count);
            //    subExpressions.Add(kvp.Value.Value);
            //}
            //if (formatBuilder.Length > 0)
            //{
            //    formatBuilder.Length = formatBuilder.Length - 2; 
            //}
            //formatBuilder.Append("}})");
            //TranslationOption option = new TranslationOption();
            //option.FormatString = formatBuilder.ToString();
            //SubexpressionTranslations.AddTranslationOption(tupleLiteral, option, subExpressions.ToArray());
            //return option.FormatString;
        }
    }

    public class PSMOCLtoXPathConverterFunctional : PSMOCLtoXPathConverter
    {
        private readonly Dictionary<string, Action<IteratorExp>> predefinedIteratorExpressionRewritings = new Dictionary<string, Action<IteratorExp>>();
        private Dictionary<string, Action<IteratorExp>> PredefinedIteratorExpressionRewritings { get { return predefinedIteratorExpressionRewritings; } }
        private readonly Dictionary<OperationInfo, Action<OperationCallExp, OperationInfo>> operationsRewritings = new Dictionary<OperationInfo, Action<OperationCallExp, OperationInfo>>();
        private Dictionary<OperationInfo, Action<OperationCallExp, OperationInfo>> OperationRewritings { get { return operationsRewritings; } }

        public PSMOCLtoXPathConverterFunctional()
        {
            if (System.Environment.MachineName.Contains("TRUPIK"))
            {
                PredefinedIteratorExpressionRewritings.Add("forAll", AddForAllOptions);
                PredefinedIteratorExpressionRewritings.Add("exists", AddExistsOptions);
                PredefinedIteratorExpressionRewritings.Add("collect", AddCollectOptions);
                PredefinedIteratorExpressionRewritings.Add("select", AddSelectOptions);
                PredefinedIteratorExpressionRewritings.Add("reject", AddRejectOptions);
                PredefinedIteratorExpressionRewritings.Add("one", AddOneOptions);
                PredefinedIteratorExpressionRewritings.Add("any", AddAnyOptions);
                PredefinedIteratorExpressionRewritings.Add("closure", AddClosureOptions);
            }
        }

        #region rewritings

        #region iterators

        private void AddAnyOptions(IteratorExp node)
        {
            if (node.Iterator.Count == 1)
            {
                TranslationOption optionFor = new TranslationOption();
                optionFor.FormatString = string.Format("(for ${0} in {{0}} return if ({{1}}) then ${0} else ())[1]",
                                                       node.Iterator[0].Name);
                optionFor.ParenthesisWhenNotTopLevel = true;
                SubexpressionTranslations.AddTranslationOption(node, optionFor);

                bool canRewrite = true;
                SubexpressionCollector collector = new SubexpressionCollector();
                collector.Visit(node);
                List<LoopExp> loopSubExps = collector.Expressions.OfType<LoopExp>().ToList();
                foreach (LoopExp nestedLoopExp in loopSubExps)
                {
                    collector.Clear();
                    nestedLoopExp.Accept(collector);
                    if (collector.ReferredVariables.Contains(node.Iterator[0]))
                    {
                        canRewrite = false;
                        break;
                    }
                }

                if (canRewrite)
                {
                    TranslationOption optionFilter = new TranslationOption();
                    optionFilter.ContextVariableForSubExpressions = node.Iterator[0];
                    optionFilter.FormatString = string.Format("({{0}}[{{1}}])[1]");
                    optionFilter.ParenthesisWhenNotTopLevel = true;
                    SubexpressionTranslations.AddTranslationOption(node, optionFilter);
                }
            }
        }

        private void AddOneOptions(IteratorExp node)
        {
            if (node.Iterator.Count == 1)
            {
                TranslationOption optionFor = new TranslationOption();
                optionFor.FormatString =
                    string.Format("count(for ${0} in {{0}} return if ({{1}}) then ${0} else ()) eq 1",
                                  node.Iterator[0].Name);
                optionFor.ParenthesisWhenNotTopLevel = true;
                SubexpressionTranslations.AddTranslationOption(node, optionFor);

                bool canRewrite = true;
                SubexpressionCollector collector = new SubexpressionCollector();
                collector.Visit(node);
                List<LoopExp> loopSubExps = collector.Expressions.OfType<LoopExp>().ToList();
                foreach (LoopExp nestedLoopExp in loopSubExps)
                {
                    collector.Clear();
                    nestedLoopExp.Accept(collector);
                    if (collector.ReferredVariables.Contains(node.Iterator[0]))
                    {
                        canRewrite = false;
                        break;
                    }
                }

                if (canRewrite)
                {
                    TranslationOption optionFilter = new TranslationOption();
                    optionFilter.ContextVariableForSubExpressions = node.Iterator[0];
                    optionFilter.FormatString = string.Format("count({{0}}[{{1}}]) eq 1");
                    optionFilter.ParenthesisWhenNotTopLevel = true;
                    SubexpressionTranslations.AddTranslationOption(node, optionFilter);
                }
            }
        }

        private void AddRejectOptions(IteratorExp node)
        {
            // select and reject differ only in not() applied in filter
            AddSelectRejectOptions(node, false);
        }

        private void AddSelectOptions(IteratorExp node)
        {
            // select and reject differ only in not() applied in filter
            AddSelectRejectOptions(node, true);
        }

        private void AddSelectRejectOptions(IteratorExp node, bool select)
        {
            // select and reject differ only in not() applied in filter
            if (node.Iterator.Count == 1)
            {
                TranslationOption optionFor = new TranslationOption();
                optionFor.ParenthesisWhenNotTopLevel = true;
                if (select)
                    optionFor.FormatString = string.Format("for ${0} in {{0}} return if ({{1}}) then ${0} else ()",
                                                           node.Iterator[0].Name);
                else
                    optionFor.FormatString = string.Format(
                        "for ${0} in {{0}} return if (not({{1}})) then ${0} else ()", node.Iterator[0].Name);
                SubexpressionTranslations.AddTranslationOption(node, optionFor);    

                /*
                 * this option can be used only when there is no iterator in body, which references the current iterator variable,
                 * because there is no XPath variable corresponding to the iterator variable (context is used instead). 
                 */
                {
                    bool canRewrite = true;
                    SubexpressionCollector collector = new SubexpressionCollector();
                    collector.Visit(node);
                    List<LoopExp> loopSubExps = collector.Expressions.OfType<LoopExp>().ToList();
                    foreach (LoopExp nestedLoopExp in loopSubExps)
                    {
                        collector.Clear();
                        nestedLoopExp.Accept(collector);
                        if (collector.ReferredVariables.Contains(node.Iterator[0]))
                        {
                            canRewrite = false;
                            break;
                        }
                    }

                    if (canRewrite)
                    {
                        TranslationOption optionFilter = new TranslationOption();
                        optionFilter.ContextVariableForSubExpressions = node.Iterator[0];
                        if (select)
                            optionFilter.FormatString = string.Format("{{0}}[{{1}}]");
                        else
                            optionFilter.FormatString = string.Format("{{0}}[not({{1}})]");
                        SubexpressionTranslations.AddTranslationOption(node, optionFilter);
                    }
                    else // translation with let
                    {
                        TranslationOption optionFilterLet = new TranslationOption();
                        optionFilterLet.ContextVariableForSubExpressions = node.Iterator[0];
                        if (select)
                            optionFilterLet.FormatString = string.Format("{{0}}[let ${0} := . return {{1}}]",
                                                                         node.Iterator[0].Name);
                        else
                            optionFilterLet.FormatString = string.Format("{{0}}[let ${0} := . return not({{1}})]",
                                                                         node.Iterator[0].Name);
                        SubexpressionTranslations.AddTranslationOption(node, optionFilterLet);
                    }
                }
            }
        }

        private void AddForAllOptions(IteratorExp node)
        {
            if (node.Iterator.Count == 1)
            {
                TranslationOption option = new TranslationOption();
                option.ParenthesisWhenNotTopLevel = true;
                option.FormatString = string.Format("every ${0} in {{0}} satisfies {{1}}", node.Iterator[0].Name);
                SubexpressionTranslations.AddTranslationOption(node, option);
            }
        }

        private void AddExistsOptions(IteratorExp node)
        {
            if (node.Iterator.Count == 1)
            {
                TranslationOption option = new TranslationOption();
                option.ParenthesisWhenNotTopLevel = true;
                option.FormatString = string.Format("some ${0} in {{0}} satisfies {{1}}", node.Iterator[0].Name);
                SubexpressionTranslations.AddTranslationOption(node, option);
            }
        }

        private void AddCollectOptions(IteratorExp node)
        {
            if (node.Iterator.Count == 1)
            {
                TranslationOption option = new TranslationOption();
                option.ParenthesisWhenNotTopLevel = true;
                option.FormatString = string.Format("for ${0} in {{0}} return {{1}}", node.Iterator[0].Name);
                SubexpressionTranslations.AddTranslationOption(node, option);

                /* when the body of collect is a navigation that can be chained, replace it */
                if (node.Body is PropertyCallExp)
                {
                    PSMPath path = PSMPathBuilder.BuildPSMPath((PropertyCallExp) node.Body, OclContext, VariableNamer,
                                                               TupleLiteralToXPath, GenericExpressionLiteralToXPath);
                    if (PathsJoinable(path, node))
                    {
                        TranslationOption option2 = new TranslationOption();
                        option2.FormatString = string.Format("{{0}}{0}", path.ToXPath(withoutFirstStep: true));
                        SubexpressionTranslations.AddTranslationOption(node, option2);
                    }
                }
            }
        }

        private void AddClosureOptions(IteratorExp node)
        {
            if (node.Iterator.Count == 1)
            {
                if (node.Body is PropertyCallExp)
                {
                    PSMPath path = PSMPathBuilder.BuildPSMPath((PropertyCallExp)node.Body, OclContext, VariableNamer,
                                                               TupleLiteralToXPath, GenericExpressionLiteralToXPath);
                    
                    // oclX:closure(departments/department, function($c) { $c/subdepartments/department })/name
                    //              departments/department/descendant-or-self::department
                    /*
                     * departments/department + /subdepartments/department
                     */                     
                    if (PathsJoinable(path, node) && path.IsDownwards && path.Steps.Count > 0)
                    {
                        TranslationOption descendantOption = new TranslationOption();
                        string lastStep = path.Steps.Last().ToXPath();
                        if (lastStep.StartsWith("/")) 
                            lastStep = lastStep.Substring(1);
                        descendantOption.FormatString = string.Format("{{0}}/descendant-or-self::{0}", lastStep);
                        SubexpressionTranslations.AddTranslationOption(node, descendantOption);
                    }
                }
            }
        }

        private bool PathsJoinable(PSMPath startingPath, IteratorExp node)
        {
            return (startingPath.StartingVariableExp.referredVariable == node.Iterator[0] &&
                    startingPath.Steps.Count > 1);
        }

        #endregion

        #region operations
        
        public void AddFirstRewritings(OperationCallExp operationCallExp, OperationInfo operationInfo)
        {
            AddCollectionAccessRewritings(operationCallExp, "1", operationInfo);
        }

        private void AddAtRewritings(OperationCallExp operationCallExp, OperationInfo operationInfo)
        {
            AddCollectionAccessRewritings(operationCallExp, "{{1}}", operationInfo);
        }

        private void AddLastRewritings(OperationCallExp operationCallExp, OperationInfo operationInfo)
        {
            AddCollectionAccessRewritings(operationCallExp, "{{last()}}", operationInfo);
        }

        private void AddCollectionAccessRewritings(OperationCallExp operationCallExp, string accesExpr, OperationInfo operationInfo)
        {
            TranslationOption optionIndex = new TranslationOption();
            SubexpressionTranslations.AddTranslationOption(operationCallExp, optionIndex);
            // XPath filter changes context - no variable corresponds to the context in this case
            optionIndex.ContextVariableForSubExpressions = null; 
            optionIndex.ContextVariableChangeOnlyIn = 1;
            // in some special cases, parentheses can be omitted
            bool omitParentheses = false;
            if (operationCallExp.Source is LiteralExp)
                omitParentheses = true;
            if (operationCallExp.Source is VariableExp)
                omitParentheses = true;
            if (!operationInfo.IsXPathInfix && !operationInfo.IsXPathPrefix &&
                !operationInfo.IsAmong(OperationHelper.firstOperationInfo, OperationHelper.lastOperationInfo, OperationHelper.atOperationInfo))
            {
                omitParentheses = true; 
            }

            if (omitParentheses)
            {
                optionIndex.FormatString = string.Format("({{0}})[{0}]", accesExpr);
            }
            else
            {
                optionIndex.FormatString = string.Format("({{0}})[{0}]", accesExpr);
            }

            optionIndex.LogMessagesWhenSelected = new List<LogMessage<OclExpression>>();
            optionIndex.LogMessagesWhenSelected.Add(new LogMessage<OclExpression>() { Tag = operationCallExp, MessageText = 
                "Using indexing returns empty sequence when indexes are out of bounds (no error reported, unlike in OCL). "});
        }

        #endregion

        #endregion

        public override void Visit(IterateExp node)
        {
            loopStack.Push(node);
            

            node.Source.Accept(this);
            node.Body.Accept(this);
            node.Result.Value.Accept(this);

            TranslationOption option = new TranslationOption();
            option.FormatString = string.Format("oclX:iterate({{0}}, {{1}}, function(${0}, ${1}) {{{{ {{2}} }}}})", node.Iterator[0].Name, node.Result.Name);
            SubexpressionTranslations.AddTranslationOption(node, option, node.Source, node.Result.Value, node.Body);
            
            loopStack.Pop();
        }

        public override void Visit(IteratorExp node)
        {
            loopStack.Push(node);
            
            node.Source.Accept(this);
            node.Body.Accept(this);

            TranslationOption option = new TranslationOption();
            if (node.Iterator.Count == 1)
            {
                option.FormatString = string.Format("oclX:{0}({{0}}, function(${1}) {{{{ {{1}} }}}})", node.IteratorName, node.Iterator[0].Name);
            }
            else
            {
                option.FormatString = string.Format("oclX:{0}N({{0}}, function({1}) {{{{ {{1}} }}}})", node.IteratorName, node.Iterator.ConcatWithSeparator(vd => "$" + vd.Name, ", "));
            }

            loopStack.Pop();

            SubexpressionTranslations.AddTranslationOption(node, option, node.Source, node.Body);
            if (PredefinedIteratorExpressionRewritings.ContainsKey(node.IteratorName))
            {
                PredefinedIteratorExpressionRewritings[node.IteratorName](node);
            }
        }

        public override void Visit(LetExp node)
        {
            node.Variable.Value.Accept(this);
            node.InExpression.Accept(this);
            TranslationOption translationOption = new TranslationOption();
            translationOption.FormatString = string.Format("let ${0} := {{0}} return {{1}}", node.Variable.Name);
            SubexpressionTranslations.AddTranslationOption(node, translationOption, node.Variable.Value, node.InExpression);
        }

        public override void Visit(TupleLiteralExp node)
        {
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer, TupleLiteralToXPath, GenericExpressionLiteralToXPath);
            string xpath = psmPath.ToXPath();

            TranslationOption option = new TranslationOption();
            option.FormatString = xpath;
            SubexpressionTranslations.AddTranslationOption(node, option, psmPath.SubExpressions.ToArray());
        }

        protected override string TupleLiteralToXPath(TupleLiteralExp tupleLiteral, List<OclExpression> subExpressions)
        {
            StringBuilder formatBuilder = new StringBuilder();
            formatBuilder.Append("(map{{");
            foreach (KeyValuePair<string, TupleLiteralPart> kvp in tupleLiteral.Parts)
            {
                kvp.Value.Value.Accept(this);
                formatBuilder.AppendFormat("'{0}' := {{{1}}}, ", kvp.Value.Attribute.Name, subExpressions.Count);
                subExpressions.Add(kvp.Value.Value);
            }
            if (formatBuilder.Length > 0)
            {
                formatBuilder.Length = formatBuilder.Length - 2; 
            }
            formatBuilder.Append("}})");
            TranslationOption option = new TranslationOption();
            option.FormatString = formatBuilder.ToString();
            SubexpressionTranslations.AddTranslationOption(tupleLiteral, option, subExpressions.ToArray());
            return option.FormatString;
        }

        public override void Visit(OperationCallExp node)
        {
            base.Visit(node);

            if (OperationRewritings.IsEmpty())
            {
                OperationRewritings[OperationHelper.firstOperationInfo] = AddFirstRewritings;
                OperationRewritings[OperationHelper.lastOperationInfo] = AddLastRewritings;
                OperationRewritings[OperationHelper.atOperationInfo] = AddAtRewritings;
            }

            TranslationOption standardTranslation = SubexpressionTranslations.GetSubexpressionTranslation(node);
            OperationInfo? operationInfo = OperationHelper.LookupOperation(node, standardTranslation.OptionsContainer.SubExpressions.ToArray());
            if (operationInfo != null && OperationRewritings.ContainsKey(operationInfo.Value))
            {
                OperationRewritings[operationInfo.Value](node, operationInfo.Value);
            }
        }
    }

    public class PSMOCLtoXPathConverterDynamic : PSMOCLtoXPathConverter
    {
        private bool insideDynamicEvaluation;

        public override void Visit(IterateExp node)
        {
            loopStack.Push(node);
            
            node.Source.Accept(this);
            node.Result.Value.Accept(this);
            var prevInsideDynamicEvaluation = insideDynamicEvaluation;
            insideDynamicEvaluation = true;
            node.Body.Accept(this);
            insideDynamicEvaluation = prevInsideDynamicEvaluation;

            string apostrophe = insideDynamicEvaluation ? "''" : "'";
            TranslationOption option = new TranslationOption();
            option.FormatString = string.Format("oclX:iterate({{0}}, {2}{0}{2}, {2}{1}{2}, {2}{{1}}{2}, {2}{{2}}{2}, $variables)", node.Iterator[0].Name, node.Result.Name, apostrophe);
            
            loopStack.Pop();

            SubexpressionTranslations.AddTranslationOption(node, option, node.Source, node.Result.Value, node.Body);
        }

        public override void Visit(IteratorExp node)
        {
            loopStack.Push(node);
            
            node.Source.Accept(this);
            var prevInsideDynamicEvaluation = insideDynamicEvaluation;
            insideDynamicEvaluation = true;
            node.Body.Accept(this);
            insideDynamicEvaluation = prevInsideDynamicEvaluation;

            string apostrophe = insideDynamicEvaluation ? "''" : "'";

            TranslationOption option = new TranslationOption();
            if (node.Iterator.Count == 1)
            {
                option.FormatString = string.Format("oclX:{0}({{0}}, {2}{1}{2}, {2}{{1}}{2}, $variables)", node.IteratorName, node.Iterator[0].Name, apostrophe);
            }
            else
            {
                option.FormatString = string.Format("oclX:{0}N({{0}}, {2}{1}{2}, {2}{{1}}{2}, $variables)", node.IteratorName, node.Iterator.ConcatWithSeparator(vd => vd.Name, ", "), apostrophe);
            }
            SubexpressionTranslations.AddTranslationOption(node, option, node.Source, node.Body);

            loopStack.Pop();
        }

        public override void Visit(LetExp node)
        {
            node.Variable.Value.Accept(this);
            node.InExpression.Accept(this);
            TranslationOption translationOption = new TranslationOption();
            translationOption.FormatString = string.Format("for ${0} in {{0}} return {{1}}", node.Variable.Name);
            SubexpressionTranslations.AddTranslationOption(node, translationOption, node.Variable.Value, node.InExpression);
        }

        public override void Visit(TupleLiteralExp node)
        {
            throw new ExpressionNotSupportedInXPath(node, "Tuples are supported only in 'functional' schemas. ");
        }

        protected override string TupleLiteralToXPath(TupleLiteralExp tupleLiteral, List<OclExpression> subExpressions)
        {
            throw new ExpressionNotSupportedInXPath(tupleLiteral, "Tuples are supported only in 'functional' schemas. ");
        }
    }

   
}
