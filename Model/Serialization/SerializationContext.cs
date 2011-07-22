using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.Serialization
{
    public class SerializationContext
    {
        public static string ExolutioPrefix
        {
            get { return "eXo"; }
        }

        public SerializationContext()
        {
            Log = new Log();
            OutputNamesWithIdReferences = true;
        }
        
        public XDocument Document { get; set; }

        public XNamespace ExolutioNS = @"http://eXolutio.eu/Project/Serialization/";

        public Log Log { get; set; }

        /// <summary>
        /// Each time id reference is serialized, <see cref="object.ToString"/> is called
        /// on the referenced object an the output is used as a value of the displayName attribute
        /// </summary>
        public bool OutputNamesWithIdReferences { get; set; }

        public Guid CurrentSchemaGuid { get; set; }

        public ProjectVersion CurrentProjectVersion { get; set; }

        #region static encoding functions

        public static string EncodeValue(int i)
        {
            return i.ToString();
        }

        public static string EncodeValue(uint i)
        {
            return i.ToString();
        }

        public static string EncodeValue(string s, bool nullAsEmpty = false)
        {
            return (s != null || !nullAsEmpty) ? s : string.Empty;
        }

        public static string EncodeValue(bool b)
        {
            return b ? "True" : "False";
        }

        public static string EncodeValue(Guid guid)
        {
            return guid.ToString();
        }

        public static string EncodeValue(UnlimitedInt upper)
        {
            if (upper.IsInfinity)
            {
                return "*";
            }
            else
            {
                return EncodeValue(upper.Value);
            }
        }

        public static string EncodeValue(PSMContentModelType upper)
        {
            return upper.ToString();
        }

        public static string DecodeString(string value)
        {
            return value;
        }

        public static Guid DecodeGuid(string value)
        {
            return Guid.Parse(value);
        }

        public static int DecodeInt(string value)
        {
            return int.Parse(value);
        }

        public static UnlimitedInt DecodeUnlimitedInt(string value)
        {
            if (value.Trim() == "*")
            {
                return new UnlimitedInt {IsInfinity = true};
            }
            else
            {
                return uint.Parse(value);
            }
        }

        public static bool DecodeBool(string value)
        {
            return bool.Parse(value);
        }

        public static PSMContentModelType DecodeContentModelType(string value)
        {
            return DecodeEnum<PSMContentModelType>(value);
        }

        private static TEnum DecodeEnum<TEnum>(string value)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }

        #endregion

        public static PSMSchemaReference.EReferenceType DecodeSchemaReferenceType(string value)
        {
            return DecodeEnum<PSMSchemaReference.EReferenceType>(value);
        }

        public static uint DecodeUInt(string value)
        {
            return uint.Parse(value);
        }
    }
}