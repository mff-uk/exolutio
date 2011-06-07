using Exolutio.ViewToolkit;

namespace Exolutio.View.Commands.View
{
    public class guiStraightenLineCommand : guiCommandBase
    {
        public ConnectorPoint ConnectorPoint { get; set; }
        
        public override bool CanExecute(object parameter = null)
        {
            return true;
        }

        public override void Execute(object parameter = null)
        {
            ConnectorPoint.Connector.StraightenLineAtPoint(ConnectorPoint);
        }

        public override string Text
        {
            get { return "Straighten line here"; }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return Text; }
        }
    }
}