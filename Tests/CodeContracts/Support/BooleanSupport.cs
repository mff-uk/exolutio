using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class BooleanSupportTest {

        private OclBoolean TrueFunc()
        {
            return OclBoolean.True;
        }
        private OclBoolean FalseFunc()
        {
            return OclBoolean.False;
        }
        private OclBoolean NullFunc()
        {
            return null;
        }
        private OclBoolean InvalidFunc()
        {
            throw new Exception();
        }

        /// <summary>
        /// Test non-catching operations of OclBoolean
        /// </summary>
        [Test]
        public void OperationsTest()
        {
            TestUtils.AreEqual(OclBoolean.False, ((OclBoolean)true).xor((OclBoolean)true));
            TestUtils.AreEqual(OclBoolean.True, ((OclBoolean)true).xor((OclBoolean)false));
            TestUtils.AreEqual(OclBoolean.False, ((OclBoolean)true).not());
            TestUtils.AreEqual(OclBoolean.True, ((OclBoolean)false).not());
        }
        /// <summary>
        /// Test catching and operation of OclBoolean
        /// </summary>
        [Test]
        public void AndTest()
        {
            Assert.IsFalse((bool)OclBoolean.and(FalseFunc, FalseFunc));
            Assert.IsFalse((bool)OclBoolean.and(TrueFunc, FalseFunc));
            Assert.IsFalse((bool)OclBoolean.and(FalseFunc, TrueFunc));
            Assert.IsTrue((bool)OclBoolean.and(TrueFunc, TrueFunc));
            Assert.IsFalse((bool)OclBoolean.and(FalseFunc, InvalidFunc));
            Assert.IsFalse((bool)OclBoolean.and(InvalidFunc, FalseFunc));
            Assert.Throws(typeof(Exception), () => OclBoolean.and(TrueFunc, InvalidFunc));
            Assert.Throws(typeof(Exception), () => OclBoolean.and(InvalidFunc, TrueFunc));
            Assert.Throws(typeof(AggregateException), () => OclBoolean.and(InvalidFunc, InvalidFunc));

            Assert.Throws(typeof(ArgumentNullException), () => OclBoolean.and(TrueFunc, NullFunc));
            Assert.Throws(typeof(NullReferenceException), () => OclBoolean.and(NullFunc, TrueFunc));
            Assert.Throws(typeof(AggregateException), () => OclBoolean.and(NullFunc, NullFunc));
            

        }
        /// <summary>
        /// Test catching or operation of OclBoolean
        /// </summary>
        [Test]
        public void OrTest()
        {
            Assert.IsFalse((bool)OclBoolean.or(FalseFunc, FalseFunc));
            Assert.IsTrue((bool)OclBoolean.or(TrueFunc, FalseFunc));
            Assert.IsTrue((bool)OclBoolean.or(FalseFunc, TrueFunc));
            Assert.IsTrue((bool)OclBoolean.or(TrueFunc, TrueFunc));
            Assert.IsTrue((bool)OclBoolean.or(TrueFunc, InvalidFunc));
            Assert.IsTrue((bool)OclBoolean.or(InvalidFunc, TrueFunc));
            Assert.Throws(typeof(Exception), () => OclBoolean.or(FalseFunc, InvalidFunc));
            Assert.Throws(typeof(Exception), () => OclBoolean.or(InvalidFunc, FalseFunc));
            Assert.Throws(typeof(AggregateException), () => OclBoolean.or(InvalidFunc, InvalidFunc));

            Assert.Throws(typeof(ArgumentNullException), () => OclBoolean.or(FalseFunc, NullFunc));
            Assert.Throws(typeof(NullReferenceException), () => OclBoolean.or(NullFunc, FalseFunc));
            Assert.Throws(typeof(AggregateException), () => OclBoolean.or(NullFunc, NullFunc));
        }


        /// <summary>
        /// Test catching implies operation of OclBoolean
        /// </summary>
        [Test]
        public void ImpliesTest()
        {
            Assert.IsTrue((bool)OclBoolean.implies(FalseFunc, FalseFunc));
            Assert.IsFalse((bool)OclBoolean.implies(TrueFunc, FalseFunc));
            Assert.IsTrue((bool)OclBoolean.implies(FalseFunc, TrueFunc));
            Assert.IsTrue((bool)OclBoolean.implies(TrueFunc, TrueFunc));
            Assert.IsTrue((bool)OclBoolean.implies(FalseFunc, InvalidFunc));
            Assert.IsTrue((bool)OclBoolean.implies(InvalidFunc, TrueFunc));
            Assert.Throws(typeof(Exception), () => OclBoolean.implies(TrueFunc, InvalidFunc));
            Assert.Throws(typeof(Exception), () => OclBoolean.implies(InvalidFunc, FalseFunc));
            Assert.Throws(typeof(AggregateException), () => OclBoolean.implies(InvalidFunc, InvalidFunc));

            Assert.Throws(typeof(ArgumentNullException), () => OclBoolean.implies(TrueFunc, NullFunc));
            Assert.Throws(typeof(NullReferenceException), () => OclBoolean.implies(NullFunc, FalseFunc));
            Assert.Throws(typeof(AggregateException), () => OclBoolean.implies(NullFunc, NullFunc));
        }

        /// <summary>
        /// Test wrapping boolean values
        /// </summary>
        [Test]
        public void WrapTest()
        {
            Assert.IsNull((bool?)(OclBoolean)null);
            Assert.AreEqual(true, (bool)(OclBoolean)true);
            Assert.AreEqual(false, (bool)(OclBoolean)false);
        }

        /// <summary>
        /// Test Equals method of OclBoolean
        /// </summary>
        [Test]
        public void EqualityTest()
        {
            TestUtils.AreEqual((OclBoolean)true, OclBoolean.True);
            TestUtils.AreEqual((OclBoolean)false, OclBoolean.False);
            TestUtils.AreNotEqual((OclBoolean)false, OclBoolean.True);
            TestUtils.AreNotEqual((OclBoolean)true, OclBoolean.False);
        }

        /// <summary>
        /// Test allInstances method of OclBoolean
        /// </summary>
        [Test]
        public void AllInstancesTest()
        {
            TestUtils.AreEqual(new OclSet(OclBoolean.Type, (OclBoolean)false, (OclBoolean) true), OclBoolean.allInstances());
        }

    }

}
