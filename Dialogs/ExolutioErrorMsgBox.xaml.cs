using System.Windows;
using System.Windows.Media;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for ExolutioErrorMsgBox.xaml
    /// </summary>
    public partial class ExolutioErrorMsgBox : Window
    {
        public ExolutioErrorMsgBox()
        {
            InitializeComponent();

            this.Icon = (ImageSource) FindResource("question_mark");
        }

        private static ExolutioErrorMsgBox msgBox;

        public static void Show(string messageText, string messageQuestion)
        {
            msgBox = new ExolutioErrorMsgBox();           
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
