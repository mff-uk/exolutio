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
        public OCLScript CreateTestEnv() {
            ProjectSerializationManager m = new ProjectSerializationManager();
            Project loadedProject = m.LoadProject(@"..\..\..\Projects\tournaments.eXo");
            ModelIntegrity.ModelConsistency.CheckProject(loadedProject);

            OCLScript script = new OCLScript(loadedProject.SingleVersion.PIMSchema);
            return script;
        }

        public CompilerResult TryCompile(string code) {
            var script = CreateTestEnv();
            script.Contents = code;
            CompilerResult result = script.CompileToAst();
            Assert.IsFalse(result.Errors.HasError);
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
        }

        [Test]
        public void StringTest() {
            TryCompile(@"context Tournament 
inv: 'a'='a'");
        }


        [Test]
        public void IntTest() {
            TryCompile(@"context Tournament 
inv: 1=1");
        }


        [Test]
        public void SetTest() {
            TryCompile(@"context Tournament 
inv: Set{1,2}->size()=2");
        }




    }
}
