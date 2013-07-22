using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class TypeSupportTest {

        private void TestConformant(OclClassifier classifier, OclClassifier to)
        {
            TestUtils.AreEqual(OclBoolean.True, classifier.conformsTo(to));
            TestUtils.AreEqual(OclBoolean.False, to.conformsTo(classifier));
        }

        private void TestNonConformant(OclClassifier classifier, OclClassifier to)
        {
            TestUtils.AreEqual(OclBoolean.False, classifier.conformsTo(to));
            TestUtils.AreEqual(OclBoolean.False, to.conformsTo(classifier));
        }

        private void TestConformantToSelf(OclClassifier classifier)
        {
            TestUtils.AreEqual(OclBoolean.True, classifier.conformsTo(classifier));
        }
  

        [Test]
        public void SimpleConformanceTest()
        {
            TestConformant(OclBoolean.Type,OclAny.Type);
            TestConformant(OclInteger.Type,OclAny.Type);
            TestConformant(OclInteger.Type,OclReal.Type);
            TestConformantToSelf(OclInteger.Type);
            

            TestConformant(OclUnlimitedNatural.Type,OclAny.Type);
            TestConformant(OclUnlimitedNatural.Type,OclReal.Type);
            TestConformantToSelf(OclUnlimitedNatural.Type);

            TestConformant(OclVoidType.OclVoid, OclAny.Type);
            TestConformant(OclVoidType.OclVoid, OclBoolean.Type);
            TestConformant(OclVoidType.OclVoid, OclString.Type);
            TestConformantToSelf(OclVoidType.OclVoid);

            TestConformant(InvalidType.OclInvalid,OclAny.Type);
            TestConformant(InvalidType.OclInvalid, OclVoidType.OclVoid);
            TestConformantToSelf(InvalidType.OclInvalid);

            TestNonConformant(OclInteger.Type, OclString.Type);
            TestNonConformant(OclInteger.Type, OclBoolean.Type);
        }

        [Test]
        public void CollectionConformanceTest()
        {
            //Collections conform to OclAny
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Collection, OclAny.Type),OclAny.Type);
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Sequence, OclAny.Type),OclAny.Type);

            TestConformant(OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type),OclAny.Type);
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type),OclAny.Type);

            //OclVoid conforms to collections
            TestConformant(OclVoidType.OclVoid, OclCollectionType.Collection(OclCollectionKind.Collection, OclAny.Type));
            TestConformant(OclVoidType.OclVoid, OclCollectionType.Collection(OclCollectionKind.Sequence, OclAny.Type));

            TestConformant(OclVoidType.OclVoid, OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type));
            TestConformant(OclVoidType.OclVoid, OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type));

            //Colections conform to colection of the same kind or kind Collection and conformant element types
            TestConformantToSelf(OclCollectionType.Collection(OclCollectionKind.Collection, OclAny.Type));

            TestConformantToSelf(OclCollectionType.Collection(OclCollectionKind.Sequence, OclAny.Type));
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Sequence, OclAny.Type), OclCollectionType.Collection(OclCollectionKind.Collection, OclAny.Type));

            TestConformantToSelf(OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type));
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type), OclCollectionType.Collection(OclCollectionKind.Collection, OclAny.Type));

            TestConformantToSelf(OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type));
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type), OclCollectionType.Collection(OclCollectionKind.Collection, OclAny.Type));
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type), OclCollectionType.Collection(OclCollectionKind.Sequence, OclAny.Type));
            TestConformant(OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type), OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type));

            TestNonConformant(OclCollectionType.Collection(OclCollectionKind.Set, OclInteger.Type), OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type));
            TestNonConformant(OclCollectionType.Collection(OclCollectionKind.Set, OclInteger.Type), OclCollectionType.Collection(OclCollectionKind.Sequence, OclAny.Type));
            TestNonConformant(OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type), OclCollectionType.Collection(OclCollectionKind.Collection, OclString.Type));
        }
        [Test]
        public void CollectionTypeTest()
        {
            TestUtils.AreEqual(new OclSequence(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.Sequence, OclInteger.Type));
            TestUtils.AreNotEqual(new OclSequence(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type));
            TestUtils.AreEqual(new OclSet(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.Set, OclInteger.Type));
            TestUtils.AreNotEqual(new OclSet(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type));
            TestUtils.AreEqual(new OclOrderedSet(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.OrderedSet, OclInteger.Type));
            TestUtils.AreNotEqual(new OclOrderedSet(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type));
            TestUtils.AreEqual(new OclBag(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.Bag, OclInteger.Type));
            TestUtils.AreNotEqual(new OclBag(OclInteger.Type).oclType(), OclCollectionType.Collection(OclCollectionKind.Collection, OclInteger.Type));
        }

        [Test]
        public void CollectionIncludingTypeTest()
        {
            TestUtils.AreEqual(OclCollectionType.Collection(OclCollectionKind.Sequence,OclReal.Type),new OclSequence(OclInteger.Type).including(OclReal.Type, (OclReal)0.5).oclType());
        }

        /// <summary>
        /// Test OclTupleType equality and conformance
        /// </summary>
        [Test]
        public void TupleTypeTest()
        {
            OclClassifier type1 = OclTupleType.Tuple(OclTupleType.Part("i", OclInteger.Type), OclTupleType.Part("s", OclString.Type));
            OclClassifier type11 = OclTupleType.Tuple(OclTupleType.Part("i", OclInteger.Type), OclTupleType.Part("s", OclString.Type));
            OclClassifier type2 = OclTupleType.Tuple(OclTupleType.Part("i", OclReal.Type), OclTupleType.Part("s", OclString.Type));
            OclClassifier type3 = OclTupleType.Tuple(OclTupleType.Part("x", OclInteger.Type), OclTupleType.Part("s", OclString.Type));
            OclClassifier type4 = OclTupleType.Tuple(OclTupleType.Part("s", OclString.Type),OclTupleType.Part("i", OclInteger.Type));

            TestUtils.AreEqual(type1, type11);
            TestUtils.AreNotEqual(type1, type2);
            TestUtils.AreNotEqual(type1, type3);
            TestUtils.AreNotEqual(type1, type4);

            TestConformantToSelf(type1);
            TestUtils.AreEqual(OclBoolean.True, type1.conformsTo(type11));
            TestUtils.AreEqual(OclBoolean.True, type11.conformsTo(type1));

            TestConformant(type1, type2);
            TestNonConformant(type1, type3);
            TestNonConformant(type1, type4);
        }
        private interface I1{}
        private interface I2:I1 { }
        private interface I3:I1 { }
        private class A : I1      {        }
        private class B : A, I2    {        }
        private class C : A, I3{ }
        /// <summary>
        /// Test OclObjectType equality and conformance
        /// </summary>
        [Test]
        public void ObjectTypeTest()
        {
            TestConformant(OclObjectType.Get<I2>(), OclObjectType.Get<I1>());
            TestConformant(OclObjectType.Get<I3>(), OclObjectType.Get<I1>());
            TestNonConformant(OclObjectType.Get<I2>(), OclObjectType.Get<I3>());

            TestConformant(OclObjectType.Get<B>(), OclObjectType.Get<A>());
            TestConformant(OclObjectType.Get<C>(), OclObjectType.Get<A>());
            TestNonConformant(OclObjectType.Get<B>(), OclObjectType.Get<C>());

            TestConformant(OclObjectType.Get<A>(), OclObjectType.Get<I1>());
            TestNonConformant(OclObjectType.Get<A>(), OclObjectType.Get<I2>());
            TestNonConformant(OclObjectType.Get<A>(), OclObjectType.Get<I3>());
            TestConformant(OclObjectType.Get<B>(), OclObjectType.Get<I1>());
            TestConformant(OclObjectType.Get<B>(), OclObjectType.Get<I2>());
            TestNonConformant(OclObjectType.Get<B>(), OclObjectType.Get<I3>());
            TestConformant(OclObjectType.Get<C>(), OclObjectType.Get<I1>());
            TestConformant(OclObjectType.Get<C>(), OclObjectType.Get<I3>());
            TestNonConformant(OclObjectType.Get<C>(), OclObjectType.Get<I2>());

            TestConformant(OclObjectType.Get<I2>(), OclAny.Type);
            TestConformant(OclVoidType.OclVoid, OclObjectType.Get<I2>());
        }
        private enum E1 { }
        private enum E2 { }
        /// <summary>
        /// Test OclEnumType equality and conformance
        /// </summary>
        [Test]
        public void EnumType()
        {
            TestUtils.AreEqual(OclEnumType.Enum(typeof(E1)), OclEnum<E1>.Type);

            TestNonConformant(OclEnum<E1>.Type, OclEnum<E2>.Type);

            TestConformant(OclEnum<E1>.Type, OclAny.Type);
            TestConformant(OclVoidType.OclVoid, OclEnum<E1>.Type);
        }
    }

}
