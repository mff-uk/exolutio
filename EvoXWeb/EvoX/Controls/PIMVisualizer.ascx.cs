using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoX.Model.PIM;
using EvoX.Web.ModelHelper;

namespace EvoX.Web.Controls
{
    public partial class PIMVisualizer : System.Web.UI.UserControl, IComponentVisualizer<PIMSchema>
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void Display(PIMSchema pimSchema)
        {
            repeaterPIMClasses.DataSource = pimSchema.PIMClasses;
            repeaterPIMClasses.DataBind();

            repeaterPIMAssociations.DataSource = pimSchema.PIMAssociations;
            repeaterPIMAssociations.DataBind();
        }

        protected void repeaterPIMClasses_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
            {
                PIMClass pimClass = (PIMClass)e.Item.DataItem;

                PIMClassVisualizer classVisualizer = (PIMClassVisualizer)e.Item.FindControl("pimClassVisualizer");
                classVisualizer.Display(pimClass);
            }
        }

        protected void repeaterPIMAssociations_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
            {
                PIMAssociation pimAssociation = (PIMAssociation)e.Item.DataItem;

                PIMAssociationVisualizer pimAssociationVisualizer = (PIMAssociationVisualizer)e.Item.FindControl("pimAssociationVisualizer");
                pimAssociationVisualizer.Display(pimAssociation);
            }
        }
    }
}