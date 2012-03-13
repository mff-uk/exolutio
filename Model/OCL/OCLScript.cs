//#define timeDebug

using System;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Model.Serialization;
using Exolutio.Model.Versioning;

using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Exolutio.Model.OCL.Compiler;
using Exolutio.Model.OCL.Types;


namespace Exolutio.Model.OCL {
    public class OCLScript : Component {
        #region constructors

        public OCLScript(Project p)
            : base(p) {

        }

        public OCLScript(Project p, Guid g)
            : base(p, g) {

        }


        /// <summary>
        /// Creates an ocl script and adds it to <paramref name="schema"/>.<see cref="Schema.OCLScripts"/> collection.
        /// </summary>
        public OCLScript(Project p, Guid g, Schema schema)
            : base(p, g) {
            this.Schema = schema;
            schema.OCLScripts.Add(this);
        }


        /// <summary>
        /// Creates an ocl script and adds it to <paramref name="schema"/>.<see cref="Schema.OCLScripts"/> collection.
        /// </summary>
        public OCLScript(Schema schema)
            : this(schema.Project, Guid.NewGuid(), schema)
        {
            
        }

        #endregion

        public string Contents { get; set; }

        public CompilerResult CompileToAst() {
#if timeDebug
            System.Diagnostics.Stopwatch translateTime = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch compileTime = new System.Diagnostics.Stopwatch();
#endif

            Bridge.BridgeFactory factor = new Bridge.BridgeFactory();
#if timeDebug
            translateTime.Start();
#endif
            Bridge.IBridgeToOCL bridge = factor.Create(Schema);
            TypesTable.TypesTable tt = bridge.TypesTable;
#if timeDebug
            translateTime.Stop();
            compileTime.Start();
#endif

            ErrorCollection errColl = new ErrorCollection();

            ANTLRStringStream stringStream = new ANTLRStringStream(Contents);
            OCLSyntaxLexer lexer = new OCLSyntaxLexer(stringStream, errColl);
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            OCLSyntaxParser parser = new OCLSyntaxParser(tokenStream, errColl);
            var output = parser.contextDeclarationList();
            Antlr.Runtime.Tree.CommonTreeNodeStream treeStream = new CommonTreeNodeStream(output.Tree);
            OCLAst semantic = new OCLAst(treeStream, errColl);
            semantic.TypesTable = tt;
            semantic.EnvironmentStack.Push(new NamespaceEnvironment(tt.Library.RootNamespace));
            AST.Constraints constraints;
           // try {
                constraints = semantic.contextDeclarationList();
           // }
            //catch{
           //     constraints = new AST.Constraints();
            //    errColl.AddError(new ErrorItem("Fatal error."));
           // }
#if timeDebug
            compileTime.Stop();
            errColl.AddError(new ErrorItem(String.Format("Translate time:{0} Compile time:{1}",translateTime.ElapsedMilliseconds,compileTime.ElapsedMilliseconds)));
#endif
            
            return new CompilerResult(constraints, errColl, tt.Library, bridge);
        }

        

        #region Implementation of IExolutioSerializable

        public override void Serialize(System.Xml.Linq.XElement parentNode, Serialization.SerializationContext context) {
            base.Serialize(parentNode, context);
            this.SerializeSimpleValueToCDATA("Contents", Contents, parentNode, context);
        }

        public override void Deserialize(System.Xml.Linq.XElement parentNode, Serialization.SerializationContext context) {
            base.Deserialize(parentNode, context);
            this.Contents = this.DeserializeSimpleValueFromCDATA("Contents", parentNode, context);
        }

        #endregion

        #region Implementation of IExolutioCloneable

        public override IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies) {
            return new OCLScript(projectVersion.Project, createdCopies.SuggestGuid(this));
        }

        public override void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion,
                                      ElementCopiesMap createdCopies) {
            base.FillCopy(copyComponent, projectVersion, createdCopies);

            OCLScript copyOCLScript = (OCLScript)copyComponent;
            copyOCLScript.Contents = this.Contents;
        }

        #endregion

        public static OCLScript CreateInstance(Project project) {
            return new OCLScript(project, Guid.Empty);
        }


        #region obsolete
        //public string Compile(string text) {
        //    //  Compile2("");

        //    TypesTable.TypesTable tt = new TypesTable.TypesTable();
        //    TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
        //    sLC.CreateStandardLibrary(tt);
        //    TranslateModel(tt);

        //    ANTLRStringStream stringStream = new ANTLRStringStream(text);
        //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
        //        System.IO.TextWriter tw = new System.IO.StreamWriter(ms);

        //        OCLSyntaxLexer lexer = new OCLSyntaxLexer(stringStream);
        //        lexer.TraceDestination = tw;

        //        CommonTokenStream tokenStream = new CommonTokenStream(lexer);
        //        OCLSyntaxParser parser = new OCLSyntaxParser(tokenStream);


        //        parser.TraceDestination = tw;
        //        var output = parser.contextDeclarationList();

        //        Antlr.Runtime.Tree.CommonTreeNodeStream treeStream = new CommonTreeNodeStream(output.Tree);

        //        OCLAst semantic = new OCLAst(treeStream);
        //        semantic.TypesTable = tt;
        //        semantic.EnvironmentStack.Push(new NamespaceEnvironment(tt.Library.RootNamespace));

        //        AST.Constraints constraints = semantic.contextDeclarationList();


        //        tw.Flush();

        //        ms.Seek(0, System.IO.SeekOrigin.Begin);

        //        System.IO.StreamReader sr = new System.IO.StreamReader(ms);


        //        return sr.ReadToEnd();
        //    }


        //    // return parser.Failed;
        //}

        //public string Compile2(string text) {
        //    TypesTable.TypesTable tt = new TypesTable.TypesTable();
        //    TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
        //    sLC.CreateStandardLibrary(tt);
        //    TranslateModel(tt);
        //    Compiler.Compiler compiler = new Compiler.Compiler();
        //    return compiler.TestCompiler2(text, tt, new NamespaceEnvironment(tt.Library.RootNamespace));
        //    // return parser.Failed;
        //}
        #endregion

    }
}