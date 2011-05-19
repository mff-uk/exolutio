using System;
using System.Linq;
using System.Collections.Generic;
using EvoX.Model;
using EvoX.SupportingClasses;

namespace EvoX.DataGenerator
{
    public class DataTypeValuesGenerator
    {
        public DataTypeValuesGenerator(bool isRandom)
        {
            IsRandom = isRandom;
        }

        public bool IsRandom { get; set; }

        public string GenerateValue(AttributeType type)
        {
            if (type == null)
            {
                return GenerateString();
            }

            if (type == null)
                throw new ArgumentException("Values can be generated only for simple data types.", "type");

            if (type.Name.ToUpper() == "int".ToUpper() || type.Name.ToUpper() == "integer".ToUpper())
                return GenerateInteger();

            if (type.Name.ToUpper() == "string".ToUpper())
                return GenerateString();

            if (type.Name.ToUpper() == "double".ToUpper())
                return GenerateDouble();

            if (type.Name.ToUpper() == "date".ToUpper())
                return GenerateDate();

            if (type.Name.ToUpper() == "datetime".ToUpper())
                return GenerateDateTime();

            if (type.Name.ToUpper() == "time".ToUpper())
                return GenerateTime();

            if (type.Name.ToUpper() == "boolean".ToUpper())
                return GenerateBoolean();

            if (type.Name.ToUpper() == "decimal".ToUpper())
                return GenerateDecimal();

            if (type.BaseType != null)
            {
                throw new NotImplementedException();
            }

            // otherwise can't help
            throw new NotImplementedException();
        }

        private string GenerateDecimal()
        {
            if (IsRandom)
            {
                return String.Format("{0}.{1}", RandomGenerator.Next(-10, 10), RandomGenerator.Next(99));
            }
            else
            {
                return "42.24";
            }

        }

        private string GenerateBoolean()
        {
            return IsRandom ? (RandomGenerator.Toss() ? "true" : "false") : "true";
        }

        private string GenerateTime()
        {
            DateTime d;
            if (IsRandom)
            {
                d = RandomGenerator.RandomDateTime();
            }
            else
            {
                d = new DateTime(1995, 5, 6, 12, 22, 33);
            }
            return String.Format("{0:00}:{1:00}:{2:00}", d.Hour, d.Minute, d.Second);
                
        }
        //2002-10-10T12:00:00-05:00
        private string GenerateDateTime()
        {
            DateTime d;
            if (IsRandom)
            {
                d = RandomGenerator.RandomDateTime();
            }
            else
            {
                d = new DateTime(1995, 5, 6, 12, 22, 33);
            }
            return String.Format("{0}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}", d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
        }

        private string GenerateDate()
        {
            DateTime d;
            if (IsRandom)
            {
                d = RandomGenerator.RandomDateTime();
            }
            else
            {
                d = new DateTime(1995, 5, 6, 12, 22, 33);
            }
            return String.Format("{0}-{1:00}-{2:00}", d.Year, d.Month, d.Day);
        }

        private string GenerateDouble()
        {
            if (IsRandom)
            {
                return String.Format("{0}.{1}", RandomGenerator.Next(-10, 10), RandomGenerator.Next(99));
            }
            else
            {
                return "42.24";
            }
        }

        // ReSharper disable StringLiteralsWordIsNotInDictionary
        private static readonly List<string> words = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin vitae odio dui. Pellentesque rutrum, nisl non viverra venenatis, ipsum tellus bibendum neque, luctus euismod augue turpis sit amet tortor. Nunc at quam et augue vulputate luctus. Suspendisse potenti. Nam malesuada placerat mauris vitae luctus. Donec euismod, dolor at pharetra ultrices, leo lacus congue erat, in blandit libero felis ut est. Curabitur lobortis bibendum facilisis. Vestibulum aliquam augue id ipsum dignissim faucibus. Aliquam porta bibendum lectus nec tincidunt. Nam quis magna ante. Curabitur interdum interdum mi eget fermentum. In vulputate sapien eget nulla mattis ac vulputate erat mattis. Nam eu erat libero, ut imperdiet justo. Morbi ullamcorper elementum arcu id porta. Quisque vestibulum consequat eleifend. Aenean tempor volutpat velit, hendrerit blandit urna aliquet ac. Vivamus dictum ullamcorper leo vitae dapibus.".Split(new[]{' ', '.', ','}, StringSplitOptions.RemoveEmptyEntries).ToList();
        // ReSharper restore StringLiteralsWordIsNotInDictionary

        private string GenerateString()
        {
            string result = string.Empty;

            if (IsRandom)
            {
                int count = RandomGenerator.Next(1, 2);

                for (int i = 0; i < count; i++)
                {
                    result += (i > 0 ? " " : string.Empty) + words.ChooseOneRandomly();
                }
            }
            else
            {
                result = "NewWord";
            }
            return result; 
        }

        private string GenerateInteger()
        {
            if (IsRandom)
            {
                return RandomGenerator.Next(-10, 10).ToString();
            }
            else return "42";
        }
    }
}