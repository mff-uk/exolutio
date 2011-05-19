namespace EvoX.View
{
    public class LayoutManager
    {
        public void DoLayout(PSMDiagramView psmDiagramView)
        {
            if (psmDiagramView.Diagram != null)
            {
                VerticalTree.LayoutDiagram(psmDiagramView);
                psmDiagramView.EvoXCanvas.InvalidateMeasure();
            }
        }
    }
}