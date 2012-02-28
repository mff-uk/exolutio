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
using Exolutio.Model.OCL.AST;
using Exolutio.Model.PSM.Grammar.SchematronTranslation;

namespace Exolutio.View.Commands.Grammar
{
    /// <summary>
    /// Interaction logic for ExpressionTweakingPanel.xaml
    /// </summary>
    public partial class ExpressionTweakingPanel : UserControl
    {
        public ExpressionTweakingPanel()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private SubexpressionTranslations SubexpressionTranslations { get; set; }

        public IFilePresenterTab FilePresenterTab { get; set; }

        public void Bind(SubexpressionTranslations subexpressionTranslations)
        {
            treeView1.ItemsSource = subexpressionTranslations.TranslationOptionsWithMorePossibilities;
            this.SubexpressionTranslations = subexpressionTranslations;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //treeView1.ItemsSource = SubexpressionTranslations.TranslationOptionsWithMorePossibilities;
            TranslationOption option = (TranslationOption) ((RadioButton) sender).DataContext;
            option.Select();
            //TranslationOption option = (TranslationOption) (((RadioButton) sender).DataContext);
            treeView1.ItemsSource = SubexpressionTranslations.TranslationOptionsWithMorePossibilities;

            RaiseTranslationTweaked(option);
        }

        public class TranslationTweakedEventArgs: EventArgs
        {
            public TranslationOption TranslationOption { get; set; }
        }
        public event EventHandler<TranslationTweakedEventArgs> TranslationTweaked;

        public void RaiseTranslationTweaked(TranslationOption option)
        {
            EventHandler<TranslationTweakedEventArgs> handler = TranslationTweaked;
            if (handler != null) handler(this, new TranslationTweakedEventArgs() { TranslationOption = option });
        }

    }
}
