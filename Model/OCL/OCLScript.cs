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

            if (Schema is PIM.PIMSchema) {
                TranslatePIM(tt);
            }

            if (Schema is PSM.PSMSchema) {
                TranslatePSM(tt);
            }
        }



        private void TranslatePSM(TypesTable.TypesTable tt) {
            PSM.PSMSchema schema = Schema as PSM.PSMSchema;

            Dictionary<string, string> PSMToOCLMap = new Dictionary<string, string>();
            PSMToOCLMap.Add("string", "String");
            PSMToOCLMap.Add("integer", "Integer");
            PSMToOCLMap.Add("boolean", "Boolean");
            PSMToOCLMap.Add("nonNegativeInteger", "Integer");
            PSMToOCLMap.Add("dateTime", "Date");
            PSMToOCLMap.Add("date", "Date");
            PSMToOCLMap.Add("positiveInteger", "Integer");

            Func<string, string> translateName = (s) => {
                string oclName;
                if (PSMToOCLMap.TryGetValue(s, out oclName)) {
                    return oclName;
                }
                else {
                    return s;
                }
            };

            //preregistrace typu na mala pismena

            List<Tuple<Class, PSM.PSMAssociationMember>> classToProcess = new List<Tuple<Class, PSM.PSMAssociationMember>>();
            foreach (PSM.PSMClass cl in schema.PSMClasses) {
                Class newClass = new Class(tt, cl.Name);
                tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(new Tuple<Class, PSM.PSMAssociationMember>(newClass, cl));
                //Hack
                newClass.Tag = cl;
            }

            foreach (var cM in schema.PSMContentModels) {
                string cMName = GetContentModelOCLName(cM);
                Class newClass = new Class(tt, cMName);
                tt.Library.RootNamespace.NestedClassifier.Add(newClass);
                tt.RegisterType(newClass);
                classToProcess.Add(new Tuple<Class, PSM.PSMAssociationMember>(newClass, cM));
                newClass.Tag = cM;
            }

            // Property
            foreach (Tuple<Class, PSM.PSMAssociationMember> item in classToProcess) {
                if (item.Item2 is PSM.PSMClass) {
                    PSM.PSMClass sourceClass = (PSM.PSMClass)item.Item2;
                    foreach (var pr in sourceClass.PSMAttributes) {
                        Classifier propType = tt.Library.RootNamespace.NestedClassifier[translateName(pr.AttributeType.Name)];
                        Property newProp = new Property(pr.Name, PropertyType.One, propType);
                        item.Item1.Properties.Add(newProp);
                        //Hack
                        newProp.Tag = pr;
                    }
                }
            }


            //Associace
            foreach (var item in classToProcess) {
                //parent
                PSM.PSMAssociation parentAss = item.Item2.ParentAssociation;
                if (parentAss != null) {
                    string parentName = null;
                    if (parentAss.Parent is PSM.PSMClass) {
                        parentName = parentAss.Parent.Name;
                    }
                    else if (parentAss.Parent is PSM.PSMContentModel) {
                        parentName = GetContentModelOCLName((PSM.PSMContentModel)parentAss.Parent);
                    }

                    if (parentName != null) {
                        Classifier propType = tt.Library.RootNamespace.NestedClassifier[translateName(parentName)];
                        Property newProp = new Property("parent", PropertyType.One, propType);
                        item.Item1.Properties.Add(newProp);
                        //Hack
                        newProp.Tag = parentAss.Parent;
                    }
                }

                int childCount = 0;
                Dictionary<PSM.PSMContentModelType, int> childContentModelCount = new Dictionary<PSM.PSMContentModelType, int>();
                childContentModelCount[PSM.PSMContentModelType.Choice] = 0;
                childContentModelCount[PSM.PSMContentModelType.Sequence] = 0;
                childContentModelCount[PSM.PSMContentModelType.Set] = 0;

                //child 
                foreach (var ass in item.Item2.ChildPSMAssociations) {
                    string childClassName;
                    List<string> namesInOcl = new List<string>();
   
                    if (ass.Child is PSM.PSMClass) {
                        childClassName = ass.Child.Name;
                    }
                    else if (ass.Child is PSM.PSMContentModel) {
                        var cM = (PSM.PSMContentModel)ass.Child;
                        childClassName = GetContentModelOCLName((PSM.PSMContentModel)ass.Child);
                        childContentModelCount[cM.Type] = childContentModelCount[cM.Type] + 1;
                        namesInOcl.Add(String.Format("{0}_{1}", cM.Type.ToString().ToLower(), childContentModelCount[cM.Type]));
                    }
                    else {
                        System.Diagnostics.Debug.Fail("Nepodporovany typ v PSM.");
                        continue;
                    }

                    namesInOcl.Add(string.Format("child_{0}", ++childCount));

                    Classifier assType = tt.Library.RootNamespace.NestedClassifier[translateName(childClassName)];

                    if (string.IsNullOrEmpty(ass.Name) == false) {
                        namesInOcl.Add(ass.Name);
                    }
                    else {
                        namesInOcl.Add(String.Format("child_{0}",childClassName));
                    }

                    Classifier propType;
                    if (ass.Upper > 1) {
                        propType = tt.Library.CreateCollection(CollectionKind.Set, assType);
                    }
                    else {
                        propType = assType;
                    }
                    tt.RegisterType(propType);

                    foreach (string name in namesInOcl) {
                        Property newass = new Property(name, PropertyType.One, propType);
                        item.Item1.Properties.Add(newass);
                        //hack
                        newass.Tag = ass;
                    }
                }
            }
        }

        private string GetContentModelOCLName(PSM.PSMContentModel c) {
            PSM.PSMAssociation parentAssocitation = c.ParentAssociation;
            string parentName = parentAssocitation.Parent.Name;
            string name = string.Format("__{0}_{1}_{2}", parentName, c.Type.ToString(), parentAssocitation.Index);
            return name;
        }

        private void TranslatePIM(TypesTable.TypesTable tt) {

            PIM.PIMSchema schema = Schema as PIM.PIMSchema;
            //obsahuje v Tuplu tridu z OCL a k ni korespondujici tridu z PIM
            List<Tuple<Class, PIM.PIMClass>> classToProcess = new List<Tuple<Class, PIM.PIMClass>>();
            //vytvoreni prazdnych trid
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
                        propType = tt.Library.CreateCollection(CollectionKind.Set, assType);
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