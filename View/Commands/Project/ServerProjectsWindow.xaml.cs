using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EvoX.View.Commands.Project
{
    public partial class ServerProjectsWindow
    {
        public ServerProjectsWindow()
        {
            InitializeComponent();
        }

        public string SelectedProject
        {
            get
            {
                return lbProjects.SelectedItem != null
                           ? ((ListBoxItem)lbProjects.SelectedItem).Content.ToString()
                           : null;
            }
        }


        private IEnumerable<string> projects;
        public IEnumerable<string> Projects
        {
            get { return projects; }
            set
            {
                projects = value;
                lbProjects.Items.Clear();
                if (value != null)
                {
                    foreach (string project in Projects)
                    {
                        lbProjects.Items.Add(new ListBoxItem() { Content = project });
                    }
                }
                else
                {
                    lbProjects.Items.Add(new ListBoxItem() { Content = "List of projects is being loaded. Please, try again. " });
                }
            }
        }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public DateTime _lastClick = DateTime.Now;
        private bool _firstClickDone = false;
        private Point _clickPosition;

        private void lbProjects_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIElement element = sender as UIElement;
            DateTime clickTime = DateTime.Now;

            TimeSpan span = clickTime - _lastClick;

            if (span.TotalMilliseconds > 300 || _firstClickDone == false)
            {
                _clickPosition = e.GetPosition(element);
                _firstClickDone = true;
                _lastClick = DateTime.Now;
            }
            else
            {
                Point position = e.GetPosition(element);
                if (Math.Abs(_clickPosition.X - position.X) < 4 &&
                    Math.Abs(_clickPosition.Y - position.Y) < 4)
                {
                    DialogResult = true;
                    Close();
                }


                _firstClickDone = false;
            }
        }
    }
}
