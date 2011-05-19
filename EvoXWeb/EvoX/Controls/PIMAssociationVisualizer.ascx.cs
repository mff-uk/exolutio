using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoX.Model.PIM;
using EvoX.Web.IO;
using EvoX.Web.ModelHelper;
using EvoX.SupportingClasses;

namespace EvoX.Web.Controls
{
    public partial class PIMAssociationVisualizer : System.Web.UI.UserControl, IComponentVisualizer<PIMAssociation>
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public Guid PIMAssociationId { get; set; }

        public string ParticipantsArray { get; set; }

        public void Display(PIMAssociation pimAssociation)
        {
            litAnchor.Text = URLHelper.GetHtmlAnchoringSpan(pimAssociation);
            PIMAssociationId = pimAssociation.ID;
            ParticipantsArray = pimAssociation.PIMClasses.Select(c => "'w" + c.ID + "'").ConcatWithSeparator(",");
            if (!string.IsNullOrEmpty(pimAssociation.Name))
            {
                lHeader.Text = pimAssociation.Name;
            }
            else
            {
                lHeader.Text = pimAssociation.ToString();
            }

            repeaterParticipants.DataSource = pimAssociation.PIMAssociationEnds;
            repeaterParticipants.DataBind();
        }

        protected void repeaterParticipants_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType.IsAmong(ListItemType.AlternatingItem, ListItemType.Item))
            {
                PIMAssociationEnd pimAssociationEnd = (PIMAssociationEnd)e.Item.DataItem;

                Label lRole = (Label)e.Item.FindControl("lRole");
                if (string.IsNullOrEmpty(pimAssociationEnd.Name))
                {
                    lRole.Visible = false;
                }
                else
                {
                    lRole.Text = "<br />Role: " + pimAssociationEnd.Name;
                    lRole.Visible = true; 
                }
                Label lCardinality = (Label)e.Item.FindControl("lCardinality");
                lCardinality.Text = pimAssociationEnd.CardinalityString;
                HyperLink lParticipantName = (HyperLink)e.Item.FindControl("lParticipantName");
                lParticipantName.Text = pimAssociationEnd.PIMClass.Name;
                lParticipantName.NavigateUrl = URLHelper.GetUrlOf(pimAssociationEnd.PIMClass);
                lParticipantName.Attributes["onclick"] = URLHelper.JSSelectComponentWrapper(pimAssociationEnd.PIMClass);
            }
        }

        
    }
}