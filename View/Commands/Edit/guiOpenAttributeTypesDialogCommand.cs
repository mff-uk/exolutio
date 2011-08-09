namespace Exolutio.View.Commands.Edit
{
    public class guiOpenAttributeTypesDialogCommand : guiCommandBase
    {
        public guiOpenAttributeTypesDialogCommand()
        {
            
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            AttributeTypeDialog d = new AttributeTypeDialog();
            d.Initialize(Current.Controller, Current.ProjectVersion, null);
            d.ShowDialog();
        }

        public override string Text
        {
            get { return "Manage types"; }
        }

        public override string ScreenTipText
        {
            get { return "Open data type manager window."; }
        }
    }
}