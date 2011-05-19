using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using EvoX.Controller;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.PSM.Grammar;
using EvoX.Model.PSM.Normalization;
using EvoX.Model.Serialization;
using EvoX.Model.Versioning;
using NUnit.Framework;
using Tests.CodeTests;

namespace Tests.Serialization
{
    [TestFixture]
    public class GrammarTests
    {
        [Test]
        public void TestGenerateGrammar()
        {
            ProjectSerializationManager projectSerializationManager = new ProjectSerializationManager();
            Project sampleProject = projectSerializationManager.LoadProject(@"..\..\..\EvoXWeb\EvoX\SampleFiles\Figure8.evox");

            PSMSchema psmSchema = sampleProject.SingleVersion.PSMSchemas[0];
            GrammarGenerator grammarGenerator = new GrammarGenerator();
            EvoX.Model.PSM.Grammar.Grammar g = grammarGenerator.GenerateGrammar(psmSchema);

            System.Diagnostics.Debug.WriteLine("Not normalized grammar: ");
            foreach (ProductionRule productionRule in g.ProductionRules)
            {
                System.Diagnostics.Debug.WriteLine(productionRule.ToString());
            }

            System.Diagnostics.Debug.WriteLine(string.Empty);

            Normalizer normalizer = new Normalizer();
            normalizer.Controller = new Controller(sampleProject);
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