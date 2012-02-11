using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public class GenericNonblockingDialog
        #if SILVERLIGHT
        : ChildWindow
        #else
        : Window
        #endif
    {
        public GenericNonblockingDialog()
        {
            
        }

        protected override void OnKeyUp(Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!e.Handled && e.Key == Key.Escape &&  Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                Close(false);
            }
        }

        /// <summary>
        /// Closes eXolutio dialog window. 
        /// </summary>
        public void CloseWindow()
        {
            #if SILVERLIGHT
            if (DialogResult == null)
            {
                DialogResult = false; 
            }
            #else
            Close();
            #endif
        }

        /// <summary>
        /// Closes eXolutio dialog window. 
        /// </summary>
        public void Close(bool? result)
        {
            this.DialogResult = result;
            CloseWindow();
        }
        

        #if SILVERLIGHT

        public new void Show()
        {
            base.Show();
        }

        public void ShowDialog()
        {
            base.Show();
        }

        public object ResizeMode { set; get; }

        public object ShowInTaskbar { set; get; }

        public object Topmost { set; get; }

        public object WindowStartupLocation { set; get; }

        public object SizeToContent { set; get; }

        public object Icon { set; get; }
        
        #else

        public new bool? DialogResult { get; set; }

        #endif

        public object FindResource(string resourceKey)
        {
            return null;
        }
    }

    public class DialogButton:
        Button
    {
#if SILVERLIGHT
        public bool IsDefault { set; get; }
        public bool IsCancel { set; get; }
#else
#endif

        protected override void OnClick()
        {
            base.OnClick();
            if (IsDefault)
            {
                
            }
            if (IsCancel)
            {

            }
        }
    }

    public class DialogLabel:
        Label
    {
        
    }
}