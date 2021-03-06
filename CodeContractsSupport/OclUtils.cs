﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Exolutio.CodeContracts.Support
{

    [Pure]
    public static class OclUtils
    {

        /// <summary>
        /// Evaluate expression with named variable
        /// </summary>
        /// <typeparam name="T">Return type of the expression</typeparam>
        /// <typeparam name="U">Type of the variable</typeparam>
        /// <param name="initValue">Value assigned to the variable</param>
        /// <param name="inExp">Expression to evaluate</param>
        /// <returns>Result of the expression</returns>
        public static T Let<T, U>(U initValue, Func<U, T> inExp)
        {
            return inExp(initValue);
        }

        /// <summary>
        /// Execute expression returning void and return null
        /// </summary>
        /// <param name="expression">Expression to execute</param>
        /// <returns>Always null.</returns>
        public static OclAny Void(Action expression)
        {
            expression();
            return null;
        }

        /// <summary>
        /// Get CultureInfo specified by OCL locale name
        /// </summary>
        /// <param name="name">OCL locale name (example: 'en_US')</param>
        /// <returns>The requested culture.</returns>
        public static System.Globalization.CultureInfo GetLocale(OclString name)
        {
            if (OclAny.IsNull(name))
                throw new ArgumentNullException();
            return System.Globalization.CultureInfo.GetCultureInfo(((string)name).Replace('_','-'));
        }
    }
    
}
