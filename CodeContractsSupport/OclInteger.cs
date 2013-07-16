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
            return i.toInt();
        }

        internal abstract int toInt();

        internal sealed override double toDouble()
        {
            return (double)toInt();
        }
        internal static new OclInteger Parse(string value)
        {
            return ValueOf(int.Parse(value, CultureInfo.InvariantCulture));
        }

        #endregion

        #region OCL Operations
        public new OclInteger op_UnaryNegation()
        {
            return ValueOf(checked(-toInt()));
        }
        public OclInteger op_Addition(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(toInt() + i.toInt()));
        }

        public OclInteger op_Subtraction(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(toInt() - i.toInt()));
        }
        public OclInteger op_Multiply(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(toInt() * i.toInt()));
        }

        public OclReal op_Division(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return OclReal.valueOf(checked(toDouble() / i.toDouble()));
        }
        public new OclInteger abs()
        {
            return ValueOf(Math.Abs(toInt()));
        }
          
        public OclInteger div(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(toInt() / i.toInt()));
        }
        public OclInteger mod(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return ValueOf(checked(toInt() % i.toInt()));
        }
        public OclInteger max(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return ValueOf(Math.Max(toInt(), i.toInt()));
        }
        public OclInteger min(OclInteger i)
        {
            if (isNull(i))
                throw new ArgumentNullException();
            return ValueOf(Math.Min(toInt(), i.toInt()));
        }
        public new OclString toString()
        {
            return new OclString(toInt().ToString(CultureInfo.InvariantCulture));
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
        internal override int toInt()
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
