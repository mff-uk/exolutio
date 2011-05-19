using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoX.Model.PSM;
using EvoX.Model.PSM.Grammar;
using EvoX.Web.IO;
using EvoX.Web.ModelHelper;

namespace EvoX.Web.Controls
{
    public partial class GrammarVisualizer : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void Display(Grammar grammar)
        {
            repNonTerminals.DataSource = grammar.NonTerminals;
            repNonTerminals.DataBind();

            repTerminals.DataSource = grammar.Terminals;
            repTerminals.DataBind();

            repInitialNonTerminals.DataSource = grammar.InitialNonTerminals;
            repInitialNonTerminals.DataBind();

            repProductionRules.DataSource = grammar.ProductionRules;
            repProductionRules.DataBind();
        }

        protected void NonTerminal_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
            {
                HyperLink hlNonTerminal = (HyperLink)e.Item.FindControl("lblNonTerminal");
                NonTerminal nonTerminal = (NonTerminal) e.Item.DataItem;
                hlNonTerminal.CssClass = "non-terminal fakelink";
                hlNonTerminal.Text = nonTerminal.ToString();
                hlNonTerminal.Attributes["onclick"] = URLHelper.JSHighlightToken(nonTerminal);
            }
        }

        protected void Terminal_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
            {
                if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
                {
                    HyperLink hlTerminal = (HyperLink)e.Item.FindControl("lblTerminal");
                    Terminal terminal = (Terminal)e.Item.DataItem;
                    hlTerminal.CssClass = "terminal fakelink";
                    hlTerminal.Text = terminal.ToString();
                    hlTerminal.Attributes["onclick"] = URLHelper.JSHighlightToken(terminal);
                }   
            }
        }

        protected void ProductionRule_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
            {
                if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
                {
                    Label labelNonTerminal = (Label)e.Item.FindControl("lblProductionRule");
                    ProductionRule productionRule = (ProductionRule)e.Item.DataItem;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("<span class=\"non-terminal\">");
                    sb.AppendFormat("<span class=\"{0}\">", productionRule.LeftHandNonTerminal);
                    sb.Append(productionRule.LeftHandNonTerminal.ToString());
                    sb.Append("</span>");
                    sb.AppendFormat("</span>&nbsp;&rarr;&nbsp;");
                    IEnumerable<ProductionRuleToken> productionRuleTokens = productionRule.RightHandSide.GetTokens();
                    foreach (ProductionRuleToken productionRuleToken in productionRuleTokens)
                    {
                        if (productionRuleToken is NonTerminalToken)
                        {
                            sb.Append("<span class=\"non-terminal\">");
                            sb.AppendFormat("<span class=\"{0}\">", ((NonTerminalToken)productionRuleToken).NonTerminal);
                            sb.Append(productionRuleToken.ToString());
                            sb.Append("</span>");
                            sb.Append("</span>");
                        }
                        else if (productionRuleToken is TerminalToken)
                        {
                            sb.Append("<span class=\"terminal\">");
                            sb.AppendFormat("<span class=\"{0}\">", ((TerminalToken)productionRuleToken).Terminal);
                            sb.Append(productionRuleToken.ToString());
                            sb.Append("</span>");
                            sb.Append("</span>");
                        }
                        else 
                            sb.Append(productionRuleToken.ToString());
                    }
                    labelNonTerminal.Text = sb.ToString();
                }
            }

        }
    }
}