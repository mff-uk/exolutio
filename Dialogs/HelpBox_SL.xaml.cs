using System.Windows;
using System.Windows.Input;
using SilverFlow.Controls;

namespace EvoX.Dialogs
{
    /// <summary>
    /// Interaction logic for HelpBox.xaml
    /// </summary>
    public partial class HelpBox 
    {
        public HelpBox()
        {
            InitializeComponent();
        }


        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
        	DialogResult = true; 
            Close();
        }

    }
}
