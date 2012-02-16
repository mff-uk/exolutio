using System;
using System.Xml.Linq;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar.SchematronTranslation;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands.Grammar
{
    public class guiGenerateSchematronSchemaCommand : guiActiveDiagramCommand
    {
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram)
            {
                XDocument schematronSchemaDocument;
                ILog log;

                SchematronSchemaGenerator.TranslationSettings settings = new SchematronSchemaGenerator.TranslationSettings();
                settings.Functional = true;
                settings.SchemaAware = true; 

                GenerateSchema((PSMSchema)Current.ActiveDiagram.Schema, settings, out schematronSchemaDocument, out log);

                FilePresenterButtonInfo[] additionalButtonsInfo = new[] {
                    new FilePresenterButtonInfo() { ButtonName = "SA", Text = "Schema aware", Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.refresh), UpdateFileContentAction = RegenerateSchema, ToggleButton = true, IsToggled = true },
                    new FilePresenterButtonInfo() { ButtonName = "F", Text = "Functional", Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.refresh), UpdateFileContentAction = RegenerateSchema, ToggleButton = true, IsToggled = true },
                    };
                IFilePresenterTab filePresenterTab
                    = Current.MainWindow.FilePresenter.DisplayFile(schematronSchemaDocument, EDisplayedFileType.SCH, Current.ActiveDiagram.Caption + ".sch", log, sourcePSMSchema:(PSMSchema)Current.ActiveDiagram.Schema,
                    additionalActions: additionalButtonsInfo, tag: settings);
                filePresenterTab.RefreshCallback += RegenerateSchema;
            }
        }

        private void RegenerateSchema(IFilePresenterTab filePresenterTab)
        {
            XDocument document;
            ILog log;

            SchematronSchemaGenerator.TranslationSettings settings = (SchematronSchemaGenerator.TranslationSettings) filePresenterTab.Tag;
            foreach (FilePresenterButtonInfo buttonInfo in filePresenterTab.FilePresenterButtons)
            {
                if (buttonInfo.ButtonName == "SA")
                {
                    settings.SchemaAware = buttonInfo.IsToggled;
                }
                if (buttonInfo.ButtonName == "F")
                {
                    settings.Functional = buttonInfo.IsToggled;
                }
            }

            GenerateSchema(filePresenterTab.SourcePSMSchema, settings, out document, out log);
            filePresenterTab.ReDisplayFile(document, EDisplayedFileType.SCH, filePresenterTab.SourcePSMSchema.Caption, log, filePresenterTab.ValidationSchema, filePresenterTab.SourcePSMSchema);
        }


        //private void GenerateSchemaAware(IFilePresenterTab filePresenterTab)
        //{
        //    XDocument document;
        //    ILog log;
        //    SchematronSchemaGenerator.TranslationSettings settings = (SchematronSchemaGenerator.TranslationSettings)filePresenterTab.Tag;
        //    GenerateSchema(filePresenterTab.SourcePSMSchema, settings, out document, out log);
        //    filePresenterTab.ReDisplayFile(document, EDisplayedFileType.SCH, filePresenterTab.SourcePSMSchema.Caption, log, filePresenterTab.ValidationSchema, filePresenterTab.SourcePSMSchema);
        //}

        private static void GenerateSchema(PSMSchema psmSchema, SchematronSchemaGenerator.TranslationSettings settings, out XDocument schematronSchemaDocument, out ILog log)
        {
            SchematronSchemaGenerator schemaGenerator = new SchematronSchemaGenerator();
            schemaGenerator.Initialize(psmSchema);

            schematronSchemaDocument = schemaGenerator.GetSchematronSchema(settings);

            if (Environment.MachineName.Contains("TRUPIK"))
            {
                schematronSchemaDocument.Save(@"D:\Programování\EVOXSVN\SchematronTest\LastSchSchema.sch");
            }
            log = schemaGenerator.Log;
        }

        //private void RefreshCallback(IFilePresenterTab filePresenterTab)
        //{
        //    XDocument document;
        //    ILog log;
        //    settings.SchemaAware = true; 
        //    GenerateSchema(filePresenterTab.SourcePSMSchema, settings, out document, out log);
        //    filePresenterTab.ReDisplayFile(document, EDisplayedFileType.SCH, filePresenterTab.SourcePSMSchema.Caption, log, filePresenterTab.ValidationSchema, filePresenterTab.SourcePSMSchema);
        //}

        public override string Text
        {
            get { return "Generate Schematron schema"; }
        }

        public override string ScreenTipText
        {
            get { return "Generate Schematron schema from the OCL scripts defined for this PSM schema"; }
        }

        public override bool CanExecute(object parameter = null)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.bilby); }
        }
    }
}