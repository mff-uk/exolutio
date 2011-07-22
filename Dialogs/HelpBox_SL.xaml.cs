using System.Windows;
using System.Windows.Input;

namespace Exolutio.Dialogs
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
        	Close(true);
        }

    }
}
