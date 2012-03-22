using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using Exolutio.Model;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.Utils;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands.OCL
{
    public partial class SuggestConstraintsWindow
    {
        public SuggestConstraintsWindow()
        {
            InitializeComponent();
        }

        public PSMSchema PSMSchema { get; set; }

        public void DisplayConstraints(IList<ClassifierConstraint> constraints)
        {
            StringBuilder sb = new StringBuilder();
            PrintVisitor pv = new PrintVisitor();

            sb.Append("/* Constraints suggested from the PIM schema */");
            sb.AppendLine();

            foreach (ClassifierConstraint constraint in constraints)
            {
                if (constraint.Self.Name == VariableDeclaration.SELF)
                {
                    sb.AppendFormat("context {0}", ((Component)constraint.Context.Tag).Name);
                }
                else
                {
                    sb.AppendFormat("context {0}:{1}", constraint.Self.Name, ((Component)constraint.Context.Tag).Name);
                }
                sb.AppendLine();
                foreach (OclExpression invariant in constraint.Invariants)
                {
                    string invariantStr = pv.AstToString(invariant);
                    sb.AppendFormat("inv: ");
                    sb.Append(invariantStr);
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            tbConstraints.Text = sb.ToString();
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            CloseWindow();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            CloseWindow();
        }

        private void bCopyToScript_Click(object sender, RoutedEventArgs e)
        {
            OCLScript s = new OCLScript(PSMSchema);
            s.Contents = tbConstraints.Text;
            CloseWindow();
            Current.MainWindow.DiagramTabManager.ShowOCLScript(s);
        }
    }
}
