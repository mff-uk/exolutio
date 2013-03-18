using System.Windows.Input;
using Exolutio.Controller.Commands;

namespace Exolutio.View.Commands.View
{
	[Scope(ScopeAttribute.EScope.PSMSchema | ScopeAttribute.EScope.PIMDiagram)]
	public class guiToggleSnapshotViewCommand : guiScopeCommand
	{
		public override string ScreenTipText
		{
			get { return "Toggle snapshot view on the diagram. "; }
		}

		public override bool CanExecute(object parameter)
		{
			return Current.ActiveDiagramView != null;
		}

		public override string Text
		{
			get
			{
				return "Toggle snapshot view on the diagram.";
			}
		}

		public override void Execute(object parameter)
		{
			if (!Current.ActiveDiagramView.ExolutioCanvas.InScreenshotView)
				Current.ActiveDiagramView.ExolutioCanvas.EnterScreenshotView();
			else
				Current.ActiveDiagramView.ExolutioCanvas.ExitScreenshotView();
		}
	}
}