using System.Windows;
using System.Windows.Media;
using SilverFlow.Controls;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for ErrorMsgBox.xaml
    /// </summary>
    public partial class ExolutioErrorMsgBox 
    {
        public ExolutioErrorMsgBox()
        {
            InitializeComponent();
            #if SILVERLIGHT
            #else
            this.Icon = (ImageSource) FindResource("question_mark");
            #endif
        }

        private static ExolutioErrorMsgBox msgBox;

#if SILVERLIGHT
        public static void Show(string messageText, string messageQuestion, FloatingWindowHost host)
#else
        public static void Show(string messageText, string messageQuestion)
#endif
        {
            msgBox = new ExolutioErrorMsgBox();           
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            #if SILVERLIGHT
            host.Add(msgBox);
            msgBox.ShowModal();
            #else
            msgBox.ShowDialog();
            #endif
            return;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            msgBox.Close();
        }
    }
}
