using System.Web.UI.WebControls;

namespace EvoX.Web.Controls
{
    public class EvoXClearablePanel: Panel
    {
        public void ClearState()
        {
            ClearChildState();
        }
    }
}