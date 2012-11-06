using System.Collections.Generic;
using NUnit.Framework;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;

namespace Exolutio.Tests.OCL
{
    [TestFixture]
    public class TypeSystemTest
    {
        [Test]
        public void PrimitiveTypeConformsToTest()
        {

            TypesTable typesTalbe = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTalbe);

            Classifier integerType = typesTalbe.Library.Integer;
            Classifier realType = typesTalbe.Library.Real;
            Classifier unlimnaturalType = typesTalbe.Library.UnlimitedNatural;

            typesTalbe.RegisterType(integerType);
            typesTalbe.RegisterType(realType);
            typesTalbe.RegisterType(unlimnaturalType);

            Assert.IsTrue(integerType.ConformsTo(realType));
            Assert.IsFalse(realType.ConformsTo(integerType));
            Assert.IsTrue(unlimnaturalType.ConformsTo(integerType));
            Assert.IsFalse(integerType.ConformsTo(unlimnaturalType));
            Assert.IsTrue(unlimnaturalType.ConformsTo(realType));
            //Pridat dalsi
        }

        [Test]
        public void ReflexivityConformsToTest()
        {
            TypesTable typesTable = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTable);

            Classifier integerType = typesTable.Library.Integer;

            Assert.IsTrue(integerType.ConformsTo(integerType));

            Classifier realType = typesTable.Library.Real;

            Assert.IsTrue(realType.ConformsTo(realType));

            CollectionType collType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag, integerType) ;
            CollectionType collType2 = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag, integerType);
            typesTable.RegisterType(collType);
            typesTable.RegisterType(collType2);

            Assert.IsTrue(collType.ConformsTo(collType));
            Assert.IsTrue(collType.ConformsTo(collType2));

            List<Property> tupleParts1 = new List<Property>();
            tupleParts1.Add(new Property("ahoj", PropertyType.One, integerType));
            TupleType tuple = new TupleType(typesTable, tupleParts1);
            
            typesTable.RegisterType(tuple);

            List<Property> tupleParts2 = new List<Property>();
            tupleParts2.Add(new Property("ahoj", PropertyType.One, integerType));
            TupleType tuple2 = new TupleType(typesTable,tupleParts2);
           
            typesTable.RegisterType(tuple2);

            Assert.IsTrue(tuple.ConformsTo(tuple));
            Assert.IsTrue(tuple.ConformsTo(tuple2));

            CollectionType bagType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag,integerType);
            CollectionType bagType2 = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag, integerType);
            
            Assert.IsTrue(bagType.ConformsTo(bagType));
            Assert.IsTrue(bagType.ConformsTo(bagType2));


            CollectionType setType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Set, integerType);
            CollectionType setType2 = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Set, integerType); 
 
            Assert.IsTrue(setType.ConformsTo(setType));
            Assert.IsTrue(setType.ConformsTo(setType2));

            Classifier voidType = typesTable.Library.Void;
            typesTable.RegisterType(voidType);
            Assert.IsTrue(voidType.ConformsTo(voidType));

            Classifier anyType = typesTable.Library.Any;
            typesTable.RegisterType(anyType);
            Assert.IsTrue(anyType.ConformsTo(anyType));

            Classifier invalidType = typesTable.Library.Invalid;
            typesTable.RegisterType(invalidType);
            Assert.IsTrue(invalidType.ConformsTo(invalidType));
            //pridat class
        }

        [Test]
        public void TupleTest()
        {
            TypesTable typesTalbe = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTalbe);

            Classifier integerType = typesTalbe.Library.Integer;
            Classifier realType = typesTalbe.Library.Real;
            Classifier anyType = typesTalbe.Library.Any;

            typesTalbe.RegisterType(integerType);
            typesTalbe.RegisterType(realType);
            typesTalbe.RegisterType(anyType);

            List<Property> tupleParts1 = new List<Property>();
            tupleParts1.Add(new Property("prop1", PropertyType.One, integerType));
            tupleParts1.Add(new Property("prop2", PropertyType.One, integerType));
            TupleType tuple = new TupleType(typesTalbe, tupleParts1);
            
            typesTalbe.RegisterType(tuple);

            List<Property> tupleParts2 = new List<Property>();
            tupleParts2.Add(new Property("prop1", PropertyType.One, realType));
            tupleParts2.Add(new Property("prop2", PropertyType.One, integerType));
            TupleType tuple2 = new TupleType(typesTalbe,tupleParts2);
            
            typesTalbe.RegisterType(tuple2);


            Assert.IsTrue(tuple.ConformsTo(tuple2));
            Assert.IsFalse(tuple2.ConformsTo(tuple));

            List<Property> tupleParts3 = new List<Property>();
            tupleParts3.Add(new Property("prop1", PropertyType.One, realType));
            tupleParts3.Add(new Property("prop2", PropertyType.One, integerType));
            tupleParts3.Add(new Property("prop3", PropertyType.One, anyType));
            TupleType tuple3 = new TupleType(typesTalbe, tupleParts3);
            typesTalbe.RegisterType(tuple3);

            Assert.IsFalse(tuple.ConformsTo(tuple3));
            Assert.IsTrue(tuple.ConformsTo(anyType));


            Assert.IsTrue(tuple["prop1"].Type == integerType);
            Assert.AreEqual(tuple.Name, "Tuple(prop1:Integer,prop2:Integer)");
            
            
           

        }

        [Test]
        public void CollectionTest()
        {
            TypesTable typesTable = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTable);

            Classifier integerType = typesTable.Library.Integer;
            Classifier realType = typesTable.Library.Real;
            Classifier anyType = typesTable.Library.Any;

            CollectionType bagInteger = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag,integerType);
            CollectionType bagReal = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag, realType);
            CollectionType setType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Set, integerType);
            CollectionType seqType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Sequence, integerType); ;
            CollectionType ordType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.OrderedSet, integerType);
            CollectionType collInteger = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Collection, integerType);

            Assert.IsTrue(bagInteger.ConformsTo(bagReal));
            Assert.IsTrue(bagInteger.ConformsTo(collInteger));

            Assert.IsFalse(bagReal.ConformsTo(collInteger));
            Assert.IsFalse(bagInteger.ConformsTo(seqType));
            Assert.IsFalse(bagInteger.ConformsTo(setType));
            Assert.IsFalse(bagInteger.ConformsTo(ordType));

            Assert.IsFalse(seqType.ConformsTo(bagInteger));
            Assert.IsFalse(seqType.ConformsTo(setType));
            Assert.IsFalse(seqType.ConformsTo(ordType));

            Assert.IsFalse(ordType.ConformsTo(bagInteger));
            Assert.IsFalse(ordType.ConformsTo(setType));
            Assert.IsFalse(ordType.ConformsTo(seqType));

            Assert.IsFalse(setType.ConformsTo(bagInteger));
            Assert.IsFalse(setType.ConformsTo(ordType));
            Assert.IsFalse(setType.ConformsTo(seqType));

            Assert.IsTrue(bagInteger.CollectionKind == Exolutio.Model.OCL.CollectionKind.Bag);
            Assert.IsTrue(collInteger.CollectionKind == Exolutio.Model.OCL.CollectionKind.Collection);
            Assert.IsTrue(setType.CollectionKind == Exolutio.Model.OCL.CollectionKind.Set);
            Assert.IsTrue(seqType.CollectionKind == Exolutio.Model.OCL.CollectionKind.Sequence);
            Assert.IsTrue(ordType.CollectionKind == Exolutio.Model.OCL.CollectionKind.OrderedSet);



            Assert.AreEqual(bagInteger.Name, "Bag(" + integerType.QualifiedName + ")");
            Assert.AreEqual(collInteger.Name, "Collection(" + integerType.QualifiedName + ")");
            Assert.AreEqual(setType.Name, "Set(" + integerType.QualifiedName + ")");
            Assert.AreEqual(ordType.Name, "OrderedSet(" + integerType.QualifiedName + ")");
            Assert.AreEqual(seqType.Name, "Sequence(" + integerType.QualifiedName + ")");

            Assert.IsTrue(bagInteger == typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag,integerType));
            Assert.IsFalse(bagInteger == null);
            Assert.IsFalse(bagInteger == bagReal);
            Assert.IsFalse(bagInteger.Equals(null));


            Assert.IsTrue(bagInteger.ConformsTo(anyType));
        }

        [Test]
        public void ClassTest()
        {
            TypesTable typeTable = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typeTable);
            Namespace ns = typeTable.Library.RootNamespace;

            Classifier integerType = typeTable.Library.Integer;
            Classifier realType = typeTable.Library.Real;
            Classifier anyType = typeTable.Library.Any;

            

            Class baseClass = new Class(typeTable,ns,"Base");

            baseClass.Properties.Add(new Property("Name", PropertyType.One, integerType));
            baseClass.Operations.Add(new Operation("Work", true, integerType, new Parameter[] { }));

            Class A = new Class(typeTable,ns,"A");
            Class B = new Class(typeTable,ns,"B");

            A.SuperClass.Add(baseClass);
            B.SuperClass.Add(baseClass);

            typeTable.RegisterType(baseClass);
            typeTable.RegisterType(A);
            typeTable.RegisterType(B);
            
            Assert.IsTrue(B.ConformsTo(baseClass));
            Assert.IsFalse(B.ConformsTo(A));
        }

        [Test]
        public void NamespaceTest()
        {
            TypesTable typeTable = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typeTable);
            Namespace basic = typeTable.Library.RootNamespace;
            Namespace subNamespace = new Namespace("subNamespace",basic);

            Classifier integerType = typeTable.Library.Integer;
            Classifier realType = typeTable.Library.Real;
            Classifier anyType = typeTable.Library.Any;

            Class baseClass = new Class(typeTable, subNamespace, "Base");

            baseClass.Properties.Add(new Property("Name", PropertyType.One, integerType));
            baseClass.Operations.Add(new Operation("Work", true, integerType, new Parameter[] { }));

            Class A = new Class(typeTable, basic, "A");
            Class B = new Class(typeTable, basic, "B");

            A.SuperClass.Add(baseClass);
            B.SuperClass.Add(baseClass);

            typeTable.RegisterType(baseClass);
            typeTable.RegisterType(A);
            typeTable.RegisterType(B);
            Assert.IsTrue(B.ConformsTo(baseClass));
            Assert.IsFalse(B.ConformsTo(A));

            Assert.AreEqual(baseClass.ToString(), "::subNamespace::Base");
        }

        [Test]
        public void CommonSuperTypeTest()
        {
            TypesTable typesTable = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTable);

            Classifier integerType = typesTable.Library.Integer;
            Classifier realType = typesTable.Library.Real;
            Classifier anyType = typesTable.Library.Any;


            Classifier unlimitedType = typesTable.Library.UnlimitedNatural;
            Classifier stringType = typesTable.Library.String;
        
       


            Assert.AreEqual(integerType.CommonSuperType(realType), realType);//real,integer -> real
            Assert.AreEqual(realType.CommonSuperType(realType), realType);//real,real -> real
            Assert.AreEqual(realType.CommonSuperType(integerType), realType);//integer,real -> real
            Assert.AreEqual(unlimitedType.CommonSuperType(realType), realType);//unlimited,real -> real
            Assert.AreEqual(integerType.CommonSuperType(unlimitedType), integerType);//integer,unlimited -> integer
            Assert.AreEqual(stringType.CommonSuperType(integerType), anyType);// string,integer -> anytype


            //Collection
            BagType bagIntegerType =  (BagType)typesTable.Library.CreateCollection( Exolutio.Model.OCL.CollectionKind.Bag, integerType);
            BagType bagRealType = (BagType)typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Bag, realType);
            SetType setType = (SetType)typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Set, integerType);
            SequenceType seqType = (SequenceType)typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Sequence, integerType);
            OrderedSetType ordType = (OrderedSetType)typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.OrderedSet, integerType);
            CollectionType collIntegerType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Collection, integerType);
            CollectionType collRealType = typesTable.Library.CreateCollection(Exolutio.Model.OCL.CollectionKind.Collection,realType); ;


            Assert.AreEqual(setType.CommonSuperType(setType), setType); //set(integer),set(integer) -> set(integer)
            Assert.AreEqual(setType.CommonSuperType(ordType), collIntegerType); //set(integer),ord(integer) -> coll(integer)
            Assert.AreEqual(setType.CommonSuperType(collIntegerType), collIntegerType); //set(integer),coll(Integer) -> coll(integer)
            Assert.AreEqual(collIntegerType.CommonSuperType(collIntegerType), collIntegerType); //coll(integer),coll(integer) -> coll(integer)
            Assert.AreEqual(collIntegerType.CommonSuperType(bagIntegerType), collIntegerType); //coll(integer),bag(integer) -> coll(integer)
            Assert.AreEqual(collIntegerType.CommonSuperType(bagRealType), collRealType); // coll(Integer),bag(real) -> coll(real)
            Assert.AreEqual(setType.CommonSuperType(bagRealType), collRealType); // set(integer),bag(real) ->col(real)
            Assert.AreEqual(ordType.CommonSuperType(ordType), ordType); // ord(integer),ord(integer) -> ord(integer)
            Assert.AreEqual(seqType.CommonSuperType(seqType), seqType); // seq(integer),seq(integer) -> seq(integer)

            Assert.AreEqual(setType.CommonSuperType(realType), anyType); //set(integer),real -> any

            //void test
            Classifier voidType = typesTable.Library.Void;

            Assert.AreEqual(voidType.CommonSuperType(integerType), integerType);
            Assert.AreEqual(realType.CommonSuperType(voidType), realType);
            Assert.AreEqual(voidType.CommonSuperType(seqType), seqType);
            Assert.AreEqual(collIntegerType.CommonSuperType(voidType), collIntegerType);
            Assert.AreEqual(voidType.CommonSuperType(anyType), anyType);

            //any test
            Assert.AreEqual(integerType.CommonSuperType(anyType), anyType);
            Assert.AreEqual(anyType.CommonSuperType(stringType), anyType);
            Assert.AreEqual(anyType.CommonSuperType(setType), anyType);
            Assert.AreEqual(setType.CommonSuperType(anyType), anyType);

            //tuple test
            List<Property> tupleParts1 = new List<Property>();
            tupleParts1.Add(new Property("prop1", PropertyType.Many, integerType));
            tupleParts1.Add(new Property("prop2", PropertyType.Many, integerType));
            tupleParts1.Add(new Property("prop3", PropertyType.Many, realType));
            TupleType tuple1 = new TupleType(typesTable,tupleParts1);
            

            typesTable.RegisterType(tuple1);

            List<Property> tupleParts2 = new List<Property>();
            tupleParts2.Add(new Property("prop1", PropertyType.Many, integerType));
            tupleParts2.Add(new Property("prop4", PropertyType.Many, integerType));
            tupleParts2.Add(new Property("prop3", PropertyType.One, anyType));
            TupleType tuple2 = new TupleType(typesTable,tupleParts2);
            

            typesTable.RegisterType(tuple2);

            TupleType tupleCommon = (TupleType)tuple1.CommonSuperType(tuple2);

            Assert.AreEqual(tupleCommon.TupleParts.Count, 2);
            Assert.AreEqual(tupleCommon["prop1"].PropertyType, tuple1["prop1"].PropertyType);
            Assert.AreEqual(tupleCommon["prop1"].Type, tuple1["prop1"].Type);


            Assert.AreEqual(tupleCommon["prop3"].PropertyType, tuple1["prop3"].PropertyType);
            Assert.AreEqual(tupleCommon["prop3"].Type, tuple2["prop3"].Type);

            Assert.AreEqual(tuple1.CommonSuperType(voidType), tuple1);
            Assert.AreEqual(tuple1.CommonSuperType(anyType), anyType);

            Assert.AreEqual(voidType.CommonSuperType(tuple1), tuple1);
            Assert.AreEqual(anyType.CommonSuperType(tuple1), anyType);

            Assert.AreEqual(tuple2.CommonSuperType(integerType), anyType);
            Assert.AreEqual(realType.CommonSuperType(tuple2), anyType);

        }
    }
}
