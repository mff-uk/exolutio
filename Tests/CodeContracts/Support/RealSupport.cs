using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class RealSupportTest {

        /// <summary>
        /// Test binary operations of OclReal
        /// </summary>
        [Test]
        public void BinaryOperationsTest()
        {
            Assert.AreEqual(5D, (double)((OclReal)2).op_Addition((OclReal)3));
            Assert.AreEqual(-1D, (double)((OclReal)2).op_Subtraction((OclReal)3));
            Assert.AreEqual(6D, (double)((OclReal)2).op_Multiply((OclReal)3));
            Assert.AreEqual(1.5D, (double)((OclReal)3).op_Division((OclReal)2));
        }

        /// <summary>
        /// Test unary operations of OclReal
        /// </summary>
        [Test]
        public void UnaryOperationsTest()
        {
            Assert.AreEqual(-2D, (double)((OclReal)2).op_UnaryNegation());
            Assert.AreEqual(2D, (double)((OclReal)(-2)).abs());
        }

        /// <summary>
        /// Test floor and round methods of OclReal
        /// </summary>
        [Test]
        public void RoundingTest()
        {
            Assert.AreEqual(2D, (double)((OclReal)2.1).floor());
            Assert.AreEqual(2D, (double)((OclReal)2.5).floor());
            Assert.AreEqual(2D, (double)((OclReal)2.9).floor());
            Assert.AreEqual(-3D, (double)((OclReal)(-2.1)).floor());
            Assert.AreEqual(-3D, (double)((OclReal)(-2.5)).floor());
            Assert.AreEqual(-3D, (double)((OclReal)(-2.9)).floor());

            Assert.AreEqual(2D, (double)((OclReal)2.1).round());
            Assert.AreEqual(3D, (double)((OclReal)2.5).round());
            Assert.AreEqual(3D, (double)((OclReal)2.9).round());
            Assert.AreEqual(-2D, (double)((OclReal)(-2.1)).round());
            Assert.AreEqual(-2D, (double)((OclReal)(-2.5)).round());
            Assert.AreEqual(-3D, (double)((OclReal)(-2.9)).round());
        }

        /// <summary>
        /// Test min and max operations of OclReal
        /// </summary>
        [Test]
        public void MinMaxOperationsTest()
        {
            Assert.AreEqual(3D, (double)((OclReal)2).max((OclReal)3));
            Assert.AreEqual(2D, (double)((OclReal)2).min((OclReal)3));
        }

        /// <summary>
        /// Test comparing operations of OclReal
        /// </summary>
        [Test]
        public void CompareOperationsTest()
        {
            Assert.IsTrue((bool)((OclReal)2).op_LessThan((OclReal)3));
            Assert.IsFalse((bool)((OclReal)2).op_GreaterThan((OclReal)3));
            Assert.IsTrue((bool)((OclReal)2).op_LessThanOrEqual((OclReal)3));
            Assert.IsFalse((bool)((OclReal)2).op_GreaterThanOrEqual((OclReal)3));
            Assert.IsFalse((bool)((OclReal)2).op_Equality((OclReal)3));
        }

        /// <summary>
        /// Test exceptions thrown from OclReal methods
        /// </summary>
        [Test]
        public void InvalidOperationsTest()
        {
            Assert.Throws<NotFiniteNumberException>(()=>((OclReal)2).op_Division((OclReal)0));
            Assert.Throws<NotFiniteNumberException>(() => ((OclReal)Double.MaxValue).op_Addition((OclReal)Double.MaxValue));
            Assert.Throws<NotFiniteNumberException>( () => {OclReal r = ((OclReal)Double.NaN);});
            Assert.Throws<NotFiniteNumberException>( () => {OclReal r = ((OclReal)Double.PositiveInfinity); });

            Assert.Throws<NullReferenceException>( () => ((OclReal)null).op_Addition((OclReal)1));
            Assert.Throws<ArgumentNullException>(() => ((OclReal)1).op_Addition((OclReal)null));
            Assert.Throws<InvalidOperationException>(() => { double d = (double)(OclReal)null; });
        }

        /// <summary>
        /// Test wrapping real values
        /// </summary>
        [Test]
        public void WrapTest()
        {
            Assert.IsNull((double?)(OclReal)null);
            Assert.AreEqual(2D, (double)(OclReal)2D);
            Assert.AreEqual(0D, (double)(OclReal)0D);
        }
    }

}
