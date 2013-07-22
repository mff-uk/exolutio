using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class AnySupportTest {


        [Test]
        public void OperationsTest()
        {
            TestUtils.AreEqual(OclBoolean.True, ((OclInteger)2).oclIsKindOf(OclUnlimitedNatural.Type));
            TestUtils.AreEqual(OclBoolean.True, ((OclInteger)2).oclIsKindOf(OclAny.Type));
            TestUtils.AreEqual(OclBoolean.False, ((OclInteger)2).oclIsKindOf(OclVoidType.OclVoid));

            TestUtils.AreEqual(OclBoolean.True, ((OclInteger)2).oclIsTypeOf(OclUnlimitedNatural.Type));
            TestUtils.AreEqual(OclBoolean.False, ((OclInteger)2).oclIsTypeOf(OclAny.Type));
            TestUtils.AreEqual(OclBoolean.False, ((OclInteger)2).oclIsTypeOf(OclVoidType.OclVoid));

            TestUtils.AreEqual(OclReal.Type, ((OclReal)2.3).oclType());
            TestUtils.AreEqual(OclUnlimitedNatural.Type, ((OclReal)2).oclType());

            TestUtils.AreEqual((OclInteger)2, ((OclReal)2).oclAsType<OclInteger>(OclInteger.Type));
            TestUtils.AreEqual((OclInteger)2, ((OclReal)2).oclAsType<OclAny>(OclAny.Type));

            Assert.Throws<InvalidCastException>(() => ((OclReal)2.4).oclAsType<OclInteger>(OclInteger.Type));
        }
        
    }

}
