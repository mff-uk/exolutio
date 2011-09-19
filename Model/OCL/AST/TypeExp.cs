﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model.OCL.Types;

namespace Exolutio.Model.OCL.AST
{
    /// <summary>
    /// A TypeExp is an expression used to refer to an existing meta type within an expression. It is used in particular to pass the
    /// reference of the meta type when invoking the operations oclIsKindOf, oclIsTypeOf, and oclAsType.
    /// </summary>
    public class TypeExp : OclExpression
    {
        public TypeExp(Classifier referedType,TypeType type)
            : base(type) {
                this.ReferredType = referedType;
        }

        public Classifier ReferredType
        {
            get;
            set;
        }

        public override T Accept<T>(IAstVisitor<T> visitor) {
            return visitor.Visit(this);
        }
    }
}
