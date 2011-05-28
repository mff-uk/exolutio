using System;

namespace Exolutio.Controller.Commands
{
    public interface ICommandWithDiagramParameter
    {
        Guid SchemaGuid { get; set; }

        Guid DiagramGuid { get; set; }
    }
}