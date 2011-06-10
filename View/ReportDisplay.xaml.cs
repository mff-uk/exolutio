using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using Exolutio.Controller.Commands;
using Exolutio.SupportingClasses;
using Exolutio.View.Commands;
using Exolutio.ViewToolkit;
using OrderedList;
using Label = System.Windows.Controls.Label;
using ListItem = OrderedList.ListItem;

namespace Exolutio.View
{
    public partial class ReportDisplay: IReportDisplay
    {
        public ReportDisplay()
        {
            InitializeComponent();
        }

        public CommandReportBase DisplayedReport { get; set; }
        
        public void Update()
        {
            OrderedList.OrderedList orderedList = RenderContents();
            this.Content = orderedList;
        }

        public void DisplayReport(CommandReportBase displayedReport)
        {
            if (!this.IsVisible)
                return;
            this.DisplayedReport = displayedReport;
            this.DisplayedLog = null; 
            Update();
        }

        public void DisplayLog(Log displayedLog)
        {
            if (!this.IsVisible)
                return;
            this.DisplayedLog = displayedLog;
            this.DisplayedReport = null;
            Update();
        }

        public Dictionary<string, Tuple<string, PublicCommandAttribute.EPulicCommandCategory>> 
            KnownOperations { get; set; }

        public Log DisplayedLog { get; set; }

        public void ExecutedCommand(CommandBase command, bool ispartofmacro, CommandBase macrocommand, bool isUndo, bool isRedo)
        {
            if (ispartofmacro)
                return;
            //step = 2
            //command.Execute();
            //step = 3;
            //Tests.ModelIntegrity.ModelConsistency.CheckProject(project);
            //step = 4;
            //SaveModifiedProject(project);
            //step = 5;
            //ddlAvailableOperations.SelectedIndex = 0;
            //ClearParamsPanel(true);
            //lCommandResult.Text = string.Format("Operation '{0}' executed succesfully. ", operationHumanFriendlyName);
            //lCommandResult.Visible = true;
            CommandReportBase finalReport; 

            if (command is MacroCommand)
            {
                NestedCommandReport commandReports = ((MacroCommand) command).GetReport();
                Type commandType = command.GetType();
                if (Controller.Commands.Reflection.PublicCommandsHelper.IsPublicCommand(commandType))
                {
                    commandReports.Contents = "Executed: " + Controller.Commands.Reflection.PublicCommandsHelper.GetCommandDescriptor(commandType).CommandDescription;    
                }
                else
                {
                    commandReports.Contents = "Executed: " + commandType.Name;    
                }
                
                //repResult.DataSource = commandReports;
                //repResult.Visible = true; 
                //repResult.DataBind();
                finalReport = commandReports;
                
            }
            else
            {
                //reportDisplay.Visible = false;
                //repResult.Visible = false;
                if (isUndo || isRedo)
                {
                    NestedCommandReport r = new NestedCommandReport();
                    if (isUndo)
                    {
                        r.Contents = "Undone command";
                    }
                    else
                    {
                        r.Contents = "Redone command";
                    }
                    r.NestedReports.Add(command.Report);
                    finalReport = r;
                }
                else
                {
                    finalReport = command.Report;
                }
            }

            DisplayReport(finalReport);
        
            //else
            //{
            //    lCommandResult.Text = "Command can not execute. " + command.ErrorDescription;
            //    lCommandResult.Visible = true;
            //    lCommandResult.ForeColor = Color.Red;
            //    lCommandResult.Font.Bold = true;
        }

        protected OrderedList.OrderedList RenderContents()
        {
            OrderedList.OrderedList topLevel = new OrderedList.OrderedList() { NumberType = NumberTypes.Disc};
            topLevel.Content = new StackPanel();

            if (DisplayedReport != null)
            {
                CommandReportBase report = DisplayedReport;
                
                if (report is NestedCommandReport && string.IsNullOrEmpty(report.Contents))
                {
                    OrderedList.OrderedList subOL = new OrderedList.OrderedList() { NumberType = NumberTypes.Disc };
                    subOL.Content = new StackPanel();
                    topLevel.P().Children.Add(subOL);
                    foreach (CommandReportBase c in ((NestedCommandReport)report).NestedReports)
                    {
                        DisplayRecursive(c, subOL);
                    }
                }
                else
                {
                    OrderedList.OrderedList subOL = new OrderedList.OrderedList() { NumberType = NumberTypes.Disc };
                    subOL.Content = new StackPanel();
                    topLevel.P().Children.Add(subOL);
                    DisplayRecursive(report, subOL);
                }
            }

            if (DisplayedLog != null)
            {
                foreach (LogMessage logMessage in DisplayedLog.Errors)
                {
                    ListItem liOuter = new ListItem();
                    StackPanel liOuterP = new StackPanel();
                    liOuter.Content = liOuterP;
                    topLevel.P().Children.Add(liOuter);
                    liOuterP.Children.Add(new Label() { Content = logMessage.MessageText, FontSize = 12, Padding = ViewToolkitResources.Thicknness5 });                    
                    IEnumerable<LogMessage> relatedMessages = DisplayedLog.Where(m => m.RelatedMessage == logMessage);
                    if (relatedMessages.Count() > 0)
                    {
                        OrderedList.OrderedList innerOL = new OrderedList.OrderedList() { NumberType = NumberTypes.Disc };
                        innerOL.Content = new StackPanel();
                        liOuterP.Children.Add(innerOL);
                        foreach (LogMessage relatedMessage in relatedMessages)
                        {
                            ListItem innerLI = new ListItem();
                            innerOL.P().Children.Add(innerLI);
                            innerLI.Content = new Label() { Content = relatedMessage.MessageText, FontSize = 12, Padding = new Thickness(5) };
                        }
                    }
                }
            }

            return topLevel;
        }

        private void DisplayRecursive(CommandReportBase displayedReport, OrderedList.OrderedList output)
        {
            ListItem li = new ListItem();
            output.P().Children.Add(li);

            StackPanel liSP = new StackPanel();
            li.Content = liSP;
            
            
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(displayedReport.Contents))
            {
                sb.Append(displayedReport.Contents);
            }

            if (KnownOperations != null && displayedReport.CommandType != null && displayedReport.CommandType.FullName != null
                && KnownOperations.ContainsKey(displayedReport.CommandType.FullName))
            {
                sb.Append(string.Format(" {0}", KnownOperations[displayedReport.CommandType.FullName]));
            }
            else
            {
                sb.Append(string.Format(" "));
            }

            liSP.Children.Add(new Label() { Content = sb.ToString(), FontSize = 12, Padding = ViewToolkitResources.Thicknness5 });

            if (displayedReport is NestedCommandReport)
            {
                OrderedList.OrderedList sublist = new OrderedList.OrderedList() { NumberType = NumberTypes.Disc };
                liSP.Children.Add(sublist);
                sublist.Content = new StackPanel();
                foreach (CommandReportBase c in ((NestedCommandReport)displayedReport).NestedReports)
                {
                    DisplayRecursive(c, sublist);
                }
            }
        }
    }

    public static class OrderedListEx
    {
        public static StackPanel P(this OrderedList.OrderedList list)
        {
            return list.Content as StackPanel;
        }
    }
}
