﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model.Versioning;
using AST = Exolutio.Model.OCL.AST;
using Exolutio.SupportingClasses;
using Version = Exolutio.Model.Versioning.Version;

namespace Exolutio.Model.OCL.Compiler {
    partial class OCLAst {
        public ErrorCollection Errors {
            get;
            private set;
        }

        public Environment Environment {
            get {
                return EnvironmentStack.Peek();
            }
        }

        public Stack<Environment> EnvironmentStack {
            get;
            private set;
        }

        public TypesTable.TypesTable TypesTable {
            get;
            set;
        }

        public TypesTable.Library Library {
            get {
                return TypesTable.Library;
            }
        }

        public IBridgeToOCL Bridge { get; set; }

        public Version SourceVersion { get; private set; }

        public OCLAst(ITreeNodeStream input, ErrorCollection errColl)
            : this(input) {
            Errors = errColl;
        }

        public override void ReportError(Antlr.Runtime.RecognitionException e) {
            Errors.AddError(new CodeErrorItem(e.ToString(), e.Token, e.Token));
            base.ReportError(e);
        }

        partial void OnCreated() {
            TypesTable = new TypesTable.TypesTable();
            Errors = new ErrorCollection();
            EnvironmentStack = new Stack<OCL.Environment>();
            //EnvironmentStack.Push(new NamespaceEnvironment(new Namespace("")));
            if (Errors == null) {
                Errors = new ErrorCollection();
            }
        }

        void ResolveVersion(string versionName)
        {
            Schema targetSchema = this.Bridge.Schema;
            VersionManager versionManager = this.Bridge.Schema.Project.VersionManager;
            Version sourceVersion = versionManager.Versions.SingleOrDefault(v => v.Label == versionName);
            if (sourceVersion == null)
            {
                Errors.AddError(new ErrorItem(string.Format("Source version `{0}` not found in the project.", versionName)));
            }
            SourceVersion = sourceVersion;
            if (!targetSchema.ExistsInVersion(sourceVersion))
            {
                Errors.AddError(new ErrorItem(string.Format("Schema '{0}' does not exist in version `{1}`.", targetSchema, versionName)));
            }
            Schema sourceSchema = targetSchema.GetInVersion(sourceVersion);
            this.Bridge.TranslateSchema(sourceSchema, true);

            foreach (Component componentT in targetSchema.SchemaComponents)
            {
                if (componentT.ExistsInVersion(SourceVersion) && (componentT is PIMClass || componentT is PSMClass || componentT is PSMContentModel))
                {
                    Component componentS = componentT.GetInVersion(SourceVersion);
                    {
                        Classifier classifierT = Bridge.Find(componentT);
                        Classifier classifierS = Bridge.Find(componentS);

                        Operation prevOp = new Operation(@"prev", true, classifierS);
                        prevOp.Tag = new PrevOperationTag() { SourceVersionClassifier = classifierS, TargetVersionClassifier = classifierT };
                        classifierT.Operations.Add(prevOp);

                        Operation nextOp = new Operation(@"next", true, classifierT);
                        nextOp.Tag = new NextOperationTag() { SourceVersionClassifier = classifierS, TargetVersionClassifier = classifierT };
                        classifierS.Operations.Add(nextOp);
                    }
                }
            }
        }

        Classifier ClassifierContextHead(List<IToken> tokenPath, string selfName, out VariableDeclaration selfOut) {
            List<string> path = tokenPath.ToStringList();
            IModelElement element = Environment.LookupPathName(path);
            if (element is Classifier == false) {
                // error
                selfOut = null;
                Errors.AddError(new ErrorItem(string.Format(CompilerErrors.OCLAst_ClassifierContextHead_ClassifierNotFound_1, path.ConcatWithSeparator(@"::"))));
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                return null;
            }
            Classifier contextClassifier = element as Classifier;
            VariableDeclaration varSelf = new VariableDeclaration(selfName, contextClassifier, null);// tady by to chtelo doplnit initValue
            selfOut = varSelf;
            Environment classifierEnv = Environment.CreateFromClassifier(contextClassifier, varSelf);
            EnvironmentStack.Push(classifierEnv);

            Environment self = Environment.AddElement(selfName, contextClassifier, varSelf, true);
            EnvironmentStack.Push(self);

            return contextClassifier;
        }

        Classifier PropertyContextHead(List<string> path, string selfName, Classifier declaredPropertyType, out VariableDeclaration selfOut, out Property property)
        {
            if (path.Count < 2)
            {
                // error
                selfOut = null; property = null;
                Errors.AddError(new ErrorItem(string.Format(CompilerErrors.OCLAst_PropertyContextHead_Incorrect_context_declaration___0___Class_and_operation_name_expected_1, path.ConcatWithSeparator("::"))));
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                return null;
            }

            string propertyName = path.Last();
            IModelElement element = Environment.LookupPathName(path.Take(path.Count - 1));
            if (element is Classifier == false)
            {
                // error
                selfOut = null; property = null;
                Errors.AddError(new ErrorItem(string.Format(CompilerErrors.OCLAst_ClassifierContextHead_ClassifierNotFound_1, path.Take(path.Count - 1).ConcatWithSeparator(@"::"))));
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                return null;
            }
            Classifier contextClassifier = element as Classifier;
            property = contextClassifier.LookupProperty(propertyName);
            if (property == null)
            {
                // error
                selfOut = null; 
                Errors.AddError(new ErrorItem(string.Format(CompilerErrors.OCLAst_PropertyContextHead_PropertyNotFound_2, propertyName, contextClassifier)));
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                return null;
            }

            if (property.Type != declaredPropertyType)
            {
                // error
                selfOut = null;
                Errors.AddError(new ErrorItem(string.Format(CompilerErrors.OCLAst_PropertyContextHead_TypeMismatch_3, property.Name, property.Type, declaredPropertyType)));
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                EnvironmentStack.Push(ErrorEnvironment.Instance);
                return null;
            }

            VariableDeclaration varSelf = new VariableDeclaration(selfName, contextClassifier, null);// tady by to chtelo doplnit initValue
            selfOut = varSelf;
            Environment classifierEnv = Environment.CreateFromClassifier(contextClassifier, varSelf);
            EnvironmentStack.Push(classifierEnv);

            Environment self = Environment.AddElement(selfName, contextClassifier, varSelf, true);
            EnvironmentStack.Push(self);

            return contextClassifier;
        }

        AST.OclExpression InfixOperation(AST.OclExpression exp1, CommonTree nameTree, AST.OclExpression exp2) {
            if (TestNull(exp1, nameTree, exp2)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            Operation op = exp1.Type.LookupOperation(nameTree.Text, new Classifier[] { exp2.Type });
            if (op == null) {
                Errors.AddError(new ErrorItem(String.Format(CompilerErrors.OCLAst_InfixOperation_NotDefined_2, exp1.Type, nameTree)));
                return new AST.ErrorExp(Library.Any);
            }
            return (new AST.OperationCallExp(exp1, false, op, new List<AST.OclExpression>(new AST.OclExpression[] { exp2 }), Environment))
                .SetCodeSource(new CodeSource(nameTree));

        }

        AST.OclExpression UnaryOperation(IToken name, AST.OclExpression exp1) {
            if (TestNull(name, exp1)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            Operation op = exp1.Type.LookupOperation(name.Text, new Classifier[] { });
            if (op == null) {
                Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_InfixOperation_NotDefined_2, exp1.Type, name), name, name));
                return new AST.ErrorExp(Library.Any);
            }
            return new AST.OperationCallExp(exp1, false, op, new List<AST.OclExpression>(new AST.OclExpression[] { }), Environment)
                .SetCodeSource(new CodeSource(name));
        }

        AST.OclExpression ProcessPropertyCall(AST.OclExpression expr, List<IToken> tokenPath, bool isPre) {
            if (TestNull(expr, tokenPath)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            List<string> path = tokenPath.ToStringList();
            if (expr.Type is CollectionType) {
                Classifier sourceType = ((CollectionType)expr.Type).ElementType;
                Property property = sourceType.LookupProperty(path[0]);
                if (property != null) {
                    return CreateImplicitPropertyIterator(expr, tokenPath[0], sourceType, property);
                }
            }
            else {
                Property property = expr.Type.LookupProperty(path[0]);
                if (property != null) {
                    //36a
                    return CheckAmbiguous(new AST.PropertyCallExp(expr, isPre, null, null, property))
                        .SetCodeSource(new CodeSource(tokenPath[0]));
                }
            }

            Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_ProcessPropertyCall_PropertyNotExists_1, path[0]), tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        private AST.OclExpression CreateImplicitPropertyIterator(AST.OclExpression expr, IToken token, Classifier sourceType, Property property) {
            if (TestNull(expr, property)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            var codeSource = new CodeSource(token);
            VariableDeclaration varDecl = new VariableDeclaration("", sourceType, null);
            List<VariableDeclaration> varList = new List<VariableDeclaration>();
            varList.Add(varDecl);
            AST.VariableExp localVar = new AST.VariableExp(varDecl)
                .SetCodeSource(codeSource);
            AST.PropertyCallExp localProp = CheckAmbiguous(new AST.PropertyCallExp(localVar, false, null, null, property))
                .SetCodeSource(codeSource);
            //Napevno zafixovany navratovy typ collect
            // JM > kdyz je typ Set(Class) nebo Bag(Class) tak by mel implicit collect vracet Bag(Class)
            Classifier returnType;
            if (property.Type is CollectionType)
            {
                returnType = Library.CreateCollection(CollectionKind.Bag, ((CollectionType)property.Type).ElementType);
            }
            else
            {
                returnType = Library.CreateCollection(CollectionKind.Collection, property.Type);
            }
            return new AST.IteratorExp(expr, localProp, @"collect", varList, returnType)
                .SetCodeSource(codeSource);
        }

        AST.OclExpression ProcessOperationCall(AST.OclExpression expr, List<IToken> tokenPath, bool isPre, List<AST.OclExpression> args) {
            if (TestNull(expr, tokenPath)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            // v pripade ze funkce ma nula argumentu neprovedese vytvoreni prazdneho listu v gramatice
            if (args == null) {
                args = new List<AST.OclExpression>();
            }
            List<string> path = tokenPath.ToStringList();
            if (path.Count == 1) {
                if (expr.Type is CollectionType) {
                    //25b
                    Classifier sourceType = ((CollectionType)expr.Type).ElementType;
                    Operation op = sourceType.LookupOperation(path[0], args.Select(arg => arg.Type));
                    if (op != null) {
                        return CreateImplicitCollectIterator(expr, tokenPath[0], args, sourceType, op);
                    }
                }
                // JM 2.5. 2012
                if (expr is AST.TypeExp)
                {
                    Operation op = (((AST.TypeExp)expr).ReferredType).LookupOperation(path[0], args.Select(arg => arg.Type));
                    if (op != null)
                    {
                        return new AST.OperationCallExp(expr, isPre, op, args, Environment)
                            .SetCodeSource(new CodeSource(tokenPath[0]));
                    }
                }
                else 
                {
                    //35eg
                    IEnumerable<Classifier> parameterTypes = args.Select(arg => arg.Type);
                    Operation op = expr.Type.LookupOperation(path[0], parameterTypes);
                    if (op != null) {
                        return new AST.OperationCallExp(expr, isPre, op, args, Environment)
                            .SetCodeSource(new CodeSource(tokenPath[0]));
                    }
                }
            }

            Errors.AddError(new CodeErrorItem(string.Format(CompilerErrors.OCLAst_ProcessOperationCall_OperationNotExists_1, path.First()), tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        private AST.OclExpression CreateImplicitCollectIterator(AST.OclExpression expr, IToken token, List<AST.OclExpression> args, Classifier sourceType, Operation op) {
            if (TestNull(expr, args, sourceType, op)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            var codeSource = new CodeSource(token);
            VariableDeclaration varDecl = new VariableDeclaration(string.Empty, sourceType, null);
            List<VariableDeclaration> varList = new List<VariableDeclaration>();
            varList.Add(varDecl);
            AST.VariableExp localVar = new AST.VariableExp(varDecl)
                .SetCodeSource(codeSource);
            AST.OperationCallExp localOp = new AST.OperationCallExp(localVar, false, op, args, Environment)
                .SetCodeSource(codeSource);
            //Napevno zafixovany navratovy typ collect
            Classifier returnType = Library.CreateCollection(CollectionKind.Collection, op.ReturnType);
            return new AST.IteratorExp(expr, localOp, @"collect", varList, returnType)
                .SetCodeSource(codeSource);
        }

        bool IsIteratorOperation(AST.OclExpression expr, List<IToken> tokenPath) {
            if (tokenPath == null && tokenPath.Count != 1) {
                return false;
            }
            if (expr.Type is InvalidType)
            {
                return false; 
            }
            string name = tokenPath[0].Text;
            IteratorOperation iteratorOperation = ((CollectionType) (expr.Type)).LookupIteratorOperation(name);
            return iteratorOperation != null;
        }

        AST.OclExpression ProcessIteratorCall(AST.OclExpression expr, List<IToken> tokenPath, List<VariableDeclaration> decls, List<AST.OclExpression> args) {
            if (TestNull(expr, tokenPath, decls, args)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            // apply oclAsSet opretion on not collect type
            if (expr.Type is CollectionType == false) {
                Operation asSetOp = expr.Type.LookupOperation(Library.OclAsSet, new Classifier[0]);
                if (asSetOp == null) {
                    Errors.AddError(new ErrorItem(String.Format(CompilerErrors.OCLParser_OperationNotFound_1, Library.OclAsSet)));
                    return new AST.ErrorExp(Library.Invalid);
                }
                expr = new AST.OperationCallExp(expr, false, asSetOp, new List<AST.OclExpression>(), Environment)
                    .SetCodeSource(new CodeSource(tokenPath[0]));
            }

            List<string> path = tokenPath.ToStringList();
            if (path.Count != 1) {
                Errors.AddError(new CodeErrorItem(string.Format(CompilerErrors.OCLAst_ProcessIteratorCall_Unknow_iterator_operation), tokenPath.First(), tokenPath.Last()));
                return new AST.ErrorExp(Library.Invalid);
            }
            if (args.Count > 1) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_ProcessIteratorCall_Iterator_ma_jenom_jedno_tělo_výrazu, tokenPath.First(), tokenPath.Last()));
            }

            string name = path[0];

            IteratorOperation iteratorOperation = ((CollectionType)(expr.Type)).LookupIteratorOperation(name);
            // Iterator variable muze byt NULL -> pak se pouziji defaultni nazvy ... neni implementovano
            if (iteratorOperation != null) {
                // Pozadovany typ na telo iteratoru, podle pouzite funkce
                Classifier bodyType = iteratorOperation.BodyType(expr.Type as CollectionType, args[0].Type, TypesTable);
                if (args[0].Type.ConformsTo(bodyType) == false) {
                    Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_ProcessIteratorCall_Body_type_inconsistency, tokenPath.First(), tokenPath.Last()));
                }
                //Navratovy typ iteratoru podle pouzite operace
                Classifier returnType = iteratorOperation.ExpressionType(expr.Type as CollectionType, args[0].Type, TypesTable);

                return new AST.IteratorExp(expr, args[0], name, decls, returnType)
                    .SetCodeSource(new CodeSource(tokenPath[0]));
            }

            Errors.AddError(new CodeErrorItem(string.Format(CompilerErrors.OCLAst_ProcessIteratorCall_Bad_iterator_operation_1, name), tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        AST.OclExpression ProcessIterate(AST.OclExpression rootExpr, IToken itToken, VariableDeclaration iteratorDecl, VariableDeclaration accDecl, AST.OclExpression body) {
            if (TestNull(rootExpr, iteratorDecl, accDecl, body)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            return new AST.IterateExp(rootExpr, body, iteratorDecl, accDecl)
                .SetCodeSource(new CodeSource(itToken));
        }

        AST.OclExpression ProcessCollectionOperationCall(AST.OclExpression expr, List<IToken> tokenPath, List<AST.OclExpression> args) {
            if (TestNull(expr, tokenPath)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            // v pripade ze funkce ma nula argumentu neprovedese vytvoreni prazdneho listu v gramatice
            if (args == null) {
                args = new List<AST.OclExpression>();
            }
            List<string> path = tokenPath.ToStringList();
            if (path.Count != 1) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_ProcessIteratorCall_Unknow_iterator_operation, tokenPath.First(), tokenPath.Last()));
                return new AST.ErrorExp(Library.Invalid);
            }
            string name = path[0];

            // apply oclAsSet opretion on not collect type
            if (expr.Type is CollectionType == false) {
                Operation asSetOp = expr.Type.LookupOperation(Library.OclAsSet, new Classifier[0]);
                if (asSetOp == null) {
                    Errors.AddError(new ErrorItem(String.Format(CompilerErrors.OCLParser_OperationNotFound_1, Library.OclAsSet)));
                    return new AST.ErrorExp(Library.Invalid);
                }
                expr = new AST.OperationCallExp(expr, false, asSetOp, new List<AST.OclExpression>(), Environment)
                    .SetCodeSource(new CodeSource(tokenPath[0]));
            }

            Operation collectionOperation = expr.Type.LookupOperation(name,
                args.Select(arg => arg.Type));

            if (collectionOperation != null) { // 36b
                return new AST.OperationCallExp(expr, false, collectionOperation, args, Environment)
                    .SetCodeSource(new CodeSource(tokenPath[0]));
            }

            Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_ProcessCollectionOperationCall_Unknown_collection_operation, name), tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        AST.OclExpression ResolvePath(List<IToken> tokenPath, bool isPre) {
            if (TestNull(tokenPath)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            var codeSource = new CodeSource(tokenPath[0]);
            List<string> path = tokenPath.ToStringList();
            if (path.Count == 1) {
                // simpleName
                IModelElement element = Environment.Lookup(path[0]);

                //Variable
                if (element is VariableDeclaration) {
                    if (isPre) {
                        Errors.AddError(new ErrorItem(CompilerErrors.OCLAst_ResolvePath_Modifikator__pre_se_nepoji_s_promenou));
                    }
                    return new AST.VariableExp((VariableDeclaration)element)
                        .SetCodeSource(codeSource);
                }
                //Property 36B
                ImplicitPropertyData implProperty = Environment.LookupImplicitAttribute(path[0]);
                if (implProperty != null) {
                    // self problem
                    AST.VariableExp propertySource = new AST.VariableExp(implProperty.Source)
                        .SetCodeSource(codeSource);
                    return CheckAmbiguous(new AST.PropertyCallExp(propertySource, isPre, null, null, implProperty.Property))
                        .SetCodeSource(codeSource);
                }
                // chyby naky to pravidlo
                
                // JM > type exp
                if (element is Classifier)
                {
                    return new AST.TypeExp((Classifier)element, Library.Type)
                        .SetCodeSource(codeSource);
                }
            }
            else {
                // path
                IModelElement element = Environment.LookupPathName(path);
                //Chyby enum !!!!!!!!!!!!!!!!!!!!!!!!
                if (element is Classifier) {
                    return new AST.TypeExp((Classifier)element, Library.Type)
                        .SetCodeSource(codeSource);
                }
                // Chyby 36C

            }
            Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_ResolvePath_Chyba_nebo_nepodporovane_pravidlo, tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        AST.OclExpression ResolveImplicitOperation(List<IToken> tokenPath, bool isPre, List<AST.OclExpression> callArgs) {
            if (TestNull(tokenPath, callArgs)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            var codeSource = new CodeSource(tokenPath[0]);
            List<string> path = tokenPath.ToStringList();
            if (path.Count == 1) {
                // simple name
                //35 d,f
                ImplicitOperationData operation = Environment.LookupImplicitOperation(path[0], callArgs.Select(arg => arg.Type));
                if (operation == null) {
                    Errors.AddError(new ErrorItem(string.Format(CompilerErrors.OCLAst_ResolveImplicitOperation_Operation_not_found_1, path[0])));
                    return new AST.ErrorExp(Library.Any);
                }
                //self problem
                // tady by melo byt vyreseno self
                AST.VariableExp operationSource = new AST.VariableExp(operation.Source)
                    .SetCodeSource(codeSource);
                return new AST.OperationCallExp(operationSource, isPre, operation.Operation, callArgs, Environment)
                    .SetCodeSource(codeSource);
            }
            else {
                //path
                // 35 g
                // lookupPathName neresi pretizeni nazvu => a nezohlednuje operace s ruznymi signaturami
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.IntegerLiteralExp CreateIntegerLiteral(CommonTree token) {
            if (TestNull(token)) {
                return new AST.IntegerLiteralExp(0, Library.Integer);
            }

            long value = 0;
            try {
                value = long.Parse(token.Text);
            }
            catch (FormatException) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_CreateIntegerLiteral_Bad_format_of_integer, token.Token, token.Token));
            }
            catch (OverflowException) {
                Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_CreateIntegerLiteral_Number__0__is_overflow_, token.Text), token.Token, token.Token));
            }

            return new AST.IntegerLiteralExp(value, Library.Integer)
                .SetCodeSource(new CodeSource(token));
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.RealLiteralExp CreateRealLiteral(CommonTree token) {
            if (TestNull(token)) {
                return new AST.RealLiteralExp(0, Library.Real);
            }

            double value = 0;
            try {
                value = double.Parse(token.Text);
            }
            catch (FormatException) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_CreateRealLiteral_Bad_format_of_real, token.Token, token.Token));
            }
            return new AST.RealLiteralExp(value, Library.Real)
                .SetCodeSource(new CodeSource(token));

        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <remarks>
        /// Chybí podpora encapsulace znaků!!!!!
        /// </remarks>
        /// <returns></returns>
        AST.StringLiteralExp CreateStringLiteral(CommonTree token) {
            if (TestNull(token)) {
                return new AST.StringLiteralExp("", Library.String);
            }

            string value = token.Text;
            if (value.Length >= 2) {
                // Remove apostrophes
                value = token.Text.Substring(1, token.Text.Length - 2);
                value = UnescapeString.Replace(value);
            }
            return new AST.StringLiteralExp(value, Library.String)
                .SetCodeSource(new CodeSource(token));
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.BooleanLiteralExp CreateBooleanLiteral(bool value, CommonTree token) {
            if (TestNull(value)) {
                return new AST.BooleanLiteralExp(true, Library.Boolean);
            }
            return new AST.BooleanLiteralExp(value, Library.Boolean)
                .SetCodeSource(new CodeSource(token));
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.UnlimitedNaturalLiteralExp CreateUnlimitedNaturalLiteral(CommonTree token) {
            return new AST.UnlimitedNaturalLiteralExp(Library.UnlimitedNatural)
                .SetCodeSource(new CodeSource(token));
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.NullLiteralExp CreateNullLiteral(CommonTree token) {
            return new AST.NullLiteralExp(Library.Void)
                .SetCodeSource(new CodeSource(token));
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.InvalidLiteralExp CreateInvalidLiteral(CommonTree token) {
            return new AST.InvalidLiteralExp(Library.Invalid)
                .SetCodeSource(new CodeSource(token));
        }

        AST.OclExpression CollectionLiteralExp(CollectionKind kind, CommonTree token, List<AST.CollectionLiteralPart> parts) {
            if (TestNull(parts)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            Classifier elementType = Library.Void;
            foreach (var part in parts) {
                elementType = elementType.CommonSuperType(part.Type);
            }
            return CollectionLiteralExp(kind, token, elementType, parts);
        }

        AST.OclExpression CollectionLiteralExp(CollectionKind kind, CommonTree token, Classifier type, List<AST.CollectionLiteralPart> parts) {
            if (TestNull(token, type, parts)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            if (kind == CollectionKind.Collection) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_CollectionLiteralExp_CollectionIsAbstract, token.Token, token.Token));
            }

            var collType = CreateCollectionType(kind, type);
            // check type
            foreach (var part in parts) {
                if (part.Type.ConformsTo(collType.ElementType) == false) {
                    Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_CollectionLiteralExp_Incorrects_mishmash_type_in_collection_literal, token.Token, token.Token));
                }
            }
            return new AST.CollectionLiteralExp(collType, parts)
                 .SetCodeSource(new CodeSource(token));
        }

        AST.TupleLiteralExp CreateTupleLiteral(IToken rootToken, List<VariableDeclarationBag> vars) {
            if (TestNull(rootToken, vars)) {
                TupleType tupleTypeErr = new TupleType(TypesTable, new List<Property>());
                TypesTable.RegisterType(tupleTypeErr);
                return new AST.TupleLiteralExp(new Dictionary<string, AST.TupleLiteralPart>(), tupleTypeErr)
                     .SetCodeSource(new CodeSource(rootToken));
            }

            Dictionary<string, AST.TupleLiteralPart> parts = new Dictionary<string, AST.TupleLiteralPart>();

            List<Property> tupleParts = new List<Property>();
            foreach (var var in vars) {
                if (parts.ContainsKey(var.Name)) {
                    Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_CreateTupleLiteral_Name_repeated_1, var.Name), rootToken, rootToken));
                    continue;
                }

                AST.OclExpression expr = var.Expression;
                if (var.Expression == null) {
                    expr = new AST.ErrorExp(Library.Invalid);
                }

                Classifier type = var.Type;
                if (type == null) {
                    type = expr.Type;
                }

                if (expr.Type.ConformsTo(type) == false) {
                    Errors.AddError(new ErrorItem(CompilerErrors.OCLAst_CreateTupleLiteral_Type_does_not_comform_to_declared_type));
                }


                //hodnota
                var newProterty = new Property(var.Name, PropertyType.One, type);
                var newPart = new AST.TupleLiteralPart(newProterty, expr);
                parts.Add(var.Name, newPart);

                //typ
                tupleParts.Add(newProterty);
            }
            TupleType tupleType = new TupleType(TypesTable, tupleParts);
            TypesTable.RegisterType(tupleType);

            return new AST.TupleLiteralExp(parts, tupleType)
                .SetCodeSource(new CodeSource(rootToken)); ;
        }

        AST.ClassLiteralExp CreateClassLiteral(IToken rootToken, List<VariableDeclarationBag> vars, List<IToken> tokenPath)
        {
            Classifier createdClass = ResolveTypePathName(tokenPath); 
            if (createdClass== null || createdClass == Library.Invalid)
            {
                Errors.AddError(new CodeErrorItem(String.Format("Unable to create type '{0}'.", tokenPath.ConcatWithSeparator(@"::")), rootToken, rootToken));
            }

            Dictionary<string, AST.TupleLiteralPart> parts = new Dictionary<string, AST.TupleLiteralPart>();

            List<Property> classParts = new List<Property>();
            foreach (var var in vars)
            {
                if (parts.ContainsKey(var.Name))
                {
                    Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_CreateTupleLiteral_Name_repeated_1, var.Name), rootToken, rootToken));
                    continue;
                }

                AST.OclExpression expr = var.Expression;
                if (var.Expression == null) 
                {
                    expr = new AST.ErrorExp(Library.Invalid);
                }

                Classifier type = var.Type;
                if (type == null)
                {
                    type = expr.Type;
                }

                if (expr.Type.ConformsTo(type) == false)
                {
                    Errors.AddError(new ErrorItem(CompilerErrors.OCLAst_CreateTupleLiteral_Type_does_not_comform_to_declared_type));
                }


                //hodnota
                var newProterty = new Property(var.Name, PropertyType.One, type);
                var newPart = new AST.TupleLiteralPart(newProterty, expr);
                parts.Add(var.Name, newPart);

                //typ 
                classParts.Add(newProterty);
            }

            return new AST.ClassLiteralExp(parts, createdClass)
                .SetCodeSource(new CodeSource(rootToken)); 
        }


        Classifier ResolveTypePathName(List<IToken> tokenPath) {
            if (TestNull(tokenPath)) {
                return Library.Invalid;
            }

            List<string> path = tokenPath.ToStringList();
            IModelElement foundType = Environment.LookupPathName(path, true);
            if (foundType == null) {
                foundType = Library.Invalid;
                Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_ResolveTypePathName_Path__0__do_not_exists_1, path), tokenPath.First(), tokenPath.Last()));
            }

            if (foundType is Classifier == false) {
                foundType = Library.Invalid;
                Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_ResolveTypePathName_Path__0__do_not_referres_type_1, path.ConcatWithSeparator("::")), tokenPath.First(), tokenPath.Last()));
            }

            return ((Classifier)foundType);
        }

        CollectionType CreateCollectionType(CollectionKind kind, Classifier type) {
            CollectionType collectionType = Library.CreateCollection(kind, type);
            return collectionType;
        }

        TupleType CreateTupleType(IToken rootToken, List<VariableDeclarationBag> variables) {
            if (TestNull(rootToken, variables)) {
                TupleType tupleType = new TupleType(TypesTable, new List<Property>());
                TypesTable.RegisterType(tupleType);
                return tupleType;
            }


            Dictionary<string, Property> tupleParts = new Dictionary<string, Property>();
            foreach (var variable in variables) {
                if (tupleParts.Keys.Contains(variable.Name)) {// Kontrola nad ramec specifikace
                    Errors.AddError(new CodeErrorItem(String.Format(CompilerErrors.OCLAst_CreateTupleLiteral_Name_repeated_1, variable.Name), rootToken, rootToken));
                    continue;
                }
                Classifier propertyType = variable.Type;
                if (variable.Type == null) {
                    propertyType = Library.Invalid;
                }
                tupleParts.Add(variable.Name, new Property(variable.Name, PropertyType.One, propertyType));
            }
            TupleType tuple = new TupleType(TypesTable, tupleParts.Values);
            TypesTable.RegisterType(tuple);

            return tuple;
        }

        VariableDeclaration ProcessAccDef(IToken name, Classifier type, AST.OclExpression initExpr, ref int pushedVar) {
            if (TestNull(name, initExpr)) {
                return new VariableDeclaration("", Library.Any, new AST.ErrorExp(Library.Invalid));
            }

            Classifier finalType = initExpr.Type;
            if (type != null) {
                if (initExpr.Type.ConformsTo(type) == false) {
                    Errors.AddError(new ErrorItem(CompilerErrors.OCLAst_ProcessAccDef_Init_value_do_not_conform_to_variable_type));
                }
                finalType = type;
            }
            return ProcessTypeDef(name, finalType, initExpr, ref pushedVar);
        }

        VariableDeclaration ProcessVarDef(AST.OclExpression expr, IToken name, Classifier type, ref int pushedVar) {
            if (TestNull(expr)) {
                return new VariableDeclaration("", Library.Invalid, new AST.ErrorExp(Library.Invalid));
            }

            Classifier finalType;
            if (type == null) {
                if (expr.Type is CollectionType) {
                    finalType = ((CollectionType)expr.Type).ElementType;
                }
                else {
                    finalType = expr.Type;
                }
            }
            else {
                finalType = type;
            }
            return ProcessTypeDef(name, finalType, null, ref pushedVar);
        }

        VariableDeclaration ProcessTypeDef(IToken name, Classifier type, AST.OclExpression initExpr, ref int pushedVar) {
            if (TestNull(name)) {
                return new VariableDeclaration("", Library.Invalid, new AST.ErrorExp(Library.Invalid));
            }
            VariableDeclaration decl = new VariableDeclaration(name.Text, type, null);
            //add variable to EnviromentStack
            var env = Environment.AddElement(decl.Name, decl.PropertyType, decl, true);
            EnvironmentStack.Push(env);
            //inc pushedVar to future pop EnviromentStack
            //pushedVar is ref variable
            pushedVar++;
            return decl;
        }

        VariableDeclaration ProcessImplicitVarDef(AST.OclExpression expr, ref int pushedVar) {
            if (TestNull(expr)) {
                return new VariableDeclaration("", Library.Invalid, new AST.ErrorExp(Library.Invalid));
            }

            Classifier type;
            if (expr.Type is CollectionType) {
                type = ((CollectionType)expr.Type).ElementType;
            }
            else {
                type = expr.Type;
            }

            VariableDeclaration decl = new VariableDeclaration("", type, null);
            //add variable to EnviromentStack
            var env = Environment.CreateFromClassifier(decl.PropertyType, decl);
            EnvironmentStack.Push(env);
            //inc pushedVar to future pop EnviromentStack
            //pushedVar is ref variable
            pushedVar++;
            return decl;

        }

        VariableDeclaration CreateVariableDeclaration(IToken name, Classifier type, AST.OclExpression value) {
            if (TestNull(name)) {
                return new VariableDeclaration("", Library.Invalid, new AST.ErrorExp(Library.Invalid));
            }
            Classifier finallType = type;
            return new VariableDeclaration(name.Text, type, value);
        }


        VariableDeclaration LetDecl(IToken letToken, VariableDeclarationBag varBag) {
            if (TestNull(letToken, varBag, varBag != null ? varBag.Type : null)) {
                return new VariableDeclaration("", Library.Invalid, new AST.ErrorExp(Library.Invalid));
            }

            if (Environment.Lookup(varBag.Name) != null) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_LetDecl_The_variable_name_must_be_unique_in_the_current_scope, letToken, letToken));
                return null;
            }

            Classifier type = varBag.Type ?? varBag.Expression.Type;

            if (varBag.Expression.Type.ConformsTo(varBag.Type) == false) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_LetDecl_Variable_type_does_not_conform_to_variable_expression_type, letToken, letToken));
            }

            VariableDeclaration var = new VariableDeclaration(varBag.Name, varBag.Type, varBag.Expression);
            EnvironmentStack.Push(Environment.AddElement(var.Name, var.PropertyType, var, true));
            return var;
        }

        AST.OclExpression CreateLet(IToken letToken, VariableDeclaration decl, AST.OclExpression inExpr) {
            if (TestNull(decl, inExpr)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            //?????? proc to tady bylo
            //if (decl == null) {
            //    return inExpr;
            //}
            return new AST.LetExp(decl, inExpr)
                .SetCodeSource(new CodeSource(letToken)); ;
        }

        AST.OclExpression CreateIf(IToken ifTok, AST.OclExpression condition, AST.OclExpression th, AST.OclExpression el) {
            if (TestNull(ifTok, condition, th, el)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            if (condition.Type.ConformsTo(Library.Boolean) == false) {
                Errors.AddError(new CodeErrorItem(CompilerErrors.OCLAst_CreateIf_Condition_of_IF_must_conform_to_bool, ifTok, ifTok));
            }

            //EnvironmentStack.Pop();

            Classifier exprType = th.Type.CommonSuperType(el.Type);

            return new AST.IfExp(exprType, condition, th, el)
                 .SetCodeSource(new CodeSource(ifTok)); ;
        }


        bool TestNull(params object[] objs) {
            if (objs.Any(o => o == null)) {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                var callingMethod = st.GetFrame(1).GetMethod();
                Errors.AddError(new ErrorItem(String.Format(CompilerErrors.OCLAst_TestNull_Compiler_panic_in_method_1, callingMethod.Name)));
                return true;
            }
            return false;
        }

        AST.PropertyCallExp CheckAmbiguous(AST.PropertyCallExp prop) {
            if (prop.ReferredProperty.IsAmbiguous) {
                Errors.AddError(new ErrorItem(String.Format(CompilerErrors.AmbiguousNavigation,
                    prop.Source.Type.Name, prop.ReferredProperty.Name)));
            }
            return prop;
        }
    }
}
