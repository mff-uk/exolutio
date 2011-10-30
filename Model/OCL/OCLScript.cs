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

        public OCLScript(Project p, Guid g, Schema schema)
            : base(p, g) {
            this.Schema = schema;
            schema.OCLScripts.Add(this);
        }

        #endregion

        public string Contents { get; set; }

        public string Compile(string text) {
          //  Compile2("");

            TypesTable.TypesTable tt = new TypesTable.TypesTable();
            TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
            sLC.CreateStandardLibrary(tt);
            TranslateModel(tt);

            ANTLRStringStream stringStream = new ANTLRStringStream(text);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                System.IO.TextWriter tw = new System.IO.StreamWriter(ms);

                OCLSyntaxLexer lexer = new OCLSyntaxLexer(stringStream);
                lexer.TraceDestination = tw;

                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                OCLSyntaxParser parser = new OCLSyntaxParser(tokenStream);


                parser.TraceDestination = tw;
                var output = parser.contextDeclarationList();

                Antlr.Runtime.Tree.CommonTreeNodeStream treeStream = new CommonTreeNodeStream(output.Tree);

                OCLAst semantic = new OCLAst(treeStream);
                semantic.TypesTable = tt;
                semantic.EnvironmentStack.Push(new NamespaceEnvironment(tt.Library.RootNamespace));

                AST.Constraints constraints = semantic.contextDeclarationList();


                tw.Flush();

                ms.Seek(0, System.IO.SeekOrigin.Begin);

                System.IO.StreamReader sr = new System.IO.StreamReader(ms);


                return sr.ReadToEnd();
            }

            
            // return parser.Failed;
        }

        public string Compile2(string text) {
            TypesTable.TypesTable tt = new TypesTable.TypesTable();
            TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
            sLC.CreateStandardLibrary(tt);
            TranslateModel(tt);
            Compiler.Compiler compiler = new Compiler.Compiler();
            return compiler.TestCompiler2(text, tt, new NamespaceEnvironment(tt.Library.RootNamespace));
            // return parser.Failed;
        }

        public CompilerResult CompileToAst() {
            TypesTable.TypesTable tt = new TypesTable.TypesTable();
            TypesTable.StandardLibraryCreator sLC = new TypesTable.StandardLibraryCreator();
            sLC.CreateStandardLibrary(tt);
            TranslateModel(tt);

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
            AST.Constraints constraints = semantic.contextDeclarationList();

            return new CompilerResult(constraints, errColl, tt.Library);
        }

        public void TranslateModel(TypesTable.TypesTable tt) {
            // Docansne reseni s Date
            Class date = new Class(tt, "Date");
            date.Operations.Add(new Operation("after", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("before", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("equals", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("<=", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));

            tt.Library.RootNamespace.NestedClassifier.Add(date);
            tt.RegisterType(date);

            Class matchesStatus = new Class(tt, "MatchStatus");
            tt.Library.RootNamespace.NestedClassifier.Add(matchesStatus);
            tt.RegisterType(matchesStatus);

            if (this.Schema is PIM.PIMSchema) {
                PIM.PIMSchema schema = Schema as PIM.PIMSchema;
                List<Tuple<Class, PIM.PIMClass>> classToProcess = new List<Tuple<Class, PIM.PIMClass>>();
                //vytvoreni trid
                //musi predchazet propertam a associacim, aby se neodkazovalo na neexistujici typy
                foreach (PIM.PIMClass cl in schema.PIMClasses) {
                    Class newClass = new Class(tt, cl.Name);
                    tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                    tt.RegisterType(newClass);
                    classToProcess.Add(new Tuple<Class, PIM.PIMClass>(newClass, cl));
                    //Hack
                    newClass.Tag = cl;
                }

                // Property
                foreach (Tuple<Class, PIM.PIMClass> item in classToProcess) {
                    foreach (var pr in item.Item2.PIMAttributes) {
                        Classifier propType = tt.Library.RootNamespace.NestedClassifier[pr.AttributeType.Name];
                        Property newProp = new Property(pr.Name, PropertyType.One, propType);
                        item.Item1.Properties.Add(newProp);
                        //Hack
                        newProp.Tag = pr;
                    }
                }

                //Associace
                foreach (Tuple<Class, PIM.PIMClass> item in classToProcess) {
                    foreach (var ass in item.Item2.PIMAssociationEnds) {
                        var end = ass.PIMAssociation.PIMAssociationEnds.Where(a => a.ID != ass.ID).First();
                        Classifier assType = tt.Library.RootNamespace.NestedClassifier[end.PIMClass.Name];
                        string name;
                        if (string.IsNullOrEmpty(end.Name)) {
                            name = assType.Name.ToLower();
                        }
                        else {
                            name = end.Name;
                        }
                        Classifier propType;
                        if (end.Upper > 1) {
                            propType = tt.Library.CreateCollection(CollectionKind.Set,assType);
                        }
                        else {
                            propType = assType;
                        }
                        tt.RegisterType(propType);
                        Property newass = new Property(name, PropertyType.One, propType);
                        item.Item1.Properties.Add(newass);

                        //hack
                        newass.Tag = ass;
                    }
                }

                //Operation
                foreach (Tuple<Class, PIM.PIMClass> item in classToProcess) {
                    foreach (var op in item.Item2.PIMOperations) {
                        Operation newOp = new Operation(op.Name, true, op.ResultType != null ? tt.Library.RootNamespace.NestedClassifier[op.ResultType.Name] : tt.Library.Void,
                        op.Parameters.Select(p => new Parameter(p.Name, tt.Library.RootNamespace.NestedClassifier[p.Type.Name])));
                        item.Item1.Operations.Add(newOp);

                        //hack
                        newOp.Tag = op;
                    }
                }
            }
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
    }
}