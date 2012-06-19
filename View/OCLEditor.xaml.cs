using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Exolutio.View;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for OCLEditor.xaml
    /// </summary>
    public partial class OCLEditor : UserControl
    {
        public OCLEditor()
        {
            InitializeComponent();
            System.Xml.XmlReader reader;
            using (StringReader sr = new StringReader(Properties.Resources.OCL_syntax))
            {
                reader = new System.Xml.XmlTextReader(sr);
                avalonEdit.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            if (!System.Environment.MachineName.Contains("TRUPIK"))
                bSaveVariants.Visibility = Visibility.Collapsed;

            Current.ActiveOCLScriptChanged += Current_ActiveOCLScriptChanged;
        }

        void Current_ActiveOCLScriptChanged()
        {
            updatesBlocked = true;
            
            avalonEdit.Clear();

            if (Current.ActiveOCLScript != null)
            {
                avalonEdit.Text = Current.ActiveOCLScript.Contents;
                avalonEdit.IsEnabled = true; 
            }
            else
            {
                avalonEdit.IsEnabled = false; 
            }
            avalonEdit.Document.UndoStack.ClearAll();
            updatesBlocked = false;
        }

        private bool updatesBlocked = false;

        private void avalonEdit_KeyUp(object sender, KeyEventArgs e)
        {
            if (Current.ActiveOCLScript != null && !updatesBlocked)
            {
                if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (avalonEdit.CanUndo)
                    {
                        avalonEdit.Undo();
                    }
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    if (avalonEdit.CanRedo)
                    {
                        avalonEdit.Redo();
                    }
                    e.Handled = true;
                    return;
                }

                if (Current.ActiveOCLScript.Contents != avalonEdit.Text)
                {
                    Current.ActiveOCLScript.Contents = avalonEdit.Text;
                    Current.Project.HasUnsavedChanges = true;
                }
            }
        }

        private void bSaveVariants_Click(object sender, RoutedEventArgs e)
        {
            //if (this.CurrentSchema != null && this.CurrentSchema is PSMSchema)
            //{ 
            //    SchematronSchemaGenerator generator = new SchematronSchemaGenerator();
            //    generator.Initialize((PSMSchema) this.CurrentSchema);

            //    Tuple<bool, bool>[] variants = new Tuple<bool, bool>[]
            //                                       {
            //                                           new Tuple<bool, bool>(true, true),
            //                                           new Tuple<bool, bool>(true, false),
            //                                           new Tuple<bool, bool>(false, true),
            //                                           new Tuple<bool, bool>(false, false)
            //                                       };

            //    foreach (Tuple<bool, bool> variant in variants)
            //    {
            //        bool schemaAware = variant.Item1;
            //        bool functional = variant.Item2;
            //        TranslationSettings settings = new TranslationSettings(schemaAware, functional);
            //        XDocument sa_fun = generator.GetSchematronSchema(settings);
            //        string fn = string.Format(@"D:\Programování\EvoXSVN\OclX\Examples\{0}\{1}\{2}{3}.sch",
            //                                  functional ? "Functional" : "Dynamic",
            //                                  schemaAware ? "SchemaAware" : "NotSchemaAware",
            //                                  this.CurrentSchema.Caption,
            //                                  schemaAware ? "SA" : string.Empty
            //            );
            //        sa_fun.Save(fn);
            //    }
            //}
        }
    }
}