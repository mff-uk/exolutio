using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;

using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;
using AST = Exolutio.Model.OCL.AST;

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

        Classifier ClassifierContextHead(List<IToken> tokenPath, string selfName, out VariableDeclaration selfOut) {
            List<string> path = tokenPath.ToStringList();
            ModelElement element = Environment.LookupPathName(path);
            if (element is Classifier == false) {
                // error
                selfOut = null;
                Errors.AddError(new ErrorItem("Nenalezena trida v ClassifierContextHeader"));
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

        AST.OclExpression InfixOperation(AST.OclExpression exp1, string name, AST.OclExpression exp2) {
            if (TestNull(exp1,name,exp2)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            Operation op = exp1.Type.LookupOperation(name, new Classifier[] { exp2.Type });
            if (op == null) {
                Errors.AddError(new ErrorItem(String.Format("On type `{0}` is not defined operation `{1}`.", exp1.Type, name)));
                return new AST.ErrorExp(Library.Any);
            }
            return new AST.OperationCallExp(exp1, false, op, new List<AST.OclExpression>(new AST.OclExpression[] { exp2 }));
        }

        AST.OclExpression UnaryOperation(IToken name, AST.OclExpression exp1) {
            if (TestNull( name, exp1)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            Operation op = exp1.Type.LookupOperation(name.Text, new Classifier[] { });
            if (op == null) {
                Errors.AddError(new CodeErrorItem(String.Format("On type `{0}` is not defined operation `{1}`.", exp1.Type, name), name, name));
                return new AST.ErrorExp(Library.Any);
            }
            return new AST.OperationCallExp(exp1, false, op, new List<AST.OclExpression>(new AST.OclExpression[] { }));
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
                    return CreateImplicitPropertyIterator(expr, sourceType, property);
                }
            }
            else {
                Property property = expr.Type.LookupProperty(path[0]);
                if (property != null) {
                    //36a
                    return new AST.PropertyCallExp(expr, isPre, null, null, property);
                }
            }

            Errors.AddError(new CodeErrorItem(String.Format("Property `{0}` does not exit.",path[0]), tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        private AST.OclExpression CreateImplicitPropertyIterator(AST.OclExpression expr, Classifier sourceType, Property property) {
            if (TestNull(expr, property)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            VariableDeclaration varDecl = new VariableDeclaration("", sourceType, null);
            List<VariableDeclaration> varList = new List<VariableDeclaration>();
            varList.Add(varDecl);
            AST.VariableExp localVar = new AST.VariableExp(varDecl);
            AST.PropertyCallExp localProp = new AST.PropertyCallExp(localVar, false, null, null, property);
            //Napevno zafixovany navratovy typ collect
            Classifier returnType = Library.CreateCollection(CollectionKind.Collection, property.Type);
            return new AST.IteratorExp(expr, localProp, "Collect", varList, returnType);
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
                        return CreateImplicitCollectIterator(expr, args, sourceType, op);
                    }
                }
                else {
                    //35eg
                    Operation op = expr.Type.LookupOperation(path[0], args.Select(arg => arg.Type));
                    if (op != null) {
                        return new AST.OperationCallExp(expr, isPre, op, args);
                    }
                }
            }

            Errors.AddError(new CodeErrorItem(string.Format("Operation `{0}` does not exit.",path.First()), tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        private AST.OclExpression CreateImplicitCollectIterator(AST.OclExpression expr, List<AST.OclExpression> args, Classifier sourceType, Operation op) {
            if (TestNull(expr, args,sourceType,op)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            VariableDeclaration varDecl = new VariableDeclaration("", sourceType, null);
            List<VariableDeclaration> varList = new List<VariableDeclaration>();
            varList.Add(varDecl);
            AST.VariableExp localVar = new AST.VariableExp(varDecl);
            AST.OperationCallExp localOp = new AST.OperationCallExp(localVar, false, op, args);
            //Napevno zafixovany navratovy typ collect
            Classifier returnType = Library.CreateCollection( CollectionKind.Collection, op.ReturnType);
            return new AST.IteratorExp(expr, localOp, "Collect", varList, returnType);
        }

        AST.OclExpression ProcessIteratorCall(AST.OclExpression expr, List<IToken> tokenPath, List<VariableDeclaration> decls, List<AST.OclExpression> args) {
            if (TestNull(expr, tokenPath,decls,args)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            if (expr.Type is CollectionType == false) {
                Errors.AddError( new ErrorItem("Compiler don't support iterator operations on not-collection type."));
                return new AST.ErrorExp(Library.Invalid);
            }

            List<string> path = tokenPath.ToStringList();
            if (path.Count != 1) {
                Errors.AddError(new CodeErrorItem("Unknow iterator operation.", tokenPath.First(), tokenPath.Last()));
                return new AST.ErrorExp(Library.Invalid);
            }
            if (args.Count > 1) {
                Errors.AddError(new CodeErrorItem("Iterator ma jenom jedno tělo výrazu.", tokenPath.First(), tokenPath.Last()));
            }

            string name = path[0];

            IteratorOperation iteratorOperation = ((CollectionType)(expr.Type)).LookupIteratorOperation(name);
            // Iterator variable muze byt NULL -> pak se pouziji defaultni nazvy ... neni implementovano
            if (iteratorOperation != null) {
                // Pozadovany typ na telo iteratoru, podle pouzite funkce
                Classifier bodyType = iteratorOperation.BodyType(expr.Type as CollectionType, args[0].Type, TypesTable);
                if (args[0].Type.ConformsTo(bodyType) == false) {
                    Errors.AddError(new CodeErrorItem("Nesedi typy v body", tokenPath.First(), tokenPath.Last()));
                }
                //Navratovy typ iteratoru podle pouzite operace
                Classifier returnType = iteratorOperation.ExpressionType(expr.Type as CollectionType, args[0].Type, TypesTable);

                return new AST.IteratorExp(expr, args[0], name, decls, returnType);
            }

            Errors.AddError(new CodeErrorItem("Bad iterator operation.", tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        AST.OclExpression ProcessIterate(AST.OclExpression rootExpr, IToken itToken, VariableDeclaration iteratorDecl,VariableDeclaration accDecl, AST.OclExpression body) {
            if (TestNull(rootExpr, iteratorDecl,accDecl,body)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            return new AST.IterateExp(rootExpr, body, iteratorDecl, accDecl);
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
                Errors.AddError(new CodeErrorItem("Unknow iterator operation.", tokenPath.First(), tokenPath.Last()));
                return new AST.ErrorExp(Library.Invalid);
            }
            string name = path[0];

            Operation collectionOperation = expr.Type.LookupOperation(name,
                args.Select(arg => arg.Type));
            //Je to operace na kolekci?
            if (collectionOperation != null) { // 36b
                return new AST.OperationCallExp(expr, false, collectionOperation, args);
            }

            Errors.AddError(new CodeErrorItem(String.Format("Unknown collection operation - {0}",tokenPath[0]), tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        AST.OclExpression ResolvePath(List<IToken> tokenPath, bool isPre) {
            if (TestNull(tokenPath)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            List<string> path = tokenPath.ToStringList();
            if (path.Count == 1) {
                // simpleName
                ModelElement element = Environment.Lookup(path[0]);

                //Variable
                if (element is VariableDeclaration) {
                    if (isPre) {
                        Errors.AddError(new ErrorItem("Modifikator @pre se nepojí s proměnou"));
                    }
                    return new AST.VariableExp((VariableDeclaration)element);
                }
                //Property 36B
                ImplicitPropertyData implProperty = Environment.LookupImplicitAttribute(path[0]);
                if (implProperty != null) {
                    // self problem
                    AST.VariableExp propertySource = new AST.VariableExp(implProperty.Source);
                    return new AST.PropertyCallExp(propertySource, isPre, null, null, implProperty.Property);
                }
                // chyby naky to pravidlo

            }
            else {
                // path
                ModelElement element = Environment.LookupPathName(path);
                //Chyby enum !!!!!!!!!!!!!!!!!!!!!!!!
                if (element is Classifier) {
                    return new AST.TypeExp((Classifier)element, Library.Type);
                }
                // Chyby 36C

            }
            Errors.AddError(new CodeErrorItem("Chyba nebo nepodporovane pravidlo", tokenPath.First(), tokenPath.Last()));
            return new AST.ErrorExp(Library.Invalid);
        }

        AST.OclExpression ResolveImplicitOperation(List<IToken> tokenPath, bool isPre, List<AST.OclExpression> callArgs) {
            if (TestNull(tokenPath, callArgs)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            List<string> path = tokenPath.ToStringList();
            if (path.Count == 1) {
                // simple name
                //35 d,f
                ImplicitOperationData operation = Environment.LookupImplicitOperation(path[0], callArgs.Select(arg => arg.Type));
                if (operation == null) {
                    Errors.AddError(new ErrorItem(string.Format("Operace `{0}` nenalezena.", path[0])));
                    return new AST.ErrorExp(Library.Any);
                }
                //self problem
                // tady by melo byt vyreseno self
                AST.VariableExp operationSource = new AST.VariableExp(operation.Source);
                return new AST.OperationCallExp(operationSource, isPre, operation.Operation, callArgs);
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
                return new AST.IntegerLiteralExp(0,Library.Integer);
            }

            long value = 0;
            try {
                value = long.Parse(token.Text);
            }
            catch (FormatException) {
                Errors.AddError(new CodeErrorItem("Bad format of integer.", token.Token, token.Token));
            }
            catch (OverflowException) {
                Errors.AddError(new CodeErrorItem(String.Format("Number {0} is overflow.", token.Text), token.Token, token.Token));
            }

            return new AST.IntegerLiteralExp(value, Library.Integer);
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
                Errors.AddError(new CodeErrorItem("Bad format of real.", token.Token, token.Token));
            }
            return new AST.RealLiteralExp(value, Library.Real);

        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <remarks>
        /// Chybí podpora encapsulace znaků!!!!!
        /// </remarks>
        /// <returns></returns>
        AST.StringLiteralExp CreateStringLiteral(CommonTree value) {
            if (TestNull(value)) {
                return new AST.StringLiteralExp("", Library.String);
            }
            return new AST.StringLiteralExp(value.Text, Library.String);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.BooleanLiteralExp CreateBooleanLiteral(bool value) {
            if (TestNull(value)) {
                return new AST.BooleanLiteralExp(true, Library.Boolean);
            }
            return new AST.BooleanLiteralExp(value, Library.Boolean);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.UnlimitedNaturalLiteralExp CreateUnlimitedNaturalLiteral() {
            return new AST.UnlimitedNaturalLiteralExp(Library.UnlimitedNatural);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.NullLiteralExp CreateNullLiteral() {
            return new AST.NullLiteralExp(Library.Void);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.InvalidLiteralExp CreateInvalidLiteral() {
            return new AST.InvalidLiteralExp(Library.Invalid);
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
            if (TestNull(token,type,parts)) {
                return new AST.ErrorExp(Library.Invalid);
            }

            if (kind == CollectionKind.Collection) {
                Errors.AddError(new CodeErrorItem("‘Collection’ is an abstract class on the M1 level and has no M0 instances.", token.Token, token.Token));
            }

            var collType = CreateCollectionType(kind, type);
            // check type
            foreach (var part in parts) {
                if (part.Type.ConformsTo(collType.ElementType) == false) {
                    Errors.AddError(new CodeErrorItem("Incorrects mishmash type in collection literal.", token.Token, token.Token));
                }
            }
            return new AST.CollectionLiteralExp(collType, parts);
        }

        AST.TupleLiteralExp CreateTupleLiteral(IToken rootToken, List<VariableDeclarationBag> vars) {
            if (TestNull(rootToken,vars)) {
                TupleType tupleTypeErr = new TupleType(TypesTable);
                TypesTable.RegisterCompositeType(tupleTypeErr);
                return new AST.TupleLiteralExp(new Dictionary<string, AST.TupleLiteralPart>(), tupleTypeErr);
            }

            Dictionary<string, AST.TupleLiteralPart> parts = new Dictionary<string, AST.TupleLiteralPart>();
            TupleType tupleType = new TupleType(TypesTable);

            foreach (var var in vars) {
                if (parts.ContainsKey(var.Name)) {
                    Errors.AddError(new CodeErrorItem(String.Format("Name {0} is used multipled.", var.Name), rootToken, rootToken));
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
                    Errors.AddError(new ErrorItem("Type does not comform to declared type."));
                }

                //hodnota
                var newProterty = new Property(var.Name, PropertyType.One, type);
                var newPart = new AST.TupleLiteralPart(newProterty, expr);
                parts.Add(var.Name, newPart);

                //typ
                tupleType.TupleParts.Add(newProterty);
            }
            TypesTable.RegisterCompositeType(tupleType);

            AST.TupleLiteralExp tupleLiteral = new AST.TupleLiteralExp(parts, tupleType);
            return tupleLiteral;
        }

        Classifier ResolveTypePathName(List<IToken> tokenPath) {
            if (TestNull(tokenPath)) {
                return Library.Invalid;
            }

            List<string> path = tokenPath.ToStringList();
            ModelElement foundType = Environment.LookupPathName(path);
            if (foundType == null) {
                foundType = Library.Invalid;
                Errors.AddError(new CodeErrorItem(String.Format("Path {0} do not exists.", path), tokenPath.First(), tokenPath.Last()));
            }

            if (foundType is Classifier == false) {
                foundType = Library.Invalid;
                Errors.AddError(new CodeErrorItem(String.Format("Path {0} do not referres type.", path), tokenPath.First(), tokenPath.Last()));
            }

            return ((Classifier)foundType);
        }

        CollectionType CreateCollectionType(CollectionKind kind, Classifier type) {
            CollectionType collectionType = Library.CreateCollection(kind, type);
            return collectionType;
        }

        TupleType CreateTupleType(IToken rootToken, List<VariableDeclarationBag> variables) {
            if (TestNull(rootToken,variables)) {
                TupleType tupleType = new TupleType(TypesTable);
                TypesTable.RegisterCompositeType(tupleType);
                return tupleType;
            }

            TupleType tuple = new TupleType(TypesTable);
            foreach (var variable in variables) {
                if (tuple.TupleParts.Keys.Contains(variable.Name)) {// Kontrola nad ramec specifikace
                    Errors.AddError(new CodeErrorItem(String.Format("Name {0} is used multipled.", variable.Name), rootToken, rootToken));
                    continue;
                }
                Classifier propertyType = variable.Type;
                if (variable.Type == null) {
                    propertyType = Library.Invalid;
                }
                tuple.TupleParts.Add(new Property(variable.Name, PropertyType.One, propertyType));
            }

            TypesTable.RegisterCompositeType(tuple);

            return tuple;
        }

        VariableDeclaration ProcessAccDef(IToken name, Classifier type, AST.OclExpression initExpr, ref int pushedVar) {
            if (TestNull(name, initExpr)) {
                return new VariableDeclaration("", Library.Any, new AST.ErrorExp(Library.Invalid));
            }

            Classifier finalType = initExpr.Type;
            if (type != null) {
                if (initExpr.Type.ConformsTo(type) == false) {
                    Errors.AddError(new ErrorItem("Init value do not conform to variable type."));
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

        VariableDeclaration CreateVariableDeclaration(IToken name, Classifier type, AST.OclExpression value) {
            if (TestNull(name)) {
                return new VariableDeclaration("", Library.Invalid, new AST.ErrorExp(Library.Invalid));
            }
            Classifier finallType = type;
            return new VariableDeclaration(name.Text, type, value);
        }


        VariableDeclaration LetDecl(IToken letToken, VariableDeclarationBag varBag) {
            if (TestNull(letToken, varBag)) {
                return new VariableDeclaration("", Library.Invalid,new AST.ErrorExp(Library.Invalid));
            }

            if (Environment.Lookup(varBag.Name) != null) {
                Errors.AddError(new CodeErrorItem("The variable name must be unique in the current scope", letToken, letToken));
                return null;
            }

            Classifier type = varBag.Type ?? varBag.Expression.Type;

            if (varBag.Expression.Type.ConformsTo(varBag.Type) == false) {
                Errors.AddError(new CodeErrorItem("Variable type does not conform to variable expression type.", letToken, letToken));
            }

            VariableDeclaration var = new VariableDeclaration(varBag.Name, varBag.Type, varBag.Expression);
            EnvironmentStack.Push(Environment.AddElement(var.Name, var.PropertyType, var, true));
            return var;
        }

        AST.OclExpression CreateLet(IToken letToken, VariableDeclaration decl, AST.OclExpression inExpr) {
            if (TestNull( decl, inExpr)) {
                return new AST.ErrorExp(Library.Invalid);
            }
            
            //?????? proc to tady bylo
            //if (decl == null) {
            //    return inExpr;
            //}
            return new AST.LetExp(decl, inExpr);
        }

        AST.OclExpression CreateIf(IToken ifTok, AST.OclExpression condition, AST.OclExpression th, AST.OclExpression el) {
            if(TestNull(ifTok,condition,th,el)){
                return new AST.ErrorExp(Library.Invalid);
            }

            if (condition.Type.ConformsTo(Library.Boolean)) {
                Errors.AddError(new CodeErrorItem("Condition of IF must conform to bool.", ifTok, ifTok));
            }

            EnvironmentStack.Pop();

            Classifier exprType = th.Type.CommonSuperType(el.Type);

            return new AST.IfExp(exprType, condition, th, el);
        }


        bool TestNull(params object[] objs) {
            if (objs.Any(o => o == null)) {
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                var callingMethod = st.GetFrame(1).GetMethod();
                Errors.AddError(new ErrorItem(String.Format("Compiler panic in method {0}.", callingMethod.Name)));
                return true;
            }
            return false;
        }
    }
}
