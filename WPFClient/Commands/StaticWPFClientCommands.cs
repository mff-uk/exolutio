
namespace Exolutio.WPFClient.Commands
{
    public static class StaticWPFClientCommands
    {
        public static guiResetWindowLayout ResetWindowLayout { get; set; }
        public static guiCheckForUpdates CheckForUpdates { get; set; }
        public static guiCloseActiveTab CloseActiveTab { get; set; }
        public static guiLayoutCommand LayoutPIMLeftPSMRight { get; set; }

        static StaticWPFClientCommands()
        {
            ResetWindowLayout = new guiResetWindowLayout();
            CheckForUpdates = new guiCheckForUpdates();
            CloseActiveTab = new guiCloseActiveTab();
            LayoutPIMLeftPSMRight = new guiLayoutCommand { LayoutType = guiLayoutCommand.ELayoutType.PIMLeftPSMRight};

        }
    }
}