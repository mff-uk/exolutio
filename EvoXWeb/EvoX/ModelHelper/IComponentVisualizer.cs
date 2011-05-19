namespace EvoX.Web.ModelHelper
{
    public interface IComponentVisualizer<TComponent>
    {
        void Display(TComponent component);
    }
}