using System.Collections.Generic;
using Exolutio.Model.OCL.Types;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.OCL.ConstraintConversion
{
    public class VariableNamer
    {
        private readonly List<string> usedNames = new List<string>();

        public List<string> UsedNames
        {
            get { return usedNames; }
        }

        public string GetName(Classifier varType)
        {
            string nameBase = varType.Name.ToLower()[0].ToString();

            string result = NameSuggestor<string>.SuggestUniqueName(UsedNames, nameBase, item => item, true, false);
            UsedNames.Add(result);
            return result;
        }
    }
}