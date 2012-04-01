using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Compiler;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Model.OCL {
    public class CompilerResult {
        public Constraints Constraints {
            private set;
            get;
        }

        public ErrorCollection Errors {
            private set;
            get;
        }

        public TypesTable.Library Library {
            private set;
            get;
        }

        public IBridgeToOCL Bridge { get; set; }

        public CompilerResult(Constraints con, ErrorCollection errColl, Library lib, IBridgeToOCL bridge) {
            this.Constraints = con;
            this.Errors = errColl;
            this.Library = lib;
            this.Bridge = bridge;
        }

        public void CompileExpressionsInMessages()
        {
            Regex inlinedExpressionPattern = new Regex(@"\{([^{}]*?)\}");
            Compiler.Compiler compiler = new Compiler.Compiler();
            foreach (ClassifierConstraintBlock constraint in Constraints.ClassifierConstraintBlocks)
            {
                foreach (InvariantWithMessage invariant in constraint.Invariants)
                {
                    if (invariant.Message != null && invariant.MessageIsString)
                    {
                        string message = ((StringLiteralExp) invariant.Message).Value;
                        MatchCollection matchCollection = inlinedExpressionPattern.Matches(message);
                        foreach (Match match in matchCollection)
                        {
                            SubExpressionInfo subInfo = new SubExpressionInfo();
                            subInfo.Parsed = false; 
                            try
                            {
                                VariableDeclaration varSelf = new VariableDeclaration(constraint.Self.Name, constraint.Context, null);
                                Environment nsEnv = new NamespaceEnvironment(this.Library.RootNamespace);
                                Environment classifierEnv = nsEnv.CreateFromClassifier(constraint.Context, varSelf);
                                Environment selfEnv = classifierEnv.AddElement(constraint.Self.Name, constraint.Context, varSelf, true);
                                if (match.Groups.Count >= 1)
                                {
                                    subInfo.MessageStartIndex = match.Groups[1].Index;
                                    subInfo.MessageEndIndex = subInfo.MessageStartIndex + match.Groups[1].Length;
                                    string subExpression = match.Groups[1].Value;
                                    subInfo.PartAsString = subExpression;
                                    ExpressionCompilerResult r =
                                        compiler.CompileStandAloneExpression(subExpression, this.Library.TypeTable, selfEnv);
                                    subInfo.SubExpression = r.Expression;
                                    subInfo.SubExpression.IsMessageInlinedSubexpression = true;
                                    if (!r.Errors.HasError)
                                        subInfo.Parsed = true; 
                                }
                            }
                            catch
                            {

                            }
                            invariant.MessageSubExpressions.Add(subInfo);
                        }

                        List<SubExpressionInfo> tmpList = invariant.MessageSubExpressions.ToList();
                        int writeIndex = 0; 
                        for (int readIndex = 0; readIndex <= tmpList.Count; readIndex++, writeIndex++)
                        {
                            int partStartIndex;
                            int partEndIndex;
                            if (readIndex == 0 && tmpList[readIndex].MessageStartIndex == 0)
                            {
                                continue;
                            }

                            bool isFirst = readIndex == 0;
                            bool isLast = readIndex == tmpList.Count; 

                            if (isFirst)
                            {
                                partStartIndex = 0;
                            }
                            else
                            {
                                partStartIndex = tmpList[readIndex - 1].MessageEndIndex + 1;
                            }
                            
                            if (!isLast)
                            {
                                partEndIndex = tmpList[readIndex].MessageStartIndex - 1;
                            }
                            else
                            {
                                partEndIndex = message.Length; 
                            }

                            if (partEndIndex - partStartIndex > 1)
                            {
                                string messagePart = message.Substring(partStartIndex, partEndIndex - partStartIndex);
                                SubExpressionInfo partInfo = new SubExpressionInfo();
                                partInfo.MessageStartIndex = partStartIndex;
                                partInfo.MessageEndIndex = partEndIndex;
                                partInfo.PartAsString = messagePart; 
                                invariant.MessageSubExpressions.Insert(writeIndex, partInfo);
                                writeIndex++;
                            }
                        }
                    }
                }
            }
        }
    }

    public class ExpressionCompilerResult {
        public OclExpression Expression {
            private set;
            get;
        }

        public ErrorCollection Errors {
            private set;
            get;
        }

        public TypesTable.Library Library {
            private set;
            get;
        }

        public ExpressionCompilerResult(OclExpression expression, ErrorCollection errColl, Library lib) {
            this.Expression = expression;
            this.Errors = errColl;
            this.Library = lib;
        }
    }
}
