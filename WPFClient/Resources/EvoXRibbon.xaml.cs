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
using EvoX.View.Commands;
using EvoX.View.Commands.Project;
using EvoX.ViewToolkit;

namespace EvoX.WPFClient
{
    /// <summary>
    /// Interaction logic for EvoXRibbon.xaml
    /// </summary>
    public partial class EvoXRibbon : UserControl
    {
        public EvoXRibbon()
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
                b.Click += delegate { EvoXGuiCommands.OpenProjectCommand.Execute(b.Tag.ToString(), false, true); };
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
                b.Click += delegate
                               {
                                   guiOpenProjectCommand c = new guiOpenProjectCommand();
                                   c.InitialDirectory = b.Tag.ToString();
                                   c.Execute(b.Tag.ToString(), false, false);
                               };
                spBackstageRightPane.Children.Add(b);
            }
        }

    }
}
