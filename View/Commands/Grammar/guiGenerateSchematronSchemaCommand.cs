using System;
using System.Linq;
using System.Xml.Linq;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar.SchematronTranslation;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;
using Exolutio.Model.OCL.AST;

namespace Exolutio.View.Commands.Grammar
{
    public class guiGenerateSchematronSchemaCommand : guiActiveDiagramCommand
    {
        private class TagClass
        {
            public TranslationSettings settings { get; set; }
            public ExpressionTweakingPanel tweakingPanel { get; set; }
        }

        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram)
            {
                XDocument schematronSchemaDocument;
                ILog log;

                TranslationSettings settings = new TranslationSettings();
                settings.Functional = true;
                settings.SchemaAware = true;

                GenerateSchema((PSMSchema)Current.ActiveDiagram.Schema, settings, out schematronSchemaDocument, out log);

                FilePresenterButtonInfo[] additionalButtonsInfo = new[] {
                    new FilePresenterButtonInfo() { ButtonName = "SA", Text = "Schema aware", Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.refresh), UpdateFileContentAction = RegenerateSchema, ToggleButton = true, IsToggled = true },
                    new FilePresenterButtonInfo() { ButtonName = "F", Text = "Functional", Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.refresh), UpdateFileContentAction = RegenerateSchema, ToggleButton = true, IsToggled = true },
                    };
                
                ExpressionTweakingPanel tweakingPanel = new ExpressionTweakingPanel();

                TagClass tag = new TagClass();
                tag.settings = settings;
                tag.tweakingPanel = tweakingPanel;

                IFilePresenterTab filePresenterTab
                    = Current.MainWindow.FilePresenter.DisplayFile(schematronSchemaDocument, EDisplayedFileType.SCH, Current.ActiveDiagram.Caption + ".sch", log, sourcePSMSchema: (PSMSchema)Current.ActiveDiagram.Schema,
                    additionalActions: additionalButtonsInfo, tag: tag);
                filePresenterTab.RefreshCallback += RegenerateSchema;
                if (settings.SubexpressionTranslations.TranslationOptionsWithMorePossibilities.Any())
                {
                    tweakingPanel.Bind(settings.SubexpressionTranslations);
                    tweakingPanel.FilePresenterTab = filePresenterTab;
                    filePresenterTab.DisplayAdditionalControl(tweakingPanel, "Expression Tweaking");
                    tweakingPanel.TranslationTweaked += tweakingPanel_TranslationTweaked;
                }
            }
        }

        private void RegenerateSchema(IFilePresenterTab filePresenterTab)
        {
            XDocument document;
            ILog log;

            TagClass tag = (TagClass) filePresenterTab.Tag;
            TranslationSettings settings = tag.settings;
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
            settings.SubexpressionTranslations.Clear();
            settings.Retranslation = false;
            GenerateSchema(filePresenterTab.SourcePSMSchema, settings, out document, out log);
            tag.tweakingPanel.Bind(settings.SubexpressionTranslations);
            filePresenterTab.ReDisplayFile(document, EDisplayedFileType.SCH, filePresenterTab.SourcePSMSchema.Caption, log, filePresenterTab.ValidationSchema, filePresenterTab.SourcePSMSchema);
        }

        void tweakingPanel_TranslationTweaked(object sender, ExpressionTweakingPanel.TranslationTweakedEventArgs translationTweakedEventArgs)
        {
            ExpressionTweakingPanel p = (ExpressionTweakingPanel) sender;
            XDocument document;
            ILog log;
            TagClass tag = (TagClass)p.FilePresenterTab.Tag;
            TranslationSettings settings = tag.settings;
            settings.Retranslation = true; 
            GenerateSchema(p.FilePresenterTab.SourcePSMSchema, settings, out document, out log);
            p.FilePresenterTab.ReDisplayFile(document, EDisplayedFileType.SCH,
                p.FilePresenterTab.SourcePSMSchema.Caption, log, p.FilePresenterTab.ValidationSchema, p.FilePresenterTab.SourcePSMSchema);
        }

        //private void GenerateSchemaAware(IFilePresenterTab filePresenterTab)
        //{
        //    XDocument document;
        //    ILog log;
        //    SchematronSchemaGenerator.TranslationSettings settings = (SchematronSchemaGenerator.TranslationSettings)filePresenterTab.Tag;
        //    GenerateSchema(filePresenterTab.SourcePSMSchema, settings, out document, out log);
        //    filePresenterTab.ReDisplayFile(document, EDisplayedFileType.SCH, filePresenterTab.SourcePSMSchema.Caption, log, filePresenterTab.ValidationSchema, filePresenterTab.SourcePSMSchema);
        //}

        private static void GenerateSchema(PSMSchema psmSchema, TranslationSettings settings, out XDocument schematronSchemaDocument, out ILog log)
        {
            SchematronSchemaGenerator schemaGenerator = new SchematronSchemaGenerator();
            schemaGenerator.Initialize(psmSchema);
            settings.SubexpressionTranslations.Log = schemaGenerator.Log;
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