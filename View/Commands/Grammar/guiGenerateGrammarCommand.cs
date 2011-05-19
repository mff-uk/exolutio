﻿using EvoX.Model.PSM;
using EvoX.Model.PSM.Grammar;
using EvoX.ResourceLibrary;

namespace EvoX.View.Commands.Grammar
{
    public class guiGenerateGrammarCommand : guiActiveDiagramCommand
    {   
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram)
            {
                GrammarGenerator generator = new GrammarGenerator();
                EvoX.Model.PSM.Grammar.Grammar grammar = generator.GenerateGrammar((PSMSchema) Current.ActiveDiagram.Schema);
                
#if SILVERLIGHT
                GrammarWindow w = new GrammarWindow();
                Current.MainWindow.FloatingWindowHost.Add(w);
                w.DisplayedGrammar = grammar; 
                w.ShowModal();
#endif
            }
        }

        public override string Text
        {
            get { return "Generate grammar"; }
        }

        public override string ScreenTipText
        {
            get { return "Generate regular tree grammar from the PSM schema"; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return EvoXResourceNames.GetResourceImageSource(EvoXResourceNames.component_new); }
        }
    }
}