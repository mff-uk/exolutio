﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A RealLiteralExp denotes a value of the predefined type Real.
    /// </summary>
    public class RealLiteralExp : NumericLiteralExp
    {
        public RealLiteralExp(double value, Types.Classifier type):base(type) {
            Value = value;
        }

        public double Value
        {
            get;
            set;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }

        public override void Accept(IAstVisitor visitor) {
            visitor.Visit(this);
        }

    }
}
