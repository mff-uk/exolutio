using System.Windows;
using System.Windows.Media;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for ExolutioMessageBox.xaml
    /// </summary>
    public partial class ExolutioMessageBox : Window
    {
        public ExolutioMessageBox()
        {
            InitializeComponent();

            this.Icon = (ImageSource) FindResource("question_mark");
        }

        public static void Show(string windowTitle, string textTitle, string textSubtitle)
        {
            ExolutioMessageBox messageBox = new ExolutioMessageBox();
            messageBox.Title = windowTitle;
            messageBox.messageText.Text = textTitle;
            messageBox.messageQuestion.Text = textSubtitle;
            messageBox.ShowDialog();
            return;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
