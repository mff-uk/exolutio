using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.Model.OCL.Compiler;
using Exolutio.Model.OCL;
using Exolutio.Model.OCL.AST;

namespace Tests.OCL {
    [TestFixture]
    class StandAloneExpressionParserTest {
        [Test]
        public void SimpleExpression() {

            TypesTable tt = new TypesTable();
            StandardLibraryCreator sLC = new StandardLibraryCreator();
            sLC.CreateStandardLibrary(tt);

            Compiler compiler = new Compiler();
            Exolutio.Model.OCL.Environment env = new NamespaceEnvironment(tt.Library.RootNamespace);
            var res = compiler.CompileStandAloneExpression("1=1", tt, env);
            Assert.IsFalse(res.Errors.HasError);
            Assert.AreEqual(typeof(OperationCallExp), res.Expression.GetType());
        }
    }
}
