using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exolutio.CodeContracts.Support
{
    /// <summary>
    /// OclAny implementation
    /// Base class for all OCL types
    /// </summary>
    public abstract class OclAny
    {
        #region Equality
        public OclBoolean op_Equality(OclAny b)
        {
            return (OclBoolean)Equals(b);
        }

        public static OclBoolean operator ==(OclAny a, OclAny b)
        {
            if (isNull(a))
                return (OclBoolean)isNull(b);
            else
                return (OclBoolean)a.Equals(b);
        }

        public static OclBoolean operator !=(OclAny a, OclAny b){
            if (isNull(a))
                return (OclBoolean)!isNull(b);
            else
                return (OclBoolean)!a.Equals(b); 
        }

        internal static bool isNull(OclAny o)
        {
            return object.ReferenceEquals(o, null);
        }
        #endregion

        #region OCL Operations
        public abstract OclClassifier oclType();

        public OclBoolean isKindOf(OclClassifier type)
        {
            return oclType().conformsTo(type);
        }

        public OclBoolean isTypeOf(OclClassifier type)
        {
            return (OclBoolean)(oclType() == type);
        }

        public T oclAsType<T>(OclClassifier type) where T : OclAny
        {
            return (T)this;
        }

        public static OclString oclLocale
        {
            get { return (OclString)"en_us"; }
        }

        #endregion

        #region OCL Catching operations
        /// <summary>
        /// Test for undefined value (null or exception).
        /// </summary>
        /// <param name="expression">Expression that could throw an exception</param>
        /// <returns>True if evaluation of expression threw an exception or returned null, false if returned any other value.</returns>
        public static bool oclIsUndefined(Func<OclAny> expression)
        {
            try
            {
                return isNull(expression());
            }
            catch (Exception)
            {
                return true;
            }
        }


        /// <summary>
        /// Test for invalid value (exception).
        /// </summary>
        /// <param name="expression">Expression that could throw an exception</param>
        /// <returns>True if evaluation of expression threw an exception, false if returned a value.</returns>
        public static bool oclIsInvalid(Func<object> expression)
        {
            try
            {
                expression();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
        #endregion
    }
}
