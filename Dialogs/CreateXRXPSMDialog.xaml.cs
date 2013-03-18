using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfCheckListBox;

namespace Exolutio.Dialogs
{
    /// <summary>
    /// Interaction logic for DeleteDependentElements.xaml
    /// </summary>
    public partial class CreateXRXPSMDialog
    {
		public CreateXRXPSMDialog()
        {
            InitializeComponent();
            this.Icon = (ImageSource)FindResource("question_mark");
        }

        public string ShortMessage
        {
            get
            {
                return this.tbShort.Content.ToString();
            }
            set
            {
                tbShort.Content = value; 
            }
        }

        public string LongMessage
        {
            get
            {
                return this.tbLong.Text;
            }
            set
            {
                tbLong.Text = value;
            }
        }

        ArrayList a = new ArrayList();

        public delegate string ToStringActionDelegate(object o);

        public ToStringActionDelegate ToStringAction { get; set; }

        public bool UseRadioButtons { get; set; }

        public void SelectItem(object o)
        {
            foreach (CheckItem item in a)
            {
                if (item.KeyValue == o)
                    item.IsChecked = true;
            }
        }

        public void SetItems(IList objects)
        {
            bool first = true; 
            foreach (object o in objects)
            {
                string toString = ToStringAction != null ? ToStringAction(o) : o.ToString();
                CheckItem item = new CheckItem(o, toString);
                if (!UseRadioButtons)
                {
                    item.IsChecked = true;
                    
                }
                else
                {
                    item.PropertyChanged += ItemOnPropertyChanged;
                    if (first)
                    {
                        item.IsChecked = true;
                        first = false;
                    }
                }
                a.Add(item);
            }

            if (!UseRadioButtons)
            {
                clbObjects.CheckListArray = a;
                clbObjects.ItemsSource = a;
                rlbObjects.Visibility = Visibility.Collapsed;
            }
            else
            {
                
                rlbObjects.CheckListArray = a;
                rlbObjects.ItemsSource = a;
                //rlbObjects.CheckViewModel = new CheckViewModel<string>() {IsRadioMode = true, CheckItems = a.Cast<CheckItem>().ToList()};
                clbObjects.Visibility = Visibility.Collapsed;
                rlbObjects.SelectionChanged += new SelectionChangedEventHandler(clbObjects_SelectionChanged);
            }

            
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsChecked" && ((CheckItem)sender).IsChecked)
            {
                foreach (CheckItem item in a)
                {
                    if (item != sender)
                        item.IsChecked = false;
                }
            }

        }

        void clbObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UseRadioButtons)
            {
                foreach (CheckItem item in a)
                {
                    item.IsChecked = false;
                }

                foreach (CheckItem item in e.AddedItems)
                {
                    item.IsChecked = true;
                }
            }
        }

        public List<object> selectedObjects; 

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            selectedObjects = new List<object>();
            ItemCollection items = UseRadioButtons ? rlbObjects.Items : clbObjects.Items;
            foreach (var _item in items)
            {
                CheckItem item = (CheckItem)_item;
                if (item.IsChecked)
                {
                    selectedObjects.Add(item.KeyValue);
                }
            }
            DialogResult = true;
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
