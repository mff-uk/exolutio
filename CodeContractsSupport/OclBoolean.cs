using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// Implementation of the OCL type Boolean as an wrapper of System.Boolean
    /// </summary>
    public class OclBoolean : OclAny, IEquatable<OclBoolean>
    {

        public static readonly OclBoolean True = new OclBoolean(true);
        public static readonly OclBoolean False = new OclBoolean(false);

        private readonly bool value;
        #region Constructors
        private OclBoolean(bool v)
        {
            value = v;
        }
        #endregion

        #region Conversions

        public static explicit operator OclBoolean(bool? from)
        {
            if (from.HasValue)
                return from.Value ? True : False;
            else
                return null;
        }
        public static explicit operator bool?(OclBoolean from)
        {
            if(IsNull(from))
                return null;
            else
                return from.value;
        }

        public static explicit operator bool(OclBoolean from)
        {
            return from.value;
        }

        public override string ToString()
        {
            return value ? "true" : "false";
        }

        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            return Equals(obj as OclBoolean);
        }

        public bool Equals(OclBoolean b)
        {
            if (IsNull(b))
                return false;
            else
                return b.value == value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        #endregion

        #region Static helper methods

        private static OclBoolean CheckNonNullArgument(OclBoolean b)
        {
            if (IsNull(b))
                throw new ArgumentNullException();
            return b;
        }
        #endregion

        #region OCL Operations
        /// <summary>
        /// Convert  Boolean value to string
        /// </summary>
        /// <returns>returns "true" for true and "false" for false</returns>
        OclString toString()
        {
            return new OclString(ToString());
        }

        /// <summary>
        /// Boolean XOR
        /// </summary>
        /// <param name="a">The second operand</param>
        /// <returns>OclBoolean representing true</returns>
        public OclBoolean xor(OclBoolean a){
            return (OclBoolean)(CheckNonNullArgument(a).value!=value);
        }

        public OclBoolean not()
        {
            return (OclBoolean)(!value);
        }
        #endregion

        
        #region OCL Invalid catching operations 
        /// <summary>
        /// Boolean OR with undefined values (exception) handling
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <returns></returns>
        public static OclBoolean or(Func<OclBoolean> e1, Func<OclBoolean> e2)
        {
            try
            {
                if ((bool)e1())
                    return True;
            }
            catch (Exception ex1)
            {
                try
                {
                    if ((bool)CheckNonNullArgument(e2()))
                        return True;
                }
                catch (Exception ex2)
                {
                    throw new AggregateException(ex1, ex2);
                }
                throw;
            }
            return CheckNonNullArgument(e2());
        }

        /// <summary>
        /// Boolean AND with undefined values (exception) handling
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <returns></returns>
        private static bool And(Func<bool> e1, Func<bool> e2)
        {
            try
            {
                if (!e1())
                    return false;
            }
            catch (Exception)
            {
                if (!e2())
                    return false;
                throw;
            }
            return e2();
        }

        public static OclBoolean and(Func<OclBoolean> e1, Func<OclBoolean> e2)
        {
            try
            {
                if (!(bool)e1())
                    return False;
            }
            catch (Exception ex1)
            {
                try
                {
                    if (!(bool)CheckNonNullArgument(e2()))
                        return False;
                }
                catch (Exception ex2)
                {
                    throw new AggregateException(ex1, ex2);
                }
                throw;
            }
            return CheckNonNullArgument(e2());
        }

        /// <summary>
        /// Boolean implication with undefined values (exception) handling
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <returns></returns>
        public static bool Implies(Func<bool> e1, Func<bool> e2)
        {
            try
            {
                if (!e1())
                    return true;
            }
            catch (Exception)
            {
                if (e2())
                    return true;
                throw;
            }
            return e2();
        }

        public static OclBoolean implies(Func<OclBoolean> e1, Func<OclBoolean> e2)
        {
            try
            {
                if (!(bool)e1())
                    return True;
            }
            catch (Exception ex1)
            {
                try{
                    if ((bool)CheckNonNullArgument(e2()))
                        return True;
                }
                catch (Exception ex2)
                {
                    throw new AggregateException(ex1, ex2);
                }
                throw;
            }
            return CheckNonNullArgument(e2());
        }

        #endregion

        #region OCL Type

        public static new readonly OclClassifier Type = PrimitiveType.Boolean;

        public override OclClassifier oclType()
        {
            return Type;
        }

        [Pure]
        public static OclSet allInstances()
        {
            return new OclSet(Type, new OclBoolean[] { True, False });
        }

        #endregion

 
    }
        
}
