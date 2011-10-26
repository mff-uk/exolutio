using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;

namespace Tests.OCL
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
            TypesTable typesTalbe = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTalbe);

            Classifier integerType = typesTalbe.Library.Integer;

            Assert.IsTrue(integerType.ConformsTo(integerType));

            Classifier realType = typesTalbe.Library.Real;

            Assert.IsTrue(realType.ConformsTo(realType));

            CollectionType collType = new CollectionType(typesTalbe,integerType);
            CollectionType collType2 = new CollectionType(typesTalbe, integerType);
            typesTalbe.RegisterType(collType);
            typesTalbe.RegisterType(collType2);

            Assert.IsTrue(collType.ConformsTo(collType));
            Assert.IsTrue(collType.ConformsTo(collType2));

            TupleType tuple = new TupleType(typesTalbe);
            tuple.TupleParts.Add(new Property("ahoj", PropertyType.One, integerType));
            typesTalbe.RegisterType(tuple);

            TupleType tuple2 = new TupleType(typesTalbe);
            tuple2.TupleParts.Add(new Property("ahoj", PropertyType.One, integerType));
            typesTalbe.RegisterType(tuple2);

            Assert.IsTrue(tuple.ConformsTo(tuple));
            Assert.IsTrue(tuple.ConformsTo(tuple2));

            CollectionType bagType = new BagType(typesTalbe,integerType);
            CollectionType bagType2 = new BagType(typesTalbe,integerType);
            typesTalbe.RegisterType(bagType);
            typesTalbe.RegisterType(bagType2);
            Assert.IsTrue(bagType.ConformsTo(bagType));
            Assert.IsTrue(bagType.ConformsTo(bagType2));


            CollectionType setType = new SetType(typesTalbe,integerType);
            CollectionType setType2 = new SetType(typesTalbe,integerType);
            typesTalbe.RegisterType(setType);
            typesTalbe.RegisterType(setType2);
            Assert.IsTrue(setType.ConformsTo(setType));
            Assert.IsTrue(setType.ConformsTo(setType2));

            Classifier voidType = typesTalbe.Library.Void;
            typesTalbe.RegisterType(voidType);
            Assert.IsTrue(voidType.ConformsTo(voidType));

            Classifier anyType = typesTalbe.Library.Any;
            typesTalbe.RegisterType(anyType);
            Assert.IsTrue(anyType.ConformsTo(anyType));

            Classifier invalidType = typesTalbe.Library.Invalid;
            typesTalbe.RegisterType(invalidType);
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

            TupleType tuple = new TupleType(typesTalbe);
            tuple.TupleParts.Add(new Property("prop1", PropertyType.One, integerType));
            Assert.AreEqual(tuple.Name, "Tuple(prop1:Integer)");
            tuple.TupleParts.Add(new Property("prop2", PropertyType.One, integerType));
            typesTalbe.RegisterType(tuple);

            TupleType tuple2 = new TupleType(typesTalbe);
            tuple2.TupleParts.Add(new Property("prop1", PropertyType.One, realType));
            tuple2.TupleParts.Add(new Property("prop2", PropertyType.One, integerType));
            typesTalbe.RegisterType(tuple2);


            Assert.IsTrue(tuple.ConformsTo(tuple2));
            Assert.IsFalse(tuple2.ConformsTo(tuple));

            TupleType tuple3 = new TupleType(typesTalbe);
            tuple3.TupleParts.Add(new Property("prop1", PropertyType.One, realType));
            tuple3.TupleParts.Add(new Property("prop2", PropertyType.One, integerType));
            tuple3.TupleParts.Add(new Property("prop3", PropertyType.One, anyType));
            typesTalbe.RegisterType(tuple3);

            Assert.IsFalse(tuple.ConformsTo(tuple3));
            Assert.IsTrue(tuple.ConformsTo(anyType));


            Assert.IsTrue(tuple["prop1"].Type == integerType);
            Assert.AreEqual(tuple.Name, "Tuple(prop1:Integer,prop2:Integer)");
            
            
           

        }

        [Test]
        public void CollectionTest()
        {
            TypesTable typesTalbe = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTalbe);

            Classifier integerType = typesTalbe.Library.Integer;
            Classifier realType = typesTalbe.Library.Real;
            Classifier anyType = typesTalbe.Library.Any;

            BagType bagInteger = new BagType(typesTalbe,integerType);
            BagType bagReal = new BagType(typesTalbe, realType);
            SetType setType = new SetType(typesTalbe, integerType);
            SequenceType seqType = new SequenceType(typesTalbe, integerType);
            OrderedSetType ordType = new OrderedSetType(typesTalbe, integerType);
            CollectionType collInteger = new CollectionType(typesTalbe, integerType);



            typesTalbe.RegisterType(bagInteger);
            typesTalbe.RegisterType(bagReal);
            typesTalbe.RegisterType(setType);
            typesTalbe.RegisterType(seqType);
            typesTalbe.RegisterType(ordType);
            typesTalbe.RegisterType(collInteger);


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

            Assert.IsTrue(bagInteger == new BagType( typesTalbe,integerType));
            Assert.IsFalse(bagInteger == null);
            Assert.IsFalse(bagInteger == bagReal);
            Assert.IsFalse(bagInteger.Equals(null));


            Assert.IsTrue(bagInteger.ConformsTo(anyType));
        }

        [Test]
        public void ClassTest()
        {
            TypesTable typesTalbe = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTalbe);

            Classifier integerType = typesTalbe.Library.Integer;
            Classifier realType = typesTalbe.Library.Real;
            Classifier anyType = typesTalbe.Library.Any;

            

            Class baseClass = new Class(typesTalbe,"Base");

            baseClass.Properties.Add(new Property("Name", PropertyType.One, integerType));
            baseClass.Operations.Add(new Operation("Work", true, integerType, new Parameter[] { }));

            Class A = new Class(typesTalbe,"A");
            Class B = new Class(typesTalbe,"B");

            A.SuperClass.Add(baseClass);
            B.SuperClass.Add(baseClass);

            typesTalbe.RegisterType(baseClass);
            typesTalbe.RegisterType(A);
            typesTalbe.RegisterType(B);
            
            Assert.IsTrue(B.ConformsTo(baseClass));
            Assert.IsFalse(B.ConformsTo(A));
        }

        [Test]
        public void NamespaceTest()
        {
            TypesTable typesTalbe = new TypesTable();
            StandardLibraryCreator sl = new StandardLibraryCreator();
            sl.CreateStandardLibrary(typesTalbe);

            Classifier integerType = typesTalbe.Library.Integer;
            Classifier realType = typesTalbe.Library.Real;
            Classifier anyType = typesTalbe.Library.Any;

            Class baseClass = new Class(typesTalbe,"Base");

            baseClass.Properties.Add(new Property("Name", PropertyType.One, integerType));
            baseClass.Operations.Add(new Operation("Work", true, integerType, new Parameter[] { }));

            Class A = new Class(typesTalbe,"A");
            Class B = new Class(typesTalbe,"B");

            A.SuperClass.Add(baseClass);
            B.SuperClass.Add(baseClass);

            typesTalbe.RegisterType(baseClass);
            typesTalbe.RegisterType(A);
            typesTalbe.RegisterType(B);
            Assert.IsTrue(B.ConformsTo(baseClass));
            Assert.IsFalse(B.ConformsTo(A));

            Namespace basic = new Namespace("");

            Namespace subNamespace = new Namespace("subNamespace");
            basic.NestedNamespace.Add(subNamespace);
            subNamespace.NestedClassifier.Add(baseClass);

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
            BagType bagIntegerType = new BagType(typesTable,integerType);
            BagType bagRealType = new BagType(typesTable, realType);
            SetType setType = new SetType(typesTable, integerType);
            SequenceType seqType = new SequenceType(typesTable, integerType);
            OrderedSetType ordType = new OrderedSetType(typesTable, integerType);
            CollectionType collIntegerType = new CollectionType(typesTable, integerType);
            CollectionType collRealType = new CollectionType(typesTable, realType);

            typesTable.RegisterType(bagIntegerType);
            typesTable.RegisterType(bagRealType);
            typesTable.RegisterType(setType);
            typesTable.RegisterType(seqType);
            typesTable.RegisterType(ordType);
            typesTable.RegisterType(collIntegerType);

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
            TupleType tuple1 = new TupleType(typesTable);
            tuple1.TupleParts.Add(new Property("prop1", PropertyType.Many, integerType));
            tuple1.TupleParts.Add(new Property("prop2", PropertyType.Many, integerType));
            tuple1.TupleParts.Add(new Property("prop3", PropertyType.Many, realType));

            typesTable.RegisterType(tuple1);

            TupleType tuple2 = new TupleType(typesTable);
            tuple2.TupleParts.Add(new Property("prop1", PropertyType.Many, integerType));
            tuple2.TupleParts.Add(new Property("prop4", PropertyType.Many, integerType));
            tuple2.TupleParts.Add(new Property("prop3", PropertyType.One, anyType));

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
