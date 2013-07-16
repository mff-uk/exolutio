using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Numerics;
using System.Globalization;

using System.Diagnostics.Contracts;

namespace Exolutio.CodeContracts.Support
{
    public abstract class OclUnlimitedNatural : OclInteger
    {    
        public static readonly OclUnlimitedNatural Unlimited = new OclUnlimited();

        #region Conversions
        internal static new OclUnlimitedNatural ValueOf(int? l)
        {
            if (l == null)
                return null;
            return new OclNatural((int)l);
        }
        #endregion

        #region OCL Operations
        /// <summary>
        /// Add two unlimited naturals.
        /// </summary>
        /// <param name="u">The second operand.</param>
        /// <returns>The sum.</returns>
        /// <exception cref="UnlimitedValueException">If either of the operands is unlimited.</exception>
        public OclUnlimitedNatural op_Addition(OclUnlimitedNatural u)
        {
            return ValueOf(toInt() + u.toInt());
        }
        /// <summary>
        /// Multiply two unlimited naturals.
        /// </summary>
        /// <param name="u">The second operand.</param>
        /// <returns>The product.</returns>
        /// <exception cref="UnlimitedValueException">If either of the operands is unlimited.</exception>
        public OclUnlimitedNatural op_Multiply(OclUnlimitedNatural u)
        {
            return ValueOf(toInt() * u.toInt());
        }
        /// <summary>
        /// Divide two unlimited naturals.
        /// </summary>
        /// <param name="u">The second operand.</param>
        /// <returns>The quotient.</returns>
        /// <exception cref="UnlimitedValueException">If either of the operands is unlimited.</exception>
        /// <exception cref="DivideByZeroException">If the second operator is zero.</exception>
        public OclReal op_Divide(OclUnlimitedNatural u)
        {
            return OclReal.valueOf(toDouble() / u.toDouble());
        }
        /// <summary>
        /// Divide two unlimited naturals.
        /// </summary>
        /// <param name="u">The second operand.</param>
        /// <returns>The quotient.</returns>
        /// <exception cref="UnlimitedValueException">If either of the operands is unlimited.</exception>
        /// <exception cref="DivideByZeroException">If the second operator is zero.</exception>
        public OclUnlimitedNatural div(OclUnlimitedNatural u)
        {
            return ValueOf(toInt() / u.toInt());
        }
        /// <summary>
        /// Compute modulo of two unlimited naturals.
        /// </summary>
        /// <param name="u">The second operand.</param>
        /// <returns>The remainder.</returns>
        /// <exception cref="UnlimitedValueException">If either of the operands is unlimited.</exception>
        /// <exception cref="DivideByZeroException">If the second operator is zero.</exception>
        public OclUnlimitedNatural mod(OclUnlimitedNatural u)
        {
            return ValueOf(toInt() % u.toInt());
        }
        public OclUnlimitedNatural max(OclUnlimitedNatural u)
        {
            return less(u) ? u : this;
        }
        public OclUnlimitedNatural min(OclUnlimitedNatural u)
        {
            return less(u) ? this : u;
        }
        public OclBoolean op_LessThan(OclUnlimitedNatural u)
        {
            return (OclBoolean)less(u);
        }
        public OclBoolean op_GreaterThan(OclUnlimitedNatural u)
        {
            return (OclBoolean)u.less(this);
        }
        public OclBoolean op_LessThanOrEqual(OclUnlimitedNatural u)
        {
            return (OclBoolean)!u.less(this);
        }
        public OclBoolean op_GreaterThanOrEqual(OclUnlimitedNatural u)
        {
            return (OclBoolean)!less(u);
        }
        public OclInteger toInteger()
        {
            return OclInteger.ValueOf(toInt());
        }
        /// <summary>
        /// Convert to OCL String. If this is OclUnlimitedNatural.Unlimited, returns "*".
        /// </summary>
        /// <returns>String representation of this.</returns>
        public new OclString toString()
        {
            if (unlimited)
                return new OclString("*");
            else
                return new OclString(toInt().ToString(CultureInfo.InvariantCulture));
        }

        public override OclClassifier oclType()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Operators
        public static OclBoolean operator <(OclUnlimitedNatural self, OclUnlimitedNatural u)
        {
            return self.op_LessThan(u);
        }
        public static OclBoolean operator <=(OclUnlimitedNatural self, OclUnlimitedNatural u)
        {
            return self.op_LessThanOrEqual(u);
        }
        public static OclBoolean operator >(OclUnlimitedNatural self, OclUnlimitedNatural u)
        {
            return self.op_GreaterThan(u);
        }
        public static OclBoolean operator >=(OclUnlimitedNatural self, OclUnlimitedNatural u)
        {
            return self.op_GreaterThanOrEqual(u);
        }
        public static OclReal operator +(OclUnlimitedNatural self, OclUnlimitedNatural u)
        {
            return self.op_Addition(u);
        }
        public static OclReal operator *(OclUnlimitedNatural self, OclUnlimitedNatural u)
        {
            return self.op_Multiply(u);
        }
        public static OclReal operator /(OclUnlimitedNatural self, OclUnlimitedNatural u)
        {
            return self.op_Division(u);
        }
        #endregion

        #region Helper methods
        private bool less(OclUnlimitedNatural u)
        {
            if (unlimited)
                return false;
            else if (u.unlimited)
                return true;
            else
                return (toInt() < u.toInt());
        }
        #endregion
    }

    /// <summary>
    /// Implementaion of finite values of UnlimitedNatural
    /// </summary>
    internal sealed class OclNatural : OclUnlimitedNatural
    {
        private int value;

        internal OclNatural(int value)
        {
            if (value < 0)
                throw new ArgumentException("Negative value for natrual constructor.");
            this.value = value;
        }
        internal override int toInt()
        {
            return value;
        }
    }

    internal sealed class OclUnlimited : OclUnlimitedNatural
    {
        internal OclUnlimited()
        {
        }
        internal override int toInt()
        {
            throw new OclUnlimitedValueException();
        }
        internal override bool unlimited
        {
            get { return true; }
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
