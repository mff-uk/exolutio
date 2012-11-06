using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Exolutio.Model.OCL.Bridge;
using Exolutio.Model.OCL.Types;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.Serialization;
using Exolutio.Model;
using Exolutio.Model.PIM;

namespace Exolutio.Tests.OCL {
    [TestFixture]
    class CreateAST {
        public Tuple<PIMBridge, Exolutio.Model.PIM.PIMSchema> CreateTestEnv() {
            ProjectSerializationManager m = new ProjectSerializationManager();
            Project loadedProject = m.LoadProject(@"..\..\..\Projects\tournaments.eXo");
            ModelIntegrity.ModelConsistency.CheckProject(loadedProject);

            Exolutio.Model.PIM.PIMSchema schema = (Exolutio.Model.PIM.PIMSchema)loadedProject.SingleVersion.PIMSchema.OCLScripts[0].Schema;

            return new Tuple<PIMBridge, Exolutio.Model.PIM.PIMSchema>(
                new PIMBridge(schema),
                schema
                );

        }

        [Test]
        // true
        public void ConstantTrue() {
            PIMBridge br = CreateTestEnv().Item1;
            Model.OCL.AST.BooleanLiteralExp boolConstant = new Model.OCL.AST.BooleanLiteralExp(true, br.Library.Boolean);
        }

        [Test]
        //true == true
        public void EqOnBoolean() {
            PIMBridge br = CreateTestEnv().Item1;
            Model.OCL.AST.BooleanLiteralExp boolConstant = new Model.OCL.AST.BooleanLiteralExp(true, br.Library.Boolean);
            Model.OCL.AST.BooleanLiteralExp boolConstant2 = new Model.OCL.AST.BooleanLiteralExp(true, br.Library.Boolean);

            var eqOp = br.Library.Boolean.LookupOperation("=",new Classifier[]{br.Library.Boolean}) ; // tohle není moc pěkné, první index je výběr operací podle jména a ve druhém jsou tyto operace s různou signaturou.
            Model.OCL.AST.OclExpression expr = new Model.OCL.AST.OperationCallExp(boolConstant, // source - na cem se daná operace volá
                false,
                eqOp,// operace která se volá 
                new List<OclExpression>(new Model.OCL.AST.OclExpression[] { boolConstant2 })); // parametry
        }

        // tednkon si dame neco tezsiho
        // self je na TournamentControl
        // self.Tournament->forAll( t:Tournament| t.open)
        [Test]
        public void forAll() {
            var envData = CreateTestEnv();
            var br = envData.Item1;
            var eXoSchema = envData.Item2;
           
            PIMClass PIMtournamentControl =  eXoSchema.PIMClasses.Single( c => c.Name =="TournamentControl");
            PIMBridgeClass OCLtournamentControl = br.Find(PIMtournamentControl);
            //self var 
            VariableDeclaration selfVarDecl = new VariableDeclaration("self",OCLtournamentControl, null);
            Model.OCL.AST.VariableExp selfVar = new Model.OCL.AST.VariableExp(selfVarDecl);

            // self.Tournament
            Model.OCL.AST.PropertyCallExp selfDotTournament = new Model.OCL.AST.PropertyCallExp( 
                selfVar,// nacem se vola properta (associace)
                false,// isPre
                null,// nezajima
                null,// nezajima
                OCLtournamentControl.LookupProperty("Tournament")); // ktera associace se vola
            
            //iterator ve forAll
            PIMClass PIMtournament =  eXoSchema.PIMClasses.Single( c => c.Name =="Tournament");
            PIMBridgeClass OCLtournament = br.Find(PIMtournament);
            // t:Tournament
            VariableDeclaration tVarDelc = new VariableDeclaration("t", OCLtournament,null);
            // t var
            Model.OCL.AST.VariableExp tVar = new Model.OCL.AST.VariableExp(tVarDelc);
            // t.open
            Model.OCL.AST.PropertyCallExp tDotOpen = new Model.OCL.AST.PropertyCallExp( tVar,
                false,
                null,
                null,
                OCLtournament.LookupProperty("open"));

            // self.Tournament.forAll( )
            Model.OCL.AST.OclExpression expr = new Model.OCL.AST.IteratorExp(
                selfDotTournament, // na cem se iterator vola
                tDotOpen, // telo iteratoru
                "forAll",
                new List<VariableDeclaration>(new VariableDeclaration[] { tVarDelc }), 
                br.Library.Boolean // navratovy typ iteratoru
                );

            // Kde se daji najit iteratory?

            // CollectionType coll = br.Library.CreateCollection(CollectionKind.Collection, br.Library.Integer);

            // IteratorOperation iterOp = coll.LookupIteratorOperation("forAll"); // Slouzi k overeni validity a zjisteni navratoveho typu

            // iterOp.IsIteratorCountValid(1) - otestuje zda dana operace muze mit zadany pocet parametru
            // iterOp.BodyType - vrati pozadovany typ na body
            // iterOp.ExpressionType - navratovy typ iteratoru

            // Example
            //
            // IteratorOperation iterOp = ((CollectionType)selfDotTournament.Type).LookupIteratorOperation("forAll");
            // AST.OclExpression expr = new AST.IteratorExp(
            //    selfDotTournament, // na cem se iterator vola
            //    tDotOpen, // telo iteratoru
            //    "forAll",
            //    new List<VariableDeclaration>(new VariableDeclaration[] { tVarDelc }),
            //    iterOp.ExpressionType((CollectionType)selfDotTournament.Type, tDotOpen.Type, br.TypesTable) // navratovy typ iteratoru
            //    );

        }
    }
}
