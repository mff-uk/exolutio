using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.ConstraintConversion;
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

        public SchematronSchemaGenerator.TranslationSettings Settings { get; set; }

        private OperationHelper OperationHelper { get; set; }

        public SubexpressionTranslations SubexpressionTranslations { get; private set; }

        public OCLScript OCLScript { get; set; }

        public PSMBridge Bridge { get; set; }

        public ClassifierConstraint OclContext { get; set; }
        
        public Log<OclExpression> Log { get; set; }
        
        protected VariableNamer VariableNamer { get; set; }

        protected OclExpression TranslatedOclExpression { get; set; }

        public abstract bool CanTranslateSelfAsCurrent { get; }

        protected LoopExp GetLoopExpForVariable(VariableExp v)
        {
            return loopStack.LastOrDefault(l => l.Iterator.Any(vd => vd.Name == v.referredVariable.Name));
        }

        public void Visit(ErrorExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public abstract void Visit(IterateExp node);

        public abstract void Visit(IteratorExp node);

        public void Visit(OperationCallExp node)
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

        public void Visit(PropertyCallExp node)
        {
            Visit(node, false);
        }

        public void Visit(PropertyCallExp node, bool isOperationArgument)
        {
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer);
            string xpath = psmPath.ToXPath(CanTranslateSelfAsCurrent);
            if (!isOperationArgument)
            {
                string formatString = OperationHelper.WrapAtomicOperand(node, null, 0);
                SubexpressionTranslations.AddTrivialTranslation(node, string.Format(formatString, xpath));
            }
            else
            {
                SubexpressionTranslations.AddTrivialTranslation(node, xpath);
            }
        }

        public void Visit(VariableExp node)
        {
            PSMPath psmPath = PSMPathBuilder.BuildPSMPath(node, OclContext, VariableNamer);
            string xpath = psmPath.ToXPath(CanTranslateSelfAsCurrent);
            SubexpressionTranslations.AddTrivialTranslation(node, xpath);
        }

        #region not supported yet 

        public abstract void Visit(LetExp node);
        
        public void Visit(TypeExp node)
        {
            // TODO: PSM2XPath: there should be some suport for types in the future
            throw new ExpressionNotSupportedInXPath(node);
        }

        #endregion


        #region structural

        public void Visit(IfExp node)
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

        public void Visit(InvalidLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, "invalid()");
        }

        public void Visit(BooleanLiteralExp node)
        {
            // instead boolean literals, functions are used
            SubexpressionTranslations.AddTrivialTranslation(node, node.Value ? "true()" : "false()");
        }

        public void Visit(CollectionLiteralExp node)
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

        public void Visit(EnumLiteralExp node)
        {
            throw new ExpressionNotSupportedInXPath(node);
        }

        public void Visit(NullLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, "()");
        }

        public void Visit(UnlimitedNaturalLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, node.ToString());
        }

        public void Visit(IntegerLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, node.Value.ToString());
        }

        public void Visit(RealLiteralExp node)
        {
            SubexpressionTranslations.AddTrivialTranslation(node, node.Value.ToString());
        }

        public void Visit(StringLiteralExp node)
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
            expression.Accept(this);
            string result = SubexpressionTranslations.GetSubexpressionTranslation(expression).GetString();
            return result;
        }
    }

    public class PSMOCLtoXPathConverterFunctional : PSMOCLtoXPathConverter
    {
        public override bool CanTranslateSelfAsCurrent
        {
            get { return loopStack.Count == 0; }
        }

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

            string iteratorName;
            // TODO HACK: WFJT 
            if (node.IteratorName != "Collect")
            {
                iteratorName = node.IteratorName;
            }
            else
            {
                iteratorName = "collect";
            }

            TranslationOption option = new TranslationOption();
            if (node.Iterator.Count == 1)
            {
                option.FormatString = string.Format("oclX:{0}({{0}}, function(${1}) {{{{ {{1}} }}}})", iteratorName, node.Iterator[0].Name);
            }
            else
            {
                option.FormatString = string.Format("oclX:{0}N({{0}}, function({1}) {{{{ {{1}} }}}})", iteratorName, node.Iterator.ConcatWithSeparator(vd => "$" + vd.Name, ", "));
            }
            loopStack.Pop();
                       
            SubexpressionTranslations.AddTranslationOption(node, option, node.Source, node.Body);
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
            StringBuilder formatBuilder = new StringBuilder();
            formatBuilder.Append("map{");
            List<OclExpression> subExpressions = new List<OclExpression>();
            foreach (KeyValuePair<string, TupleLiteralPart> kvp in node.Parts)
            {
                kvp.Value.Value.Accept(this);
                formatBuilder.AppendFormat("'{0}' := {{{1}}}, ", kvp.Value.Attribute.Name, subExpressions.Count);
                subExpressions.Add(kvp.Value.Value);
            }
            if (formatBuilder.Length > 0)
            {
                formatBuilder.Length = formatBuilder.Length - 2; 
            }
            formatBuilder.Append("}");
            TranslationOption option = new TranslationOption();
            option.FormatString = formatBuilder.ToString();
            SubexpressionTranslations.AddTranslationOption(node, option, subExpressions.ToArray());
        }
    }

    public class PSMOCLtoXPathConverterDynamic : PSMOCLtoXPathConverter
    {
        private bool insideDynamicEvaluation;

        public override bool CanTranslateSelfAsCurrent
        {
            get { return !insideDynamicEvaluation; }
        }

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

            string iteratorName;
            // TODO HACK: WFJT 
            if (node.IteratorName != "Collect")
            {
                iteratorName = node.IteratorName;
            }
            else
            {
                iteratorName = "collect";
            }
            string apostrophe = insideDynamicEvaluation ? "''" : "'";

            TranslationOption option = new TranslationOption();
            if (node.Iterator.Count == 1)
            {
                option.FormatString = string.Format("oclX:{0}({{0}}, {2}{1}{2}, {2}{{1}}{2}, $variables)", iteratorName, node.Iterator[0].Name, apostrophe);
            }
            else
            {
                option.FormatString = string.Format("oclX:{0}N({{0}}, {2}{1}{2}, {2}{{1}}{2}, $variables)", iteratorName, node.Iterator.ConcatWithSeparator(vd => vd.Name, ", "), apostrophe);
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
    }

    public class ExpressionNotSupportedInXPath: Exception
    {
        public OclExpression Expression
        {
            get; set; 
        }

        public ExpressionNotSupportedInXPath(OclExpression expression)
        {
            Expression = expression; 
        }

        public ExpressionNotSupportedInXPath(OclExpression expression, string message) : base(message)
        {
            Expression = expression; 
        }
    }

    public class SubexpressionTranslations
    {
        private readonly  Dictionary<OclExpression, TranslationOptions> translations = new Dictionary<OclExpression, TranslationOptions>();
        
        private readonly Dictionary<OclExpression, int> selectedTranslations = new Dictionary<OclExpression, int>();

        public Dictionary<OclExpression, TranslationOptions> Translations
        {
            get { return translations; }
        }

        public Dictionary<OclExpression, int> SelectedTranslations
        {
            get { return selectedTranslations; }
        }

        public TranslationOption GetSubexpressionTranslation(OclExpression expression)
        {
            return Translations[expression].Options[SelectedTranslations[expression]];
        }

        public void AddTrivialTranslation(OclExpression expression, string translation)
        {
            TranslationOptions options = new TranslationOptions();
            options.SubexpressionTranslations = this;
            options.TranslatedExpression = expression;
            TranslationOption option = new TranslationOption();
            option.OptionsContainer = options;
            option.FormatString = translation;
            if (this.Translations.ContainsKey(expression))
            {
                throw new InvalidOperationException(string.Format("Expression '{0}' translated reapeatedly. ", expression));
            }
            options.Options.Add(option);
            this.Translations[expression] = options;
            this.SelectedTranslations[expression] = 0;
        }

        public void AddTranslationOption(OclExpression expression, TranslationOption option, params OclExpression[] subExpressions)
        {
            TranslationOptions options;
            if (!this.Translations.ContainsKey(expression))
            {
                options = new TranslationOptions();
                options.SubexpressionTranslations = this;
                options.TranslatedExpression = expression;
                foreach (OclExpression subExpression in subExpressions)
                {
                    options.SubExpressions.Add(subExpression);
                }
                this.Translations[expression] = options;
            }
            else
            {
                options = this.Translations[expression];
                if (subExpressions != null && subExpressions.Length > 0)
                {
                    if (subExpressions.Length != options.SubExpressions.Count)
                    {
                        throw new InvalidOperationException(string.Format("Inconsistency of subexpressions when adding options for '{0}'", expression));
                    }

                    for (int index = 0; index < subExpressions.Length; index++)
                    {
                        OclExpression subExpression = subExpressions[index];
                        if (subExpression != options.SubExpressions[index])
                            throw new InvalidOperationException(string.Format("Inconsistency of subexpressions when adding options for '{0}'", expression));
                    }
                }
            }

            option.OptionsContainer = options;
            this.SelectedTranslations[expression] = 0;
            options.Options.Add(option);
        }
    }

    public class TranslationOptions
    {
        private readonly List<TranslationOption> options = new List<TranslationOption>();
        private readonly List<OclExpression> subExpressions = new List<OclExpression>();

        public OclExpression TranslatedExpression { get; set; }

        public SubexpressionTranslations SubexpressionTranslations { get; set; }

        public List<TranslationOption> Options { get { return options; } }

        public List<OclExpression> SubExpressions { get { return subExpressions; } }

        public string GetString(int option = 0)
        {
            return Options[option].GetString();
        }
        
        public override string ToString()
        {
            throw new Exception("TO STRING IN TRANSLATIONOPTIONS");
            //return null;
        }
    }

    public class TranslationOption
    {
        public TranslationOptions OptionsContainer
        {
            get; set;
        }

        public string FormatString { get; set; }

        public override string ToString()
        {
            throw new Exception("TO STRING IN TRANSLATIONOPTION");
            //return null;
        }

        public string GetString()
        {
            object[] translatedSubexpressions = new object[OptionsContainer.SubExpressions.Count];
            for (int index = 0; index < OptionsContainer.SubExpressions.Count; index++)
            {
                OclExpression subExpression = OptionsContainer.SubExpressions[index];
                translatedSubexpressions[index] = OptionsContainer.SubexpressionTranslations.GetSubexpressionTranslation(subExpression).GetString();
            }
            return string.Format(FormatString, translatedSubexpressions); 
        }
    }
}
