using System.Windows;
using System.Windows.Media;

namespace EvoX.Dialogs
{
    /// <summary>
    /// Interaction logic for ErrorMsgBox.xaml
    /// </summary>
    public partial class EvoXMsgBox : Window
    {
        public EvoXMsgBox()
        {
            InitializeComponent();

            this.Icon = (ImageSource) FindResource("question_mark");
        }

        public static void Show(string windowTitle, string textTitle, string textSubtitle)
        {
            EvoXMsgBox msgBox = new EvoXMsgBox();
            msgBox.Title = windowTitle;
            msgBox.messageText.Text = textTitle;
            msgBox.messageQuestion.Text = textSubtitle;
            msgBox.ShowDialog();
            return;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
