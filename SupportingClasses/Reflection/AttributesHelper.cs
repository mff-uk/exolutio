using System;
using System.Collections.Generic;
using System.Reflection;

namespace Exolutio.SupportingClasses.Reflection
{
    public static class AttributesHelper
    {
        public static List<PropertyInfo> FindPropertiesWithAttribute<TAttributeType>(Type inspectedClass)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            PropertyInfo[] propertyInfos = inspectedClass.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetCustomAttributes(typeof(TAttributeType), true).Length > 0)
                {
                    result.Add(propertyInfo);
                }
            }

            return result;
        }
    }
}