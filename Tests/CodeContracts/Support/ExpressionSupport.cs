using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class ExpressionSupportTest {

        [Test]
        public void LetExpressionTest()
        {
            Assert.AreEqual(40, OclUtils.Let(10, (int i) => (i + OclUtils.Let(20, (int j) => j + i))));
        }

        private void VoidFunc()
        {
            
        }

        [Test]
        public void VoidExpressionTest()
        {
            Assert.IsNull(OclUtils.Void(()=>VoidFunc()));
        }

        
    }

}
