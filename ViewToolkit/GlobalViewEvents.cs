using System;

namespace Exolutio.ViewToolkit
{
    public static class GlobalViewEvents
    {
        public static event Action CanvasContentChanged;

        static internal void InvokeCanvasContentChanged()
        {
            Action handler = CanvasContentChanged;
            if (handler != null) handler();
        }

        public static event Action DiagramDisplayChanged;

        public static void InvokeDiagramDisplayChanged()
        {
            Action handler = DiagramDisplayChanged;
            if (handler != null) handler();
        }
    }
}