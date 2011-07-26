using System.Windows;
using System.Windows.Media;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for ExolutioErrorMsgBox.xaml
    /// </summary>
    public partial class ExolutioErrorMsgBox
    {
        public ExolutioErrorMsgBox()
        {
            InitializeComponent();

            this.Icon = (ImageSource) FindResource("question_mark");
        }

        private static ExolutioErrorMsgBox msgBox;

        /// <summary>
        /// Shows error message box
        /// </summary>
        /// <param name="messageText">Bold text</param>
        /// <param name="additionalText">additional text</param>
        public static void Show(string messageText, string additionalText)
        {
            msgBox = new ExolutioErrorMsgBox();           
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = additionalText;
            msgBox.ShowDialog();
            return;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.CloseWindow();
        }
    }
}
