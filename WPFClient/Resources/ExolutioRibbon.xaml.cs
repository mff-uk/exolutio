using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Exolutio.Dialogs;
using Exolutio.View.Commands;
using Exolutio.View.Commands.Project;
using Exolutio.ViewToolkit;
using System.Windows.Threading;
using System.Threading;

namespace Exolutio.WPFClient
{
    /// <summary>
    /// Interaction logic for ExolutioRibbon.xaml
    /// </summary>
    public partial class ExolutioRibbon : UserControl
    {
        public ExolutioRibbon()
        {
            InitializeComponent();
        }

        public void FillRecent(IEnumerable<FileInfo> recentFiles, IEnumerable<DirectoryInfo> recentDirectories)
        {
            spBackstageLeftPane.Children.Clear();
            spBackstageRightPane.Children.Clear();
            foreach (FileInfo recentFile in recentFiles) 
            {
                //<Button Style="{DynamicResource BackStageStyle}" HorizontalAlignment="Left" Width="345" >
                //    <StackPanel Orientation="Horizontal" HorizontalAlignment="Left" Width="345">
                //        <Image Width="64" Height="64" HorizontalAlignment="Left" />
                //        <TextBlock Margin="20,6,4,6">Manufactured Housing Manager 2011<LineBreak/>
                //                <Run Foreground="#FFa16f89" Text="Get help using ATD Software Solutions."/>
                //        </TextBlock>
                //    </StackPanel>
                //</Button>
                Button b = new Button() { HorizontalAlignment = HorizontalAlignment.Left, Width = 345 };
                b.Style = (Style)FindResource("BackStageStyle");
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, Width = 345 };
                b.Content = sp;
                Image image = new Image() { Width = 32, Height = 32, HorizontalAlignment = HorizontalAlignment.Left };
                image.Source = (ImageSource)FindResource("GenericDocument");
                sp.Children.Add(image);
                TextBlock textBlock = new TextBlock() { Margin = new Thickness(20, 6, 4, 6) };
                textBlock.Inlines.Add(recentFile.Name);
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new Run(recentFile.FullName) { Foreground = ViewToolkitResources.RibbonBackstageDimText });
                b.Tag = recentFile.FullName;
                b.ToolTip = recentFile.FullName;
                sp.Children.Add(textBlock);
                b.Click += new RoutedEventHandler(recentFile_Click);
                spBackstageLeftPane.Children.Add(b);
            }

            foreach (DirectoryInfo recentDirectory in recentDirectories)
            {
                //<Button Style="{DynamicResource BackStageStyle}" HorizontalAlignment="Left" Width="345" >
                //    <StackPanel Orientation="Horizontal" HorizontalAlignment="Left" Width="345">
                //        <Image Width="64" Height="64" HorizontalAlignment="Left" />
                //        <TextBlock Margin="20,6,4,6">Manufactured Housing Manager 2011<LineBreak/>
                //                <Run Foreground="#FFa16f89" Text="Get help using ATD Software Solutions."/>
                //        </TextBlock>
                //    </StackPanel>
                //</Button>
                Button b = new Button() { HorizontalAlignment = HorizontalAlignment.Left, Width = 345 };
                b.Style = (Style)FindResource("BackStageStyle");
                StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left, Width = 345 };
                b.Content = sp;
                Image image = new Image() { Width = 32, Height = 32, HorizontalAlignment = HorizontalAlignment.Left };
                image.Source = (ImageSource)FindResource("folder");
                sp.Children.Add(image);
                TextBlock textBlock = new TextBlock() { Margin = new Thickness(20, 6, 4, 6) };
                textBlock.Inlines.Add(recentDirectory.Name);
                textBlock.Inlines.Add(new LineBreak());
                textBlock.Inlines.Add(new Run(recentDirectory.FullName) { Foreground = ViewToolkitResources.RibbonBackstageDimText });
                b.Tag = recentDirectory.FullName;
                b.ToolTip = recentDirectory.FullName;
                sp.Children.Add(textBlock);
                b.Click += new RoutedEventHandler(recentDirectory_Click); 
                spBackstageRightPane.Children.Add(b);
            }
        }

        void recentDirectory_Click(object sender, RoutedEventArgs e)
        {
            string directory = ((Control)sender).Tag.ToString();
            if (Directory.Exists(directory))
            {
                guiOpenProjectCommand c = new guiOpenProjectCommand();
                c.InitialDirectory = ((Control)sender).Tag.ToString();
                c.Execute(((Control)sender).Tag.ToString(), false, false);
            }
            else
            {
                if (ExolutioYesNoBox.Show("Directory no longer exists.", "Do you wish to remove the directory from recent directories list?") == MessageBoxResult.Yes)
                {
                    ConfigurationManager.Configuration.RecentDirectories.RemoveAll(d => d.FullName == directory);
                    FillRecent(ConfigurationManager.Configuration.RecentFiles,
                               ConfigurationManager.Configuration.RecentDirectories);
                }
            }
        }

        void recentFile_Click(object sender, RoutedEventArgs e)
        {
            string file = ((Control)sender).Tag.ToString();
            if (File.Exists(file))
            {
                GuiCommands.OpenProjectCommand.Execute(file, false, true);
            }
            else
            {
                if (ExolutioYesNoBox.Show("File no longer exists.", "Do you wish to remove the file from recent files list?") == MessageBoxResult.Yes)
                {
                    ConfigurationManager.Configuration.RecentFiles.RemoveAll(f => f.FullName == file);
                    FillRecent(ConfigurationManager.Configuration.RecentFiles,
                               ConfigurationManager.Configuration.RecentDirectories);
                }
            }
        }

        private void OnSilverClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (ThreadStart)(() =>
            {
                Application.Current.Resources.BeginInit();
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Fluent;component/Themes/Office2010/Silver.xaml") });
                Application.Current.Resources.MergedDictionaries.RemoveAt(1);
                try
                {
                    Application.Current.Resources.EndInit();
                }
                catch
                {

                }
                AvalonDock.ThemeFactory.ChangeTheme("aero.normalcolor");
                ((this.Parent as DockPanel).Parent as MainWindow).dockManager.MainDocumentPane.Background = Brushes.Transparent;
            }));
        }

        private void OnBlackClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (ThreadStart)(() =>
            {
                Application.Current.Resources.BeginInit();
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Fluent;component/Themes/Office2010/Black.xaml") });
                Application.Current.Resources.MergedDictionaries.RemoveAt(1);
                try
                {
                    Application.Current.Resources.EndInit();
                }
                catch
                {

                }
                AvalonDock.ThemeFactory.ChangeTheme("Classic");
                ((this.Parent as DockPanel).Parent as MainWindow).dockManager.MainDocumentPane.Background = Brushes.Transparent;
            }));
        }

        private void OnBlueClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (ThreadStart)(() =>
            {
                Application.Current.Resources.BeginInit();
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Fluent;component/Themes/Office2010/Blue.xaml") });
                Application.Current.Resources.MergedDictionaries.RemoveAt(1);
                try
                {
                    Application.Current.Resources.EndInit();
                }
                catch
                {

                }
                AvalonDock.ThemeFactory.ChangeTheme("aero.normalcolor");
                ((this.Parent as DockPanel).Parent as MainWindow).dockManager.MainDocumentPane.Background = Brushes.Transparent;
            }));
        }

    
       

    }
}
