using System.Windows;
using System.Windows.Media;
using SilverFlow.Controls;

namespace Exolutio.Dialogs
{
    public partial class ExolutioYesNoBox
    {
        private static ExolutioYesNoBox msgBox;

        private MessageBoxResult result = MessageBoxResult.Cancel;

        public ExolutioYesNoBox()
        {
            InitializeComponent();

            #if SILVERLIGHT
            #else
            Icon = (ImageSource) FindResource("question_mark");
            #endif
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

#if SILVERLIGHT
        public ExolutioYesNoBox(bool showOk, bool showCancel, bool showYes, bool showNo, FloatingWindowHost host)
#else
        public EvoXYesNoBox(bool showOk, bool showCancel, bool showYes, bool showNo)
#endif
        {
            InitializeComponent();
            #if SILVERLIGHT
            #else
            Icon = (ImageSource)FindResource("question_mark");
            #endif
            SetVisibility(bOk, showOk);
            SetVisibility(bStorno, showCancel);
            SetVisibility(bYes, showYes);
            SetVisibility(bNo, showNo);
            
        }

#if SILVERLIGHT
        public static MessageBoxResult Show(string messageText, string messageQuestion, FloatingWindowHost host)
#else
        public static MessageBoxResult Show(string messageText, string messageQuestion)
#endif
        {
            msgBox = new ExolutioYesNoBox(false, true, true, true, host);           
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
#if SILVERLIGHT
            host.Add(msgBox);
            msgBox.ShowModal();
#else
            msgBox.ShowDialog();
#endif
            return msgBox.result;
        }

#if SILVERLIGHT
        public static MessageBoxResult ShowYesNoCancel(string messageText, string messageQuestion, FloatingWindowHost host)
#else
        public static MessageBoxResult ShowYesNoCancel(string messageText, string messageQuestion)
#endif
        {
            msgBox = new ExolutioYesNoBox(false, true, true, true, host);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
#if SILVERLIGHT
            host.Add(msgBox);
            msgBox.ShowModal();
#else
            msgBox.ShowDialog();
#endif
            return msgBox.result;
        }

#if SILVERLIGHT
        public static MessageBoxResult ShowOKCancel(string messageText, string messageQuestion, FloatingWindowHost host)
#else
        public static MessageBoxResult ShowOKCancel(string messageText, string messageQuestion)
#endif
        {
            msgBox = new ExolutioYesNoBox(true, true, false, false, host);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
#if SILVERLIGHT
            host.Add(msgBox);
            msgBox.ShowModal();
#else
            msgBox.ShowDialog();
#endif
            return msgBox.result;
        }

#if SILVERLIGHT
        public static MessageBoxResult ShowOK(string messageText, string messageQuestion, FloatingWindowHost host)
#else
        public static MessageBoxResult ShowOK(string messageText, string messageQuestion)
#endif
        {
            msgBox = new ExolutioYesNoBox(true, false, false, false, host);
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
#if SILVERLIGHT
            host.Add(msgBox);
            msgBox.ShowModal();
#else
            msgBox.ShowDialog();
#endif
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
