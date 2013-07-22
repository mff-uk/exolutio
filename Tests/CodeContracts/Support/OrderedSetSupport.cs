using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;


namespace Tests.CodeContracts.Support {
    
    [TestFixture]
    public class OrderedSetSupportTest {
      
     

        /// <summary>
        /// Test Equals method of OclOrderedSet
        /// </summary>
        [Test]
        public void EqualityTest()
        {
            Assert.IsTrue(TestUtils.CreateIntOrderedSet(1, 2, 2, 3 ).Equals(TestUtils.CreateIntOrderedSet(1, 2, 2, 3 )));
            Assert.IsTrue(TestUtils.CreateIntOrderedSet(1, 2, 2, 3 ).Equals(TestUtils.CreateIntOrderedSet(1, 2, 3, 3)));
            Assert.IsFalse(TestUtils.CreateIntOrderedSet(1, 2, 2, 3 ).Equals(TestUtils.CreateIntOrderedSet(3, 2, 1, 2 )));
            Assert.IsFalse(TestUtils.CreateIntOrderedSet(1, 2, 2, 3 ).Equals(TestUtils.CreateIntOrderedSet(3, 2, 1, 1 )));
        }

        /// <summary>
        /// Test OCL operations of OclSet
        /// </summary>
        [Test]
        public void OperationsTest()
        {
            OclOrderedSet source = TestUtils.CreateIntOrderedSet(1, 2, 3, 1);

            TestUtils.AreEqual((OclInteger)3, source.size());
            TestUtils.AreEqual((OclBoolean)true, source.includes((OclInteger)1));
            TestUtils.AreEqual((OclBoolean)false, source.excludes((OclInteger)1));
            TestUtils.AreEqual((OclInteger)1, source.count((OclInteger)1));
            TestUtils.AreEqual((OclBoolean)false, source.includesAll(TestUtils.CreateIntSet(1, 4)));
            TestUtils.AreEqual((OclBoolean)false, source.excludesAll(TestUtils.CreateIntSet(1, 4)));
            TestUtils.AreEqual((OclBoolean)false, source.isEmpty());
            TestUtils.AreEqual((OclBoolean)true, source.notEmpty());

            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(1, 2, 3, 4), source.append(OclInteger.Type,(OclInteger) 4));
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(4, 1, 2, 3), source.prepend(OclInteger.Type, (OclInteger)4));
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(1, 4, 2, 3), source.insertAt(OclInteger.Type, (OclInteger)2, (OclInteger)4));
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(2, 3), source.subOrderedSet((OclInteger)2, (OclInteger)3));
            TestUtils.AreEqual((OclInteger)2, source.at<OclInteger>((OclInteger)2));
            TestUtils.AreEqual((OclInteger)3, source.indexOf((OclInteger)3));

            TestUtils.AreEqual((OclInteger)1, source.first<OclInteger>());
            TestUtils.AreEqual((OclInteger)3, source.last<OclInteger>());

            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(3, 2, 1), source.reverse());

        }

        /// <summary>
        /// Test OCL operations of OclOrderedSet
        /// </summary>
        [Test]
        public void IterationsTest()
        {
            OclOrderedSet source = TestUtils.CreateIntOrderedSet(1, 2, 3);
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(2, 3), source.select<OclInteger>(x => x >= (OclInteger)2));
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(1), source.reject<OclInteger>(x => x >= (OclInteger)2));
            TestUtils.AreEqual(TestUtils.CreateIntSequence(0, 1, 1), source.collectNested<OclInteger, OclInteger>(OclInteger.Type, x => x.div((OclInteger)2)));
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(3, 2, 1), source.sortedBy<OclInteger, OclInteger>(x => -x));

            TestUtils.AreEqual((OclInteger)1, source.any<OclInteger>(x => x < (OclInteger)2));
            TestUtils.AreEqual(OclBoolean.True, source.one<OclInteger>(x => x < (OclInteger)2));
            TestUtils.AreEqual(OclBoolean.False, source.one<OclInteger>(x => x < (OclInteger)3));
            TestUtils.AreEqual(OclBoolean.True, source.exists<OclInteger>(x => x < (OclInteger)2));
            TestUtils.AreEqual(OclBoolean.False, source.exists<OclInteger>(x => x < (OclInteger)1));
            TestUtils.AreEqual(OclBoolean.False, source.forAll<OclInteger>(x => x < (OclInteger)2));
            TestUtils.AreEqual(OclBoolean.True, source.forAll<OclInteger>(x => x < (OclInteger)4));
            TestUtils.AreEqual(OclBoolean.True, source.isUnique<OclInteger, OclReal>(x => x / (OclInteger)2));
            TestUtils.AreEqual(OclBoolean.False, source.isUnique<OclInteger, OclReal>(x => x.div((OclInteger)2)));
        }

        /// <summary>
        /// Test OCL min, max and sum operations of OclOrderedSet
        /// </summary>
        [Test]
        public void MinMaxSumTest()
        {
            OclOrderedSet source = TestUtils.CreateIntOrderedSet(1, 2, 3, 1);
            TestUtils.AreEqual((OclInteger)6, source.sum((OclInteger)0, (x, y) => x + y));
            TestUtils.AreEqual((OclInteger)1, source.min<OclInteger>((x, y) => x.min(y)));
            TestUtils.AreEqual((OclInteger)3, source.max<OclInteger>((x, y) => x.max(y)));
        }

        /// <summary>
        /// Test OCL product operation of OclOrderedSet
        /// </summary>
        [Test]
        public void ProductTest()
        {
            OclOrderedSet a = TestUtils.CreateIntOrderedSet(1, 2);
            OclOrderedSet b = TestUtils.CreateIntOrderedSet(1, 3);
            OclTupleType tupleType = OclTupleType.Tuple(OclTupleType.Part("first", OclInteger.Type), OclTupleType.Part("second", OclInteger.Type));
            OclSet pr = new OclSet(tupleType,
                new OclTuple(tupleType, (OclInteger)1, (OclInteger)1),
                new OclTuple(tupleType, (OclInteger)1, (OclInteger)3),
                new OclTuple(tupleType, (OclInteger)2, (OclInteger)1),
                new OclTuple(tupleType, (OclInteger)2, (OclInteger)3)
                );
            TestUtils.AreEqual(pr, a.product(b));
        }

        /// <summary>
        /// Test flatten operation of OclOrderedSet
        /// </summary>
        [Test]
        public void FlattenTest()
        {
            OclOrderedSet level1 = TestUtils.CreateIntOrderedSet(1, 2);
            OclOrderedSet level2 = new OclOrderedSet(level1.oclType(), level1, level1);
            OclOrderedSet level3 = new OclOrderedSet(level2.oclType(), level2, level2);

            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(1, 2), level1.flatten());
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(1, 2), level2.flatten());
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(1, 2), level3.flatten());
        }
    }

}
