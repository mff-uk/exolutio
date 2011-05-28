using System;
using System.Reflection;
using Exolutio.SupportingClasses.Reflection;

namespace Exolutio.Controller.Commands.Reflection
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public static string GetDescription(PublicCommandAttribute.EPulicCommandCategory commandCategory)
        {
            //Type dataType = Enum.GetUnderlyingType(typeof(PublicCommandAttribute.EPulicCommandCategory));
            foreach (FieldInfo field in typeof(PublicCommandAttribute.EPulicCommandCategory).
                GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
            {
                object value = field.GetValue(null);
                //Console.WriteLine("{0}={1}", field.Name, Convert.ChangeType(value, dataType));
                if (value is PublicCommandAttribute.EPulicCommandCategory &&
                    (PublicCommandAttribute.EPulicCommandCategory)value == commandCategory)
                {
                    DescriptionAttribute desca;

                    if (field.TryGetAttribute(out desca))
                    {
                        return desca.Description;
                    }
                }
            }

            return commandCategory.ToString();
        }
    }
}