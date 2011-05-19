using System.Collections.Generic;
using System.Linq;
using EvoX.SupportingClasses;

namespace EvoX.Model.PSM.Grammar
{
    public class ProductionRule
    {
        public NonTerminal LeftHandNonTerminal { get; set; }

        public RightHandSide RightHandSide { get; set; }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", LeftHandNonTerminal, RightHandSide);
        }
    }

    public abstract class RightHandSide
    {
        public Terminal t { get; set; }

        public abstract IEnumerable<ProductionRuleToken> GetTokens();
    }

    public abstract class ProductionRuleToken
    {

    }

    public class SpecialCharToken : ProductionRuleToken
    {
        public string c { get; set; }

        public SpecialCharToken(string c)
        {
            this.c = c;
        }

        private static readonly SpecialCharToken tokenA = new SpecialCharToken("@");
        public static SpecialCharToken TokenA { get { return tokenA; } }

        private static readonly SpecialCharToken tokenLBracket = new SpecialCharToken("[");
        public static SpecialCharToken TokenLBracket { get { return tokenLBracket; } }

        private static readonly SpecialCharToken tokenRBracket = new SpecialCharToken("]");
        public static SpecialCharToken TokenRBracket { get { return tokenRBracket; } }

        private static readonly SpecialCharToken tokenLPar = new SpecialCharToken("(");
        public static SpecialCharToken TokenLPar { get { return tokenLPar; } }

        private static readonly SpecialCharToken tokenRPar = new SpecialCharToken(")");
        public static SpecialCharToken TokenRPar { get { return tokenRPar; } }

        private static readonly SpecialCharToken tokenLBrace = new SpecialCharToken("{");
        public static SpecialCharToken TokenLBrace { get { return tokenLBrace; } }

        private static readonly SpecialCharToken tokenRBrace = new SpecialCharToken("}");
        public static SpecialCharToken TokenRBrace { get { return tokenRBrace; } }

        private static readonly SpecialCharToken tokenPipe = new SpecialCharToken("|");
        public static SpecialCharToken TokenPipe { get { return tokenPipe; } }

        private static readonly SpecialCharToken tokenComma = new SpecialCharToken(",");
        public static SpecialCharToken TokenComma { get { return tokenComma; } }

        private static readonly SpecialCharToken tokenSpace = new SpecialCharToken(" ");
        public static SpecialCharToken TokenSpace { get { return tokenSpace; } }

        public override string ToString()
        {
            return c;
        }
    }

    public class TerminalToken : ProductionRuleToken
    {
        public Terminal Terminal { get; private set; }

        public TerminalToken(Terminal terminal)
        {
            Terminal = terminal;
        }

        public override string ToString()
        {
            return Terminal.ToString();
        }
    }

    public class CardinalityToken : ProductionRuleToken
    {
        public uint Lower { get; set; }
        public UnlimitedInt Upper { get; set; }

        public CardinalityToken(uint lower, UnlimitedInt upper)
        {
            Lower = lower;
            Upper = upper;
        }

        public override string ToString()
        {
            return string.Format("{0}..{1}", Lower, Upper);
        }
    }

    public class NonTerminalToken : ProductionRuleToken
    {
        public NonTerminal NonTerminal { get; private set; }

        public NonTerminalToken(NonTerminal nonTerminal)
        {
            NonTerminal = nonTerminal;
        }

        public override string ToString()
        {
            return NonTerminal.ToString();
        }
    }

    public class AttributeTypeToken : ProductionRuleToken
    {
        public AttributeType AttributeType { get; set; }

        public AttributeTypeToken(AttributeType attributeType)
        {
            AttributeType = attributeType;
        }

        public override string ToString()
        {
            return AttributeType != null ? AttributeType.Name : string.Empty;
        }
    }

    public class RightHandAttributeDeclaration : RightHandSide
    {
        public AttributeType D { get; set; }

        public override string ToString()
        {
            return string.Format("@{0}[{1}]", t, D != null ? D.Name: string.Empty);
        }

        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            return new ProductionRuleToken[]
                       {
                           SpecialCharToken.TokenA,
                           new TerminalToken(t),
                           SpecialCharToken.TokenLBracket,
                           new AttributeTypeToken(D),
                           SpecialCharToken.TokenRBracket
                        };
        }
    }

    public class RightHandElementSimpleContentDeclaration : RightHandSide
    {
        public AttributeType D { get; set; }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", t, D != null ? D.Name : string.Empty);
        }

        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            return new ProductionRuleToken[]
                       {
                           new TerminalToken(t),
                           SpecialCharToken.TokenLBracket,
                           new AttributeTypeToken(D),
                           SpecialCharToken.TokenRBracket
                        };
        }
    }

    public class RightHandElementComplexContentDeclaration : RightHandSide
    {
        public RegularExpression re { get; set; }

        public override string ToString()
        {
            string reString = re.ToString();
            if (reString.StartsWith("("))
                reString = reString.Substring(1);
            if (reString.EndsWith(")"))
                reString = reString.Substring(0, reString.Length - 2);
            return string.Format("{0}[{1}]", t, reString);
        }

        public override IEnumerable<ProductionRuleToken> GetTokens()
        {
            IEnumerable<ProductionRuleToken> _regexTokens = re.GetTokens();
            List<ProductionRuleToken> regexTokens = new List<ProductionRuleToken>(_regexTokens);
            if (regexTokens.FirstOrDefault() == SpecialCharToken.TokenLPar)
                regexTokens.RemoveAt(0);
            if (regexTokens.LastOrDefault() == SpecialCharToken.TokenRPar)
                regexTokens.RemoveAt(regexTokens.Count - 1);

            return regexTokens.Prepend(new TerminalToken(t), SpecialCharToken.TokenLBracket).
                Append(SpecialCharToken.TokenRBracket);
        }
    }
}