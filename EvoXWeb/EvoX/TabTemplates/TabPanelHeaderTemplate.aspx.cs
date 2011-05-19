using System.Web.UI;
using System.Web.UI.WebControls;

namespace EvoX.Web.Controls.TabTemplates
{
    public class TabPanelHeaderTemplate : ITemplate
    {
        private string text;

        public TabPanelHeaderTemplate(string text)
        {
            this.text = text;
        }

        public void InstantiateIn(Control container)
        {
            Literal lc = new Literal();
            lc.Text = text;
            container.Controls.Add(lc);
        }
    }
}   