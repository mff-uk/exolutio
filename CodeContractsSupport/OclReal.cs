using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Globalization;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// OCL Real implementation
    /// </summary>
    public abstract class OclReal : OclAny, IEquatable<OclReal>, IComparable<OclReal>
    {
        #region Conversions
        /// <summary>
        /// Create an instance of Real holding a specified value.
        /// </summary>
        /// <param name="value">The value. Should be finite</param>
        /// <returns>OclReal wrapping the value.</returns>
        internal static OclReal valueOf(double? valueOrNull)
        {
            if (valueOrNull == null)
                return null;
            double value = (double)valueOrNull;
            if (Math.Truncate(value) == value && value >= int.MinValue && value <= int.MaxValue)
            {
                return OclInteger.ValueOf((int)value);
            }
            else
            {
                return new OclProperReal(value);
            }
        }
        public static explicit operator OclReal(double? value)
        {
            return valueOf(value);
        }

        public static explicit operator double?(OclReal r){
            if (IsNull(r))
                return null;
            return r.toDouble();
        }

        /// <summary>
        /// Retrieve the wrapped value.
        /// </summary>
        /// <returns>Double precision float.</returns>
        internal abstract double toDouble();

        internal static OclReal Parse(string value)
        {
            return valueOf(double.Parse(value,CultureInfo.InvariantCulture));
        }

        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            return Equals(obj as OclReal);
        }
        public bool Equals(OclReal obj)
        {
            if (IsNull(obj))
                return false;
            else if (IsUnlimited)
                return obj.IsUnlimited;
            else
                return !obj.IsUnlimited && toDouble() == obj.toDouble();
        }
        public override int GetHashCode()
        {
            if (IsUnlimited)
                return Double.PositiveInfinity.GetHashCode();
            return toDouble().GetHashCode();
        }
        #endregion

        #region OCL Operations
        /// <summary>
        /// Add two values.
        /// </summary>
        /// <param name="r">The second operand.</param>
        /// <returns>Sum of two OclReal values.</returns>
        public OclReal op_Addition(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return OclReal.valueOf(toDouble() + r.toDouble());
        }
        /// <summary>
        /// Subtract two values.
        /// </summary>
        /// <param name="r">The second operand.</param>
        /// <returns>Difference of two OclReal values.</returns>
        public OclReal op_Subtraction(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return OclReal.valueOf(toDouble() - r.toDouble());
        }
        public OclReal op_Multiply(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return OclReal.valueOf(toDouble() * r.toDouble());
        }
        public OclReal op_UnaryNegation()
        {
            return OclReal.valueOf(-toDouble());
        }
        public OclReal op_Division(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return OclReal.valueOf(toDouble()/r.toDouble());
        }
        public OclReal abs()
        {
            return OclReal.valueOf(Math.Abs(toDouble()));
        }
        public OclInteger floor()
        {
            return OclInteger.ValueOf(checked((int)Math.Floor(toDouble())));
        }
        public OclInteger round()
        {
            double v = toDouble();
            double vabs = Math.Abs(v);
            double d = vabs - Math.Truncate(vabs);
            if (d == 0.5)
                v = Math.Ceiling(v);
            else
                v = Math.Round(v);
            return OclInteger.ValueOf(checked((int)v));
        }
        public OclReal max(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return toDouble() < r.toDouble() ? r : this;
        }
        public OclReal min(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return toDouble() < r.toDouble() ? this : r;
        }
        public OclBoolean op_LessThan(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return (OclBoolean)(toDouble() < r.toDouble());
        }
        public OclBoolean op_GreaterThan(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return (OclBoolean)(toDouble() > r.toDouble());
        }
        public OclBoolean op_LessThanOrEqual(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return (OclBoolean)(toDouble() <= r.toDouble());
        }
        public OclBoolean op_GreaterThanOrEqual(OclReal r)
        {
            if (IsNull(r))
                throw new ArgumentNullException();
            return (OclBoolean)(toDouble() >= r.toDouble());
        }
        public OclString toString()
        {
            return new OclString(toDouble().ToString(CultureInfo.InvariantCulture));
        }
        #endregion
        
        #region Operators
        public static OclBoolean operator <(OclReal a, OclReal b)
        {
            return a.op_LessThan(b);
        }
        public static OclBoolean operator <=(OclReal a, OclReal b)
        {
            return a.op_LessThanOrEqual(b);
        }
        public static OclBoolean operator >(OclReal a, OclReal b)
        {
            return a.op_GreaterThan(b);
        }
        public static OclBoolean operator >=(OclReal a, OclReal b)
        {
            return a.op_GreaterThanOrEqual(b);
        }
        public static OclReal operator +(OclReal self, OclReal r)
        {
            return self.op_Addition(r);
        }
        public static OclReal operator *(OclReal self, OclReal r)
        {
            return self.op_Multiply(r);
        }
        public static OclReal operator /(OclReal self, OclReal r)
        {
            return self.op_Division(r);
        }
        public static OclReal operator -(OclReal self, OclReal r)
        {
            return self.op_Subtraction(r);
        }
        public static OclReal operator -(OclReal self)
        {
            return self.op_UnaryNegation();
        }

        #endregion

        #region Helper methods
        /// <summary>
        /// Check whether the objecgt wraps an integer or is unlimited.
        /// </summary>
        internal virtual bool IsUnlimited { get { return false; } }
        #endregion

        #region Comparable
        public int CompareTo(OclReal other)
        {
            return toDouble().CompareTo(other.toDouble());
        }
        #endregion

        #region OCL Type
        public static new readonly OclClassifier Type = OclProperReal.Type;
        #endregion
    }

    /// <summary>
    /// Double precision floating point
    /// Implemented as an immutable reference type.
    /// </summary>
    internal sealed class OclProperReal : OclReal
    {
        private readonly double value;
         
        #region Constructors
        /// <summary>
        /// Create new OclReal with the specified value.
        /// </summary>
        /// <param name="value">Value. If Infinite or Nan, throw exception.</param>
        public OclProperReal(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                throw new NotFiniteNumberException(value);
            this.value = value;
        }
        #endregion

        #region Conversion
        internal override double toDouble()
        {
            return value;
        }
        #endregion

        #region OCL Type
        internal static new readonly OclClassifier Type = PrimitiveType.Real;
        public override OclClassifier oclType()
        {
            return Type;
        }
        #endregion
    }
}
