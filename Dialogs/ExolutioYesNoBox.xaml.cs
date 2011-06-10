using System.Windows;
using System.Windows.Media;

namespace Exolutio.Dialogs
{
    public partial class ExolutioYesNoBox : Window
    {
        private static ExolutioYesNoBox msgBox;

        private MessageBoxResult result = MessageBoxResult.Cancel;

        public ExolutioYesNoBox()
        {
            InitializeComponent();

            Icon = (ImageSource) FindResource("question_mark");
        }


        private void SetVisibility(UIElement control, bool visible)
        {
            if (visible)
            {
                control.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                control.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public ExolutioYesNoBox(bool showOk, bool showCancel, bool showYes, bool showNo)
        {
            InitializeComponent();

            Icon = (ImageSource)FindResource("question_mark");

            SetVisibility(bOk, showOk);
            SetVisibility(bStorno, showCancel);
            SetVisibility(bYes, showYes);
            SetVisibility(bNo, showNo);
            
        }

        public static MessageBoxResult Show(string messageText, string messageQuestion)
        {
            msgBox = new ExolutioYesNoBox(false, true, true, true);           
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        public static MessageBoxResult ShowYesNoCancel(string messageText, string messageQuestion)
        {
            msgBox = new ExolutioYesNoBox(false, true, true, true);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        public static MessageBoxResult ShowYesNo(string messageText, string messageQuestion)
        {
            msgBox = new ExolutioYesNoBox(false, false, true, true);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        public static MessageBoxResult ShowOKCancel(string messageText, string messageQuestion)
        {
            msgBox = new ExolutioYesNoBox(true, true, false, false);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        public static MessageBoxResult ShowOK(string messageText, string messageQuestion)
        {
            msgBox = new ExolutioYesNoBox(true, false, false, false);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        private void buttonNo_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            msgBox.Close();
        }

        private void buttonYes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            msgBox.Close();
        }

        private void buttonStorno_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            msgBox.Close();
        }


        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            msgBox.Close();
        }
    }
}
