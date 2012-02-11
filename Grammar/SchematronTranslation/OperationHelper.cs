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
        public string XsdNamespacePrefix
        {
            get { return xsdNamespacePrefix; }
            set { xsdNamespacePrefix = value; }
        }

        public void InitStandard()
        {
            Add(new OperationInfo { Priority = 1, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "*", XPathName = "*", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 1, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "/", XPathName = "div", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 2, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "+", XPathName = "+", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 2, IsXPathInfix = true, IsOclInfix = true, OclName = "-", XPathName = "-", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "<", XPathName = "lt", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = ">", XPathName = "gt", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "<=", XPathName = "le", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 3, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = ">=", XPathName = "ge", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "=", XPathName = "eq", TypeDependent = true, ArgumentCondition = IsXPathAtomic, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, OclName = "=", XPathName = "is", TypeDependent = true, ArgumentCondition = IsXPathNode, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "<>", XPathName = "ne", TypeDependent = true, ArgumentCondition = IsXPathAtomic, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 4, IsXPathInfix = true, IsOclInfix = true, OclName = "<>", XPathName = "is not", TypeDependent = true, ArgumentCondition = IsXPathNode, CustomTranslateHandler = EqualityTestHandler });
            Add(new OperationInfo { Priority = 5, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "and", XPathName = "and", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 6, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "or", XPathName = "or", CustomTranslateHandler = InfixAtomicHandler });
            Add(new OperationInfo { Priority = 7, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "xor", XPathName = "oclBoolean:xor", CustomXPathSyntaxFormatString = "oclBoolean:xor({0}, {1})", CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = 8, IsXPathInfix = true, IsOclInfix = true, CanOmitDataCall = true, OclName = "implies", XPathName = "#NDEF", CustomXPathSyntaxFormatString = "if ({0}) then {1} else true()", CustomTranslateHandler = FunctionAtomicHandler });


            Add(new OperationInfo { Priority = -1, IsXPathPrefix = true, IsOclPrefix = true, CanOmitDataCall = true, OclName = "-", XPathName = "-", CustomTranslateHandler = PrefixAtomicHandler });
            Add(new OperationInfo { Priority = -1, IsXPathPrefix = true, IsOclPrefix = true, CanOmitDataCall = true, OclName = "not", XPathName = "#NDEF", CustomXPathSyntaxFormatString = "not({0})", CustomTranslateHandler = FunctionAtomicHandler });

            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "after", XPathName = "oclDate:after", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
            Add(new OperationInfo { Priority = -1, CanOmitDataCall = true, OclName = "before", XPathName = "oclDate:before", Arity = 2, CustomTranslateHandler = FunctionAtomicHandler });
        }

        private string PrefixAtomicHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments, ConvertedExp[] convertedArguments)
        {
            string op = WrapAtomicOperand(arguments[0], operationInfo, convertedArguments[0]);
            return string.Format("{0} {1}", operationInfo.XPathName, op);
        }

        private string InfixAtomicHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments, ConvertedExp[] convertedArguments)
        {
            string leftOp = WrapAtomicOperand(arguments[0], operationInfo, convertedArguments[0]);
            string rightOp = WrapAtomicOperand(arguments[1], operationInfo, convertedArguments[1]);
            return string.Format("{1} {0} {2}", operationInfo.XPathName, leftOp, rightOp);
        }

        private string FunctionAtomicHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments, ConvertedExp[] convertedArguments)
        {
            IEnumerable<string> wrappedSequence = from ind in Enumerable.Range(0, arguments.Count()) select WrapAtomicOperand(arguments[ind], operationInfo, convertedArguments[ind]);

            IEnumerable<string> withoutDataCall = convertedArguments.Select(a => a.RawString);
            IEnumerable<string> withDataCall = convertedArguments.Select(a => a.GetString());
            IEnumerable<string> stringifiedArguments = !operationInfo.CanOmitDataCall ? withDataCall : withoutDataCall;


            if (!string.IsNullOrEmpty(operationInfo.CustomXPathSyntaxFormatString))
            {
                if (operationInfo.TypeDependent)
                {
                    return string.Format(operationInfo.CustomXPathSyntaxFormatString, wrappedSequence.ToArray());
                }
                else
                {
                    return string.Format(operationInfo.CustomXPathSyntaxFormatString, stringifiedArguments.ToArray());
                }
            }
            else
            {
                if (operationInfo.TypeDependent)
                {
                    return string.Format("{0}({1})", operationInfo.XPathName, wrappedSequence.ConcatWithSeparator(", "));
                }
                else
                {
                    return string.Format("{0}({1})", operationInfo.XPathName, stringifiedArguments.ConcatWithSeparator(", "));
                }
            }
        }

        private string EqualityTestHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments, ConvertedExp[] convertedArguments)
        {
            Debug.Assert(arguments.Count() == 2 && convertedArguments.Count() == 2);
            OclExpression leftOpExpr = arguments[0];
            OclExpression rightOpExpr = arguments[1];
            string leftOp = WrapAtomicOperand(leftOpExpr, operationInfo, convertedArguments[0]);
            string rightOp = WrapAtomicOperand(rightOpExpr, operationInfo, convertedArguments[1]);

            Classifier voidType = PSMBridge.TypesTable.Library.Void;
            Classifier invalidType = PSMBridge.TypesTable.Library.Void;

            if (leftOpExpr.Type == voidType && rightOpExpr.Type == voidType)
            {
                Log.AddWarningTaggedFormat("Comparison expression is always true. ", operationExpression);
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
                ConvertedExp comparedOp = leftOpExpr.Type != voidType ? convertedArguments[0] : convertedArguments[1];
                string comparedOpStr = WrapAtomicOperand(operationExpression, operationInfo, comparedOp);
                if (operationInfo.OclName == "=")
                {
                    return string.Format("not(exists({0}))", comparedOpStr);
                }
                else
                {
                    return string.Format("exists({0})", comparedOpStr);
                }
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

        private string WrapAtomicOperand(OclExpression operandExpression, OperationInfo parentOperationInfo, ConvertedExp stringifiedOperand)
        {
            // solving priorities - parentheses for infix operations
            bool needsParentheses = false; 
            if (parentOperationInfo.IsXPathInfix && operandExpression is OperationCallExp)
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
                    needsParentheses = parentOperationInfo.Priority < childOperation.Value.Priority;
                }
            }

            if (ExplicitCastAtomicOperands
                && operandExpression is PropertyCallExp
                && IsXPathAtomic(operandExpression.Type)
                && (!operandExpression.Type.IsAmong(PSMBridge.Library.String, PSMBridge.Library.Any)))
            {
                // needsParentheses can be ignored, because parentheses are added by the constructor syntax anyway
                return string.Format("{0}:{1}({2})", XsdNamespacePrefix, operandExpression.Type.Name, stringifiedOperand.RawString);
            }
            else if (stringifiedOperand.AddDataCall && !parentOperationInfo.CanOmitDataCall)
            {
                return string.Format("data({0})", stringifiedOperand.RawString);
            }
            else
            {
                if (needsParentheses)
                    return string.Format("({0})", stringifiedOperand.RawString);
                else
                    return stringifiedOperand.RawString;
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

        private bool IsXPathNode(Classifier obj)
        {
            return !IsXPathAtomic(obj);
        }

        public string ToStringWithArgs(OperationCallExp expression, OclExpression[] arguments, ConvertedExp[] convertedArguments)
        {
            Operation referredOperation = expression.ReferredOperation;
            // HACK - WFJT: the test should verify, that referredOperation is a built in operation 
            if (true)
            {
                OperationInfo? info = LookupOperation(expression, arguments);
                if (info.HasValue)
                    return ToStringWithArgsStandard(expression, referredOperation, info.Value, arguments, convertedArguments);
                else
                    throw new OclSubexpressionNotConvertible(string.Format("Operation not found {0} (arity: {1})", referredOperation.Name, convertedArguments.Length));
            }
            else
            {
                return ToStringWithArgsModel(referredOperation, convertedArguments);
            }
        }

        private OperationInfo? LookupOperation(OperationCallExp operationExpression, OclExpression[] arguments)
        {
            Operation referredOperation = operationExpression.ReferredOperation;
            var nameMatch = this.Where(i => i.OclName == referredOperation.Name);
            if (nameMatch.IsEmpty())
            {
                Log.AddError(string.Format("Operation named {0} not found. ", referredOperation.Name), operationExpression);
                return null;
            }
            var arityMatch = nameMatch.Where(i => i.Arity == arguments.Count());
            if (arityMatch.IsEmpty())
            {
                Log.AddError(string.Format("Operation named {0} with {1} {2} not found. ", referredOperation.Name, arguments.Count(), arguments.Count() == 1 ? "argument" : "arguments"), operationExpression);
                return null;
            }

            var typedArityMatch = arityMatch.Where(i => i.TypeDependent
                    && (i.ArgumentCondition == null || arguments.All(a => i.ArgumentCondition(a.Type)))
                    && (i.ArgumentTypes == null || i.TypesConform(arguments))
                    );
            if (typedArityMatch.Count() > 1)
            {
                Log.AddError(string.Format("Call of operation {0} with {1} {2} is ambiguous. ", referredOperation.Name, arguments.Count(), arguments.Count() == 1 ? "argument" : "arguments"), operationExpression);
                return typedArityMatch.Single();
            }

            if (typedArityMatch.Count() == 1)
            {
                return typedArityMatch.Single();
            }

            var untypedArityMatch = arityMatch.Where(i => !i.TypeDependent);
            if (untypedArityMatch.Count() > 1)
            {
                Log.AddError(string.Format("Call of operation {0} with {1} {2} is ambiguous. ", referredOperation.Name, arguments.Count(), arguments.Count() == 1 ? "argument" : "arguments"), operationExpression);
                return typedArityMatch.Single();
            }

            if (untypedArityMatch.IsEmpty())
            {
                Log.AddError(string.Format("Operation named {0} with {1} {2} not found. ", referredOperation.Name, arguments.Count(), arguments.Count() == 1 ? "argument" : "arguments"), operationExpression);
                return null;
            }

            return untypedArityMatch.Single();
        }

        private string ToStringWithArgsStandard(OperationCallExp operationExpression, Operation referredOperation, OperationInfo info, OclExpression[] arguments, ConvertedExp[] convertedArguments)
        {
            if (info.UseCustomTranslateHandler)
            {
                Debug.Assert(info.CustomTranslateHandler != null);
                return info.CustomTranslateHandler(operationExpression, info, arguments, convertedArguments);
            }
            else if (info.UseCustomXPathSyntaxFormatString)
            {
                Debug.Assert(!string.IsNullOrEmpty(info.CustomXPathSyntaxFormatString));
                return string.Format(info.CustomXPathSyntaxFormatString, convertedArguments);
            }
            else
            {
                if (info.IsXPathInfix)
                {
                    Debug.Assert(convertedArguments.Length == 2 && info.Arity == 2);
                    return string.Format("{0} {2} {1}", convertedArguments[0], convertedArguments[1], info.XPathName);
                }
                else if (info.IsXPathPrefix)
                {
                    Debug.Assert(convertedArguments.Length == 1 && info.Arity == 1);
                    return string.Format("{1} {0}", convertedArguments[0], info.XPathName);
                }
                else
                {
                    return string.Format("{0}({1})", info.XPathName, convertedArguments.ConcatWithSeparator(", "));
                }
            }
        }

        private string ToStringWithArgsModel(Operation referredOperation, ConvertedExp[] args)
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

        public delegate string TranslateHandler(OperationCallExp operationExpression, OperationInfo operationInfo, OclExpression[] arguments, ConvertedExp[] convertedArguments);

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