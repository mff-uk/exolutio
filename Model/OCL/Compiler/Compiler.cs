//#define timeDebug

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.Model.OCL.Types;


namespace Exolutio.Model.OCL.Compiler {
    public class Compiler {

        public CompilerResult CompileScript(string text, Bridge.IBridgeToOCL bridge) 
        {
            TypesTable.TypesTable tt = bridge.TypesTable;
            ErrorCollection errColl = new ErrorCollection();

            // lexer
            ANTLRStringStream stringStream = new ANTLRStringStream(text);
            OCLSyntaxLexer lexer = new OCLSyntaxLexer(stringStream, errColl);

            // syntax
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            OCLSyntaxParser parser = new OCLSyntaxParser(tokenStream, errColl);
            var output = parser.contextDeclarationList();

            // semantic
            Antlr.Runtime.Tree.CommonTreeNodeStream treeStream = new CommonTreeNodeStream(output.Tree);
            OCLAst semantic = new OCLAst(treeStream, errColl);
            semantic.TypesTable = tt;
            semantic.EnvironmentStack.Push(new NamespaceEnvironment(tt.Library.RootNamespace));
            AST.Constraints constraints;
            AST.PropertyInitializations initializations;
            // try {
            OCLAst.contextDeclarationList_return contextDeclarationListReturn = semantic.contextDeclarationList();
            constraints = contextDeclarationListReturn.Constraints;
            initializations = contextDeclarationListReturn.PropertyInitializations;
            // }
            //catch{
            //     constraints = new AST.Constraints();
            //    errColl.AddError(new ErrorItem("Fatal error."));
            // }

            return new CompilerResult(constraints, initializations, errColl, tt.Library, bridge);
        }

        public CompilerResult CompileEvolutionScript(string text, Bridge.IBridgeToOCL bridge)
        {
            TypesTable.TypesTable tt = bridge.TypesTable;
            ErrorCollection errColl = new ErrorCollection();

            // lexer
            ANTLRStringStream stringStream = new ANTLRStringStream(text);
            OCLSyntaxLexer lexer = new OCLSyntaxLexer(stringStream, errColl);

            // syntax
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            OCLSyntaxParser parser = new OCLSyntaxParser(tokenStream, errColl);
            var output = parser.evolutionDeclarationList();

            // semantic
            Antlr.Runtime.Tree.CommonTreeNodeStream treeStream = new CommonTreeNodeStream(output.Tree);
            OCLAst semantic = new OCLAst(treeStream, errColl);
            semantic.TypesTable = tt;
            semantic.Bridge = bridge; 
            semantic.EnvironmentStack.Push(new NamespaceEnvironment(tt.Library.RootNamespace));
            AST.Constraints constraints;
            AST.PropertyInitializations initializations;
            // try {
            OCLAst.evolutionDeclarationList_return evolutionDeclarationListReturn = semantic.evolutionDeclarationList();
            constraints = evolutionDeclarationListReturn.Constraints;
            initializations = evolutionDeclarationListReturn.PropertyInitializations;
            // }
            //catch{
            //     constraints = new AST.Constraints();
            //    errColl.AddError(new ErrorItem("Fatal error."));
            // }

            return new CompilerResult(constraints, initializations, errColl, tt.Library, bridge);
        }

        /// <summary>
        /// Compile stand-alone expression.
        /// </summary>
        /// <example>
        /// Example shows how to parse stand-alone invariant.
        /// <code>
        /// <![CDATA[
        /// // ...
        /// Environment nsEnv = new NamespaceEnvironment(tt.Library.RootNamespace);
        /// VariableDeclaration varSelf = new VariableDeclaration(selfName, contextClassifier, null);
        /// Environment classifierEnv = nsEnv.CreateFromClassifier(contextClassifier, varSelf);
        /// Environment selfEnv = Environment.AddElement(selfName, contextClassifier, varSelf, true);
        /// 
        /// Compiler.Compiler compiler = new Compiler.Compiler();
        /// var result = compiler.CompileStandAloneExpression(expressionText, tt, selfEnv);
        /// ]]>
        /// </code>
        /// </example>
        public ExpressionCompilerResult CompileStandAloneExpression(string text, TypesTable.TypesTable tt, Environment env) {
            ErrorCollection errColl = new ErrorCollection();

            // lexer
            ANTLRStringStream stringStream = new ANTLRStringStream(text);
            OCLSyntaxLexer lexer = new OCLSyntaxLexer(stringStream, errColl);

            // syntax
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            OCLSyntaxParser parser = new OCLSyntaxParser(tokenStream, errColl);
            var output = parser.oclExpression();

            // semantic
            Antlr.Runtime.Tree.CommonTreeNodeStream treeStream = new CommonTreeNodeStream(output.Tree);
            OCLAst semantic = new OCLAst(treeStream, errColl);
            semantic.TypesTable = tt;
            semantic.EnvironmentStack.Push(env);
            AST.OclExpression expression;
            // try {
            expression = semantic.oclExpression();
            // }
            //catch{
            //     constraints = new AST.Constraints();
            //    errColl.AddError(new ErrorItem("Fatal error."));
            // }

            return new ExpressionCompilerResult(expression, errColl, tt.Library);

        }
    }
}
