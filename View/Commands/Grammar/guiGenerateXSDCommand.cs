﻿using System;
using System.Xml.Linq;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar;
using Exolutio.Model.PSM.Grammar.XSDTranslation;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands.Grammar
{
    public class guiGenerateXsdCommand : guiActiveDiagramCommand
    {   
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram)
            {
                XsdSchemaGenerator schemaGenerator = new XsdSchemaGenerator();
                schemaGenerator.Initialize((PSMSchema) Current.ActiveDiagram.Schema);
                schemaGenerator.GenerateXSDStructure();
                XDocument xmlSchemaDocument = schemaGenerator.GetXsd();
                
                if (Environment.MachineName.Contains("TRUPIK"))
                {
					xmlSchemaDocument.Save(@"d:\Development\Exolutio\XSLTTest\LastSchema.xsd");
                }

                Current.MainWindow.FilePresenter.DisplayFile(xmlSchemaDocument, EDisplayedFileType.XSD, Current.ActiveDiagram.Caption + ".xsd", schemaGenerator.Log);
            }
        }

        public override string Text
        {
            get { return "Generate XSD"; }
        }

        public override string ScreenTipText
        {
            get { return "Generate XML Schema Definition document from the PSM schema"; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.XmlSchema); }
        }
    }
}