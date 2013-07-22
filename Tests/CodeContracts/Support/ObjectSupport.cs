using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class ObjectSupportTest {

        private class TestClass1
        {
            public virtual int op1()
            {
                return 1;
            }
        }

        [Test]
        public void Wrap()
        {
            Assert.IsNull((OclObject.Wrap(null)).OclUnwrap<TestClass1>());

            TestClass1 tc = new TestClass1();
            Assert.AreEqual(tc, (OclObject.Wrap(tc)).OclUnwrap<TestClass1>());
        }

    }

}
