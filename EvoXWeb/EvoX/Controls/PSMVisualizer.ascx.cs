using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoX.Model.PSM;
using EvoX.Web.ModelHelper;

namespace EvoX.Web.Controls
{
    public partial class PSMVisualizer : System.Web.UI.UserControl, IComponentVisualizer<PSMSchema>
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void Display(PSMSchema PSMSchema)
        {
            PSMTreeView.DataSource = new PSMSchemaAsDataSource(PSMSchema);
            PSMTreeView.DataBind();
            DisableSelectAction(PSMTreeView.Nodes);
        }

        private static void DisableSelectAction(TreeNodeCollection nodes)
        {
            foreach (TreeNode treeNode in nodes)
            {
                treeNode.SelectAction = TreeNodeSelectAction.None;
                DisableSelectAction(treeNode.ChildNodes);
            }
        }
    }
}