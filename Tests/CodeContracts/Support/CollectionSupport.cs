using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;


namespace Tests.CodeContracts.Support {
    
    [TestFixture]
    public class CollectionSupportTest {
        /// <summary>
        /// Test collection conversions
        /// </summary>
        [Test]
        public void ConversionTest()
        {
            Assert.IsTrue(TestUtils.CreateIntOrderedSet(1, 2).Equals(TestUtils.CreateIntSequence(1, 2).asOrderedSet()));
            Assert.IsTrue(TestUtils.CreateIntOrderedSet(1, 2).Equals(TestUtils.CreateIntOrderedSet(1, 2).asOrderedSet()));

            Assert.IsTrue(TestUtils.CreateIntBag(1, 2).Equals(TestUtils.CreateIntBag(1, 2).asBag()));
            Assert.IsTrue(TestUtils.CreateIntBag(1, 2).Equals(TestUtils.CreateIntSet(1, 2).asBag()));
            Assert.IsTrue(TestUtils.CreateIntBag(1, 2).Equals(TestUtils.CreateIntSequence(1, 2).asBag()));
            Assert.IsTrue(TestUtils.CreateIntBag(1, 2).Equals(TestUtils.CreateIntOrderedSet(1, 2).asBag()));

            Assert.IsTrue(TestUtils.CreateIntSequence(1, 2).Equals(TestUtils.CreateIntSequence(1, 2).asSequence()));
            Assert.IsTrue(TestUtils.CreateIntSequence(1, 2).Equals(TestUtils.CreateIntOrderedSet(1, 2, 2).asSequence()));

            Assert.IsTrue(TestUtils.CreateIntSet(1, 2).Equals(TestUtils.CreateIntBag(1, 2, 2).asSet()));
            Assert.IsTrue(TestUtils.CreateIntSet(1, 2).Equals(TestUtils.CreateIntSet(1, 2, 2).asSet()));
            Assert.IsTrue(TestUtils.CreateIntSet(1, 2).Equals(TestUtils.CreateIntSequence(1, 2, 2).asSet()));
            Assert.IsTrue(TestUtils.CreateIntSet(1, 2).Equals(TestUtils.CreateIntOrderedSet(1, 2, 2).asSet()));  
        }


        /// <summary>
        /// Test collection constructors with ranges.
        /// </summary>
        [Test]
        public void RangeTest()
        {
            TestUtils.AreEqual(TestUtils.CreateIntSequence(100, 1, 2, 3, 4, 5, 200), new OclSequence(OclInteger.Type, (OclInteger)100, new OclCollectionLiteralPartRange((OclInteger)1, (OclInteger)5), (OclInteger)200));
            TestUtils.AreEqual(TestUtils.CreateIntSet(100, 1, 2, 3, 4, 5, 200), new OclSet(OclInteger.Type, (OclInteger)100, new OclCollectionLiteralPartRange((OclInteger)1, (OclInteger)5), (OclInteger)200));
            TestUtils.AreEqual(TestUtils.CreateIntOrderedSet(100, 1, 2, 3, 4, 5, 200), new OclOrderedSet(OclInteger.Type, (OclInteger)100, new OclCollectionLiteralPartRange((OclInteger)1, (OclInteger)5), (OclInteger)200));
            TestUtils.AreEqual(TestUtils.CreateIntBag(100, 1, 2, 3, 4, 5, 200), new OclBag(OclInteger.Type, (OclInteger)100, new OclCollectionLiteralPartRange((OclInteger)1, (OclInteger)5), (OclInteger)200));
        }

    }

}
