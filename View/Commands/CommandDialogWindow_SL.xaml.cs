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
using System.Windows.Shapes;

namespace Exolutio.View.Commands
{
    /// <summary>
    /// Interaction logic for CommandDialogWindow.xaml
    /// </summary>
    public partial class CommandDialogWindow 
    {
        public CommandDialogWindow()
        {
            InitializeComponent();
           
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
