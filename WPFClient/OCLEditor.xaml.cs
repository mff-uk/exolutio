using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Linq;
using Exolutio.Controller.Commands.Atomic.MacroWrappers;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.OCL;
using Exolutio.Model.PSM;
using Exolutio.View;
using ICSharpCode.AvalonEdit.Highlighting;
using Exolutio.Model.PSM.Grammar.SchematronTranslation;

namespace Exolutio.WPFClient
{
    /// <summary>
    /// Interaction logic for OCLEditor.xaml
    /// </summary>
    public partial class OCLEditor : UserControl
    {
        public OCLEditor()
        {
            InitializeComponent();
            StringReader sr = new StringReader(Properties.Resources.OCL_syntax);
            System.Xml.XmlReader reader = new System.Xml.XmlTextReader(sr);
            textBox1.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
            if (!System.Environment.MachineName.Contains("TRUPIK"))
                bSaveVariants.Visibility = Visibility.Collapsed;
        }

        private Schema CurrentSchema
        {
            get { return Current.ActiveDiagram != null ? Current.ActiveDiagram.Schema : null; }
        }

        public OCLScript DisplayedScript { get; private set; }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentSchema != null)
            {
                OCLScript oclScript = new OCLScript(Current.Project, Guid.NewGuid(), CurrentSchema);
                oclScript.Contents = string.Format("-- new empty script, created {0}. ", DateTime.Now);
                cbScripts.SelectedIndex = cbScripts.Items.Count - 1;
            }
        }

        private void DisplayScript(OCLScript oclScript)
        {
            updatesBlocked = true;
            DisplayedScript = oclScript;

            textBox1.Clear();

            if (oclScript != null)
            {
                textBox1.Text = oclScript.Contents;
            }
            textBox1.Document.UndoStack.ClearAll();
            updatesBlocked = false;
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayedScript != null)
            {
                DisplayedScript.Schema.OCLScripts.Remove(DisplayedScript);
                DisplayScript(null);
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayedScript != null)
            {
                DisplayedScript.Contents = textBox1.Text;
            }
        }

        public void LoadScriptsForActiveSchema()
        {
            DisplayedScript = null;
            textBox1.Clear();
            if (CurrentSchema != null)
            {
                cbScripts.ItemsSource = CurrentSchema.OCLScripts;
                if (cbScripts.Items.Count > 0)
                {
                    cbScripts.SelectedIndex = 0;
                    DisplayScript(CurrentSchema.OCLScripts[0]);
                }
            }
            else
            {
                cbScripts.ItemsSource = null;
            }
        }

        private void cbScripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayScript((OCLScript)cbScripts.SelectedItem);
        }

        private bool updatesBlocked = false;

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {

            if (DisplayedScript != null && !updatesBlocked)
            {

                if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (textBox1.CanUndo)
                    {
                        textBox1.Undo();
                    }
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (textBox1.CanRedo)
                    {
                        textBox1.Redo();
                    }
                    e.Handled = true;
                    return;
                }

                if (DisplayedScript.Contents != textBox1.Text)
                {
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

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayedScript != null)
            {
                var res = DisplayedScript.CompileToAst();
                StringBuilder sb = new StringBuilder();
                if (res.Errors.HasError)
                {
                    sb.AppendLine("Errors:");
                    foreach (var er in res.Errors.Errors)
                    {
                        sb.AppendLine(er.ToString());
                    }
                }
                else
                {
                    sb.AppendLine("Compilation OK.");
                    foreach (var context in res.Constraints.ClassifierConstraintBlocks)
                    {
                        sb.AppendLine("context " + context.Context.ToString());
                        foreach (var constraint in context.Invariants)
                        {
                            sb.AppendLine("inv: " + constraint.ToString());
                        }
                    }
                }
                MessageBox.Show(sb.ToString());
            }
        }

        public void ShowScript(OCLScript oclScript)
        {
            cbScripts.SelectedItem = oclScript;
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayedScript != null)
            {
                string newName;
                if (ExolutioInputBox.Show("Enter new name of the script", DisplayedScript.Name, out newName) == true)
                {
                    cmdRenameComponent c = new cmdRenameComponent(Current.Controller);
                    c.ComponentGuid = DisplayedScript;
                    c.NewName = newName;
                    c.Execute();
                }
            }

        }

        private void bSaveVariants_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentSchema != null && this.CurrentSchema is PSMSchema)
            { 
                SchematronSchemaGenerator generator = new SchematronSchemaGenerator();
                generator.Initialize((PSMSchema) this.CurrentSchema);

                Tuple<bool, bool>[] variants = new Tuple<bool, bool>[]
                                                   {
                                                       new Tuple<bool, bool>(true, true),
                                                       new Tuple<bool, bool>(true, false),
                                                       new Tuple<bool, bool>(false, true),
                                                       new Tuple<bool, bool>(false, false)
                                                   };

                foreach (Tuple<bool, bool> variant in variants)
                {
                    bool schemaAware = variant.Item1;
                    bool functional = variant.Item2;
                    TranslationSettings settings = new TranslationSettings(schemaAware, functional);
                    XDocument sa_fun = generator.GetSchematronSchema(settings);
                    string fn = string.Format(@"D:\Programování\EvoXSVN\OclX\Examples\{0}\{1}\{2}{3}.sch",
                                              functional ? "Functional" : "Dynamic",
                                              schemaAware ? "SchemaAware" : "NotSchemaAware",
                                              this.CurrentSchema.Caption,
                                              schemaAware ? "SA" : string.Empty
                        );
                    sa_fun.Save(fn);
                }
            }
        }
    }
}