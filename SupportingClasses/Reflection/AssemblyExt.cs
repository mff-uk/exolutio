using System;
using System.Collections.Generic;
using System.Reflection;

namespace Exolutio.SupportingClasses.Reflection
{
    public static class AssemblyExt
    {
        public static List<Type> GetTypesWithAttribute<TAttributeType>(this Assembly assembly) 
            where TAttributeType : Attribute
        {
            List<Type> result = new List<Type>();

            foreach (Type type in assembly.GetTypes())
            {
                TAttributeType dummy;
                if (type.TryGetAttribute(out dummy))
                {
                    result.Add(type);
                }
            }

            return result;
        }
    }
}