using System.Xml.Linq;

namespace Exolutio.Model.PSM.Grammar
{
    public class GrammarXMLGenerator
    {
        public static XNamespace GRAMMAR_NAMESPACE = @"http://eXolutio.eu/Grammar/";

        public Grammar Grammar { get; set; }

        public XDocument TranslateRTGtoXML(Grammar grammar)
        {
            Grammar = grammar;

            XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", null));

            XElement grammarElement = new XElement(GRAMMAR_NAMESPACE + "Grammar");
            grammarElement.Add(new XAttribute(XNamespace.Xmlns + "eXoG", GRAMMAR_NAMESPACE.NamespaceName));
            document.Add(grammarElement);

            TranslateTerminals(grammarElement, grammar);
            TranslateNonTerminals(grammarElement, grammar);
            TranslateInitialNonTerminals(grammarElement, grammar);
            TranslateProductionRules(grammarElement, grammar);

            return document;
        }

        private void TranslateTerminals(XElement grammarElement, Grammar grammar)
        {
            XElement terminalsE = new XElement(GRAMMAR_NAMESPACE + "Terminals");
            foreach (Terminal nonTerminal in grammar.Terminals)
            {
                XElement terminalE = new XElement(GRAMMAR_NAMESPACE + "Terminal");
                terminalE.Add(new XText(nonTerminal.Term));
                terminalsE.Add(terminalE);
            }
            grammarElement.Add(terminalsE);
        }

        private void TranslateNonTerminals(XElement grammarElement, Grammar grammar)
        {
            XElement nonTerminalsE = new XElement(GRAMMAR_NAMESPACE + "NonTerminals");
            foreach (NonTerminal nonTerminal in grammar.NonTerminals)
            {
                XElement nonTerminalE = new XElement(GRAMMAR_NAMESPACE + "NonTerminal");
                nonTerminalE.Add(new XAttribute("CorrespondingConstruct", nonTerminal.Component.ToString().Replace("\"","'")));
                nonTerminalE.Add(new XText(nonTerminal.ToString()));
                nonTerminalsE.Add(nonTerminalE);
            }
            grammarElement.Add(nonTerminalsE);
        }

        private void TranslateInitialNonTerminals(XElement grammarElement, Grammar grammar)
        {
            XElement initialNonTerminalsE = new XElement(GRAMMAR_NAMESPACE + "InitialNonTerminals");
            foreach (NonTerminal nonTerminal in grammar.InitialNonTerminals)
            {
                XElement nonTerminalE = new XElement(GRAMMAR_NAMESPACE + "InitialNonTerminal");
                nonTerminalE.Add(new XAttribute("CorrespondingConstruct", nonTerminal.Component.ToString().Replace("\"", "'")));
                nonTerminalE.Add(new XText(nonTerminal.ToString()));
                initialNonTerminalsE.Add(nonTerminalE);
            }
            grammarElement.Add(initialNonTerminalsE);
        }

        private void TranslateProductionRules(XElement grammarElement, Grammar grammar)
        {
            XElement productionRulesE = new XElement(GRAMMAR_NAMESPACE + "ProductionRules");
            foreach (ProductionRule productionRule in grammar.ProductionRules)
            {
                XElement productionRuleE = new XElement(GRAMMAR_NAMESPACE + "ProductionRule");
                productionRuleE.Add(new XElement(GRAMMAR_NAMESPACE + "Left", productionRule.LeftHandNonTerminal.ToString()));
                XElement rightE = new XElement(GRAMMAR_NAMESPACE + "Right");

                foreach (ProductionRuleToken token in productionRule.RightHandSide.GetTokens())
                {
                    XElement tokenE = new XElement(GRAMMAR_NAMESPACE + token.GetType().Name);
                    if (token is SpecialCharToken)
                    {
                        tokenE.Add(new XText(((SpecialCharToken)token).c));
                    }
                    if (token is TerminalToken)
                    {
                        tokenE.Add(new XText(((TerminalToken)token).Terminal.Term));
                    }
                    if (token is CardinalityToken)
                    {
                        tokenE.Add(new XAttribute("Lower", ((CardinalityToken)token).Lower));
                        tokenE.Add(new XAttribute("Upper", ((CardinalityToken)token).Upper));
                    }
                    if (token is NonTerminalToken)
                    {
                        tokenE.Add(new XText(((NonTerminalToken)token).NonTerminal.ToString()));
                    }
                    if (token is AttributeTypeToken)
                    {
                        tokenE.Add(new XText(((AttributeTypeToken)token).AttributeType.Name));
                    }
                    rightE.Add(tokenE);
                }

                productionRuleE.Add(rightE);
                productionRulesE.Add(productionRuleE);
            }
            grammarElement.Add(productionRulesE);
        }
    }
}