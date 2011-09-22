using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr.Runtime;

using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;
using AST = Exolutio.Model.OCL.AST;


namespace Exolutio.Model.OCL.Compiler {
    public partial class OCLParser {
        public const string TrueConstant = "true";



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

        partial void OnCreated() {
            TypesTable = new TypesTable.TypesTable();
            Errors = new ErrorCollection();
            EnvironmentStack = new Stack<OCL.Environment>();
            //EnvironmentStack.Push(new NamespaceEnvironment(new Namespace("")));
        }

        Classifier ClassifierContextHead(List<string> path,string selfName) {
            ModelElement element =  Environment.LookupPathName(path);
            if (element is Classifier == false) {
                // error
                return null;
            }
            Classifier contextClassifier = element as Classifier;

            Environment classifierEnv = Environment.CreateFromClassifier(contextClassifier);
            EnvironmentStack.Push(classifierEnv);
            VariableDeclaration varSelf = new VariableDeclaration(selfName, contextClassifier, null);
            Environment self = Environment.AddElement(selfName, contextClassifier, varSelf, true);
            EnvironmentStack.Push(self);

            return contextClassifier;

        }

        AST.OclExpression InfixOperation(AST.OclExpression exp1,string name,AST.OclExpression exp2) {
            Operation op =exp1.Type.LookupOperation(name, new Classifier[] { exp2.Type });
            if (op == null) {
                Errors.AddError(new ErrorItem(String.Format("On type `{0}` is not defined operation `{1}`.",exp1.Type,name)));
                return new AST.ErrorExp(TypesTable.Library.Any);
            }
            return new AST.OperationCallExp( exp1, false,op,new List<AST.OclExpression>(new AST.OclExpression[] { exp2 }));
        }

        AST.OclExpression UnaryOperation(CommonToken name,AST.OclExpression exp1 ) {
            Operation op = exp1.Type.LookupOperation(name.Text, new Classifier[] {  });
            if (op == null) {
                Errors.AddError(new CodeErrorItem(String.Format("On type `{0}` is not defined operation `{1}`.", exp1.Type, name),name,name));
                return new AST.ErrorExp(TypesTable.Library.Any);
            }
            return new AST.OperationCallExp(exp1, false, op, new List<AST.OclExpression>(new AST.OclExpression[] {}));
        }


        AST.OclExpression PropertyCall(bool isRoot, SeparatorType separator, List<string> path, AST.OclExpression rootExpr, List<ArgumentBag> indexArgs, bool isPre,List<TypeDefBag> varDecl, List<ArgumentBag> callArgs) {
            if (isRoot) {
                return PropertyCallRoot(path, indexArgs, isPre, callArgs);
            }
            else {
                return PropertyCallBody(separator, path,rootExpr, indexArgs, isPre, varDecl,callArgs);
            }
        }


        AST.OclExpression PropertyCallRoot(List<string> path, List<ArgumentBag> indexArgs, bool isPre, List<ArgumentBag> callArgs) {
            // variable, enum
            if (indexArgs == null  && callArgs == null) {
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
                        return new AST.PropertyCallExp(null, isPre, null, null, implProperty.Property);
                    }
                    throw new NotImplementedException();
                }
                else {
                    // path
                    ModelElement element = Environment.LookupPathName(path);
                    //Chyby enum !!!!!!!!!!!!!!!!!!!!!!!!
                    if (element is Classifier) {
                        return new AST.TypeExp((Classifier)element, TypesTable.Library.Type);
                    }
                    // Chyby 36C
                    throw new NotImplementedException();
                }
            }
            //Operation call
            if (indexArgs == null && callArgs != null) {
                if (path.Count == 1) {
                    // simple name
                    //35 d,f
                    ImplicitOperationData operation = Environment.LookupImplicitOperation(path[0], callArgs.Select(arg => arg.Expression.Type));
                    if (operation == null) {
                        Errors.AddError(new ErrorItem(string.Format("Operace `{0}` nenalezena.", path[0])));
                        return new AST.ErrorExp(TypesTable.Library.Any);
                    }
                    //self problem
                    // tady by melo byt vyreseno self
                    return new AST.OperationCallExp(null, isPre, operation.Operation, callArgs.Select(arg => arg.Expression).ToList());
                }
                else {
                    //path
                    // 35 g
                    // lookupPathName neresi pretizeni nazvu => a nezohlednuje operace s ruznymi signaturami
                    throw new NotSupportedException();
                }
            }
            throw new NotImplementedException();
        }

        private AST.OclExpression PropertyCallBody(SeparatorType separator, List<string> path,AST.OclExpression rootExpr, List<ArgumentBag> indexArgs, bool isPre, List<TypeDefBag> varDecl, List<ArgumentBag> callArgs) {
            if (separator == SeparatorType.Arrow) {
                //25a,36b
                if (rootExpr.Type is CollectionType == false) {
                    Errors.AddError(new ErrorItem("Illegal use of '->', source is not collection."));
                    return new AST.ErrorExp(TypesTable.Library.Any);
                }

                bool correctCall = path.Count == 1 && indexArgs == null && isPre == false /*&& varDecl != null && callArgs != null && callArgs.Count == 1*/ ;
                if (correctCall == false) {
                    Errors.AddError(new ErrorItem("Incorect call convention for iterator."));
                    return new AST.ErrorExp(TypesTable.Library.Any);
                }
                return ProcessArrowIterator(rootExpr, path[0], varDecl, callArgs);
                
            }
            else {
                if (rootExpr.Type is CollectionType) {
                    //25 bcde
                    if (path.Count == 1 && indexArgs == null && isPre == false && varDecl == null && callArgs != null) {
                        // 25 b
                    }
                    if (path.Count == 1 && indexArgs == null && isPre == false && varDecl == null && callArgs == null) {
                        // 25 c
                    }
                    //25de
                    throw new NotSupportedException();
                }

                if (callArgs == null) {
                    Property property = rootExpr.Type.LookupProperty(path[0]);
                    if (path.Count == 1 && indexArgs == null && varDecl == null && callArgs == null && property != null) {
                        return new AST.PropertyCallExp(rootExpr, isPre, null, null, property);
                    }
                }
                else {
                    Operation op = rootExpr.Type.LookupOperation(path[0], callArgs.Select(arg => arg.Expression.Type));
                    if (path.Count == 1 && indexArgs == null && varDecl == null && callArgs != null && op != null) {
                        //35eg
                        return new AST.OperationCallExp(rootExpr, isPre, op, callArgs.Select(arg => arg.Expression).ToList());
                    }
                }
            }
            throw new NotSupportedException();
        }

        private AST.OclExpression ProcessArrowIterator(AST.OclExpression rootExpr, string name, List<TypeDefBag> varDecl, List<ArgumentBag> callArgs) {
            // muzeto byt but iterator nebo operace na collection
            // budeme doufat, ze se nazvi iterator operaci neprekryvaji s operacemi na kolekcich
            Operation collectionOperation = rootExpr.Type.LookupOperation(name,
                callArgs.Select(arg => arg.Expression.Type));
            //Je to operace na kolekci?
            if (collectionOperation != null) { // 36b
                if (varDecl != null) {
                    Errors.AddError(new ErrorItem("Nepovolene pouziti deklarace promene."));
                }

                return new AST.OperationCallExp(rootExpr, false, collectionOperation, callArgs.Select(arg => arg.Expression).ToList());
            }

            // 25a
            if (callArgs == null || callArgs.Count != 1) {
                Errors.AddError(new ErrorItem("Incorect call convention for iterator."));
                return new AST.ErrorExp(TypesTable.Library.Any);
            }

            IteratorOperation iteratorOperation = ((CollectionType)(rootExpr.Type)).LookupIteratorOperation(name);
            // Iterator expresion muze byt NULL -> pak se pouziji defaultni nazvy ... neni implementovano
            if (iteratorOperation != null) {
                // Pozadovany typ na telo iteratoru, podle pouzite funkce
                Classifier bodyType = iteratorOperation.BodyType(rootExpr.Type as CollectionType, callArgs[0].Expression.Type, TypesTable);
                if (callArgs[0].Expression.Type.ConformsTo(bodyType) == false) {
                    Errors.AddError(new CodeErrorItem("Nesedi typy v body",callArgs[0].Start,callArgs[0].Stop));
                }
                //Navratovy typ iteratoru podle pouzite operace
                Classifier returnType = iteratorOperation.ExpressionType(rootExpr.Type as CollectionType, callArgs[0].Expression.Type, TypesTable);

                return new AST.IteratorExp(rootExpr, callArgs[0].Expression, name, varDecl.Select(bag => bag.Declaration).ToList(),returnType);
            }

            throw new NotImplementedException("rule 25a");
        }


        AST.OclExpression CollectionLiteralExpAndType(CollectionKind kind, CommonToken token, Classifier type, List<AST.CollectionLiteralPart> parts) {
            if (type != null && parts == null) {
                // type
                var collType = CreateCollectionType(kind, type);
                return new AST.TypeExp(collType, TypesTable.Library.Type);
            }

            if (kind == CollectionKind.Collection) {
                Errors.AddError(new CodeErrorItem("‘Collection’ is an abstract class on the M1 level and has no M0 instances.", token, token));
            }

            if (type != null && parts != null) {
                var collType = CreateCollectionType(kind, type);
                // check type
                foreach (var part in parts) {
                    if (part.Type.ConformsTo(collType.ElementType) == false) {
                        Errors.AddError(new CodeErrorItem("Incorrects mishmash type in collection literal.", token, token));
                    }
                }
                // type check
                return new AST.CollectionLiteralExp(collType, parts);
            }

            if (type == null && parts != null) {
                Classifier elementType = TypesTable.Library.Void;
                foreach (var part in parts) {
                    elementType = elementType.CommonSuperType(part.Type);
                }
                var collType = CreateCollectionType(kind, elementType);
                return new AST.CollectionLiteralExp(collType, parts);
            }

            return new AST.ErrorExp(TypesTable.Library.Any);

        }


        AST.CollectionLiteralPart CreateCollectionLiteralPart(AST.OclExpression exp1, AST.OclExpression exp2) {
            if (exp2 == null) {
                // single value part
                return new AST.CollectionItem(exp1);
            }
            else {
                // range
                return new AST.CollectionRange(exp1, exp2);
            }
        }


        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.IntegerLiteralExp CreateIntegerLiteral(CommonToken token) {
            long value = 0;
            try {
                value = long.Parse(token.Text);
            }
            catch (FormatException) {
                Errors.AddError(new CodeErrorItem("Bad format of integer.", token, token));
            }
            catch (OverflowException) {
                Errors.AddError(new CodeErrorItem(String.Format("Number {0} is overflow.", token.Text), token, token));
            }

            return new AST.IntegerLiteralExp(value, TypesTable.Library.Integer);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.RealLiteralExp CreateRealLiteral(CommonToken token) {
            double value = 0;
            try {
                value = double.Parse(token.Text);
            }
            catch (FormatException) {
                Errors.AddError(new CodeErrorItem("Bad format of real.", token, token));
            }
            return new AST.RealLiteralExp(value, TypesTable.Library.Real);

        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <remarks>
        /// Chybí podpora encapsulace znaků!!!!!
        /// </remarks>
        /// <returns></returns>
        AST.StringLiteralExp CreateStringLiteral(CommonToken value) {
            return new AST.StringLiteralExp(value.Text, TypesTable.Library.String);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.BooleanLiteralExp CreateBooleanLiteral(string value) {
            bool newValue;
            newValue = value == TrueConstant;

            return new AST.BooleanLiteralExp(newValue, TypesTable.Library.Boolean);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.UnlimitedNaturalLiteralExp CreateUnlimitedNaturalLiteral() {
            return new AST.UnlimitedNaturalLiteralExp(TypesTable.Library.UnlimitedNatural);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.NullLiteralExp CreateNullLiteral() {
            return new AST.NullLiteralExp(TypesTable.Library.Void);
        }

        /// <summary>
        /// primitiveLiteralExp,rule 15
        /// </summary>
        /// <returns></returns>
        AST.InvalidLiteralExp CreateInvalidLiteral() {
            return new AST.InvalidLiteralExp(TypesTable.Library.Invalid);
        }

        List<string> ResolvePathName(CommonToken first, List<CommonToken> other) {
            List<string> path = new List<string>();
            path.Add(first.Text);
            if (other != null) {
                other.ForEach(t => path.Add(t.Text));
            }
            return path;
        }


        AST.TupleLiteralExp CreateTupleLiteral(CommonToken rootToken, List<VariableDeclaration> vars) {

            Dictionary<string, AST.TupleLiteralPart> parts = new Dictionary<string, AST.TupleLiteralPart>();
            TupleType tupleType = new TupleType(TypesTable);

            foreach (var var in vars) {
                if (parts.ContainsKey(var.Name)) {
                    Errors.AddError(new CodeErrorItem(String.Format("Name {0} is used multipled.", var.Name), rootToken, rootToken));
                    continue;
                }
                //hodnota
                var newProterty = new Property(var.Name, PropertyType.One, var.PropertyType);
                var newPart = new AST.TupleLiteralPart(newProterty, var.Value);
                parts.Add(var.Name, newPart);

                //typ
                tupleType.TupleParts.Add(newProterty);
            }
            TypesTable.RegisterCompositeType(tupleType);

            AST.TupleLiteralExp tupleLiteral = new AST.TupleLiteralExp(parts, tupleType);
            return tupleLiteral;
        }

        CollectionType CreateCollectionType(CollectionKind kind, Classifier type) {
            CollectionType collectionType;
            switch (kind) {
                case CollectionKind.Bag:
                    collectionType = new BagType(TypesTable, type);
                    break;
                case CollectionKind.Collection:
                    collectionType = new CollectionType(TypesTable,type);
                    break;
                case CollectionKind.OrderedSet:
                    collectionType = new OrderedSetType(TypesTable,type);
                    break;
                case CollectionKind.Sequence:
                    collectionType = new SequenceType(TypesTable,type);
                    break;
                case CollectionKind.Set:
                    collectionType = new SetType(TypesTable,type);
                    break;

                default:
                    collectionType = null;
                    System.Diagnostics.Debug.Fail("CreateCollectionType( ... ): missing case for CollectionKind.");
                    break;
            }

            TypesTable.RegisterCompositeType(collectionType);

            return collectionType;
        }


        TupleType CreateTupleType(CommonToken rootToken, List<VariableDeclaration> variables) {
            TupleType tuple = new TupleType(TypesTable);
            foreach (var variable in variables) {
                if (tuple.TupleParts.Keys.Contains(variable.Name)) {// Kontrola nad ramec specifikace
                    Errors.AddError(new CodeErrorItem(String.Format("Name {0} is used multipled.", variable.Name), rootToken, rootToken));
                    continue;
                }
                tuple.TupleParts.Add(new Property(variable.Name, PropertyType.One, variable.PropertyType));
            }

            TypesTable.RegisterCompositeType(tuple);

            return tuple;
        }

        VariableDeclaration CreateVariableDeclaration(CommonToken name, Classifier type, AST.OclExpression value, VariableDeclarationRequirement requirement) {
            Classifier finallType = type;
            switch (requirement) {
                case VariableDeclarationRequirement.Iterator:
                    bool okConditionIterator = type != null && value == null;
                    if (okConditionIterator == false) {
                        type = TypesTable.Library.Invalid;
                        value = null;
                        Errors.AddError(new CodeErrorItem("The loop variable of an iterator expression has no init expression.", name, name));
                    }
                    break;
                case VariableDeclarationRequirement.TupleType:
                    bool okCondition = type != null && value == null;
                    if (okCondition == false) {
                        type = TypesTable.Library.Invalid;
                        value = null;
                        Errors.AddError(new CodeErrorItem("Of all VariableDeclarations the initExpression must be empty and the type must exist.", name, name));
                    }
                    break;
                case VariableDeclarationRequirement.TupleLiteral:
                    bool okConditionLit = type != null && value != null;
                    if (okConditionLit == false) {
                        type = TypesTable.Library.Invalid;
                        value = new AST.ErrorExp(TypesTable.Library.Invalid);
                        Errors.AddError(new CodeErrorItem("The initExpression and type of all VariableDeclarations must exist.", name, name));
                    }
                    break;
                default:
                    System.Diagnostics.Debug.Fail("CreateVariableDeclaration( ... ): missing case for VariableDeclarationRequirement.");
                    break;
            }

            if (value != null) {
                if (type == null) {
                    finallType = value.Type;
                }
                else {
                    //rozsirene chovani oprati spec.
                    if (value.Type.ConformsTo(type) == false) {
                        Errors.AddError(new CodeErrorItem(String.Format("Type {0} does not conforms to {1}.", value.Type.Name, type.Name), name, name));
                        //Nastevena zustane hodnova z type, chovani dle spec.
                    }
                }
            }
            return new VariableDeclaration(name.Text, type, value);
        }

        Classifier ResolveTypePathName(List<string> path, CommonToken FirstPathToken, CommonToken LastPathToken) {
            ModelElement foundType = Environment.LookupPathName(path);



            if (foundType == null) {
                foundType = TypesTable.Library.Invalid;
                Errors.AddError(new CodeErrorItem(String.Format("Path {0} do not exists.", path), FirstPathToken, LastPathToken));
            }


            if (foundType is Classifier == false) {
                foundType = TypesTable.Library.Invalid;
                Errors.AddError(new CodeErrorItem(String.Format("Path {0} do not referres type.", path), FirstPathToken, LastPathToken));
            }

            return ((Classifier)foundType);
        }


        List<VariableDeclaration> CreatevariableDeclarationList(VariableDeclaration decl, List<VariableDeclaration> exist) {
            List<VariableDeclaration> output;
            if (exist != null) {
                output = exist;
            }
            else {
                output = new List<VariableDeclaration>();
            }
            output.Add(decl);
            return output;
        }
    }
}
