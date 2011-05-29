using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Exolutio.Model.PSM.Grammar;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands.Grammar
{
    public partial class GrammarWindow
    {
        private Model.PSM.Grammar.Grammar displayedGrammar;

        public GrammarWindow()
        {
            InitializeComponent();
        }

        public Model.PSM.Grammar.Grammar DisplayedGrammar
        {
            get
            {
                return displayedGrammar;
            }
            set
            {
                displayedGrammar = value;
                DisplayGrammar(DisplayedGrammar);
            }
        }

        public void DisplayGrammar(Model.PSM.Grammar.Grammar grammar)
        {
            string terminals = grammar.Terminals.ConcatWithSeparator(", ");
            tbTerminals.Text = terminals;
            string nonTerminals = grammar.NonTerminals.ConcatWithSeparator(", ");
            tbNonTerminals.Text = nonTerminals;
            string initialNonTerminals = grammar.InitialNonTerminals.ConcatWithSeparator(", ");
            tbInitialNonTerminals.Text = initialNonTerminals;

            foreach (ProductionRule productionRule in grammar.ProductionRules)
            {
                //tbNonTerminals.
            }
            string productionRules = grammar.ProductionRules.ConcatWithSeparator(", " + Environment.NewLine);
            tbProductionRules.Text = productionRules; 
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; 
            Close();
        }
    }
}
