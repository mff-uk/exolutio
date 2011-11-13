using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Exolutio.Model;
using Exolutio.Model.OCL;
using Exolutio.View;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Exolutio.WPFClient {
    /// <summary>
    /// Interaction logic for OCLEditor.xaml
    /// </summary>
    public partial class OCLEditor : UserControl {
        public OCLEditor() {
            InitializeComponent();
            System.Xml.XmlReader reader = new System.Xml.XmlTextReader("ocl.xshd");
            textBox1.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        private Schema CurrentSchema {
            get { return Current.ActiveDiagram != null ? Current.ActiveDiagram.Schema : null; }
        }

        public OCLScript DisplayedScript { get; private set; }

        private void Create_Click(object sender, RoutedEventArgs e) {
            if (CurrentSchema != null) {
                OCLScript oclScript = new OCLScript(Current.Project, Guid.NewGuid(), CurrentSchema);
                oclScript.Contents = string.Format("-- new empty script, created {0}. ", DateTime.Now);
                cbScripts.SelectedIndex = cbScripts.Items.Count - 1;
            }
        }

        private void DisplayScript(OCLScript oclScript) {
            updatesBlocked = true;
            DisplayedScript = oclScript;

            textBox1.Clear();

            if (oclScript != null) {
                textBox1.Text = oclScript.Contents;
            }
            updatesBlocked = false;
        }

        private void Remove_Click(object sender, RoutedEventArgs e) {
            if (DisplayedScript != null) {
                DisplayedScript.Schema.OCLScripts.Remove(DisplayedScript);
                DisplayScript(null);
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e) {
            if (DisplayedScript != null) {
                DisplayedScript.Contents = textBox1.Text;
            }
        }

        public void LoadScriptsForActiveSchema() {
            DisplayedScript = null;
            textBox1.Clear();
            if (CurrentSchema != null) {
                cbScripts.ItemsSource = CurrentSchema.OCLScripts;
                if (cbScripts.Items.Count > 0) {
                    cbScripts.SelectedIndex = 0;
                    DisplayScript(CurrentSchema.OCLScripts[0]);
                }
            }
            else {
                cbScripts.ItemsSource = null;
            }
        }

        private void cbScripts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            DisplayScript((OCLScript)cbScripts.SelectedItem);
        }

        private bool updatesBlocked = false;

        private void textBox1_KeyUp(object sender, KeyEventArgs e) {
            if (DisplayedScript != null && !updatesBlocked) {
                if (DisplayedScript.Contents != textBox1.Text) {
                    DisplayedScript.Contents = textBox1.Text;
                    DisplayedScript.Project.HasUnsavedChanges = true;
                }
            }
        }

       /* private void button1_Click(object sender, RoutedEventArgs e) {
            if (DisplayedScript != null) {
                string error = DisplayedScript.Compile(DisplayedScript.Contents);
                if (string.IsNullOrWhiteSpace(error) == false) {
                    MessageBox.Show(error);
                }
                else {
                    MessageBox.Show("OK");
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            if (DisplayedScript != null) {
                string error = DisplayedScript.Compile2(DisplayedScript.Contents);

                MessageBox.Show(error);

            }
        }*/

        private void button3_Click(object sender, RoutedEventArgs e) {
            if (DisplayedScript != null) {
                var res = DisplayedScript.CompileToAst();
                StringBuilder sb = new StringBuilder();
                if (res.Errors.HasError) {
                    sb.AppendLine("Errors:");
                    foreach (var er in res.Errors.Errors) {
                        sb.AppendLine(er.Text);
                    }
                }
                else {
                    sb.AppendLine("Compilation OK.");
                    foreach (var context in res.Constraints.Classifiers) {
                        sb.AppendLine("context " + context.Context.ToString());
                        foreach (var constraint in context.Invariants) {
                            sb.AppendLine("inv: " + constraint.ToString());
                        }
                    }
                }
                MessageBox.Show(sb.ToString());
            }
        }
    }
}