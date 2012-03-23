﻿using System;
using System.Collections;
using System.Linq; 
using System.Collections.Generic;
using System.Xml.Linq;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.SupportingClasses;
using Exolutio.SupportingClasses.Annotations;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class SchematronSchemaGenerator
    {
        private PSMSchema psmSchema;

        public PSMSchema PSMSchema
        {
            get { return psmSchema; }
        }

        public Log<OclExpression> Log { get; private set; }

        private static readonly XNamespace oclXNamespace = "http://eXolutio.com/oclX";
        public static XNamespace OclXNamespace
        {
            get { return oclXNamespace; }
        }

        public void Initialize(PSMSchema psmSchema)
        {
            this.psmSchema = psmSchema;
            Log = new Log<OclExpression>();
        }

        public XDocument GetSchematronSchema([NotNull]TranslationSettings translationSettings)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement schSchema = doc.SchematronSchema();
            XComment comment = new XComment(string.Format(" Generated by eXolutio on {0} {1} from {2}/{3}. ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), PSMSchema.Project.Name, PSMSchema.Caption));
            schSchema.Add(comment);

            foreach (OCLScript oclScript in PSMSchema.OCLScripts)
            {
                TranslateScript(schSchema, oclScript, translationSettings);
            }

            foreach (LogMessage<OclExpression> e in Log)
            {
                if (e.Tag != null && e.Tag.CodeSource != null && e.Tag.CodeSource.IsFromCode)
                {
                    e.MessageTextFull = e.MessageText + System.Environment.NewLine + string.Format("Expression: > {0} <", e.Tag);
                    e.Column = e.Tag.CodeSource.Column;
                    e.Line = e.Tag.CodeSource.Line;
                }
            }

            return doc;
        }

        private class PatternInfo
        {
            public string PatternName { get; set; }

            private readonly List<string> contextVariableNames = new List<string>();
            public List<string> ContextVariableNames
            {
                get { return contextVariableNames; }
            }
        }

        private void TranslateScript(XElement schSchema, OCLScript oclScript, TranslationSettings translationSettings)
        {
            CompilerResult compilerResult = oclScript.CompileToAst();

            Dictionary<PSMClass, PatternInfo> patterns = new Dictionary<PSMClass, PatternInfo>();

            if (!compilerResult.Errors.HasError)
            {
                XComment comment = new XComment(string.Format("Below follow constraints from OCL script '{0}'. ", oclScript.Name));
                schSchema.Add(comment);
                IEnumerable<IGrouping<PSMClass, ClassifierConstraint>> grouped = compilerResult.Constraints.Classifiers.GroupBy(GetContextTag);
                IEnumerable<PSMClass> keys = grouped.GetKeys();

                foreach (IGrouping<PSMClass, ClassifierConstraint> group in grouped)
                {
                    PSMClass contextClass = @group.Key;
                    XElement patternElement = schSchema.SchematronPattern(contextClass.Name);
                    patterns[contextClass] = new PatternInfo { PatternName = contextClass.Name };
                                        
                    bool abstractPattern = !contextClass.GeneralizationsAsGeneral.IsEmpty() 
                        && SpecificHasConstraits(keys, contextClass);

                    #region classes requiring abstract patterns

                    if (abstractPattern)
                    {
                        // abstract pattern
                        patternElement.AddAttributeWithValue("abstract", "true");
                    }

                    #endregion

                    foreach (ClassifierConstraint constraint in group)
                    {
                        XElement ruleElement = patternElement.SchematronRule(!abstractPattern ? contextClass.XPathFull.ToString() : "$" + constraint.Self.Name);
                        patterns[contextClass].ContextVariableNames.AddIfNotContained(constraint.Self.Name);
                        if (!abstractPattern && constraint.Self.Name != VariableDeclaration.SELF)
                        {
                            XElement contextVarElement = new XElement(OclXNamespace + "context-variable");
                            contextVarElement.AddAttributeWithValue("name", constraint.Self.Name);
                            ruleElement.Add(contextVarElement);
                        }

                        TranslateInvariantsToXPath(constraint, ruleElement, oclScript, (PSMBridge)compilerResult.Bridge, translationSettings);
                    }

                    #region create instance patterns 

                    IEnumerable<PSMClass> ancestorsAndSelf = ModelIterator.GetGeneralizationsWithSelf(contextClass);
                    foreach (PSMClass ancestorClass in ancestorsAndSelf)
                    {
                        if (patterns.ContainsKey(ancestorClass) && (abstractPattern || ancestorClass != contextClass))
                        {
                            if (ancestorClass != contextClass)
                            {
                                schSchema.Add(new XComment(string.Format("instance pattern for {0}'s ancestor {1}", contextClass, ancestorClass.Name)));
                            }
                            else
                            {
                                schSchema.Add(new XComment(string.Format("instance pattern for {0}'s", contextClass)));
                            }
                            XElement instancePattern = schSchema.SchematronPattern();
                            instancePattern.AddAttributeWithValue("id", string.Format("{0}-as-{1}", contextClass.Name, ancestorClass.Name));
                            instancePattern.AddAttributeWithValue("is-a", patterns[ancestorClass].PatternName);
                            foreach (string contextVariableName in patterns[ancestorClass].ContextVariableNames)
                            {
                                instancePattern.SchematronParam(contextVariableName, ".");
                            }
                        }
                    }

                    #endregion                    
                }
            }
            else
            {
                XComment comment = new XComment(string.Format("OCL script '{0}' contains errors and thus can not be translated. ", oclScript.Name));
                schSchema.Add(comment);
            }
        }

        private static bool SpecificHasConstraits(IEnumerable<PSMClass> keys, PSMClass contextClass)
        {
            return contextClass.GeneralizationsAsGeneral.Any(gen => keys.Contains(gen.Specific));
        }

        private PSMClass GetContextTag(ClassifierConstraint cc)
        {
            return (PSMClass) cc.Context.Tag;
        }

        private void TranslateInvariantsToXPath(ClassifierConstraint constraint, XElement ruleElement, OCLScript oclScript, PSMBridge psmBridge, TranslationSettings translationSettings)
        {
            foreach (OclExpression invariant in constraint.Invariants)
            {
                string xpath = TranslateInvariantToXPath(oclScript, constraint, psmBridge, invariant, translationSettings);

                try
                {
                    ruleElement.Add(new XComment(invariant.ToString()));
                    ruleElement.SchematronAssert(xpath);
                }
                catch
                {
                    ruleElement.Add(new XComment("Translation of the constraint failed. "));
                }
            }
        }

        private string TranslateInvariantToXPath(OCLScript oclScript, ClassifierConstraint constraint, IBridgeToOCL bridge, OclExpression invariant, TranslationSettings translationSettings)
        {
            invariant.IsInvariant = true;
            invariant.ClassifierConstraint = constraint;

            PSMOCLtoXPathConverter xpathConverter = translationSettings.Functional ? 
                (PSMOCLtoXPathConverter) new PSMOCLtoXPathConverterFunctional() : 
                (PSMOCLtoXPathConverter) new PSMOCLtoXPathConverterDynamic();
            xpathConverter.OCLScript = oclScript;
            xpathConverter.Bridge = (PSMBridge) bridge;
            xpathConverter.OclContext = constraint;
            xpathConverter.Log = Log;
            xpathConverter.Settings = translationSettings;
            if (!translationSettings.Retranslation)
            {
                try
                {
                    string invariantStr = xpathConverter.TranslateExpression(invariant);
                    translationSettings.SubexpressionTranslations.Merge(xpathConverter.SubexpressionTranslations);
                    return invariantStr;
                }
                catch (ExpressionNotSupportedInXPath e)
                {
                    Log.AddError(e.Message, e.Expression);
                }
                catch
                {
                    Log.AddError("Unable to translate invariant. ", invariant);
                }
            }
            else
            {
                try
                {
                    // this must stay here because of the string comparison - translation renames some variables
                    xpathConverter.TranslateExpression(invariant);

                    foreach (OclExpression translatedExp in translationSettings.SubexpressionTranslations.Translations.Keys)
                    {
                        if (translatedExp.ToString() == invariant.ToString())
                        {
                            translationSettings.SubexpressionTranslations.SelfVariableDeclaration = translatedExp.ClassifierConstraint.Self;
                            return translationSettings.SubexpressionTranslations.GetSubexpressionTranslation(translatedExp).GetString(true);
                        }
                    }
                    
                }
                catch (ExpressionNotSupportedInXPath e)
                {
                    Log.AddError(e.Message, e.Expression);
                }
                catch
                {
                    Log.AddError("Unable to translate invariant. ", invariant);
                }
            }
            return "### ERROR";
        }
    }
}