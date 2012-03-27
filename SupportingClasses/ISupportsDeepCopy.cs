namespace Exolutio.SupportingClasses
{
    public interface ISupportsDeepCopy<out T>
    {
        T DeepCopy();
    }
}