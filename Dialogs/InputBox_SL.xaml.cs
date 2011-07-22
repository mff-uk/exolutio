using System.Windows;
using System.Windows.Input;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox 
    {
        // the InputBox
        private static InputBox newInputBox;
        // the string that will be returned to the calling form
        private static string returnString;

    	protected InputBox()
        {
            InitializeComponent();
        }

        public static bool? Show(string inputBoxText, string defaultText, out string resultString)
        {
        	newInputBox = new InputBox();
            newInputBox.Title = inputBoxText;
            newInputBox.textBox1.Text = defaultText;
            newInputBox.textBox1.SelectAll();
            newInputBox.textBox1.Focus();
#if SILVERLIGHT
            newInputBox.ShowDialog();
            bool? dialog = newInputBox.DialogResult;
#else
            bool? dialog = newInputBox.ShowDialog();
#endif

            resultString = returnString;
        	return dialog;
        }

        public static bool? Show(string inputBoxText, out string resultString)
        {
            newInputBox = new InputBox();
            newInputBox.Title = inputBoxText;
            newInputBox.textBox1.Focus();
            #if SILVERLIGHT
            newInputBox.ShowDialog();
		    bool? dialog = newInputBox.DialogResult;
            #else
            bool? dialog = newInputBox.ShowDialog();
            #endif

            resultString = returnString; 
			return dialog;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            returnString = textBox1.Text;
        	DialogResult = true;
            newInputBox.CloseWindow();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            returnString = string.Empty;
            newInputBox.CloseWindow();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                buttonOK_Click(this, e);
            }
        }
    }
}
