using System;
using EvoX.Model;
using EvoX.Model.PSM.Grammar;

namespace EvoX.Web.IO
{
    public static class URLHelper
    {
        public static string GetOnlineDemoBaseUrl()
        {
            return "~/Evox/OnlineDemo.aspx";
        }

        public static string GetUrlOf(Component component)
        {
            return GetOnlineDemoBaseUrl() + "#" + component.ID;
        }

        public static string GetHtmlAnchor(Component component, bool focusPIMTab = false, int? focusedTab = null)
        {
            return GetHtmlAnchor(component, component.Name, focusPIMTab, focusedTab);
        }

        public static string GetHtmlAnchor(Component component, string anchorText, bool focusPIMTab = false, int ? focusedTab = null)
        {
            string onClick;
            if (focusPIMTab || !focusedTab.HasValue)
                onClick = string.Format("onclick=\"{0}{1}\"", JSSelectComponentWrapper(component), focusPIMTab ? "FocusPIMTab();" : string.Empty);
            else
                onClick = string.Format("onclick=\"{0}FocusTab('{1}');\"", JSSelectComponentWrapper(component), focusedTab.Value);
            
            
            return string.Format("<a href=\"#{1}\" {2}>{0}</a>", anchorText, component.ID, onClick);
        }

        public static string GetHtmlAnchoringSpan(Component component)
        {
            return string.Format("<span id=\"{0}\"/>", component.ID);
        }

        public static string JSSelectComponentWrapper(Component component)
        {
            return string.Format("selectComponent('w{0}');", component.ID);
        }

        public static string JSHighlightToken(NonTerminal nonTerminal)
        {
            return string.Format("highlightTokenUsage('{0}');", nonTerminal);
        }

        public static string JSHighlightToken(Terminal terminal)
        {
            return string.Format("highlightTokenUsage(\"{0}\");", terminal);
        }
    }
}