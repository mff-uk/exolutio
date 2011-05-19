using System.Windows;
using System.Windows.Media;
using SilverFlow.Controls;

namespace EvoX.Dialogs
{
    /// <summary>
    /// Interaction logic for ErrorMsgBox.xaml
    /// </summary>
    public partial class EvoXMsgBox
    {
        public EvoXMsgBox()
        {
            InitializeComponent();

            #if SILVERLIGHT
            #else
            this.Icon = (ImageSource) FindResource("question_mark");
            #endif
        }

#if SILVERLIGHT
        public static void Show(string windowTitle, string textTitle, string textSubtitle, FloatingWindowHost host)
        {
            EvoXMsgBox msgBox = new EvoXMsgBox();
            msgBox.Title = windowTitle;
            msgBox.messageText.Text = textTitle;
            msgBox.messageQuestion.Text = textSubtitle;
            host.Add(msgBox);
            msgBox.ShowModal();
            return;
        }
#else
        public static void Show(string windowTitle, string textTitle, string textSubtitle)
        {
            EvoXMsgBox msgBox = new EvoXMsgBox();
            msgBox.Title = windowTitle;
            msgBox.messageText.Text = textTitle;
            msgBox.messageQuestion.Text = textSubtitle;
            msgBox.ShowDialog();
            return;
        }
#endif


        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
