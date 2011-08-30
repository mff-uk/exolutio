using Exolutio.Model;

namespace Exolutio.View
{
    public interface IComponentTextBox
    {
        Component Component { get; }
        bool Selected { get; set; }
    }
}