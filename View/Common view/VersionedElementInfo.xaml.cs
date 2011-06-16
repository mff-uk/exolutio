using System;
using System.Collections.Generic;
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
using Exolutio.Model.Versioning;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for VersionedElementInfo.xaml
    /// </summary>
    public partial class VersionedElementInfo : Window
    {
        private IVersionedItem component;

        public VersionedElementInfo()
        {
            InitializeComponent();
        }

        public IVersionedItem Component
        {
            get {
                return component;
            }
            set {
                component = value;
                border.Background = Brushes.LightYellow;
                if (Component != null)
                {
                    Version firstAppearedIn = Component.Version;
                    while (firstAppearedIn != null
                        && firstAppearedIn.BranchedFrom != null
                        && Component.ExistsInVersion(firstAppearedIn.BranchedFrom))
                    {
                        firstAppearedIn = firstAppearedIn.BranchedFrom;
                    }

                    if (Component.Version != null && Component.Version == firstAppearedIn)
                    {
                        border.BorderBrush = Brushes.Green;
                    }
                    else if (Component.Version == null || Component.ProjectVersion.Project.VersionManager == null)
                    {
                        border.Background = Brushes.Pink;
                        border.BorderBrush = Brushes.Red;
                    }
                    else
                        border.BorderBrush = Brushes.Blue;

                    if (Component.ProjectVersion.Project.VersionManager != null)
                    {
                        if (!Component.ProjectVersion.Project.VersionManager.Versions.All(v => Component.ExistsInVersion(v)))
                            border.Background = Brushes.LavenderBlush;
                        else
                            border.Background = Brushes.LightYellow;
                    }


                    lName.Content = Component.ToString();
                    lVersionManager.Content = (Component.ProjectVersion.Project.VersionManager != null) ? "defined" : "not defined";
                    lCurrentVersion.Content = (Component.Version != null) ? Component.Version.ToString() : "(null)";

                    
                    lFirstAppearedIn.Content = firstAppearedIn != null ? firstAppearedIn.ToString() : "(null)";
                    lAllVersions.Content = (Component.ProjectVersion.Project.VersionManager != null) ? Component.ProjectVersion.Project.VersionManager.Versions.Aggregate(string.Empty,
                                                                                 (s, version) => s += version + ", ",
                                                                                 result => result.Substring(0, result.Length - 2)) : "(null)";

                    lPresent.Content = (Component.ProjectVersion.Project.VersionManager != null) ?
                        Component.ProjectVersion.Project.VersionManager.Versions.Aggregate(
                            string.Empty,
                            (s, version) => component.ExistsInVersion(version) ? s += version + ", " : s += " - ,",
                            result => result.Substring(0, result.Length - 2)) 
                        : "(null)";
                }
                else
                {
                    lName.Content = "(null)";
                    lCurrentVersion.Content = "(null)";
                    lFirstAppearedIn.Content = "(null)";
                    lAllVersions.Content = "(null)";
                }
            }
        }
    }
}