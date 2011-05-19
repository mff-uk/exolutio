﻿using EvoX.Model.PSM;
using EvoX.Model.PSM.Normalization;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Grammar
{
    public class guiNormalizeSchemaCommand : guiActiveDiagramCommand
    {
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram)
            {
                Normalizer normalizer = new Normalizer();
                normalizer.Controller = Current.Controller;
                normalizer.NormalizeSchema((PSMSchema) Current.ActiveDiagram.Schema);
                Current.MainWindow.DisplayReport(normalizer.FinalReport);
            }
        }

        public override string Text
        {
            get { return "Normalize"; }
        }

        public override string ScreenTipText
        {
            get { return "Normalize PSM schema"; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.component_edit); }
        }
    }
}