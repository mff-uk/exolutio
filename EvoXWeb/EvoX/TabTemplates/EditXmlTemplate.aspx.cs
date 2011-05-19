using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ActiproSoftware.CodeHighlighter;

namespace EvoX.Web.Controls.TabTemplates
{
    public class EditXmlTemplate: ITemplate
    {
        private string text;

        public EditXmlTemplate(string text)
        {
            this.text = text;
        }

     
        private TextBox inputArea;

        public void InstantiateIn(Control container)
        {
            //Button editButton = new Button();
            //editButton.Text = "Edit";
            //editButton.Click += new System.EventHandler(editButton_Click);
            //container.Controls.Add(editButton);

            inputArea = new TextBox();
            inputArea.Text = text;
            inputArea.TextMode = TextBoxMode.MultiLine;
            inputArea.CssClass = "inputArea";
            container.Controls.Add(inputArea);
        }
    }
}