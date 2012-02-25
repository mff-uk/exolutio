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
using Exolutio.Model.Serialization;
using Exolutio.Model;

namespace Tests.OCL {
    [TestFixture]
    public class CompilerTest {
        public Tuple<TypesTable, Exolutio.Model.OCL.Environment> CreateTestEnv() {
            


            ProjectSerializationManager m = new ProjectSerializationManager();
            Project loadedProject = m.LoadProject(@"..\..\..\Projects\tournaments.eXo");
            ModelIntegrity.ModelConsistency.CheckProject(loadedProject);

            BridgeFactory brFactory = new BridgeFactory();
            TypesTable tt = brFactory.Create(loadedProject.SingleVersion.PIMSchema.OCLScripts[0].Schema).TypesTable;

            NamespaceEnvironment env = new NamespaceEnvironment(tt.Library.RootNamespace);
            


            return new Tuple<TypesTable, Exolutio.Model.OCL.Environment>(tt,env );
        }

        [Test]
        public void SimpleTest() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* All Matches in a Tournament occur
within the Tournament’s time frame */

context Tournament
inv:
matches->forAll(m:Match |
m.start.after(start) and m.end.before(end))", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }

        [Test]
        public void SimpleTest2() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* Each Tournament conducts at
least one Match on the first
day of the Tournament */

context Tournament
inv: matches->exists(m:Match | m.start.equals(start))", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }

        [Test]
        public void SimpleTest3() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* No Player can take part in two or
more Tournaments that overlap */
context TournamentControl
inv: Tournament.players->forAll(p|
p.tournaments->forAll(t|
t <> Tournament implies
not t.overlap(Tournament)))", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }

        [Test]
        public void SimpleTest4() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* A match can only involve players who are
accepted in the tournament */

context Match
inv: players->forAll(p|
p.tournaments->exists(t|
t.matches->includes(self)))", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);
        }



        [Test]
        public void SimpleTest5() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* dates consistency */
context Tournament
inv : start <= end", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }

        [Test]
        public void SimpleTest6() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* participating players meet the requirements of the tournament */
context Tournament
inv: requiredQualificationPoints = null or 
players->forAll(p | p.points >= requiredQualificationPoints)", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }

        [Test]
        public void SimpleTest7() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* players playing leagues must have registration numbers */
context Player
inv: not League->isEmpty() implies regNo <> null ", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }


        [Test]
        public void TupleTest() {
            
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"context Tournament 
inv: (Tuple {x: Integer = 5, y: String = 'hi'}).x = 5", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);
        }

        [Test]
        public void StringTest() {

            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"context Tournament 
inv: 'a'='a'", root.Item1, root.Item2);

            

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);
        }


        [Test]
        public void IntTest() {

            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"context Tournament 
inv: 1=1", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);
        }


        [Test]
        public void SetTest() {

            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"context Tournament 
inv: Set{1,2}->size()=2", root.Item1, root.Item2);

            Exolutio.Model.OCL.Utils.PrintVisitor printer = new Exolutio.Model.OCL.Utils.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);
        }




    }
}
