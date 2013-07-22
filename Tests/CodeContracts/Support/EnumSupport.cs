using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class EnumSupportTest {

        private enum TestEnum
        {
            Value1, Value2, Value3
        }

        /// <summary>
        /// Test allInstances method of OclEnum
        /// </summary>
        [Test]
        public void AllInstancesTest()
        {
            TestUtils.AreEqual(new OclSet(OclEnum<TestEnum>.Type, (OclEnum<TestEnum>)TestEnum.Value1, (OclEnum<TestEnum>)TestEnum.Value2, (OclEnum<TestEnum>)TestEnum.Value3), OclEnum<TestEnum>.allInstances());
        }

        /// <summary>
        /// Test wrapping enum values
        /// </summary>
        [Test]
        public void WrapTest()
        {
            Assert.IsNull((TestEnum?)(OclEnum<TestEnum>)null);
            Assert.AreEqual(TestEnum.Value1, (TestEnum)(OclEnum<TestEnum>)TestEnum.Value1);
            Assert.AreEqual(TestEnum.Value2, (TestEnum)(OclEnum<TestEnum>)TestEnum.Value2);
        }

    }

}
