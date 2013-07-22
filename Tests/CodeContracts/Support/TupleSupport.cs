using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class TupleSupportTest {

        [Test]
        public void Wrap()
        {
            OclTuple tuple = new OclTuple(OclTuple.Part("i",OclInteger.Type,(OclInteger)1));
            
            TestUtils.AreEqual((OclInteger)1, tuple.Get<OclInteger>("i"));
            TestUtils.AreEqual((OclInteger)1, tuple.Get<OclInteger>(0));

            OclTuple tuple2 = new OclTuple(OclTuple.Part("s", OclString.Type, (OclString)"x"), OclTuple.Part("t", tuple.oclType(), tuple));

            TestUtils.AreEqual((OclString)"x", tuple2.Get<OclString>("s"));
            TestUtils.AreEqual((OclString)"x", tuple2.Get<OclString>(0));

            TestUtils.AreEqual(tuple, tuple2.Get<OclTuple>("t"));
            TestUtils.AreEqual(tuple, tuple2.Get<OclTuple>(1));

            TestUtils.AreEqual((OclInteger)1, tuple2.Get<OclTuple>(1).Get<OclInteger>(0));
        }

        [Test]
        public void Equality()
        {
            OclTuple tuple1 = new OclTuple(OclTuple.Part("i", OclInteger.Type, (OclInteger)1), OclTuple.Part("s", OclString.Type, (OclString)"x"));
            OclTuple tuple2 = new OclTuple(OclTuple.Part("i", OclInteger.Type, (OclInteger)1), OclTuple.Part("s", OclString.Type, (OclString)"x"));
            OclTuple tuple3 = new OclTuple(OclTuple.Part("i", OclInteger.Type, (OclInteger)2), OclTuple.Part("s", OclString.Type, (OclString)"x"));

            Assert.IsTrue((bool)(tuple1 == tuple2));
            Assert.IsFalse((bool)(tuple1 == tuple3));

            Assert.IsFalse((bool)(tuple1 != tuple2));
            Assert.IsTrue((bool)(tuple1 != tuple3));
        }

    }

}
