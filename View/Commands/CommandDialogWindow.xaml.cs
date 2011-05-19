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

namespace EvoX.View.Commands
{
    /// <summary>
    /// Interaction logic for CommandDialogWindow.xaml
    /// </summary>
    public partial class CommandDialogWindow : Window
    {
        public CommandDialogWindow()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(CommandDialogWindow_Loaded); 
        }

        void CommandDialogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (Control child in spParameters.Children)
            {
                if (!(child is Label))
                {    
                    child.Focus();
                    break;
                }
                
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;    
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; 
            Close();
        }
    }
}
