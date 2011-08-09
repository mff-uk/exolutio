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

        public void DoLayout(PSMDiagramView psmDiagramView)
        {
            if (psmDiagramView.Diagram != null)
            {
                //verticalTree.LayoutDiagram(psmDiagramView);
                verticalTreeRightAngles.LayoutDiagram(psmDiagramView);
                psmDiagramView.ExolutioCanvas.InvalidateMeasure();
            }
        }
    }
}