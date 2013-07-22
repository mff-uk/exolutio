using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;


namespace Tests.CodeContracts.Support {
    
    [TestFixture]
    public class BagSupportTest {
        
  


        /// <summary>
        /// Test Equals method of Bag
        /// </summary>
        [Test]
        public void EqualityTest()
        {
            Assert.IsTrue(TestUtils.CreateIntBag(1, 2, 2, 3).Equals(TestUtils.CreateIntBag(1, 2, 2, 3)));
            Assert.IsFalse(TestUtils.CreateIntBag(1, 2, 2, 3).Equals(TestUtils.CreateIntBag(1, 2, 3, 3)));
            Assert.IsTrue(TestUtils.CreateIntBag(1, 2, 2, 3).Equals(TestUtils.CreateIntBag(3, 2, 1, 2)));
            Assert.IsFalse(TestUtils.CreateIntBag(1, 2, 2, 3).Equals(TestUtils.CreateIntBag(3, 2, 1, 1)));
        }
        /// <summary>
        /// Test OCL operations of Bag 
        /// </summary>
        [Test]
        public void OperationsTest()
        {
            OclBag bag = TestUtils.CreateIntBag(1,1,2);
            OclBag bag2 = TestUtils.CreateIntBag(2, 3);
            OclBag bag3 = TestUtils.CreateIntBag(1, 2, 2);

            TestUtils.AreEqual((OclInteger)3, bag.size());
            TestUtils.AreEqual((OclBoolean)true, bag.includes((OclInteger)1));
            TestUtils.AreEqual((OclBoolean)false, bag.excludes((OclInteger)1));
            TestUtils.AreEqual((OclInteger)2, bag.count((OclInteger)1));
            TestUtils.AreEqual((OclBoolean)false, bag.includesAll(TestUtils.CreateIntSet(1, 4)));
            TestUtils.AreEqual((OclBoolean)false, bag.excludesAll(TestUtils.CreateIntSet(1, 4)));
            TestUtils.AreEqual((OclBoolean)false, bag.isEmpty());
            TestUtils.AreEqual((OclBoolean)true, bag.notEmpty());
            

            TestUtils.AreEqual(TestUtils.CreateIntBag(1, 1, 2, 2, 3), bag.union(OclInteger.Type, bag2));
            TestUtils.AreEqual(TestUtils.CreateIntBag(1, 1, 2, 2, 3), bag.union(OclInteger.Type, TestUtils.CreateIntSet(2, 3)));
            TestUtils.AreEqual(TestUtils.CreateIntBag(1, 2), bag.intersection(bag3));
            TestUtils.AreEqual(TestUtils.CreateIntSet(2), bag.intersection(TestUtils.CreateIntSet(2, 3)));
            TestUtils.AreEqual(TestUtils.CreateIntBag(1, 1, 1, 2), bag.including(OclInteger.Type, (OclInteger)1));
            TestUtils.AreEqual(TestUtils.CreateIntBag(2), bag.excluding(OclInteger.Type, (OclInteger)1));
            TestUtils.AreEqual((OclInteger)2, bag.count((OclInteger)1));

        }

        /// <summary>
        /// Test OCL iterations of Bag 
        /// </summary>
        [Test]
        public void IterationsTest()
        {
            TestUtils.AreEqual(TestUtils.CreateIntBag(0, 0, 1, 1), TestUtils.CreateIntBag(1, 1, 2, 3).collectNested<OclInteger, OclInteger>(OclInteger.Type, x => x.div((OclInteger)2)));
            TestUtils.AreEqual(TestUtils.CreateIntSequence(3, 2, 1, 1), TestUtils.CreateIntBag(1, 1, 2, 3).sortedBy<OclInteger, OclInteger>(x => x.op_UnaryNegation()));
            TestUtils.AreEqual(TestUtils.CreateIntBag(2, 3), TestUtils.CreateIntBag(1, 1, 2, 3).select<OclInteger>( x => x.op_GreaterThan((OclInteger)1)));
            TestUtils.AreEqual(TestUtils.CreateIntBag(1, 1), TestUtils.CreateIntBag(1, 1, 2, 3).reject<OclInteger>(x => x.op_GreaterThan((OclInteger)1)));

            TestUtils.AreEqual((OclInteger)3, TestUtils.CreateIntBag(1, 1, 2, 3).any<OclInteger>(x => x.op_GreaterThan((OclInteger)2)));

            TestUtils.AreEqual(OclBoolean.False, TestUtils.CreateIntBag(1, 1, 2, 3).forAll<OclInteger>(x => x.op_GreaterThan((OclInteger)1)));
            TestUtils.AreEqual(OclBoolean.True, TestUtils.CreateIntBag(2, 3).forAll<OclInteger>(x => x.op_GreaterThan((OclInteger)1)));

            TestUtils.AreEqual(OclBoolean.True, TestUtils.CreateIntBag(1, 1, 2, 3).exists<OclInteger>(x => x.op_GreaterThan((OclInteger)1)));
            TestUtils.AreEqual(OclBoolean.False, TestUtils.CreateIntBag(1, 1).exists<OclInteger>(x => x.op_GreaterThan((OclInteger)1)));

            TestUtils.AreEqual(OclBoolean.False, TestUtils.CreateIntBag(1, 1, 2, 3).one<OclInteger>(x => x.op_GreaterThan((OclInteger)1)));
            TestUtils.AreEqual(OclBoolean.True, TestUtils.CreateIntBag(1, 1, 2).one<OclInteger>(x => x.op_GreaterThan((OclInteger)1)));

            TestUtils.AreEqual(OclBoolean.False, TestUtils.CreateIntBag(1, 2, 3).isUnique<OclInteger, OclInteger>(x => x.div((OclInteger)2)));
            TestUtils.AreEqual(OclBoolean.True, TestUtils.CreateIntBag(1, 3).isUnique<OclInteger, OclInteger>(x => x.div((OclInteger)2)));
            
        }

        /// <summary>
        /// Test OCL min, max and sum operations of OclBag
        /// </summary>
        [Test]
        public void MinMaxSumTest()
        {
            OclBag source = TestUtils.CreateIntBag(1, 2, 3, 1);
            TestUtils.AreEqual((OclInteger)7, source.sum((OclInteger)0, (x, y) => x + y));
            TestUtils.AreEqual((OclInteger)1, source.min<OclInteger>((x, y) => x.min(y)));
            TestUtils.AreEqual((OclInteger)3, source.max<OclInteger>((x, y) => x.max(y)));
        }

        /// <summary>
        /// Test OCL product operation of OclBag
        /// </summary>
        [Test]
        public void ProductTest()
        {
            OclBag a = TestUtils.CreateIntBag(1, 2);
            OclBag b = TestUtils.CreateIntBag(1, 3);
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
        /// Test closure iteration of OclBag
        /// </summary>
        [Test]
        public void UnorderedClosureTest()
        {
            
            Func<OclInteger, OclAny> body = x=>new OclSequence(OclInteger.Type,x.div((OclInteger)2), x.op_UnaryNegation());

            OclBag l = TestUtils.CreateIntBag();
            TestUtils.AreEqual(TestUtils.CreateIntSet(), l.closure(OclInteger.Type, body));
            l = l.including(OclInteger.Type, (OclInteger)3);
            TestUtils.AreEqual(TestUtils.CreateIntSet(3, 1, 0, -1, -3), l.closure(OclInteger.Type, body));
            l = l.including(OclInteger.Type, (OclInteger)16);
            OclSet set = TestUtils.CreateIntSet(0, 1, 2, 3, 4, 8, 16, -1, -2, -3, -4, -8, -16);
            TestUtils.AreEqual(set, l.closure(OclInteger.Type, body));
            l = l.including(OclInteger.Type, (OclInteger)(-4));
            TestUtils.AreEqual(set, l.closure(OclInteger.Type, body));
        }

      
    }

}
