using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.OCL.Types;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.SchematronTranslation
{
    public class OperationHelper : List<OperationInfo>
    {
        private bool explicitCastAtomicOperands = true;
        public bool ExplicitCastAtomicOperands
        {
            get { return explicitCastAtomicOperands; }
            set { explicitCastAtomicOperands = value; }
        }

        public Log<OclExpression> Log { get; set; }

        public PSMSchema PSMSchema { get; set; }

        public PSMBridge PSMBridge { get; set; }

        private string xsdNamespacePrefix = "xs";
        internal OperationInfo atOperationInfo;
        internal OperationInfo firstOperationInfo;
        internal OperationInfo lastOperationInfo;

        public string XsdNamespacePrefix
        {
            get { return xsdNamespacePrefix; }
            set { xsdNamespacePrefix = value; }
        }

        public void InitStandard()
        {
            // Any
            Add(new OperationInfo { Priority = 1, CanOmitDataCall = true, OclName = "oclAsSet", XPathName = "oclAsSet", Arity = 1, CustomTranslateHandler = ReplaceByArgumentHandler });

            // operators
            Add(new OperationInfo { Priority = 1, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "*", XPathName = "*", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 1, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "/", XPathName = "div", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 2, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "+", XPathName = "+", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 2, IsXPathInfix = true, IsOclInfix = true, OclName = "-", XPathName = "-", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "<", XPathName = "lt", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = ">", XPathName = "gt", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "<=", XPathName = "le", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = ">=", XPathName = "ge", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "=", XPathName = "eq", TypeDependent = true, ArgumentCondition = IsXPathAtomic, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, OclName = "=", XPathName = "is", TypeDependent = true, ArgumentCondition = IsXPathNonAtomic, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "<>", XPathName = "ne", TypeDependent = true, ArgumentCondition = IsXPathAtomic, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, OclName = "<>", XPathName = "is not", TypeDependent = true, ArgumentCondition = IsXPathNonAtomic, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 5, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "and", XPathName = "and", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 6, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "or", XPathName = "or", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 7, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "xor", XPathName = "oclBoolean:xor", CustomXPathSyntaxFormatString = "oclBoolean:xor({0}, {1})", CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = 8, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "implies", XPathName = "#NDEF", CustomXPathSyntaxFormatString = "if ({0}) then {1} else true()", CustomTranslateHandler = FunctionAtomicHandler });


            Add(new OperationInfo { Priority = -1, IsXPathPrefix = true, IsOclPrefix = true, CanOmitDataCall = true, OclName = "-", XPathName = "-", CustomTranslateHandler = PrefixAtomicHandler });
            Add(new OperationInfo { Priority = -1, IsXPathPrefix = true, IsOclPrefix = true, CanOmitDataCall = true, OclName = "not", XPathName = "#NDEF", CustomXPathSyntaxFormatString = "not({0})", CustomTranslateHandler = FunctionAtomicHandler });

            // date
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "after", XPathName = "oclDate:after", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "before", XPathName = "oclDate:before", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });

            // conversions 
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "toString", XPathName = "xs:string", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "toInteger", XPathName = "xs:integer", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "toReal", XPathName = "xs:double", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });

            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "oclIsInvalid", XPathName = "oclX:oclIsInvalid", Arity = 1, CustomXPathSyntaxFormatString = "oclX:oclIsInvalid(function() {{ {0} }})", CustomTranslateHandler = FunctionAtomicHandler });

            // string handling 
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "concat", XPathName = "concat", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "substring", XPathName = "substring", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "toUpperCase", XPathName = "upper-case", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "toLowerCase", XPathName = "lower-case", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "equalsIgnoreCase", XPathName = "oclString:equalsIgnoreCase", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "at", XPathName = "oclString:at", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentTypes = new[] { PSMBridge.TypesTable.Library.String, PSMBridge.TypesTable.Library.Integer }});
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "size", XPathName = "string-length", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentTypes = new[] { PSMBridge.TypesTable.Library.String }});
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "indexOf", XPathName = "oclString:indexOf", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentTypes = new[] { PSMBridge.TypesTable.Library.String, PSMBridge.TypesTable.Library.String} });

            // arithmetic 
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "abs", XPathName = "abs", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "floor", XPathName = "floor", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "round", XPathName = "round", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "max", XPathName = "max", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "min", XPathName = "min", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "div", XPathName = "idiv", Arity = 2, CustomTranslateHandler = InfixAtomicHandler, IsXPathInfix = true, IsOclInfix = false });

            // collection
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "includes", XPathName = "oclX:includes", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "excludes", XPathName = "oclX:excludes", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "includesAll", XPathName = "oclX:includesAll", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "excludesAll", XPathName = "oclX:excludesAll", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "size", XPathName = "count", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "count", XPathName = "oclX:count", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "product", XPathName = "oclX:product", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "sum", XPathName = "sum", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "append", XPathName = "oclX:append", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "prepend", XPathName = "oclX:prepend", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "insertAt", XPathName = "oclX:insertAt", Arity = 3, CustomTranslateHandler = FunctionAtomicHandler });
            atOperationInfo = new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "at", XPathName = "oclX:at", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler };
            Add(atOperationInfo);
            firstOperationInfo = new OperationInfo {Priority = -1, CanOmitDataCall = true, OclName = "first", XPathName = "oclX:first", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler};
            Add(firstOperationInfo);
            lastOperationInfo = new OperationInfo {Priority = -1, CanOmitDataCall = true, OclName = "last", XPathName = "oclX:last", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler};
            Add(lastOperationInfo);
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "indexOf", XPathName = "oclX:indexOf", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "reverse", XPathName = "reverse", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "max", XPathName = "max", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "min", XPathName = "min", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "sum", XPathName = "sum", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "union", XPathName = "oclX:union", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "subOrderedSet", XPathName = "oclX:subOrderedSet", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "subSequence", XPathName = "oclX:subSequence", Arity = 1, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "-", XPathName = "oclX:setMinus", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "symmetricDifference", XPathName = "oclX:symmetricDifference", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler, TypeDependent = true, ArgumentCondition = IsXPathNonAtomic });
        }

        private string ReplaceByArgumentHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments)
        {
            string op = WrapAtomicOperand(arguments[0], operationInfo, 0);
            return string.Format("{0}", op);
        }

        private string PrefixAtomicHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments)
        {
            string op = WrapAtomicOperand(arguments[0], operationInfo, 0);
            return string.Format("{0} {1}", operationInfo.XPathName, op);
        }

        private string InfixAtomicHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments)
        {
            string leftOp = WrapAtomicOperand(arguments[0], operationInfo, 0);
            string rightOp = WrapAtomicOperand(arguments[1], operationInfo, 1);
            return string.Format("{1} {0} {2}", operationInfo.XPathName, leftOp, rightOp);
        }

        private string FunctionAtomicHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments)
        {
            IEnumerable<string> wrappedSequence = from ind in Enumerable.Range(0, arguments.Count()) 
                                                  select WrapAtomicOperand(arguments[ind], operationInfo, ind);

            //IEnumerable<string> withoutDataCall = convertedArguments.Select(a => a.RawString);
            //IEnumerable<string> withDataCall = convertedArguments.Select(a => a.GetString());
            //IEnumerable<string> stringifiedArguments = !operationInfo.CanOmitDataCall ? withDataCall : withoutDataCall;


            if (!string.IsNullOrEmpty(operationInfo.CustomXPathSyntaxFormatString))
            {
                return string.Format(operationInfo.CustomXPathSyntaxFormatString, wrappedSequence.ToArray());
                //if (operationInfo.TypeDependent)
                //{
                //    return string.Format(operationInfo.CustomXPathSyntaxFormatString, wrappedSequence.ToArray());
                //}
                //else
                //{
                //    return string.Format(operationInfo.CustomXPathSyntaxFormatString, stringifiedArguments.ToArray());
                //}
            }
            else
            {
                return string.Format("{0}({1})", operationInfo.XPathName, wrappedSequence.ConcatWithSeparator(", "));
                //if (operationInfo.TypeDependent)
                //{
                //    return string.Format("{0}({1})", operationInfo.XPathName, wrappedSequence.ConcatWithSeparator(", "));
                //}
                //else
                //{
                //    return string.Format("{0}({1})", operationInfo.XPathName, stringifiedArguments.ConcatWithSeparator(", "));
                //}
            }
        }

        private string EqualityTestHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments)
        {
            Debug.Assert(arguments.Count() == 2);
            OclExpression leftOpExpr = arguments[0];
            OclExpression rightOpExpr = arguments[1];
            string leftOp = WrapAtomicOperand(leftOpExpr, operationInfo, 0);
            string rightOp = WrapAtomicOperand(rightOpExpr, operationInfo, 1);

            Classifier voidType = PSMBridge.TypesTable.Library.Void;

            if (leftOpExpr.Type == voidType && rightOpExpr.Type == voidType)
            {
                Log.AddWarningTaggedFormat("Comparison expression is always true (comparing two null literals). ", operationExpression);
                return "true()";
            }

            if (IsXPathAtomic(leftOpExpr.Type) && IsXPathAtomic(rightOpExpr.Type)
                && leftOpExpr.Type != voidType && rightOpExpr.Type != voidType)
            {

                if (operationInfo.OclName == "=")
                {
                    return string.Format("{0} eq {1}", leftOp, rightOp);
                }
                else
                {
                    return string.Format("{0} ne {1}", leftOp, rightOp);
                }
            }

            // one of the operands is void (null)
            if (leftOpExpr.Type == voidType || rightOpExpr.Type == voidType)
            {
                OclExpression comparedOpExpr = leftOpExpr.Type != voidType ? leftOpExpr : rightOpExpr;
                string comparedOpStr = WrapAtomicOperand(comparedOpExpr, operationInfo, 0);
                if (operationInfo.OclName == "=")
                {
                    return string.Format("not(exists({0}))", comparedOpStr);
                }
                else
                {
                    return string.Format("exists({0})", comparedOpStr);
                }
            }

            // comparing collections
            if (arguments[0].Type is SetType || arguments[0].Type is OrderedSetType || arguments[0].Type is BagType)
            {
                return string.Format("oclX:setEqual({0}, {1})", leftOp, rightOp);
            }
            if (arguments[0].Type is SequenceType)
            {
                return string.Format("oclX:seqEqual({0}, {1})", leftOp, rightOp);
            }

            // finally - comparing two non atomic types 
            if (operationInfo.OclName == "=")
            {
                return string.Format("{0} is {1}", leftOp, rightOp);
            }
            else
            {
                return string.Format("not({0} is {1})", leftOp, rightOp);
            }
        }

        public string WrapAtomicOperand(OclExpression operandExpression, OperationInfo ? parentOperationInfo, int number)
        {
            // solving priorities - parentheses for infix operations
            bool needsParentheses = false; 
            if (parentOperationInfo.HasValue && parentOperationInfo.Value.IsXPathInfix && operandExpression is OperationCallExp)
            {
                OperationCallExp childExp = (OperationCallExp) operandExpression;
                OclExpression[] childArguments = new OclExpression[childExp.Arguments.Count + 1];
                childArguments[0] = childExp.Source;
                for (int i = 0; i < childExp.Arguments.Count; i++)
                {
                    childArguments[i + 1] = childExp.Arguments[i];
                }

                OperationInfo? childOperation = LookupOperation(childExp, childArguments);
                if (childOperation.HasValue && childOperation.Value.IsXPathInfix)
                {
                    needsParentheses = parentOperationInfo.Value.Priority < childOperation.Value.Priority;
                }
            }

            if (ExplicitCastAtomicOperands
                && operandExpression is PropertyCallExp
                && IsXPathAtomic(operandExpression.Type)
                && (!operandExpression.Type.IsAmong(PSMBridge.Library.String, PSMBridge.Library.Any)))
            {
                // needsParentheses can be ignored, because parentheses are added by the constructor syntax anyway
                return string.Format("{0}:{1}({{{2}}})", XsdNamespacePrefix, operandExpression.Type.Name, number);
            }
            else if (RequiresDataCall(operandExpression, parentOperationInfo))
            {
                return string.Format("data({{{0}}})", number);
            }
            else
            {
                if (needsParentheses)
                    return string.Format("({{{0}}})", number);
                else
                    return string.Format("{{{0}}}", number);
            }
        }

        public bool IsXPathAtomic(Classifier obj)
        {
            // HACK - WFJT : this should be known from obj.Tag

            if (obj is VoidType)
            {
                return true;
            }

            if (obj is InvalidType)
            {
                return true;
            }

            foreach (var t in PSMSchema.Project.PSMBuiltInTypes)
            {
                if (t.Name == obj.Name)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsXPathNonAtomic(Classifier obj)
        {
            return !IsXPathAtomic(obj);
        }

        public bool RequiresDataCall(OclExpression expression, OperationInfo ? operationInfo)
        {
            return (!(operationInfo != null && operationInfo.Value.CanOmitDataCall )
                && (expression is VariableExp || expression is PropertyCallExp) 
                && IsXPathAtomic(expression.Type));
        }

        public string CreateBasicFormatString(OperationCallExp expression, OclExpression[] arguments)
        //public string ToStringWithArgs(OperationCallExp expression, OclExpression[] arguments, TranslationOptions[] convertedArguments)
        {
            Operation referredOperation = expression.ReferredOperation;
            // TODO HACK - WFJT: the test should verify, that referredOperation is a built in operation 
            if (true)
            {
                OperationInfo? info = LookupOperation(expression, arguments);
                if (info.HasValue)
                    return ToStringWithArgsStandard(expression, referredOperation, info.Value, arguments);
                else
                    throw new OclSubexpressionNotConvertible(string.Format("Operation not found {0} (arity: {1})", referredOperation.Name, arguments.Count() - 1));
            }
            else
            {
                return ToStringWithArgsModel(referredOperation, arguments);
            }
        }

        public OperationInfo? LookupOperation(OperationCallExp operationExpression, OclExpression[] arguments)
        {
            Operation referredOperation = operationExpression.ReferredOperation;
            var nameMatch = this.Where(i => i.OclName == referredOperation.Name);
            if (nameMatch.IsEmpty())
            {
                Log.AddError(string.Format("Operation named {0} not found. ", referredOperation.Name), operationExpression);
                return null;
            }
            int argCountMinus1 = arguments.Count() - 1;
            var arityMatch = nameMatch.Where(i => i.Arity == arguments.Count());
            if (arityMatch.IsEmpty())
            {
                Log.AddError(string.Format("Operation named {0} with {1} {2} not found. ", referredOperation.Name, argCountMinus1, argCountMinus1 == 1 ? "argument" : "arguments"), operationExpression);
                return null;
            }

            var typedArityMatch = arityMatch.Where(i => i.TypeDependent
                    && (i.ArgumentCondition == null || arguments.All(a => i.ArgumentCondition(a.Type)))
                    && (i.ArgumentTypes == null || i.TypesConform(arguments))
                    );
            if (typedArityMatch.Count() > 1)
            {
                Log.AddError(string.Format("Call of operation {0} with {1} {2} is ambiguous. ", referredOperation.Name, argCountMinus1, argCountMinus1 == 1 ? "argument" : "arguments"), operationExpression);
                return typedArityMatch.Single();
            }

            if (typedArityMatch.Count() == 1)
            {
                return typedArityMatch.Single();
            }

            var untypedArityMatch = arityMatch.Where(i => !i.TypeDependent);
            if (untypedArityMatch.Count() > 1)
            {
                Log.AddError(string.Format("Call of operation {0} with {1} {2} is ambiguous. ", referredOperation.Name, argCountMinus1, argCountMinus1 == 1 ? "argument" : "arguments"), operationExpression);
                return typedArityMatch.Single();
            }

            if (untypedArityMatch.IsEmpty())
            {
                Log.AddError(string.Format("Operation named {0} with {1} {2} not found. ", referredOperation.Name, argCountMinus1, argCountMinus1 == 1 ? "argument" : "arguments"), operationExpression);
                return null;
            }

            return untypedArityMatch.Single();
        }

        private string ToStringWithArgsStandard(OperationCallExp operationExpression, Operation referredOperation, OperationInfo info, OclExpression[] arguments)
        {
            if (info.UseCustomTranslateHandler)
            {
                Debug.Assert(info.CustomTranslateHandler != null);
                return info.CustomTranslateHandler(operationExpression, info, arguments);
            }
            else if (info.UseCustomXPathSyntaxFormatString)
            {
                Debug.Assert(!string.IsNullOrEmpty(info.CustomXPathSyntaxFormatString));
                return string.Format(info.CustomXPathSyntaxFormatString);
            }
            else
            {
                if (info.IsXPathInfix)
                {
                    Debug.Assert(arguments.Length == 2 && info.Arity == 2);
                    string leftOp = WrapAtomicOperand(arguments[0], info, 0);
                    string rightOp = WrapAtomicOperand(arguments[1], info, 1);
                    return string.Format("{0} {1} {2}", leftOp, info.XPathName, rightOp);
                }
                else if (info.IsXPathPrefix)
                {
                    Debug.Assert(arguments.Length == 1 && info.Arity == 1);
                    string op = WrapAtomicOperand(arguments[0], info, 0);
                    return string.Format("{0} {1}", info.XPathName, op);
                }
                else
                {
                    return string.Format("{0}({1})", info.XPathName, arguments.ConcatWithSeparator(a => WrapAtomicOperand(a, info, Array.IndexOf(arguments, a)), ", "));
                }
            }
        }

        private string ToStringWithArgsModel(Operation referredOperation, OclExpression[] args)
        {
            Debug.Assert(referredOperation.Tag != null);
            throw new NotImplementedException();
        }
    }

    public struct OperationInfo
    {
        /// <summary>
        /// used by infix operations
        /// </summary>
        public int Priority { get; set; }

        public string OclName { get; set; }

        public string XPathName { get; set; }

        public uint Arity { get; set; }

        public bool UseCustomXPathSyntaxFormatString { get; set; }

        private string customXPathSyntaxFormatString;
        public string CustomXPathSyntaxFormatString
        {
            get { return customXPathSyntaxFormatString; }
            set { customXPathSyntaxFormatString = value; if (!string.IsNullOrEmpty(value)) UseCustomXPathSyntaxFormatString = true; }
        }

        public delegate string TranslateHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments);

        public bool UseCustomTranslateHandler { get; set; }

        private TranslateHandler customTranslateHandler;
        public TranslateHandler CustomTranslateHandler
        {
            get { return customTranslateHandler; }
            set { customTranslateHandler = value; if (value != null) UseCustomTranslateHandler = true; }
        }

        private bool isOclInfix;
        public bool IsOclInfix
        {
            get { return isOclInfix; }
            set
            {
                isOclInfix = value;
                Arity = 2;
            }
        }

        private bool isOclPrefix;
        public bool IsOclPrefix
        {
            get { return isOclPrefix; }
            set
            {
                isOclPrefix = value;
                Arity = 1;
            }
        }

        public bool IsXPathInfix { get; set; }

        public bool IsXPathPrefix { get; set; }

        public bool TypeDependent { get; set; }

        public bool CanOmitDataCall { get; set; }

        public Classifier[] ArgumentTypes { get; set; }

        public Predicate<Classifier> ArgumentCondition { get; set; }

        public bool TypesConform(OclExpression[] arguments)
        {
            Debug.Assert(ArgumentTypes != null);
            if (arguments.Length != ArgumentTypes.Length)
            {
                return false;
            }
            for (int index = 0; index < arguments.Length; index++)
            {
                OclExpression argumentExpr = arguments[index];
                if (!argumentExpr.Type.ConformsTo(ArgumentTypes[index]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}