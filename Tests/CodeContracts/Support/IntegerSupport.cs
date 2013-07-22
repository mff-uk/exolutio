using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class IntegerSupportTest {
        /// <summary>
        /// Test binary operations of OclInteger
        /// </summary>
        [Test]
        public void BinaryOperationsTest()
        {
            Assert.AreEqual(5, (int)((OclInteger)2).op_Addition((OclInteger)3));
            Assert.AreEqual(-1, (int)((OclInteger)2).op_Subtraction((OclInteger)3));
            Assert.AreEqual(6, (int)((OclInteger)2).op_Multiply((OclInteger)3));
            Assert.AreEqual(1.5D, (double)((OclInteger)3).op_Division((OclInteger)2));
            Assert.AreEqual(1, (int)((OclInteger)3).div((OclInteger)2));
            Assert.AreEqual(1, (int)((OclInteger)3).mod((OclInteger)2));

            Assert.AreEqual(-1, (int)((OclInteger)3).div((OclInteger)(-2)));
            Assert.AreEqual(1, (int)((OclInteger)3).mod((OclInteger)(-2)));
        }
        /// <summary>
        /// Test unary operations of OclInteger
        /// </summary>
        [Test]
        public void UnaryOperationsTest()
        {
            Assert.AreEqual(-2, (int)((OclInteger)2).op_UnaryNegation());
            Assert.AreEqual(2, (int)((OclInteger)(-2)).abs());
        }

        /// <summary>
        /// Test overloaded operators of OclIntegr
        /// </summary>
        [Test]
        public void OperatorsTest()
        {
            Assert.AreEqual(5, (int)((OclInteger)2+(OclInteger)3));
            Assert.AreEqual(-1, (int)((OclInteger)2-(OclInteger)3));
            Assert.AreEqual(6, (int)((OclInteger)2*(OclInteger)3));
            Assert.AreEqual(1.5D, (double)((OclInteger)3 / (OclInteger)2));

            Assert.AreEqual(-2, (int)(-(OclInteger)2));
        }

        /// <summary>
        /// Test min and max operations of OclInteger
        /// </summary>
        [Test]
        public void MinMaxOperationsTest()
        {
            Assert.AreEqual(3D, (int)((OclInteger)2).max((OclInteger)3));
            Assert.AreEqual(2D, (int)((OclInteger)2).min((OclInteger)3));
        }

        /// <summary>
        /// Test comparing operations of OclInteger
        /// </summary>
        [Test]
        public void CompareOperationsTest()
        {
            Assert.IsTrue((bool)((OclInteger)2).op_LessThan((OclInteger)3));
            Assert.IsFalse((bool)((OclInteger)2).op_GreaterThan((OclInteger)3));
            Assert.IsTrue((bool)((OclInteger)2).op_LessThanOrEqual((OclInteger)3));
            Assert.IsFalse((bool)((OclInteger)2).op_GreaterThanOrEqual((OclInteger)3));
            Assert.IsFalse((bool)((OclInteger)2).op_Equality((OclInteger)3));
        }

        /// <summary>
        /// Test overloaded comparing operators of OclInteger
        /// </summary>
        [Test]
        public void CompareOperatorsTest()
        {
            Assert.IsTrue((bool)((OclInteger)2<(OclInteger)3));
            Assert.IsFalse((bool)((OclInteger)2>(OclInteger)3));
            Assert.IsTrue((bool)((OclInteger)2<=(OclInteger)3));
            Assert.IsFalse((bool)((OclInteger)2>=(OclInteger)3));
            Assert.IsFalse((bool)((OclInteger)2==(OclInteger)3));
        }
        /// <summary>
        /// Test exceptions thrown from OclInteger methods
        /// </summary>
        [Test]
        public void InvalidOperationsTest()
        {
            Assert.Throws<NotFiniteNumberException>(() => ((OclInteger)2).op_Division((OclInteger)0));
            Assert.Throws<OverflowException>(() => ((OclInteger)Int32.MaxValue).op_Addition((OclInteger)Int32.MaxValue));

            Assert.Throws<NullReferenceException>(() => ((OclInteger)null).op_Addition((OclInteger)1));
            Assert.Throws<ArgumentNullException>(() => ((OclInteger)1).op_Addition((OclInteger)null));
            Assert.Throws<InvalidOperationException>(() => { int d = (int)(OclInteger)null; });
        }
        /// <summary>
        /// Test wrapping integer values
        /// </summary>
        [Test]
        public void WrapTest()
        {
            Assert.IsNull((int?)(OclInteger)null);
            Assert.AreEqual(2, (int)(OclInteger)2);
            Assert.AreEqual(-2, (int)(OclInteger)(-2));
            Assert.AreEqual(0, (int)(OclInteger)0);
        }
    }

}
