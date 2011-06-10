
using Exolutio.Controller;
using Exolutio.View;

namespace Exolutio.WPFClient.Commands
{
    public static class StaticWPFClientCommands
    {
        public static guiResetWindowLayout ResetWindowLayout { get; set; }
        public static guiCheckForUpdates CheckForUpdates { get; set; }
        public static guiCloseActiveTab CloseActiveTab { get; set; }
        public static guiLayoutCommand LayoutPIMLeftPSMRight { get; set; }
        public static guiLayoutCommand LayoutByVersions { get; set; }

        public static guiPauseLogging guiPauseLogging { get; set; }
        public static guiStartLogging guiStartLogging { get; set; }
        public static guiStopLogging guiStopLogging { get; set; }

        public static guiEndReplay guiEndReplay { get; set; }
        public static guiExecuteNextInLog guiExecuteNextInLog { get; set; }
        public static guiLoadLog guiLoadLog { get; set; }

        static StaticWPFClientCommands()
        {
            ResetWindowLayout = new guiResetWindowLayout();
            CheckForUpdates = new guiCheckForUpdates();
            CloseActiveTab = new guiCloseActiveTab();
            LayoutPIMLeftPSMRight = new guiLayoutCommand { LayoutType = guiLayoutCommand.ELayoutType.PIMLeftPSMRight};
            LayoutByVersions = new guiLayoutCommand { LayoutType = guiLayoutCommand.ELayoutType.ByVersions };

            CommandLogger commandLogger = new CommandLogger();
            Current.ExecutedCommand += commandLogger.OnCommandExecuted;
            guiPauseLogging = new guiPauseLogging { CommandLogger = commandLogger };
            guiStartLogging = new guiStartLogging { CommandLogger = commandLogger };
            guiStopLogging = new guiStopLogging { CommandLogger = commandLogger };

            guiEndReplay = new guiEndReplay { CommandLogger = commandLogger };
            guiExecuteNextInLog = new guiExecuteNextInLog { CommandLogger = commandLogger };
            guiLoadLog = new guiLoadLog { CommandLogger = commandLogger };
        }
    }
}