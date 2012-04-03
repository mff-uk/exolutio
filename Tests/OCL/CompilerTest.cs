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

namespace Tests.OCL {
    [TestFixture]
    public class CompilerTest {
        public OCLScript CreateTestEnv() {
            ProjectSerializationManager m = new ProjectSerializationManager();
            Project loadedProject = m.LoadProject(@"..\..\..\Projects\tournaments.eXo");
            ModelIntegrity.ModelConsistency.CheckProject(loadedProject);

            OCLScript script = new OCLScript(loadedProject.SingleVersion.PIMSchema);
            return script;
        }


        public CompilerResult TryCompile(string code) {
            return TryCompile(code, false);
        }

        public CompilerResult TryCompileError(string code) {
            return TryCompile(code, true);
        }

        public CompilerResult TryCompile(string code, bool hasError) {
            var script = CreateTestEnv();
            script.Contents = code;
            CompilerResult result = script.CompileToAst();
            Assert.AreEqual(hasError, result.Errors.HasError);
            return result;
        }

        [Test]
        public void ExampleTest01() {
            TryCompile(@"/* All Matches in a Tournament occur
within the Tournament’s time frame */

context Tournament
inv:
matches->forAll(m:Match |
m.start.after(start) and m.end.before(end))");
        }

        [Test]
        public void ExampleTest02() {
            TryCompile(@"/* Each Tournament conducts at
least one Match on the first
day of the Tournament */

context Tournament
inv: matches->exists(m:Match | m.start.equals(start))");
        }

        [Test]
        public void ExampleTest03() {
            TryCompile(@"/* No Player can take part in two or
more Tournaments that overlap */
context TournamentControl
inv: Tournament.players->forAll(p|
p.tournaments->forAll(t|
t <> Tournament implies
not t.overlap(Tournament)))");
        }

        [Test]
        public void ExampleTest04() {
            TryCompile(@"/* A match can only involve players who are
accepted in the tournament */

context Match
inv: players->forAll(p|
p.tournaments->exists(t|
t.matches->includes(self)))");
        }



        [Test]
        public void ExampleTest05() {
            TryCompile(@"/* dates consistency */
context Tournament
inv : start <= end");
        }

        [Test]
        public void ExampleTest06() {
            TryCompile(@"/* participating players meet the requirements of the tournament */
context Tournament
inv: requiredQualificationPoints = null or 
players->forAll(p | p.points >= requiredQualificationPoints)");
        }

        [Test]
        public void ExampleTest07() {
            TryCompile(@"/* players playing leagues must have registration numbers */
context Player
inv: not League->isEmpty() implies regNo <> null ");
        }


        [Test]
        public void TupleTest() {
            TryCompile(@"context Tournament 
inv: (Tuple {x: Integer = 5, y: String = 'hi'}).x = 5");

            TryCompile(@"context Tournament 
inv: Tuple {name = 'John', age = 10}.age = 10");
        }

        [Test]
        public void NegativeTupleTest() {
            TryCompileError(@"context Tournament 
inv: (Tuple {x: Integer = 'ahoj', y: String = 'hi'}).x = 5");

            TryCompileError(@"context Tournament 
inv: (Tuple {x: Integer = 5, y: String = 10}).x = 5");

            TryCompileError(@"context Tournament 
inv: Tuple {name = 'John' age = 10}.age = 10");
        }



        void testString(Compiler compiler, TypesTable tt, Exolutio.Model.OCL.Environment env, string oclString,string expected) {
            var res = compiler.CompileStandAloneExpression(oclString, tt, env);
            Assert.IsFalse(res.Errors.HasError);
            Assert.IsTrue(res.Expression is StringLiteralExp);
            Assert.AreEqual(expected, (res.Expression as StringLiteralExp).Value);
        }

        [Test]
        public void StringTest() {
            TryCompile(@"context Tournament 
inv: 'a'='a'");

            TypesTable tt = new TypesTable();
            StandardLibraryCreator sLC = new StandardLibraryCreator();
            sLC.CreateStandardLibrary(tt);

            Compiler compiler = new Compiler();
            Exolutio.Model.OCL.Environment env = new NamespaceEnvironment(tt.Library.RootNamespace);
            testString(compiler, tt, env, "''", "");
            testString(compiler, tt, env, "'a'", "a");
            testString(compiler, tt, env, "'aa'", "aa");
            testString(compiler, tt, env, "'\\b'", "\b");
            testString(compiler, tt, env, "'\\t'", "\t");
            testString(compiler, tt, env, "'\\n'", "\n");
            testString(compiler, tt, env, "'\\f'", "\f");
            testString(compiler, tt, env, "'\\r'", "\r");
            testString(compiler, tt, env, "'\\\"'", "\"");
            testString(compiler, tt, env, "'\\''", "'");
            testString(compiler, tt, env, "'\\x27'", "\x27");
            testString(compiler, tt, env, "'\\u1127'", "\x1127");
            testString(compiler, tt, env, @"'\\t'", @"\t");
            testString(compiler, tt, env, @"'\\\t'", "\\\t");
            testString(compiler, tt, env, @"'\\\\t'", "\\\\t");
            testString(compiler, tt, env, @"'\\\\\t'", "\\\\\t");
            testString(compiler, tt, env, @"'\\\\\\t'", "\\\\\\t");
        }


        [Test]
        public void IntTest() {
            TryCompile(@"context Tournament 
inv: 1=1");
            TryCompile(@"context Tournament 
inv: -1=-1");

            TryCompile(@"context Tournament 
inv: 0=0");
        }


        [Test]
        public void CollectionTest() {
            TryCompile(@"context Tournament 
inv: Set{1,2}->size()=2");

            TryCompile(@"context Tournament 
inv: Bag{1..3}->size()=2");

            TryCompile(@"context Tournament 
inv: OrderedSet{10,1..3,4}->size()=2");

            TryCompile(@"context Tournament 
inv: Sequence{10,1..3,4}->size()=2");

            TryCompile(@"context Tournament 
inv: Sequence(Integer){10,1..3,4}->size()=2");

            TryCompile(@"context Tournament 
inv: Sequence(String){'ahoj'}->size()=2");

            TryCompile(@"context Tournament 
inv: Sequence(String){}->size()=0");
            TryCompile(@"context Tournament 
inv: Bag(Integer){}->size()=0");
        }

        [Test]
        public void IfTest() {
            TryCompile(@"context Tournament 
inv: if true then true else 1 = 2 endif"); 
            TryCompile(@"context Tournament 
inv: if 1=1 then true else false endif");
        }

        [Test]
        public void NegativeIfTest() {
            TryCompileError(@"context Tournament 
inv: if 'string' then true else false endif");
        }

        [Test]
        public void SelfTest() {
            TryCompile(@"context Tournament 
inv: self = self");
        }

        [Test]
        public void LiteralTest() {
            TryCompile(@"context Tournament 
inv: true");
            TryCompile(@"context Tournament 
inv: false");
            TryCompile(@"context Tournament 
inv: null = null");

            TryCompile(@"context Tournament 
inv: invalid = invalid");
        }

      




    }
}
