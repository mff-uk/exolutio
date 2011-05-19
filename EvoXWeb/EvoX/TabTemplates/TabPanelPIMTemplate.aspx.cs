using System.Web.UI;
using System.Web.UI.HtmlControls;
using EvoX.Model.PIM;
using EvoX.Web.ModelHelper;

namespace EvoX.Web.Controls.TabTemplates
{
    public class TabPanelPIMTemplate : ITemplate
    {
        public PIMSchema PIMSchema { get; private set; }

        public Page Page { get; set; }

        public TabPanelPIMTemplate(Page page, PIMSchema pimSchema)
        {
            this.PIMSchema = pimSchema;
            this.Page = page;
        }

        public void InstantiateIn(Control container)
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes["class"] = "tabContainer";

            IComponentVisualizer<PIMSchema> pimVisualizer = (IComponentVisualizer<PIMSchema>)Page.LoadControl("Controls/PIMVisualizer.ascx");
            pimVisualizer.Display(PIMSchema);
            div.Controls.Add((Control)pimVisualizer);

            container.Controls.Add(div);

        }
    }
}