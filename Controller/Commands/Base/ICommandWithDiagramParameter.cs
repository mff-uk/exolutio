using System;

namespace EvoX.Controller.Commands
{
    public interface ICommandWithDiagramParameter
    {
        Guid SchemaGuid { get; set; }

        Guid DiagramGuid { get; set; }
    }
}