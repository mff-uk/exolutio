﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;


namespace Exolutio.Model.OCL.AST
{
    public class StringLiteralExp : PrimitiveLiteralExp
    {
        /// <summary>
        /// A StringLiteralExp denotes a value of the predefined type String.
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        public override Types.Classifier Type
        {
            get
            {
                return new  StringType ();
            }
            protected set
            {
                throw new InvalidOperationException();
            }
        }
    }
}
