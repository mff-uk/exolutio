using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class UnlimitedNaturalSupportTest {

        [Test]
        public void OperationsTest()
        {
            Assert.AreEqual((OclUnlimitedNatural)(3), ((OclUnlimitedNatural)1).op_Addition((OclUnlimitedNatural)(2)));
            Assert.AreEqual((OclUnlimitedNatural)(9728), ((OclUnlimitedNatural)128).op_Multiply((OclUnlimitedNatural)(76)));


            Assert.Throws(typeof(OclUnlimitedValueException), () => { var x = ((OclUnlimitedNatural)1).op_Multiply(OclUnlimitedNatural.Unlimited); });
            Assert.Throws(typeof(OclUnlimitedValueException), () => { var x = OclUnlimitedNatural.Unlimited.op_Addition((OclUnlimitedNatural)2); });
            Assert.Throws(typeof(OclUnlimitedValueException), () => { var x = (int)OclUnlimitedNatural.Unlimited;});
        }


        [Test]
        public void MinMax()
        {
            Assert.AreEqual((OclUnlimitedNatural)(10), ((OclUnlimitedNatural)1).max((OclUnlimitedNatural)10));
            Assert.AreEqual(OclUnlimitedNatural.Unlimited, ((OclUnlimitedNatural)1).max(OclUnlimitedNatural.Unlimited));
            Assert.AreEqual((OclUnlimitedNatural)(1), ((OclUnlimitedNatural)1).min(OclUnlimitedNatural.Unlimited));
            Assert.AreEqual(OclUnlimitedNatural.Unlimited, (OclUnlimitedNatural.Unlimited).max(OclUnlimitedNatural.Unlimited));
            Assert.AreEqual(OclUnlimitedNatural.Unlimited, (OclUnlimitedNatural.Unlimited).min(OclUnlimitedNatural.Unlimited));
        }

        [Test]
        public void Compare()
        {
            Assert.IsTrue((bool)OclUnlimitedNatural.Unlimited.op_GreaterThan((OclUnlimitedNatural)(int.MaxValue)));
            Assert.IsFalse((bool)OclUnlimitedNatural.Unlimited.op_LessThanOrEqual((OclUnlimitedNatural)(int.MaxValue)));

            Assert.IsTrue((bool)((OclUnlimitedNatural)10).op_GreaterThan((OclUnlimitedNatural)(3)));
            Assert.IsFalse((bool)((OclUnlimitedNatural)10).op_GreaterThan((OclUnlimitedNatural)(10)));

            Assert.IsFalse((bool)((OclUnlimitedNatural)10).op_LessThanOrEqual((OclUnlimitedNatural)(3)));
            Assert.IsTrue((bool)((OclUnlimitedNatural)10).op_LessThanOrEqual((OclUnlimitedNatural)(10)));


            Assert.IsFalse((bool)((OclUnlimitedNatural)10).op_LessThan((OclUnlimitedNatural)(3)));
            Assert.IsFalse((bool)((OclUnlimitedNatural)10).op_LessThan((OclUnlimitedNatural)(10)));

            Assert.IsTrue((bool)((OclUnlimitedNatural)10).op_GreaterThanOrEqual((OclUnlimitedNatural)(3)));
            Assert.IsTrue((bool)((OclUnlimitedNatural)10).op_GreaterThanOrEqual((OclUnlimitedNatural)(10)));


        }

        [Test]
        public void Wrap()
        {
            Assert.IsNull((int?)(OclUnlimitedNatural)null);
            Assert.AreEqual(2, (int)(OclUnlimitedNatural)2);
        }

    }

}
