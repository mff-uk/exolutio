using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Types;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class SubexpressionTranslations
    {
        public SubexpressionTranslations()
        {
            
        }

        private readonly  Dictionary<OclExpression, TranslationOptions> translations = new Dictionary<OclExpression, TranslationOptions>();

        public Dictionary<OclExpression, TranslationOptions> Translations
        {
            get { return translations; }
        }

        public IEnumerable<TranslationOptions> TranslationOptionsWithMorePossibilities
        {
            get { return Translations.Where(kvp => kvp.Value.Options.Count > 1).Select(o => o.Value); }        
        }

        private readonly Dictionary<OclExpression, int> selectedTranslations = new Dictionary<OclExpression, int>();

        public Dictionary<OclExpression, int> SelectedTranslations
        {
            get { return selectedTranslations; }
        }

        public VariableDeclaration SelfVariableDeclaration { get; set; }

        public Log<OclExpression> Log { get; set; }

        public TranslationOption GetSubexpressionTranslation(OclExpression expression)
        {
            TranslationOption selectedOption = Translations[expression].Options[SelectedTranslations[expression]];
            if (Log != null && selectedOption.LogMessagesWhenSelected != null)
            {
                foreach(var v in selectedOption.LogMessagesWhenSelected) 
                    Log.AddLogMessage(v);
            }
            return selectedOption;
        }

        public void AddTrivialTranslation(OclExpression expression, string translation)
        {
            TranslationOptions options = new TranslationOptions();
            options.Expression = expression; 
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
                options.Expression = expression; 
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
    
        public void Clear()
        {
            Translations.Clear();
            SelectedTranslations.Clear();
        }

        public void Merge(SubexpressionTranslations other)
        {
            foreach (KeyValuePair<OclExpression, TranslationOptions> otherKVP in other.Translations)
            {
                this.Translations.Add(otherKVP.Key, otherKVP.Value);
                otherKVP.Value.SubexpressionTranslations = this;
            }

            foreach (KeyValuePair<OclExpression, int> otherKVP in other.SelectedTranslations)
            {
                this.SelectedTranslations.Add(otherKVP.Key, otherKVP.Value);
            }
        }
    
        public enum EContextVariableReplacementMode
        {
            OutermostContextOnly, 
            AnyContextVariable
        }

        public EContextVariableReplacementMode XPathContextVariableReplacementMode { get; set; }
    }

    public class TranslationOptions
    {
        public OclExpression Expression { get; set; }

        private readonly Guid identifier = Guid.NewGuid();
        public Guid Identifier { get { return identifier; } }

        private readonly List<TranslationOption> options = new List<TranslationOption>();

        private readonly List<OclExpression> subExpressions = new List<OclExpression>();

        public OclExpression TranslatedExpression { get; set; }

        public SubexpressionTranslations SubexpressionTranslations { get; set; }

        public List<TranslationOption> Options { get { return options; } }

        public List<OclExpression> SubExpressions { get { return subExpressions; } }

        public override string ToString()
        {
            //throw new Exception("TO STRING IN TRANSLATIONOPTIONS");
            return null;
        }
    }

    public class TranslationOption
    {
        public TranslationOptions OptionsContainer
        {
            get;
            set;
        }

        public SubexpressionTranslations SubexpressionTranslations
        {
            get { return OptionsContainer != null ? OptionsContainer.SubexpressionTranslations : null; }
        }

        public string FormatString { get; set; }

        public override string ToString()
        {
            throw new Exception("TO STRING IN TRANSLATIONOPTION");
        }

        private VariableDeclaration FindContextVariable()
        {
            VariableDeclaration result = null;

            TranslationOption co = this.CallingOption;
            while (co != null)
            {
                if (co.ContextVariableForSubExpressions != null
                    && (co.ContextVariableChangeOnlyIn == -1 || co.ContextVariableChangeOnlyIn == this.SubexpressionIndex))
                {
                    result = co.ContextVariableForSubExpressions;
                    break;
                }
                
                if (co.CallingOption == null)
                {
                    result = co.OptionsContainer.Expression.ConstraintContext.Self;
                }
                co = co.CallingOption;
            }

            return result;
        }

        public string GetString(bool topLevel)
        {
            object[] translatedSubexpressions = new object[OptionsContainer.SubExpressions.Count];
            for (int index = 0; index < OptionsContainer.SubExpressions.Count; index++)
            {
                OclExpression subExpression = OptionsContainer.SubExpressions[index];
                TranslationOption selectedSubexpressionTranslationOption = OptionsContainer.SubexpressionTranslations.GetSubexpressionTranslation(subExpression);
                foreach (TranslationOption subexpressionTranslationOption in OptionsContainer.SubexpressionTranslations.Translations[subExpression].Options)
                {
                    subexpressionTranslationOption.CallingOption = this;
                    subexpressionTranslationOption.SubexpressionIndex = index;
                }
                translatedSubexpressions[index] = selectedSubexpressionTranslationOption.GetString(false);
            }

            string result = null;
            if (!ContextVariableSubstitution)
            {
                result = string.Format(FormatString, translatedSubexpressions);
            }
            else
            {
                bool standardVariableTranslation = true;
                if (StartingVariable == FindContextVariable())
                {
                    if (SubexpressionTranslations.XPathContextVariableReplacementMode == SubexpressionTranslations.EContextVariableReplacementMode.AnyContextVariable)
                    {
                        result = string.Format(FormatString, @".");
                        if (result.StartsWith(@"./"))
                            result = result.Substring(2);
                        standardVariableTranslation = false; 
                    }
                    else //i.e. SubexpressionTranslations.ContextVariableReplacementMode == SubexpressionTranslations.EContextVariableReplacementMode.OutermostSelfOnly
                    {
                        if (!this.OptionsContainer.Expression.IsPartOfIteratorBody && StartingVariable.IsContextVariable)
                        {
                            result = string.Format(FormatString, @".");
                            if (result.StartsWith(@"./"))
                                result = result.Substring(2);
                            standardVariableTranslation = false; 
                        }
                    }
                }
                
                if (standardVariableTranslation)
                {
                    result = string.Format(FormatString, "$" + StartingVariable.Name);
                }
            }

            if (ParenthesisWhenNotTopLevel && !topLevel)
            {
                return "(" + result + ")";
            }
            else
            {
                return result;
            }
        }

        protected int SubexpressionIndex { get; set; }

        private TranslationOption CallingOption { get; set; }

        public string GetStringProp
        {
            get { return GetString(true); }
        }

        public bool IsSelectedOption
        {
            get
            {
                return OptionsContainer.Options.IndexOf(this) ==
                       OptionsContainer.SubexpressionTranslations.SelectedTranslations[this.OptionsContainer.Expression];
            }
        }

        public bool ParenthesisWhenNotTopLevel { get; set; }

        /// <summary>
        /// When <see cref="StartingVariable"/> is the same as context variable 
        /// (variable returned by <see cref="FindContextVariable"/>), it is 
        /// replaced by "."
        /// </summary>
        public bool ContextVariableSubstitution { get; set; }

        public VariableDeclaration ContextVariableForSubExpressions { get; set; }

        private int contextVariableChangeOnlyIn = -1;
        public int ContextVariableChangeOnlyIn
        {
            get { return contextVariableChangeOnlyIn; }
            set { contextVariableChangeOnlyIn = value; }
        }

        public VariableDeclaration StartingVariable { get; set; }

        public void Select()
        {
            OptionsContainer.SubexpressionTranslations.SelectedTranslations[this.OptionsContainer.Expression] =
                OptionsContainer.Options.IndexOf(this);

            Debug.Assert(IsSelectedOption);
        }

        public List<LogMessage<OclExpression>> LogMessagesWhenSelected;
    }
}