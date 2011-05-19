using System;
using System.Diagnostics;
using System.Linq;

namespace EvoX.Model.PSM.Grammar
{
    public class GrammarGenerator
    {
        public PSMSchema Schema { get; private set; }

        public Grammar Grammar { get; private set; }

        public Grammar GenerateGrammar(PSMSchema schema)
        {
            Schema = schema;
            Grammar = new Grammar();


            GenerateInitialNonterminals();
            GenerateElementSimpleContentProductionRules();
            GenerateElementAttributeDeclarationProductionRules();
            GenerateElementComplexContentProductionRules();

            Grammar.OrderProductionRules();
            
            return Grammar;
        }

        public void GenerateInitialNonterminals()
        {
            foreach (PSMClass topClass in Schema.TopClasses)
            {
                if (topClass.ParentAssociation.IsNamed)
                {
                    NonTerminal nonTerminal = Grammar.GetNonTerminal(topClass.ParentAssociation);
                    Grammar.InitialNonTerminals.Add(nonTerminal);
                }
            }
        }

        public void GenerateElementSimpleContentProductionRules()
        {
            foreach (PSMAttribute psmAttribute in Schema.PSMAttributes.Where(a => a.Element))
            {
                NonTerminal nonTerminal = Grammar.GetNonTerminal(psmAttribute);
                Terminal terminal = Grammar.GetTerminal(psmAttribute.Name);
                Grammar.Terminals.Add(terminal);
                
                ProductionRule p = new ProductionRule();
                p.LeftHandNonTerminal = nonTerminal;
                p.RightHandSide = new RightHandElementSimpleContentDeclaration
                                      {
                                          t = terminal,
                                          D = psmAttribute.AttributeType
                                      };
                Grammar.ProductionRules.Add(p);
                Grammar.Interpretation.Add(nonTerminal, psmAttribute);
            }
        }

        public void GenerateElementAttributeDeclarationProductionRules()
        {
            foreach (PSMAttribute psmAttribute in Schema.PSMAttributes.Where(a => !a.Element))
            {
                NonTerminal nonTerminal = Grammar.GetNonTerminal(psmAttribute);
                Terminal terminal = Grammar.GetTerminal(psmAttribute.Name);
                Grammar.Terminals.Add(terminal);

                ProductionRule p = new ProductionRule();
                p.LeftHandNonTerminal = nonTerminal;
                p.RightHandSide = new RightHandAttributeDeclaration
                {
                    t = terminal,
                    D = psmAttribute.AttributeType
                };
                Grammar.ProductionRules.Add(p);
                Grammar.Interpretation.Add(nonTerminal, psmAttribute);
            }
        }

        public void GenerateElementComplexContentProductionRules()
        {
            foreach (PSMAssociation association in Schema.PSMAssociations.Where(a => a.IsNamed && a.Child is PSMClass))
            {
                NonTerminal nonTerminal = Grammar.GetNonTerminal(association);
                Terminal terminal = Grammar.GetTerminal(association.Name);
                Grammar.Terminals.Add(terminal);

                ProductionRule p = new ProductionRule();
                p.LeftHandNonTerminal = nonTerminal;
                p.RightHandSide = new RightHandElementComplexContentDeclaration
                                      {
                                          t = terminal,
                                          re = RewriteDown(association.Child)
                                      };

                Grammar.ProductionRules.Add(p);
                Grammar.Interpretation.Add(nonTerminal, association.Child);
            }
        }

        private RegularExpression RewriteDown(PSMComponent component)
        {
            if (component is PSMAssociation)
                return RewriteDownAssociation((PSMAssociation) component);
            if (component is PSMAttribute)
                return RewriteDownAttribute((PSMAttribute) component);
            if (component is PSMClass)
                return RewriteDownClass((PSMClass) component);
            if (component is PSMContentModel)
            {
                switch (((PSMContentModel)component).Type)
                {
                    case PSMContentModelType.Sequence:
                        return RewriteDownSequenceCM((PSMContentModel) component);
                    case PSMContentModelType.Choice:
                        return RewriteDownChoiceCM((PSMContentModel)component);
                    case PSMContentModelType.Set:
                        return RewriteDownSetCM((PSMContentModel)component);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            throw new NotImplementedException();
        }

        private RegularExpression RewriteDownSetCM(PSMContentModel contentModel)
        {
            SetRegularExpression result = new SetRegularExpression();
            return RewriteDownCM(contentModel, result);
        }

        private RegularExpression RewriteDownChoiceCM(PSMContentModel contentModel)
        {
            ChoiceRegularExpression result = new ChoiceRegularExpression();
            return RewriteDownCM(contentModel, result);
        }

        private RegularExpression RewriteDownSequenceCM(PSMContentModel contentModel)
        {
            SequenceRegularExpression result = new SequenceRegularExpression();
            return RewriteDownCM(contentModel, result);
        }

        private RegularExpression RewriteDownCM(PSMContentModel contentModel, CompoundRegularExpression result)
        {
            foreach (PSMAssociation association in contentModel.ChildPSMAssociations)
            {
                result.Compounds.Add(RewriteDownAssociation(association));
            }
            return result;
        }

        private RegularExpression RewriteDownClass(PSMClass psmClass)
        {
            Debug.Assert(psmClass.ParentAssociation != null);
            //Debug.Assert(!psmClass.ParentAssociation.IsNamed);

            SequenceRegularExpression sequenceRegularExpression = new SequenceRegularExpression();
            RewriteDownClassWithSR(psmClass, sequenceRegularExpression);
            //RewriteDownClassAttributes(psmClass, sequenceRegularExpression);
            //RewriteDownClassElements(psmClass, sequenceRegularExpression);

            return sequenceRegularExpression;
        }

        private void RewriteDownClassWithSR(PSMClass psmClass, SequenceRegularExpression regularExpression)
        {
            if (psmClass.IsStructuralRepresentative)
            {
                RewriteDownClassWithSR(psmClass.RepresentedClass, regularExpression);
            }
            foreach (PSMAttribute attribute in psmClass.PSMAttributes)
            {
                regularExpression.Compounds.Add(RewriteDownAttribute(attribute));
            }
            foreach (PSMAssociation association in psmClass.ChildPSMAssociations)
            {
                regularExpression.Compounds.Add(RewriteDownAssociation(association));
            }
        }

        private void RewriteDownClassAttributes(PSMClass psmClass, CompoundRegularExpression regularExpression)
        {
            if (psmClass.IsStructuralRepresentative)
            {
                RewriteDownClassAttributes(psmClass.RepresentedClass, regularExpression);
            }
            foreach (PSMAttribute attribute in psmClass.PSMAttributes)
            {
                regularExpression.Compounds.Add(RewriteDownAttribute(attribute));
            }            
        }

        private void RewriteDownClassElements(PSMClass psmClass, CompoundRegularExpression regularExpression)
        {
            if (psmClass.IsStructuralRepresentative)
            {
                RewriteDownClassElements(psmClass.RepresentedClass, regularExpression);
            }
            foreach (PSMAssociation association in psmClass.ChildPSMAssociations)
            {
                regularExpression.Compounds.Add(RewriteDownAssociation(association));
            }
        }

        private RegularExpression RewriteDownAssociation(PSMAssociation association)
        {
            if (association.IsNamed && association.Child is PSMClass) // named association rewriting rule 
            {
                NonTerminalRegularExpression nonTerminalRE = new NonTerminalRegularExpression {Z = Grammar.GetNonTerminal(association)};
                if (!association.HasNondefaultCardinality())
                {
                    return nonTerminalRE;
                }
                else
                {
                    return new CardinalityRegularExpression
                               {
                                   RE = nonTerminalRE,
                                   Lower = association.Lower,
                                   Upper = association.Upper
                               };
                }
            }
            else // unnamed association rewriting rule 
            {
                if (!association.HasNondefaultCardinality())
                {
                    return RewriteDown(association.Child);
                }
                else
                {
                    return new CardinalityRegularExpression
                               {
                                   RE = RewriteDown(association.Child),
                                   Lower = association.Lower,
                                   Upper = association.Upper
                               };
                }
            }
        }

        private RegularExpression RewriteDownAttribute(PSMAttribute attribute)
        {
            NonTerminalRegularExpression nonTerminalRE = new NonTerminalRegularExpression { Z = Grammar.GetNonTerminal(attribute) };
            if (!attribute.HasNondefaultCardinality())
            {
                return nonTerminalRE;
            }
            else
            {
                return new CardinalityRegularExpression
                {
                    RE = nonTerminalRE,
                    Lower = attribute.Lower,
                    Upper = attribute.Upper
                };
            }
        }
    }
}
