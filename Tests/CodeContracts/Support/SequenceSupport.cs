using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;


namespace Tests.CodeContracts.Support {
    
    [TestFixture]
    public class SequenceSupportTest {
        
        /// <summary>
        /// Test Equals method of OclSequence
        /// </summary>
        [Test]
        public void EqualityTest()
        {
            Assert.IsTrue(TestUtils.CreateIntSequence( 1, 2, 2, 3 ).Equals(TestUtils.CreateIntSequence( 1, 2, 2, 3 )));
            Assert.IsFalse(TestUtils.CreateIntSequence( 1, 2, 2, 3 ).Equals(TestUtils.CreateIntSequence( 1, 2, 3, 3 )));
            Assert.IsFalse(TestUtils.CreateIntSequence( 1, 2, 2, 3 ).Equals(TestUtils.CreateIntSequence( 3, 2, 1, 2 )));
            Assert.IsFalse(TestUtils.CreateIntSequence( 1, 2, 2, 3 ).Equals(TestUtils.CreateIntSequence( 3, 2, 1, 1 )));
        }

        /// <summary>
        /// Test OCL operations of OclSequence
        /// </summary>
        [Test]
        public void OperationsTest()
        {
            OclSequence source = TestUtils.CreateIntSequence(1, 2, 3, 1);

            TestUtils.AreEqual((OclInteger)4, source.size());
            TestUtils.AreEqual((OclBoolean)true, source.includes((OclInteger)1));
            TestUtils.AreEqual((OclBoolean)false, source.excludes((OclInteger)1));
            TestUtils.AreEqual((OclInteger)2, source.count((OclInteger)1));
            TestUtils.AreEqual((OclBoolean)false, source.includesAll(TestUtils.CreateIntSet(1,4)));
            TestUtils.AreEqual((OclBoolean)false, source.excludesAll(TestUtils.CreateIntSet(1, 4)));
            TestUtils.AreEqual((OclBoolean)false, source.isEmpty());
            TestUtils.AreEqual((OclBoolean)true, source.notEmpty());
            

            TestUtils.AreEqual(TestUtils.CreateIntSequence(5, 1, 2, 3, 1), source.prepend(OclInteger.Type, (OclInteger)5));
            TestUtils.AreEqual(source.append(OclInteger.Type, (OclInteger)5), TestUtils.CreateIntSequence(1, 2, 3, 1, 5));
            TestUtils.AreEqual(source.insertAt(OclInteger.Type, (OclInteger)3, (OclInteger)5), TestUtils.CreateIntSequence(1, 2, 5, 3, 1));
            TestUtils.AreEqual(source.excluding((OclInteger)1),TestUtils.CreateIntSequence(2, 3));
            TestUtils.AreEqual(source.union(OclInteger.Type, source), TestUtils.CreateIntSequence(1, 2, 3, 1, 1, 2, 3, 1));
            TestUtils.AreEqual(source.reverse(),TestUtils.CreateIntSequence(1, 3, 2, 1));
            TestUtils.AreEqual(source.subSequence((OclInteger)2, (OclInteger)3),TestUtils.CreateIntSequence(2, 3));
            TestUtils.AreEqual((OclInteger)1, source.at<OclInteger>((OclInteger)4));
        }

        /// <summary>
        /// Test OCL iterations of OclSequence
        /// </summary>
        [Test]
        public void IterationsTest()
        {
            OclSequence source = TestUtils.CreateIntSequence ( 1, 2, 3, 1 );
            TestUtils.AreEqual(source.select<OclInteger>(i => i.op_GreaterThanOrEqual((OclInteger)2)),TestUtils.CreateIntSequence(2, 3));
            TestUtils.AreEqual(source.reject<OclInteger>(i => i.op_GreaterThanOrEqual((OclInteger)2)),TestUtils.CreateIntSequence(1, 1));
            TestUtils.AreEqual(source.collectNested<OclInteger, OclInteger>(OclInteger.Type, i => i.op_Addition((OclInteger)2)),TestUtils.CreateIntSequence(3, 4, 5, 3));
            TestUtils.AreEqual(source.sortedBy<OclInteger, OclInteger>(i => i),TestUtils.CreateIntSequence(1, 1, 2, 3));

            TestUtils.AreEqual(source.collectToSequence<OclInteger>(OclInteger.Type, i => i.op_Addition((OclInteger)2)), TestUtils.CreateIntSequence(3, 4, 5, 3));

            TestUtils.AreEqual((OclInteger)2, source.any<OclInteger>(i => i.op_Equality((OclInteger)2)));
            TestUtils.AreEqual(OclBoolean.False, source.one<OclInteger>(i => i.op_Equality((OclInteger)1)));
            TestUtils.AreEqual(OclBoolean.True, source.one<OclInteger>(i => i.op_Equality((OclInteger)2)));
            TestUtils.AreEqual(OclBoolean.True, source.exists<OclInteger>(x => x < (OclInteger)2));
            TestUtils.AreEqual(OclBoolean.False, source.exists<OclInteger>(x => x < (OclInteger)1));
            TestUtils.AreEqual(OclBoolean.False, source.forAll<OclInteger>(x => x < (OclInteger)2));
            TestUtils.AreEqual(OclBoolean.True, source.forAll<OclInteger>(x => x < (OclInteger)4));
            TestUtils.AreEqual(OclBoolean.True, TestUtils.CreateIntSequence(1, 2, 3).isUnique<OclInteger, OclInteger>(i => i.op_Addition((OclInteger)2)));
            TestUtils.AreEqual(OclBoolean.False, TestUtils.CreateIntSequence(1, 2, 3).isUnique<OclInteger, OclInteger>(i => i.div((OclInteger)2)));
        }
        /// <summary>
        /// Test flatten operation of OclSequence
        /// </summary>
        [Test]
        public void FlattenTest()
        {
            OclSequence level1 = TestUtils.CreateIntSequence(1, 2);
            OclSequence level2 = new OclSequence(level1.oclType(), level1, level1);
            OclSequence level3 = new OclSequence(level2.oclType(), level2, level2);

            TestUtils.AreEqual(TestUtils.CreateIntSequence(1, 2),level1.flatten());
            TestUtils.AreEqual(TestUtils.CreateIntSequence(1, 2, 1, 2),level2.flatten());
            TestUtils.AreEqual(TestUtils.CreateIntSequence(1, 2, 1, 2, 1, 2, 1, 2),level3.flatten());
        }

        /// <summary>
        /// Test OCL min, max and sum operations of OclSequence
        /// </summary>
        [Test]
        public void MinMaxSumTest()
        {
            OclSequence source = TestUtils.CreateIntSequence(1, 2, 3, 1);
            TestUtils.AreEqual((OclInteger)7, source.sum((OclInteger)0, (x, y) => x + y));
            TestUtils.AreEqual((OclInteger)1, source.min<OclInteger>((x, y) => x.min(y)));
            TestUtils.AreEqual((OclInteger)3, source.max<OclInteger>((x, y) => x.max(y)));
        }

        /// <summary>
        /// Test OCL product operation of OclSequence
        /// </summary>
        [Test]
        public void ProductTest()
        {
            OclSequence a = TestUtils.CreateIntSequence(1, 2);
            OclSequence b = TestUtils.CreateIntSequence(1, 3);
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
        /// Test closure iteration of OclSequence
        /// </summary>
        [Test]
        public void OrderedClosureTest()
        {
            //Closure - depth first search
            OclSequence l = TestUtils.CreateIntSequence();
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(),l.closure<OclInteger>(OclInteger.Type,Plus2Plus3Mod12));
            l = l.append(OclInteger.Type, (OclInteger)0);
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(0, 2, 4, 6, 8, 10, 1, 3, 5, 7, 9, 11), l.closure<OclInteger>(OclInteger.Type, Plus2Plus3Mod12));
            l = l.append(OclInteger.Type, (OclInteger)2);
            l = l.append(OclInteger.Type, (OclInteger)5);
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(0, 2, 4, 6, 8, 10, 1, 3, 5, 7, 9, 11), l.closure<OclInteger>(OclInteger.Type, Plus2Plus3Mod12));

            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(0, 2, 4, 6, 8, 10, 5, 7, 9, 11, 1, 3), l.closure<OclInteger>(OclInteger.Type, (i) => (i.op_Addition((OclInteger)2).mod((OclInteger)12))));
        }

        private OclSequence Plus2Plus3Mod12(OclInteger i)
        {
            return TestUtils.CreateIntSequence(((int)i + 2) % 12, ((int)i + 3) % 12);
        }

    }

}
