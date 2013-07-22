using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts.Support {
    [TestFixture]
    public class StringSupportTest {

        [Test]
        public void OperationsTest()
        {
            Assert.AreEqual("abcdef", (string)((OclString)"abc").op_Addition((OclString)"def"));
            Assert.AreEqual(3, (int)((OclString)"abc").size());
            Assert.AreEqual("abcdef", (string)((OclString)"abc").concat((OclString)"def"));
            Assert.AreEqual("eStr", (string)((OclString)"SomeString").substring((OclInteger)4, (OclInteger)7));
            Assert.AreEqual(1234, (int)((OclString)"1234").toInteger());
            Assert.AreEqual(1234.567, (double)((OclString)"1234.567").toReal());
            Assert.AreEqual("ABC", (string)((OclString)"abc").toUpperCase((OclString)"en_us"));
            Assert.AreEqual("abc", (string)((OclString)"ABC").toLowerCase((OclString)"en_us"));
            Assert.AreEqual(5, (int)((OclString)"SomeString").indexOf((OclString)"St"));
            Assert.AreEqual(true, (bool)((OclString)"ABC").equalsIgnoreCase((OclString)"abc", (OclString)"en_us"));
            Assert.AreEqual("S", (string)((OclString)"SomeString").at((OclInteger)5));
            Assert.AreEqual(new OclSequence(OclString.Type, (OclString)"a", (OclString)"b", (OclString)"c"), ((OclString)"abc").characters());
            Assert.AreEqual(true, (bool)((OclString)"true").toBoolean());
        }

        [Test]
        public void OperatorsTest()
        {
            Assert.AreEqual("abcdef", (string)((OclString)"abc"+(OclString)"def"));
        }

        [Test]
        public void UTF16OperationsTest()
        {
            Assert.AreEqual(1, (int)((OclString)"\xD800\xDC00").size());
            Assert.AreEqual("\xD800\xDC01\xD800\xDC02", (string)((OclString)"\xD800\xDC00\xD800\xDC01\xD800\xDC02\xD800\xDC03").substring((OclInteger)2, (OclInteger)3));
            Assert.AreEqual(3, (int)((OclString)"\xD800\xDC00\xD800\xDC01\xD800\xDC02").indexOf((OclString)"\xD800\xDC02"));
            Assert.AreEqual("\xD800\xDC01", (string)((OclString)"\xD800\xDC00\xD800\xDC01\xD800\xDC02").at((OclInteger)2));
            Assert.AreEqual(new OclSequence(OclString.Type, (OclString)"\xD800\xDC00", (OclString)"\xD800\xDC01", (OclString)"\xD800\xDC02"), ((OclString)"\xD800\xDC00\xD800\xDC01\xD800\xDC02").characters());
        }

        [Test]
        public void WrapTest()  
        {
            Assert.IsNull((string)(OclString)null);
            Assert.AreEqual("", (string)(OclString)"");
        }

        [Test]
        public void EqualityTest()
        {
            Assert.AreEqual((OclString)"", (OclString)"");
            Assert.AreEqual((OclString)"abc", (OclString)"abc");
            Assert.AreNotEqual((OclString)"", (OclInteger)0);
            Assert.AreNotEqual((OclString)"0", (OclInteger)0);
        }

        [Test]
        public void TypeTest()
        {
            Assert.AreEqual(OclString.Type, ((OclString)"A").oclType());
            Assert.AreEqual(OclString.Type, ((OclAny)(OclString)"A").oclType());
        }

        [Test]
        public void CultureTest()
        {
            Assert.AreEqual(System.Globalization.CultureInfo.GetCultureInfo("en-US"), OclUtils.GetLocale((OclString)"en_US"));
        }
     
    }

}
