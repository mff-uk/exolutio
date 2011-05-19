using System.Windows;
using System.Windows.Media;

namespace EvoX.Dialogs
{
    public partial class EvoXYesNoBox : Window
    {
        private static EvoXYesNoBox msgBox;

        private MessageBoxResult result = MessageBoxResult.Cancel;

        public EvoXYesNoBox()
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

        public EvoXYesNoBox(bool showOk, bool showCancel, bool showYes, bool showNo)
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
            msgBox = new EvoXYesNoBox(false, true, true, true);           
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        public static MessageBoxResult ShowYesNoCancel(string messageText, string messageQuestion)
        {
            msgBox = new EvoXYesNoBox(false, true, true, true);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        public static MessageBoxResult ShowOKCancel(string messageText, string messageQuestion)
        {
            msgBox = new EvoXYesNoBox(true, true, false, false);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return msgBox.result;
        }

        public static MessageBoxResult ShowOK(string messageText, string messageQuestion)
        {
            msgBox = new EvoXYesNoBox(true, false, false, false);
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
