using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// If the string is not a valid UTF-16 string (mismatched surrogates).
    /// </summary>
    public class InvalidUTF16StringException : Exception
    {
    }

    public class OclString : OclAny, IEquatable<OclString>
    {
        private readonly string value;

        #region Constructors
        public OclString(string str)
        {
            value = str;
        }

        public OclString(char ch)
        {
            value = ch.ToString();
        }
        #endregion

        #region Conversions
        public static explicit operator string(OclString s)
        {
            if (IsNull(s))
                return null;
            return s.value;
        }
        public static explicit operator OclString(string s)
        {
            if (s == null)
                return null;
            return new OclString(s);
        }
        #endregion

        #region Helper methods
      

        /// <summary>
        /// Check whether there is a surrogate pair.
        /// </summary>
        /// <param name="str">The examined string.</param>
        /// <param name="limit">String size limit. The string is treated as if this was its length.</param>
        /// <param name="index">Index of the character.</param>
        /// <returns>True if there is a valid surrogate pair, false if there is a valid non-surrogate character.</returns>
        private static bool CheckSurrogatePair(string str, int limit, int index)
        {
            if (char.IsHighSurrogate(str[index]))
            {
                //If there is a surrogate pair, skip the next char
                if (index + 1 < limit && char.IsLowSurrogate(str[index + 1]))
                {
                    return true;
                }
                else
                    throw new InvalidUTF16StringException();
            }
            else if (char.IsLowSurrogate(str[index]))
            {
                throw new InvalidUTF16StringException();
            }
            else
                return false;
        }

        private static int CharOffsetToUnicode(string str, int offset)
        {
            int unicodeOffset = 0;
            for (int ptr = 0; ptr < offset; ++ptr)
            {
                ++unicodeOffset;
                if (CheckSurrogatePair(str, offset, ptr))
                    ++ptr;
                
            }
            return unicodeOffset;
        }
        private static int UnicodeOffsetToChar(string str, int offset)
        {
            int unicodeOffset = 0;
            int ptr;
            for (ptr = 0; ptr < str.Length && unicodeOffset < offset; ++ptr)
            {
                ++unicodeOffset;
                if (CheckSurrogatePair(str,str.Length, ptr))
                    ++ptr;
            }

            if (unicodeOffset == offset)
                return ptr;
            else
                throw new ArgumentOutOfRangeException("offset");
        }

        #endregion

        #region OCL Operations

        /// <summary>
        /// Concatenate strings.
        /// </summary>
        /// <param name="s">The second part.</param>
        /// <returns>Concatenation of this and s.</returns>
        [Pure]
        public OclString op_Addition(OclString s)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            return concat(s);
        }

        /// <summary>
        /// Length of the string in code points.
        /// </summary>
        /// <returns>Length of the string in code points.</returns>
        [Pure]
        public OclInteger size()
        {
            return (OclInteger)CharOffsetToUnicode(value, value.Length);
        }

        /// <summary>
        /// Concatenate strings.
        /// </summary>
        /// <param name="s">The second part.</param>
        /// <returns>Concatenation of this and s.</returns>
        [Pure]
        public OclString concat(OclString s)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            return new OclString(value + s.value);
        }

        /// <summary>
        /// Extract substring.
        /// </summary>
        /// <param name="from">Lower index (including). Characters numbered from 1.</param>
        /// <param name="to">Upper index (including). Characters numbered from 1.</param>
        /// <returns>Substring from lower to upper.</returns>
        [Pure]
        public OclString substring(OclInteger from, OclInteger to)
        {
            if (IsNull(from))
                throw new ArgumentNullException("from");
            if (IsNull(to))
                throw new ArgumentNullException("to");

            //Convert to 32-bit integers.
            int fromInt = (int)from;
            int toInt = (int)to;

            //Check valid bounds.
            if (fromInt < 1 || toInt < fromInt || CharOffsetToUnicode(value, value.Length) < fromInt)
                throw new ArgumentOutOfRangeException();

            //Convert to 0-offset and char offset.
            int fromUnicode = UnicodeOffsetToChar(value, fromInt - 1);
            int toUnicode = UnicodeOffsetToChar(value, toInt);

            return new OclString(value.Substring(fromUnicode, toUnicode - fromUnicode));
        }

        /// <summary>
        /// Convert to integer.
        /// </summary>
        /// <returns>Integer value of the string.</returns>
        [Pure]
        public OclInteger toInteger()
        {
            return OclInteger.Parse(value);
        }

        /// <summary>
        /// Convert to real.
        /// </summary>
        /// <returns>Integer value of the string.</returns>
        [Pure]
        public OclReal toReal()
        {
            return OclReal.Parse(value);
        }

        /// <summary>
        /// Convert to upper case. Depends on the specified locale.
        /// </summary>
        /// <param name="locale">OCL name of the locale</param>
        /// <returns>Uppercase value.</returns>
        [Pure]
        public OclString toUpperCase(OclString locale)
        {
            return new OclString(value.ToUpper(OclUtils.GetLocale(locale)));
        }

        /// <summary>
        /// Convert to lower case. Depends on the specified locale.
        /// </summary>
        /// <param name="locale">OCL name of the locale</param>
        /// <returns>Lowercase value.</returns>
        [Pure]
        public OclString toLowerCase(OclString locale)
        {
            return new OclString(value.ToLower(OclUtils.GetLocale(locale)));
        }

        /// <summary>
        /// Find substring s in this string.
        /// </summary>
        /// <param name="s">Substring.</param>
        /// <returns>Index of the substring indexed from 1. 0 if no substring is found.</returns>
        [Pure]
        public OclInteger indexOf(OclString s)
        {
            if (IsNull(s))
                throw new ArgumentNullException();

            int intIndex = value.IndexOf(s.value);
            if(intIndex < 0)
                return (OclInteger)0;
            else
                return (OclInteger)(CharOffsetToUnicode(value, intIndex) + 1);
        }

        /// <summary>
        /// Compare for equality using non-case sensitive comparison.
        /// </summary>
        /// <param name="s">The second string to be compared</param>
        /// <param name="locale">OCL name of the locale used to compare case-insensitively.</param>
        /// <returns>True if the strings are equivalent, false otherwise.</returns>
        [Pure]
        public OclBoolean equalsIgnoreCase(OclString s, OclString locale)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            return (OclBoolean)(String.Compare(value, s.value, true, OclUtils.GetLocale(locale)) == 0);
        }

        [Pure]
        public OclString at(OclInteger pos)
        {
            if (IsNull(pos))
                throw new ArgumentNullException();
            return substring(pos, pos);
        }

        /// <summary>
        /// Split the string into characters.
        /// </summary>
        /// <returns>Sequence of 1-character strings.</returns>
        [Pure]
        public OclSequence characters()
        {
            OclSequence seq = new OclSequence(OclString.Type);
            for (int ptr = 0; ptr < value.Length; ++ptr)
            {
                if(CheckSurrogatePair(value, value.Length, ptr)){
                    seq.list.Add(new OclString(value.Substring(ptr, 2)));
                    ++ptr;
                }
                else
                    seq.list.Add(new OclString(value[ptr]));
            }
            return seq;
        }

        /// <summary>
        /// Convert to boolean.
        /// </summary>
        /// <returns>Boolean value of the string.</returns>
        [Pure]
        public OclBoolean toBoolean()
        {
            return (OclBoolean)(value == "true");//TODO: if not true neither false.
        }

        [Pure]
        public OclBoolean op_LessThan(OclString s, OclString locale)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            return (OclBoolean)(String.Compare(value, s.value, false, OclUtils.GetLocale(locale)) < 0);
        }

        [Pure]
        public OclBoolean op_LessThanOrEqual(OclString s, OclString locale)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            return (OclBoolean)(String.Compare(value, s.value, false, OclUtils.GetLocale(locale)) <= 0);
        }

        [Pure]
        public OclBoolean op_GreaterThan(OclString s, OclString locale)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            return (OclBoolean)(String.Compare(value, s.value, false, OclUtils.GetLocale(locale)) > 0);
        }

        [Pure]
        public OclBoolean op_GreaterThanOrEqual(OclString s, OclString locale)
        {
            if (IsNull(s))
                throw new ArgumentNullException();
            return (OclBoolean)(String.Compare(value, s.value, false, OclUtils.GetLocale(locale)) >= 0);
        }

        #endregion

        #region Operators
        public static OclString operator +(OclString a, OclString b)
        {
            return a.op_Addition(b);
        }
        #endregion

        #region Equality
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as OclString);
        }
        public bool Equals(OclString s)
        {
            if (IsNull(s))
                return false;
            else
                return s.value == value;
        }
        #endregion

        #region OCL Type

        public static new readonly OclClassifier Type = PrimitiveType.String;

        public override OclClassifier oclType()
        {
            return Type;
        }
        #endregion
    }
}
