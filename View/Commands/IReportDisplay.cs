using EvoX.Controller.Commands;
using EvoX.SupportingClasses;

namespace EvoX.View.Commands
{
    public interface IReportDisplay
    {
        Log DisplayedLog { get; set; }
        CommandReportBase DisplayedReport { get; set; }
        void Update();
    }
}