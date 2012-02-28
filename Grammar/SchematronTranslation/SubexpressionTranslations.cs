using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Exolutio.Model.OCL.AST;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class SubexpressionTranslations
    {
        public SubexpressionTranslations()
        {
        }

        public Guid Guid = Guid.NewGuid();

        private readonly  Dictionary<OclExpression, TranslationOptions> translations = new Dictionary<OclExpression, TranslationOptions>();
        
        private readonly Dictionary<OclExpression, int> selectedTranslations = new Dictionary<OclExpression, int>();

        public Dictionary<OclExpression, TranslationOptions> Translations
        {
            get { return translations; }
        }

        public IEnumerable<TranslationOptions> TranslationOptionsWithMorePossibilities
        {
            get { return Translations.Where(kvp => kvp.Value.Options.Count > 1).Select(o => o.Value); }        
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
            throw new Exception("TO STRING IN TRANSLATIONOPTIONS");
        }
    }

    public class TranslationOption
    {
        public TranslationOptions OptionsContainer
        {
            get;
            set;
        }

        public string FormatString { get; set; }

        public override string ToString()
        {
            throw new Exception("TO STRING IN TRANSLATIONOPTION");
        }

        public string GetString(bool topLevel)
        {
            object[] translatedSubexpressions = new object[OptionsContainer.SubExpressions.Count];
            for (int index = 0; index < OptionsContainer.SubExpressions.Count; index++)
            {
                OclExpression subExpression = OptionsContainer.SubExpressions[index];
                translatedSubexpressions[index] = OptionsContainer.SubexpressionTranslations.GetSubexpressionTranslation(subExpression).GetString(false);
            }
            if (ParenthesisWhenNotTopLevel && !topLevel)
            {
                return string.Format("(" + FormatString + ")", translatedSubexpressions);
            }
            else
            {
                return string.Format(FormatString, translatedSubexpressions);
            }
        }

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

        public void Select()
        {
            OptionsContainer.SubexpressionTranslations.SelectedTranslations[this.OptionsContainer.Expression] =
                OptionsContainer.Options.IndexOf(this);

            Debug.Assert(IsSelectedOption);
        }
    }
}