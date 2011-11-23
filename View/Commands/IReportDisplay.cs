using Exolutio.Controller.Commands;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands
{
    public interface IReportDisplay
    {
        ILog DisplayedLog { get; set; }
        CommandReportBase DisplayedReport { get; set; }
        void Update();
    }
}