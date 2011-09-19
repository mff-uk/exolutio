using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.Model.OCL.Types;


namespace Exolutio.Model.OCL.Compiler {
    public class Compiler {
        public OCLParser Parser {
            get;
            protected set;
        }
        public OCLLexer Lexer {
            get;
            protected set;
        }

        public TypesTable.TypesTable TypesTable {
            set {
                Parser.TypesTable = value;
            }

            get {
                return Parser.TypesTable;
            }
        }

        public ClassifierConstraint TestCompiler(string s,TypesTable.TypesTable tt, Environment env) {
            ANTLRStringStream stringStream = new ANTLRStringStream(s);
            OCLLexer lexer = new OCLLexer(stringStream);

            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            OCLParser parser = new OCLParser(tokenStream);

            parser.TypesTable = tt;
            parser.EnvironmentStack.Push(env);
           
            return parser.contextDeclaratio();
        }
    }
}
