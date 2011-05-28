using System;

namespace Exolutio.Model
{
    public struct UnlimitedInt : IEquatable<UnlimitedInt>, IComparable<UnlimitedInt>, IComparable<uint>
    {
        private const string unlimitedChar = "*";

        public bool IsInfinity { get; set; }

        public uint Value { get; set; }

        public UnlimitedInt(uint value) : this()
        {
            IsInfinity = false;
            Value = value;
        }

        public static UnlimitedInt Infinity
        {
            get
            {
                return new UnlimitedInt { IsInfinity = true };
            }
        }

        public static implicit operator UnlimitedInt(uint value)
        {
            return new UnlimitedInt(value);
        }

        public static explicit operator uint(UnlimitedInt unlimitedInt)
        {
            return unlimitedInt.Value;
        }

        public static explicit operator string(UnlimitedInt unlimitedInt)
        {
            if (unlimitedInt.IsInfinity) return unlimitedChar;
            else return unlimitedInt.Value.ToString();
        }

        public override string ToString()
        {
            if (IsInfinity) return unlimitedChar;
            else return Value.ToString();
        }

        #region comparison

        public static bool operator <(UnlimitedInt obj1, UnlimitedInt obj2)
        {
            return obj1.CompareTo(obj2) < 0;
        }
        public static bool operator >(UnlimitedInt obj1, UnlimitedInt obj2)
        {
            return obj1.CompareTo(obj2) > 0;
        }
        public static bool operator <=(UnlimitedInt obj1, UnlimitedInt obj2)
        {
            return obj1.CompareTo(obj2) <= 0;
        }
        public static bool operator >=(UnlimitedInt obj1, UnlimitedInt obj2)
        {
            return obj1.CompareTo(obj2) >= 0;
        }

        public int CompareTo(uint other)
        {
            return CompareTo((UnlimitedInt) other);
        }

        public int CompareTo(UnlimitedInt other)
        {
            if (!this.IsInfinity && other.IsInfinity)
                return -1;
            if (this.IsInfinity && other.IsInfinity)
                return 1;
            if (this.IsInfinity && other.IsInfinity)
                return 0;

            return this.Value.CompareTo(other.Value);
        }

        #endregion

        #region equality

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(UnlimitedInt other)
        {
            return other.IsInfinity.Equals(IsInfinity) && other.Value == Value;
        }


        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (UnlimitedInt)) return false;
            return Equals((UnlimitedInt) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)((IsInfinity.GetHashCode()*397) ^ Value);
            }
        }

        public static bool operator ==(UnlimitedInt left, UnlimitedInt right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnlimitedInt left, UnlimitedInt right)
        {
            return !left.Equals(right);
        }

        #endregion

        public static UnlimitedInt Parse(string s)
        {
            if (s == unlimitedChar)
                return UnlimitedInt.Infinity;
            else
                return uint.Parse(s);
        }

        public static bool TryParse(string s, out UnlimitedInt result)
        {
            uint intresult;
            if (s == unlimitedChar)
            {
                result = UnlimitedInt.Infinity;
                return true; 
            }
            else if (uint.TryParse(s, out intresult))
            {
                result = intresult;
                return true;
            }
            result = 0;
            return false;
        }
    }
}