using Exolutio.Model;
using Exolutio.Model.ViewHelper;
using Exolutio.ViewToolkit;

namespace Exolutio.View
{
    public interface INodeComponentViewBase
    {
        double X { get; set; }
        double Y { get; set; }
        double ActualWidth { get; }
        double ActualHeight { get; }
        Node MainNode { get; }
        Component ModelComponent { get; }
        DiagramView DiagramView { get; }
        bool IsBindingSuspended { get; }
        bool Selected { get; set; }
        ViewHelper ViewHelper { get;  }

        /// <summary>
        /// True if the view is highlighted. 
        /// </summary>
        bool Highlighted { get; set; }

        bool CanPutInDiagram(DiagramView diagramView);
        
        bool CanRemoveFromDiagram();
        void RemoveFromDiagram();
        void SuspendModelBinding();
        void ResumeModelBinding();

        /// <summary>
        /// This method is safe to be called repeatedly. 
        /// </summary>
        void UpdateView();

        void PutInDiagramDeferred(DiagramView diagramView, Component component, ViewHelper viewHelper);
        void RemoveFromDiagramDeferred();

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This instance of <see cref="ComponentViewBase"/> is not yet in <see cref="ComponentViewBase.DiagramView"/>.<see cref="View.DiagramView.RepresentantsCollection"/>,
        /// in which it is added in the method's body. 
        /// </remarks>
        /// <param name="diagramView"></param>
        /// <param name="viewHelper"></param>
        void PutInDiagram(DiagramView diagramView, ViewHelper viewHelper);
    }

    public interface IConnectorViewBase
    {
        Connector Connector { get; }
    }
}