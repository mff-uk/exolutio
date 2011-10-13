using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.TypesTable;
using Exolutio.Model.OCL.Compiler;
using Exolutio.Model.OCL;

namespace Tests.OCL {
    [TestFixture]
    public class CompilerTest {


        public Tuple<TypesTable, Exolutio.Model.OCL.Environment> CreateTestEnv() {
            TypesTable tt = new TypesTable();
            StandardLibraryCreator slc = new StandardLibraryCreator();
            slc.CreateStandardLibrary(tt);


            //tt.Library.Boolean.Operations.Add(new Operation("and", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", tt.Library.Boolean) }));
            //tt.Library.Boolean.Operations.Add(new Operation("or", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", tt.Library.Boolean) }));
            //tt.Library.Boolean.Operations.Add(new Operation("implies", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", tt.Library.Boolean) }));
            //tt.Library.Boolean.Operations.Add(new Operation("not", true, tt.Library.Boolean));


            Class date = new Class(tt,"Date");
            date.Operations.Add(new Operation("after", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("before", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("equals", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("<=", true, tt.Library.Boolean, new Parameter[] { new Parameter("time", date) }));

            Class tournament = new Class(tt,"Tournament");
            tournament.Properties.Add(new Property("name", PropertyType.One, tt.Library.String));
            tournament.Properties.Add(new Property("start", PropertyType.One, date));
            tournament.Properties.Add(new Property("end", PropertyType.One, date));
            tournament.Properties.Add(new Property("maxNumPlayers", PropertyType.One, tt.Library.Integer));
            tournament.Properties.Add(new Property("advertised", PropertyType.ZeroToOne, tt.Library.Boolean));
            tournament.Properties.Add(new Property("requiredQualificationPoints", PropertyType.One, tt.Library.Integer));
            tournament.Properties.Add(new Property("open", PropertyType.One, tt.Library.Boolean));
            tournament.Operations.Add(new Operation("overlap", true, tt.Library.Boolean, new Parameter[] { new Parameter("t", tournament) }));
            tournament.Operations.Add(new Operation("<>", true, tt.Library.Boolean, new Parameter[] { new Parameter("tournament", tournament) }));

            Class match = new Class(tt,"Match");
            match.Properties.Add(new Property("start", PropertyType.One, date));
            match.Properties.Add(new Property("end", PropertyType.One, date));
            // chyby status

            Class tournamentControl = new Class(tt,"TournamentControl");

            Class player = new Class(tt,"Player");
            player.Properties.Add(new Property("points", PropertyType.One, tt.Library.Integer));
            player.Properties.Add(new Property("regno", PropertyType.One, tt.Library.String));

            Class league = new Class(tt, "League");


            //Vzjemne propejeni trid
            BagType matchesBagType = new BagType(tt,match);
          //  matchesBagType.Operations.Add(new Operation("includes", true, tt.Library.Boolean, new Parameter[] { new Parameter("tournament", match) }));

            tournament.Properties.Add(new Property("matches", PropertyType.Many, matchesBagType));
            BagType playersBagType = new BagType(tt,player);
            tournament.Properties.Add(new Property("players", PropertyType.Many, playersBagType));
            BagType tournamentBagType = new BagType(tt,tournament);
            player.Properties.Add(new Property("tournaments", PropertyType.Many, tournamentBagType));
            player.Properties.Add(new Property("matches", PropertyType.Many, matchesBagType));
            tournamentControl.Properties.Add(new Property("tournament", PropertyType.One, tournament));
            match.Properties.Add(new Property("players", PropertyType.Many, playersBagType));
            BagType leagueBag = new BagType(tt, league);
            player.Properties.Add(new Property("league", PropertyType.ZeroToOne, leagueBag));

            Namespace ns = tt.Library.RootNamespace;
            ns.NestedClassifier.Add(date);
            ns.NestedClassifier.Add(tournament);
            ns.NestedClassifier.Add(match);
            ns.NestedClassifier.Add(tournamentControl);
            ns.NestedClassifier.Add(player);
            ns.NestedClassifier.Add(league);


            tt.RegisterType(date);
            tt.RegisterType(tournament);
            tt.RegisterType(match);
            tt.RegisterType(matchesBagType);
            tt.RegisterType(tournamentControl);
            tt.RegisterType(playersBagType);
            tt.RegisterType(tournamentBagType);
            tt.RegisterType(player);
            tt.RegisterType(league);
            tt.RegisterType(leagueBag);


            NamespaceEnvironment env = new NamespaceEnvironment(ns);



            return new Tuple<TypesTable, Exolutio.Model.OCL.Environment>(tt, env);
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

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
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

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
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
inv: tournament.players->forAll(p|
p.tournaments->forAll(t|
t <> tournament implies
not t.overlap(tournament)))", root.Item1, root.Item2);

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
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

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
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

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
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

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }

        [Test]
        public void SimpleTest7() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* players playing leagues must have registration numbers */
context Player
inv: not league->isEmpty() implies regno <> null ", root.Item1, root.Item2);

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
            string text = printer.AstToString(ast.Invariants[0]);

        }



    }
}
