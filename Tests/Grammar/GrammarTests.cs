using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar;
using Exolutio.Model.PSM.Normalization;
using Exolutio.Model.Serialization;
using NUnit.Framework;

namespace Exolutio.Tests.Grammar
{
    [TestFixture]
    public class GrammarTests
    {
        [Test]
        public void TestGenerateGrammar()
        {
            ProjectSerializationManager projectSerializationManager = new ProjectSerializationManager();
            Project sampleProject = projectSerializationManager.LoadProject(@"..\..\..\Projects\Figure8.eXo");

            PSMSchema psmSchema = sampleProject.SingleVersion.PSMSchemas[0];
            GrammarGenerator grammarGenerator = new GrammarGenerator();
            Exolutio.Model.PSM.Grammar.Grammar g = grammarGenerator.GenerateGrammar(psmSchema);

            System.Diagnostics.Debug.WriteLine("Not normalized grammar: ");
            foreach (ProductionRule productionRule in g.ProductionRules)
            {
                System.Diagnostics.Debug.WriteLine(productionRule.ToString());
            }

            System.Diagnostics.Debug.WriteLine(string.Empty);

            Normalizer normalizer = new Normalizer();
            normalizer.Controller = new Controller.Controller(sampleProject);
            normalizer.NormalizeSchema(psmSchema);

            g = grammarGenerator.GenerateGrammar(psmSchema);
            System.Diagnostics.Debug.WriteLine("Normalized grammar: ");
            foreach (ProductionRule productionRule in g.ProductionRules)
            {
                System.Diagnostics.Debug.WriteLine(productionRule.ToString());
            }

            System.Diagnostics.Debug.WriteLine(string.Empty);
        }

       

        [Test]
        public void TestVersioning()
        {
           

        } 
    }
}