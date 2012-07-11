using System;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.PSM;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.Utils
{
    public class SubexpressionCollector: IAstVisitor
    {
        private readonly List<OclExpression> expressions = new List<OclExpression>();
     
        public List<OclExpression> Expressions { get { return expressions; } }

        private readonly List<VariableDeclaration> referredVariables = new List<VariableDeclaration>();

        public List<VariableDeclaration> ReferredVariables { get { return referredVariables; } }

        public void Clear()
        {
            Expressions.Clear();
            ReferredVariables.Clear();
        }

        public VariableNamer VariableNamer { get; set; }

        public void Visit(BooleanLiteralExp node) { }

        public void Visit(CollectionLiteralExp node)
        {
            foreach (CollectionLiteralPart collectionLiteralPart in node.Parts)
            {
                CollectionItem item = collectionLiteralPart as CollectionItem;
                if (item != null)
                {
                    Expressions.Add(item.Item);
                    item.Item.Accept(this);
                }

                CollectionRange range = collectionLiteralPart as CollectionRange;
                if (range != null)
                {
                    Expressions.Add(range.First);
                    Expressions.Add(range.Last);
                    range.First.Accept(this);
                    range.Last.Accept(this);
                }
            }
        }

        public void Visit(EnumLiteralExp node) { }

        public void Visit(ErrorExp node) { }

        public void Visit(IfExp node)
        {
            Expressions.Add(node.Condition);
            Expressions.Add(node.ThenExpression);
            Expressions.Add(node.ElseExpression);
            node.Condition.Accept(this);
            node.ThenExpression.Accept(this);
            node.ElseExpression.Accept(this);
        }

        public void Visit(IntegerLiteralExp node) { }

        public void Visit(InvalidLiteralExp node) { }

        public void Visit(IterateExp node)
        {
            Expressions.Add(node.Result.Value);
            Expressions.Add(node.Body);
            node.Body.Accept(this);
            node.Result.Value.Accept(this);
        }

        public void Visit(IteratorExp node)
        {
            Expressions.Add(node.Body);
            node.Body.Accept(this);
        }

        public void Visit(LetExp node)
        {
            Expressions.Add(node.Variable.Value);
            Expressions.Add(node.InExpression);
            node.Variable.Value.Accept(this);
            node.InExpression.Accept(this);
        }

        public void Visit(NullLiteralExp node) { }

        public void Visit(OperationCallExp node)
        {
            Expressions.Add(node.Source);
            node.Source.Accept(this);
            foreach (OclExpression argExp in node.Arguments)
            {
                Expressions.Add(argExp);
                argExp.Accept(this);
            }
        }

        public void Visit(PropertyCallExp node)
        {
            PSMPath path = PSMPathBuilder.BuildPSMPath(node, null, VariableNamer,
                new BuildPSMPathParams(TupleLiteralToXPathCallback, ClassLiteralToXPathCallback, GenericExpressionToXPathCallback, GetRelativeXPathEvolutionCallback));
            if (path.StartingVariableExp != null)
            {
                ReferredVariables.AddIfNotContained(path.StartingVariableExp.referredVariable);
            }
        }

        private string GetRelativeXPathEvolutionCallback(PSMComponent node)
        {
            return string.Empty;
        }

        private string GenericExpressionToXPathCallback(OclExpression oclexpression, List<OclExpression> oclexpressions)
        {
            Expressions.Add(oclexpression);
            oclexpression.Accept(this);
            return string.Empty;
        }

        private string TupleLiteralToXPathCallback(TupleLiteralExp tupleLiteral, List<OclExpression> subExpressions)
        {
            return string.Empty;
        }

        private string ClassLiteralToXPathCallback(ClassLiteralExp tupleLiteral, List<OclExpression> subExpressions)
        {
            return string.Empty;
        }

        public void Visit(VariableExp node)
        {
            referredVariables.AddIfNotContained(node.referredVariable);
        }

        public void Visit(RealLiteralExp node) { }

        public void Visit(StringLiteralExp node) { }

        public void Visit(TupleLiteralExp node)
        {
            foreach (KeyValuePair<string, TupleLiteralPart> kvp in node.Parts)
            {
                Expressions.Add(kvp.Value.Value);
                kvp.Value.Value.Accept(this);
            }
        }

        public void Visit(ClassLiteralExp node)
        {
            foreach (KeyValuePair<string, TupleLiteralPart> kvp in node.Parts)
            {
                Expressions.Add(kvp.Value.Value);
                kvp.Value.Value.Accept(this);
            }
        }

        public void Visit(TypeExp node) { }

        public void Visit(UnlimitedNaturalLiteralExp node) { }
    }
}