using AvalonDock;

namespace Exolutio.WPFClient
{
    public partial class DiagramTabManager
    {
        public DocumentPane ActivePane
        {
            get
            {
                if (DockManager.ActiveDocument != null && DockManager.ActiveDocument.Parent != null
                    && DockManager.ActiveDocument.Parent is DocumentPane)
                {
                    return (DocumentPane)DockManager.ActiveDocument.Parent;
                }
                else
                {
                    return DockManager.MainDocumentPane;
                }
            }
        }
    }
	
	public static class AvalonDockExtension
    {
        public static bool IsActiveInItsGroup(this ManagedContent managedContent)
        {
            return managedContent.ContainerPane.SelectedItem == managedContent;
        }
    }
}