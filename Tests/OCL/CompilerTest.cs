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


        public Tuple<TypesTable,Exolutio.Model.OCL.Environment> CreateTestEnv() {
            TypesTable tt = new TypesTable();

            tt.Boolean.Operations.Add(new Operation("and",true, tt.Boolean, new Parameter[] { new Parameter("time", tt.Boolean) }));
             tt.Boolean.Operations.Add(new Operation("or",true, tt.Boolean, new Parameter[] { new Parameter("time", tt.Boolean) }));
            tt.Boolean.Operations.Add(new Operation("implies", true, tt.Boolean, new Parameter[] { new Parameter("time", tt.Boolean) }));
            tt.Boolean.Operations.Add(new Operation("not", true, tt.Boolean));
            

            Class date = new Class("Date");
            date.Operations.Add(new Operation("after", true, tt.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("before", true, tt.Boolean, new Parameter[] { new Parameter("time", date) }));
            date.Operations.Add(new Operation("equals", true, tt.Boolean, new Parameter[] { new Parameter("time", date) }));
             date.Operations.Add(new Operation("<=", true, tt.Boolean, new Parameter[] { new Parameter("time", date) }));

            Class tournament = new Class("Tournament");
            tournament.Properties.Add(new Property("name", PropertyType.One, tt.String));
            tournament.Properties.Add(new Property("start", PropertyType.One, date));
            tournament.Properties.Add(new Property("end", PropertyType.One, date));
            tournament.Properties.Add(new Property("maxNumPlayers", PropertyType.One, tt.Integer));
            tournament.Properties.Add(new Property("advertised", PropertyType.ZeroToOne, tt.Boolean));
            tournament.Properties.Add(new Property("requiredQualificationPoints", PropertyType.One, tt.Integer));
            tournament.Properties.Add(new Property("open", PropertyType.One, tt.Boolean));
            tournament.Operations.Add(new Operation("overlap", true, tt.Boolean, new Parameter[] { new Parameter("t", tournament) }));
            tournament.Operations.Add(new Operation("<>", true, tt.Boolean, new Parameter[] { new Parameter("tournament", tournament) }));

            Class match = new Class("Match");
            match.Properties.Add(new Property("start", PropertyType.One, date));
            match.Properties.Add(new Property("end", PropertyType.One, date));
            // chyby status

            Class tournamentControl = new Class("TournamentControl");

            Class player = new Class("Player");

           

            //Vzjemne propejeni trid
            BagType matchesBagType = new BagType(match);
            matchesBagType.Operations.Add(new Operation("includes",true,tt.Boolean,new Parameter[] { new Parameter("tournament", match) }));
            tournament.Properties.Add(new Property("matches", PropertyType.Many, matchesBagType));
            BagType playersBagType = new BagType(player);
            tournament.Properties.Add(new Property("players", PropertyType.Many, playersBagType));
            BagType tournamentBagType = new BagType(tournament);
            player.Properties.Add(new Property("tournaments", PropertyType.Many, tournamentBagType));
            player.Properties.Add(new Property("matches", PropertyType.Many, matchesBagType));
            tournamentControl.Properties.Add(new Property("tournament", PropertyType.One, tournament));
            match.Properties.Add(new Property("players", PropertyType.Many, playersBagType));

            Namespace ns = new Namespace("");
            ns.NestedClassifier.Add(date);
            ns.NestedClassifier.Add(tournament);
            ns.NestedClassifier.Add(match);
            ns.NestedClassifier.Add(tournamentControl);


            tt.RegisterType(date);
            tt.RegisterType(tournament);
            tt.RegisterType(match);
            tt.RegisterType(matchesBagType);
            tt.RegisterType(tournamentControl);
            tt.RegisterType(playersBagType);
            tt.RegisterType(tournamentBagType);
            

            NamespaceEnvironment env = new NamespaceEnvironment(ns);



            return new Tuple<TypesTable, Exolutio.Model.OCL.Environment>(tt, env);
        }

        [Test]
        public void SimpleTest() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable,Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* All Matches in a Tournament occur
within the Tournament’s time frame */

context Tournament
inv:
matches->forAll(m:Match |
m.start.after(start) and m.end.before(end))", root.Item1,root.Item2);

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
            string text = printer.AstToString(ast.Constraints[0]);
             
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
            string text = printer.AstToString(ast.Constraints[0]);

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
            string text = printer.AstToString(ast.Constraints[0]);

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
            string text = printer.AstToString(ast.Constraints[0]);
        }

        [Test]
        public void SimpleTest5() {
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
            string text = printer.AstToString(ast.Constraints[0]);

        }

        [Test]
        public void SimpleTest6() {
            Compiler compiler = new Compiler();
            Tuple<TypesTable, Exolutio.Model.OCL.Environment> root = CreateTestEnv();
            //compiler.TypesTable = tt;

            var ast = compiler.TestCompiler(@"/* dates consistency */
context Tournament
inv : start <= end", root.Item1, root.Item2);

            Exolutio.Model.OCL.AST.PrintVisitor printer = new Exolutio.Model.OCL.AST.PrintVisitor();
            string text = printer.AstToString(ast.Constraints[0]);

        }



    }
}
