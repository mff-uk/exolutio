using System.Reflection;

namespace Exolutio.Controller.Commands.Reflection
{
    public abstract class ParameterConsistency
    {
        public abstract bool VerifyConsistency(object superordinateObject, object candidate);
    }
}