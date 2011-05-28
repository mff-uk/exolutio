﻿using Exolutio.Controller.Commands;
using Exolutio.SupportingClasses;

namespace Exolutio.View.Commands
{
    public interface IReportDisplay
    {
        Log DisplayedLog { get; set; }
        CommandReportBase DisplayedReport { get; set; }
        void Update();
    }
}