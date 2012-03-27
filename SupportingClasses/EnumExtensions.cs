using System;
using System.Collections.Generic;
using System.Reflection;

namespace Exolutio.SupportingClasses
{
    public static class EnumExtensions
    {
        static private Dictionary<Enum, string> cache = new Dictionary<Enum, string>();

        public static string GetDescription(this Enum @enum)
        {
            Type type = @enum.GetType();
            if (cache.ContainsKey(@enum))
            {
                return cache[@enum];
            }
            string result = null; 
            MemberInfo[] memInfo = type.GetMember(@enum.ToString());
            if (memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(EnumDescription), false);
                if (attrs.Length > 0)
                    result = ((EnumDescription)attrs[0]).Text;
            }
            if (result == null)
                result = @enum.ToString();
            cache[@enum] = result; 
            return result;
        }
    }

    public class EnumDescription : Attribute
    {
        public string Text;

        public EnumDescription(string text)
        {
            Text = text;
        }
    }
}