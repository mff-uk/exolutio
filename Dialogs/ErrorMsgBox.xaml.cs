using System.Windows;
using System.Windows.Media;

namespace EvoX.Dialogs
{
    /// <summary>
    /// Interaction logic for ErrorMsgBox.xaml
    /// </summary>
    public partial class ErrorMsgBox : Window
    {
        public ErrorMsgBox()
        {
            InitializeComponent();

            this.Icon = (ImageSource) FindResource("question_mark");
        }

        private static ErrorMsgBox msgBox;

        public static void Show(string messageText, string messageQuestion)
        {
            msgBox = new ErrorMsgBox();           
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
