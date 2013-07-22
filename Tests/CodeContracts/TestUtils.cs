using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.Model.OCL.Compiler;
using Exolutio.Model.OCL;
using Exolutio.Model.Serialization;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.CodeContracts.Translation;
using Exolutio.CodeContracts.Support;

namespace Tests.CodeContracts
{
    static class TestUtils
    {
        /// <summary>
        /// Fix strange behavior of Assert.AreEqual for enumerables.
        /// </summary>
        /// <param name="expected">Expected value</param>
        /// <param name="actual">Actual value</param>
        public static void AreEqual(object expected, object actual)
        {
            if (expected is IEnumerable<object> && actual is IEnumerable<object>)
                Assert.IsTrue(expected.Equals(actual));
            else
                Assert.AreEqual(expected, actual);
        }
        public static void AreNotEqual(object expected, object actual)
        {
            if (expected is IEnumerable<object> && actual is IEnumerable<object>)
                Assert.IsFalse(expected.Equals(actual));
            else
                Assert.AreNotEqual(expected, actual);
        }

        public static void AssertEqualIgnoreSpace(string expected, string real)
        {
            expected = System.Text.RegularExpressions.Regex.Replace(expected.Trim(), @"\s+", " ");
            real = System.Text.RegularExpressions.Regex.Replace(real.Trim(), @"\s+", " ");
            Assert.AreEqual(expected, real);
        }


        public static void TestTranslation(PIMSchema schema, string expected, TranslationSettings settings = null)
        {
            if (settings == null)
            {
                settings = new TranslationSettings();
            }
            Exolutio.SupportingClasses.ILog log;
            SourceFile[] files = Translations.TranslatePIMToCSharp(schema, settings, out log);
            string s = files[0].Code;
            string prolog = "using Exolutio.CodeContracts.Support; using System; using System.Collections.Generic; using System.Diagnostics.Contracts; ";
            TestUtils.AssertEqualIgnoreSpace(prolog + expected, s);
        }

        public static OclSequence CreateIntSequence(params int[] ints)
        {
            return new OclSequence(OclInteger.Type, from i in ints select (OclInteger)i);
        }
        public static OclSet CreateIntSet(params int[] ints)
        {
            return new OclSet(OclInteger.Type, from i in ints select (OclInteger)i);
        }
        public static OclBag CreateIntBag(params int[] ints)
        {
            return new OclBag(OclInteger.Type, from i in ints select (OclInteger)i);
        }
        public static OclOrderedSet CreateIntOrderedSet(params int[] ints)
        {
            return new OclOrderedSet(OclInteger.Type, from i in ints select (OclInteger)i);
        }

    }
}
