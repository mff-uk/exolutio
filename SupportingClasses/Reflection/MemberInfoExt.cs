using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace EvoX.SupportingClasses.Reflection
{
    public static class MemberInfoExt
    {
        public static bool TryGetAttribute<TAttribute>(this MemberInfo memberInfo, out TAttribute attribute)
            where TAttribute: Attribute
        {
            object[] customAttributes = memberInfo.GetCustomAttributes(typeof(TAttribute), true);
            if (customAttributes.Length == 0)
            {
                attribute = null;
                return false;
            }

            if (customAttributes.Length > 1)
            {
                throw new InvalidOperationException("Multiple instances of attribute found.");
            }

            attribute = (TAttribute) customAttributes[0];
            return true;
        }

        public static bool TryGetAttributes<TAttribute>(this MemberInfo memberInfo, out IEnumerable<TAttribute> attributes)
            where TAttribute : Attribute
        {
            object[] customAttributes = memberInfo.GetCustomAttributes(typeof(TAttribute), true);
            if (customAttributes.Length == 0)
            {
                attributes = null;
                return false;
            }


            attributes = customAttributes.Cast<TAttribute>();
            return true;
        }
    }
}