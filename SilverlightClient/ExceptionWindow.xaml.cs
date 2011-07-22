using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Exolutio.SupportingClasses;

namespace SilverlightClient
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow 
    {
        private Exception exception;

        public ExceptionWindow()
        {
            InitializeComponent();
        }

        public ExceptionWindow(Exception exception): this()
        {
            this.exception = exception;

            ExolutioException xe = exception as ExolutioException;
			
            tbExMsg.Content = exception.Message;
			if (xe != null)
			{
				tbExInner.Content = String.Empty;
				expander1.Visibility = Visibility.Collapsed;
				textBlock1.Content = xe.ExceptionTitle;
                if (!string.IsNullOrEmpty(xe.ExceptionTitle))
                {
                    this.Title = xe.ExceptionTitle;
                }
				button1.Content = "Ok";
				button2.Visibility = Visibility.Collapsed;
			}
			else
			{
				if (exception.InnerException != null)
				{
					tbExInner.Content = "Inner exception: " + exception.InnerException.Message;
				}
				else
				{
					tbExInner.Content = String.Empty;
				}
			}
        }


        private void button2_Click(object sender, RoutedEventArgs e)
        {
            HtmlPage.Window.Navigate(HtmlPage.Document.DocumentUri);
        }

        private void expander1_Expanded(object sender, RoutedEventArgs e)
        {
            if (expander1.IsExpanded)
            {
                tbExStack.Content = exception.StackTrace;
            }
            else
            {
                tbExStack.Content = String.Empty;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            CloseWindow();
        }
    }
}
