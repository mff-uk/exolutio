using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Model.PSM;
using EvoX.Web.IO;
using EvoX.Web.ModelHelper;

namespace EvoX.Web.Controls
{
    public partial class PIMClassVisualizer : System.Web.UI.UserControl, IComponentVisualizer<PIMClass>
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public Guid PIMClassId { get; set; }

        public string PIMClassName { get; set; }

        public void Display(PIMClass pimClass)
        {
            PIMClassId = pimClass;
            PIMClassName = pimClass.Name;
            litAnchor.Text = URLHelper.GetHtmlAnchoringSpan(pimClass);
            lHeader.Text = pimClass.Name;
            repeaterAttributes.DataSource = pimClass.PIMAttributes;
            repeaterAttributes.DataBind();

            repeaterAssociations.DataSource = pimClass.PIMAssociationEnds;
            repeaterAssociations.DataBind();

            repeaterDerivedClasses.DataSource = ModelIterator.GetAllModelItems(pimClass.ProjectVersion).OfType<PSMClass>().Where(c => c.Interpretation == pimClass);
            repeaterDerivedClasses.DataBind();
        }

        protected static string FullDisplayAttribute(PIMAttribute attribute)
        {
            string anchor = URLHelper.GetHtmlAnchoringSpan(attribute);
            string type = attribute.AttributeType != null ? attribute.AttributeType.Name + " " : string.Empty;
            string name = attribute.Name;
            string card = (attribute.Lower == 1 && attribute.Upper == 1)
                              ? string.Empty
                              : " " + attribute.CardinalityString;

            return anchor + type + name + card;
        }

        protected static string FullDisplayClassAssociation(PIMAssociationEnd dataItem)
        {
            string role = String.Empty;
            if (dataItem.Name != null)
            {
                role = string.Format(" (as {0})", dataItem.Name);
            }
            return URLHelper.GetHtmlAnchor(dataItem.PIMAssociation, dataItem.PIMAssociation + role);
        }


        protected static string FullDisplayDerivedClass(PSMClass psmClass)
        {
            return URLHelper.GetHtmlAnchor(psmClass, psmClass.Schema + "." + psmClass.Name, focusedTab:psmClass.ProjectVersion.PSMSchemas.IndexOf((PSMSchema) psmClass.Schema) + 3);
        }
    }
}