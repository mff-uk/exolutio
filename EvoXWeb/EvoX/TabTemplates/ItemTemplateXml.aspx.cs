using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ActiproSoftware.CodeHighlighter;

namespace EvoX.Web.Controls.TabTemplates
{
    public class DisplayXmlTemplate : ITemplate
    {
        private string encodedXml;
        private string text;

        public DisplayXmlTemplate(string text)
        {
            this.encodedXml = HttpUtility.HtmlEncode(text);
            this.text = text;
        }

        private CodeHighlighter ch;

        public void InstantiateIn(Control container)
        {
            //Button editButton = new Button();
            //editButton.Text = "Edit";
            //editButton.Click += new System.EventHandler(editButton_Click);
            //container.Controls.Add(editButton);
    
            //inputArea = new TextBox();
            //inputArea.TextMode = TextBoxMode.MultiLine;
            //inputArea.Visible = false; 
            //container.Controls.Add(inputArea);


            HtmlGenericControl pre = new HtmlGenericControl("pre");
            pre.Attributes["class"] = "serializedProjectArea";
            ch = new CodeHighlighter();
            ch.LineNumberMarginVisible = true;
            ch.OutliningEnabled = true;
            ch.LanguageKey = "XML";
            ch.Text = text;
            pre.Controls.Add(ch);
            container.Controls.Add(pre);
        }
    }
}