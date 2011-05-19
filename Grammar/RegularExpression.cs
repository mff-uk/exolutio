using System.Collections.Generic;
using EvoX.SupportingClasses;

namespace EvoX.Model.PSM.Grammar
{
    public abstract class RegularExpression
    {
        public abstract IEnumerable<ProductionRuleToken> GetTokens();
    }

    public class NonTerminalRegularExpression : RegularExpression
    {
        public NonTerminal Z { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", Z);
        }

        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            return new ProductionRuleToken[] { new NonTerminalToken(Z) };
        }
    }

    public class CardinalityRegularExpression : RegularExpression
    {
        public RegularExpression RE { get; set; }

        public uint Lower { get; set; }

        public EvoX.Model.UnlimitedInt Upper { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}..{2}", RE, Lower, Upper);
        }

        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            IEnumerable<ProductionRuleToken> productionRuleTokens = RE.GetTokens();
            return productionRuleTokens.Append(SpecialCharToken.TokenSpace, new CardinalityToken(Lower, Upper));
        }
    }

    public abstract class CompoundRegularExpression : RegularExpression
    {
        private readonly List<RegularExpression> compounds = new List<RegularExpression>();
        public List<RegularExpression> Compounds { get { return compounds; } }
    }

    public class SequenceRegularExpression : CompoundRegularExpression
    {
        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            List<ProductionRuleToken> result = new List<ProductionRuleToken>();
            if (Compounds.Count > 1)
            {
                result.Add(SpecialCharToken.TokenLPar);
            }

            for (int index = 0; index < Compounds.Count; index++)
            {
                RegularExpression regularExpression = Compounds[index];
                result.AddRange(regularExpression.GetTokens());
                if (index + 1 < Compounds.Count)
                {
                    result.Add(SpecialCharToken.TokenComma);
                }
            }
            if (Compounds.Count > 1)
            {
                result.Add(SpecialCharToken.TokenRPar);
            }
            return result;
        }

        public override string ToString()
        {
            if (Compounds.Count > 1)
            {
                return string.Format("({0})", Compounds.ConcatWithSeparator(","));
            }
            else
            {
                return string.Format("{0}", Compounds.ConcatWithSeparator(","));
            }
        }
    }

    public class ChoiceRegularExpression : CompoundRegularExpression
    {
        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            List<ProductionRuleToken> result = new List<ProductionRuleToken>();
            if (Compounds.Count > 1)
            {
                result.Add(SpecialCharToken.TokenLPar);
            }
            for (int index = 0; index < Compounds.Count; index++)
            {
                RegularExpression regularExpression = Compounds[index];
                result.AddRange(regularExpression.GetTokens());
                if (index + 1 < Compounds.Count)
                {
                    result.Add(SpecialCharToken.TokenPipe);
                }
            }
            if (Compounds.Count > 1)
            {
                result.Add(SpecialCharToken.TokenRPar);
            }
            return result;
        }

        public override string ToString()
        {
            if (Compounds.Count > 1)
            {
                return string.Format("({0})", Compounds.ConcatWithSeparator("|"));
            }
            else
            {
                return string.Format("{0}", Compounds.ConcatWithSeparator("|"));
            }
        }
    }

    public class SetRegularExpression : CompoundRegularExpression
    {
        #region Overrides of CompoundRegularExpression

        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            List<ProductionRuleToken> result = new List<ProductionRuleToken>();
            result.Add(SpecialCharToken.TokenLBrace);
            for (int index = 0; index < Compounds.Count; index++)
            {
                RegularExpression regularExpression = Compounds[index];
                result.AddRange(regularExpression.GetTokens());
                if (index + 1 < Compounds.Count)
                {
                    result.Add(SpecialCharToken.TokenComma);
                }
            }
            result.Add(SpecialCharToken.TokenRBrace);
            return result;
        }


        public override string ToString()
        {
            return string.Format("{{{0}}}", Compounds.ConcatWithSeparator(","));
        }

        #endregion
    }
}