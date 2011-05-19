using System.Linq;
using System.Collections.Generic;

namespace EvoX.Model.PSM.Grammar
{
    public class Grammar
    {
        private readonly List<Terminal> terminals = new List<Terminal>();
        public List<Terminal> Terminals { get { return terminals; } }

        private readonly List<NonTerminal> nonTerminals = new List<NonTerminal>();
        public List<NonTerminal> NonTerminals { get { return nonTerminals; } }

        private readonly List<ProductionRule> productionRules = new List<ProductionRule>();
        public List<ProductionRule> ProductionRules { get { return productionRules; } }

        private readonly List<NonTerminal> initialNonTerminals = new List<NonTerminal>();
        public List<NonTerminal> InitialNonTerminals { get { return initialNonTerminals; } }

        private readonly Dictionary<PSMComponent, NonTerminal> nonTermialsDictionary = new Dictionary<PSMComponent, NonTerminal>();
        public Dictionary<PSMComponent, NonTerminal> NonTermialsDictionary { get { return nonTermialsDictionary; } }

        private readonly Dictionary<NonTerminal, PSMComponent> interpretation = new Dictionary<NonTerminal, PSMComponent>();
        public Dictionary<NonTerminal, PSMComponent> Interpretation { get { return interpretation; } }

        public IEnumerable<ProductionRule> AttributeDeclarations
        {
            get
            {
                return ProductionRules.Where(
                    r => r.RightHandSide is RightHandAttributeDeclaration);
            }
        }

        public IEnumerable<ProductionRule> ElementDeclarations
        {
            get
            {
                return ProductionRules.Where(
                    r => (r.RightHandSide is RightHandElementSimpleContentDeclaration) || (r.RightHandSide is RightHandElementComplexContentDeclaration));
            }
        }

        public NonTerminal GetNonTerminal(PSMComponent psmComponent)
        {
            NonTerminal nonTerminal;
            if (!NonTermialsDictionary.TryGetValue(psmComponent, out nonTerminal))
            {
                nonTerminal = new NonTerminal(psmComponent);
                if (!EvoX.SupportingClasses.NameSuggestor<NonTerminal>.IsNameUnique(NonTerminals, nonTerminal.ToString(), n => n.ToString()))
                {
                    string name = EvoX.SupportingClasses.NameSuggestor<NonTerminal>.SuggestUniqueName(NonTerminals, nonTerminal.ToString(), n => n.ToString(), true, false);
                    nonTerminal.UniqueName = name;
                }
                NonTermialsDictionary[psmComponent] = nonTerminal;
                NonTerminals.Add(nonTerminal);
            }
            return nonTerminal;
        }


        public Terminal GetTerminal(string term)
        {
            Terminal result = Terminals.FirstOrDefault(t => t.Term == term);
            if (result == null)
            {
                result = new Terminal(term);
            }
            return result;
        }

        public void OrderProductionRules()
        {
            Dictionary<ProductionRule, int> orderDict = new Dictionary<ProductionRule, int>();

            Queue<ProductionRule> q = new Queue<ProductionRule>();
            int num = 0; 
            foreach (NonTerminal initialNonTerminal in InitialNonTerminals)
            {
                foreach (ProductionRule productionRule in ProductionRules.Where(r => r.LeftHandNonTerminal == initialNonTerminal))
                {
                    if (!q.Contains(productionRule) && !orderDict.ContainsKey(productionRule))
                    {
                        q.Enqueue(productionRule);
                        orderDict[productionRule] = num++;
                    }
                }
            }

            while (q.Count != 0)
            {
                ProductionRule takenRule = q.Dequeue();
                foreach (ProductionRuleToken productionRuleToken in takenRule.RightHandSide.GetTokens())
                {
                    if (productionRuleToken is NonTerminalToken)
                    {
                        foreach (ProductionRule productionRule in ProductionRules.Where(r => r.LeftHandNonTerminal == ((NonTerminalToken)productionRuleToken).NonTerminal))
                        {
                            if (!q.Contains(productionRule)  && !orderDict.ContainsKey(productionRule))
                            {
                                q.Enqueue(productionRule);
                                orderDict[productionRule] = num++;
                            }
                        }
                    }
                }
            }

            foreach (ProductionRule productionRule in ProductionRules)
            {
                 if (!orderDict.ContainsKey(productionRule))
                 {
                     q.Enqueue(productionRule);
                     orderDict[productionRule] = num++;
                 }
            }

            productionRules.Sort(delegate(ProductionRule r1, ProductionRule r2) { return orderDict[r1].CompareTo(orderDict[r2]); });
        }
    }
}