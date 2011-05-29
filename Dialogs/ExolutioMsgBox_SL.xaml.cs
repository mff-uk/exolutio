using System.Windows;
using System.Windows.Media;
using SilverFlow.Controls;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for ExolutioMsgBox.xaml
    /// </summary>
    public partial class ExolutioMsgBox
    {
        public ExolutioMsgBox()
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
            ExolutioMsgBox msgBox = new ExolutioMsgBox();
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
