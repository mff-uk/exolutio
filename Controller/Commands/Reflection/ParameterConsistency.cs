using System.Reflection;

namespace EvoX.Controller.Commands.Reflection
{
    public abstract class ParameterConsistency
    {
        public abstract bool VerifyConsistency(object superordinateObject, object candidate);
    }
}