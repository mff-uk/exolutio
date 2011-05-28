using System.Windows;
using System.Windows.Input;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class ExolutioInputBox : Window
    {
        // the InputBox
        private static ExolutioInputBox newExolutioInputBox;
        // the string that will be returned to the calling form
        private static string returnString;

    	protected ExolutioInputBox()
        {
            InitializeComponent();
        }

        public static bool ? Show(string inputBoxText, string defaultText, out string resultString)
        {
        	newExolutioInputBox = new ExolutioInputBox();
            newExolutioInputBox.Title = inputBoxText;
            newExolutioInputBox.textBox1.Text = defaultText;
            newExolutioInputBox.textBox1.SelectAll();
            newExolutioInputBox.textBox1.Focus();
        	bool? dialog = newExolutioInputBox.ShowDialog();
			resultString = returnString;
        	return dialog;
        }

		public static bool? Show(string inputBoxText, out string resultString)
        {
            newExolutioInputBox = new ExolutioInputBox();
            newExolutioInputBox.Title = inputBoxText;
            newExolutioInputBox.textBox1.Focus();
			bool? dialog = newExolutioInputBox.ShowDialog();
			resultString = returnString; 
			return dialog;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            returnString = textBox1.Text;
        	DialogResult = true; 
            newExolutioInputBox.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            returnString = string.Empty;
            newExolutioInputBox.Close();
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
