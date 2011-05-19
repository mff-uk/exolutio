#define WRITECOMMANDTYPE

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EvoX.Controller.Commands;
using EvoX.SupportingClasses;

namespace EvoX.Web.Controls
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:CommandReportDisplay runat=server></{0}:CommandReportDisplay>")]
    public class CommandReportDisplay : WebControl
    {
        //[Bindable(true)]
        //[Category("Appearance")]
        //[DefaultValue("")]
        //[Localizable(true)]
        //public string Text
        //{
        //    get
        //    {
        //        String s = (String)ViewState["Text"];
        //        return ((s == null) ? String.Empty : s);
        //    }

        //    set
        //    {
        //        ViewState["Text"] = value;
        //    }
        //}

        public CommandReportBase DisplayedReport
        {
            get
            {
                CommandReportBase r = (CommandReportBase)ViewState["DisplayedReport"];
                return r;
            }
            set
            {
                ViewState["DisplayedReport"] = value;
            }
        }

        private Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> KnownOperations { get; set; }

        public Log DisplayedLog { get; set; }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.AddAttribute(HtmlTextWriterAttribute.Class, "commandResult");
            if (DisplayedReport != null)
            {
                CommandReportBase report = DisplayedReport;
                if (KnownOperations == null)
                {
                    KnownOperations = ModelHelper.ModelHelper.GetAvailableOperations();
                }

                if (report is NestedCommandReport && string.IsNullOrEmpty(report.Contents))
                {
                    output.RenderBeginTag(HtmlTextWriterTag.Ul);
                    foreach (CommandReportBase c in ((NestedCommandReport)report).NestedReports)
                    {
                        DisplayRecursive(c, output);
                    }
                    output.RenderEndTag();
                }
                else
                {
                    output.RenderBeginTag(HtmlTextWriterTag.Ul);
                    DisplayRecursive(report, output);
                    output.RenderEndTag();
                }
            }
            if (DisplayedLog != null)
            {
                output.RenderBeginTag(HtmlTextWriterTag.Ul);

                foreach (LogMessage logMessage in DisplayedLog.Errors)
                {
                    output.RenderBeginTag(HtmlTextWriterTag.Li);
                    output.WriteEncodedText(logMessage.MessageText);
                    IEnumerable<LogMessage> relatedMessages = DisplayedLog.Where(m => m.RelatedMessage == logMessage);
                    if (relatedMessages.Count() > 0)
                    {
                        output.RenderBeginTag(HtmlTextWriterTag.Ul);
                        foreach (LogMessage relatedMessage in relatedMessages)
                        {
                            output.RenderBeginTag(HtmlTextWriterTag.Li);
                            output.WriteEncodedText(relatedMessage.MessageText);
                            output.RenderEndTag();
                        }
                        output.RenderEndTag();
                    }
                    output.RenderEndTag();
                }

                output.RenderEndTag();
            }
            output.RenderEndTag();
        }

        private void DisplayRecursive(CommandReportBase displayedReport, HtmlTextWriter output)
        {
            output.RenderBeginTag(HtmlTextWriterTag.Li);
            if (!string.IsNullOrEmpty(displayedReport.Contents))
            {
                output.WriteEncodedText(displayedReport.Contents);
            }

            if (KnownOperations != null && displayedReport.CommandType != null && displayedReport.CommandType.FullName != null
                && KnownOperations.ContainsKey(displayedReport.CommandType.FullName))
            {
                output.WriteEncodedText(string.Format(" {0}", KnownOperations[displayedReport.CommandType.FullName]));
            }
            else
            {
                output.WriteEncodedText(string.Format(" "));
            }

            if (displayedReport is NestedCommandReport)
            {
                output.RenderBeginTag(HtmlTextWriterTag.Ul);
                foreach (CommandReportBase c in ((NestedCommandReport)displayedReport).NestedReports)
                {
                    DisplayRecursive(c, output);
                }
                output.RenderEndTag();
            }
            output.RenderEndTag();
        }
    }
}
