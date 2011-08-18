using System.Diagnostics;

namespace Exolutio.View
{
    public class LayoutManager
    {
        private VerticalTree verticalTree;
        
        private VerticalTreeRightAngles verticalTreeRightAngles;

        public LayoutManager()
        {
            verticalTree = new VerticalTree();
            verticalTreeRightAngles = new VerticalTreeRightAngles();
        }

        bool layouting = false; 

        public void DoLayout(PSMDiagramView psmDiagramView)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (psmDiagramView.Diagram != null && !layouting)
            {
                layouting = true; 
                //verticalTree.LayoutDiagram(psmDiagramView);
                verticalTreeRightAngles.LayoutDiagram(psmDiagramView);
                psmDiagramView.ExolutioCanvas.InvalidateMeasure();
                layouting = false;
                stopwatch.Stop();
                Debug.WriteLine("Layouting {0} took {1}.", psmDiagramView.Diagram, stopwatch.Elapsed);
            }
        }
    }
}