using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Numerics;
using System.Globalization;

using System.Diagnostics.Contracts;

namespace Exolutio.CodeContracts.Support
{
    public abstract class OclInteger : OclReal
    {
        #region Conversions
        public static OclInteger ValueOf(int? l)
        {
            if (l == null)
                return null;
            if (l < 0)
                return new OclProperInteger((int)l);
            else
                return OclUnlimitedNatural.ValueOf(l);
        }
        public static explicit operator OclInteger(int? l)
        {
            return ValueOf(l);
        }
        public static explicit operator int?(OclInteger i)
        {
            if (object.ReferenceEquals(i, null))
                return null;
            return i.ToInt();
        }

        internal abstract int ToInt();

        internal sealed override double toDouble()
        {
            return (double)ToInt();
        }
        internal static new OclInteger Parse(string value)
        {
            return ValueOf(int.Parse(value, CultureInfo.InvariantCulture));
        }

        #endregion

        #region OCL Operations
        public new OclInteger op_UnaryNegation()
        {
            return ValueOf(checked(-ToInt()));
        }
        public OclInteger op_Addition(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(ToInt() + i.ToInt()));
        }

        public OclInteger op_Subtraction(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(ToInt() - i.ToInt()));
        }
        public OclInteger op_Multiply(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(ToInt() * i.ToInt()));
        }

        public OclReal op_Division(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return OclReal.valueOf(checked(toDouble() / i.toDouble()));
        }
        public new OclInteger abs()
        {
            return ValueOf(Math.Abs(ToInt()));
        }
          
        public OclInteger div(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(ToInt() / i.ToInt()));
        }
        public OclInteger mod(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(ToInt() % i.ToInt()));
        }
        public OclInteger max(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return ValueOf(Math.Max(ToInt(), i.ToInt()));
        }
        public OclInteger min(OclInteger i)
        {
            if (IsNull(i))
                throw new ArgumentNullException();
            return ValueOf(Math.Min(ToInt(), i.ToInt()));
        }
        public new OclString toString()
        {
            return new OclString(ToInt().ToString(CultureInfo.InvariantCulture));
        }
        #endregion

        #region OCL Type
        public static new readonly OclClassifier Type = OclProperInteger.Type;
        #endregion

        #region Operators

        public static OclInteger operator +(OclInteger self, OclInteger i)
        {
            return self.op_Addition(i);
        }
        public static OclInteger operator *(OclInteger self, OclInteger i)
        {
            return self.op_Multiply(i);
        }
        public static OclReal operator /(OclInteger self, OclInteger i)
        {
            return self.op_Division(i);
        }
        public static OclInteger operator -(OclInteger self, OclInteger i)
        {
            return self.op_Subtraction(i);
        }
        public static OclInteger operator -(OclInteger self)
        {
            return self.op_UnaryNegation();
        }

        #endregion

    }

    internal sealed class OclProperInteger : OclInteger
    {
        private int value;

        #region Constructors
        internal OclProperInteger(int value)
        {
            this.value = value;
        }
        #endregion
        #region Conversions
        internal override int ToInt()
        {
            return value;
        }
        #endregion

        #region OCL Type
        internal static new readonly OclClassifier Type = PrimitiveType.Integer;
        public override OclClassifier oclType()
        {
            return Type;
        }
        #endregion
    }

}
