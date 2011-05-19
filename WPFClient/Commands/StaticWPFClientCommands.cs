
namespace EvoX.WPFClient.Commands
{
    public static class StaticWPFClientCommands
    {
        public static guiResetWindowLayout ResetWindowLayout { get; set; }
        public static guiCheckForUpdates CheckForUpdates { get; set; }
        public static guiCloseActiveTab CloseActiveTab { get; set; }

        static StaticWPFClientCommands()
        {
            ResetWindowLayout = new guiResetWindowLayout();
            CheckForUpdates = new guiCheckForUpdates();
            CloseActiveTab = new guiCloseActiveTab();
        }
    }
}