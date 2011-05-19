using System.Web.UI;
using System.Web.UI.HtmlControls;
using EvoX.Model.PSM;
using EvoX.Web.ModelHelper;

namespace EvoX.Web.Controls.TabTemplates
{
    public class TabPanelPSMTemplate : ITemplate
    {
        public PSMSchema PSMSchema { get; private set; }

        public Page Page { get; set; }

        public TabPanelPSMTemplate(Page page, PSMSchema PSMSchema)
        {
            this.PSMSchema = PSMSchema;
            this.Page = page;
        }

        public void InstantiateIn(Control container)
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes["class"] = "tabContainer";
            
            IComponentVisualizer<PSMSchema> PSMVisualizer = (IComponentVisualizer<PSMSchema>)Page.LoadControl("Controls/PSMVisualizer.ascx");
            PSMVisualizer.Display(PSMSchema);
            div.Controls.Add((Control) PSMVisualizer);
            
            container.Controls.Add(div);
        }
    }
}