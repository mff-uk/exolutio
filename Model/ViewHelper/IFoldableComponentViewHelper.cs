namespace Exolutio.Model.ViewHelper
{
    public interface IFoldableComponentViewHelper
    {
        bool CanFold();

        bool IsFolded { get; set; }
    }

    public enum EFoldingAction
    {
        Fold,
        Unfold
    }
}