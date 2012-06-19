using System;
using System.Text.RegularExpressions;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.RNGTranslation
{
    public class NamingSupport
    {
        public PSMSchema PSMSchema { get; set; }

        public Log Log { get; set; }

        public void Initialize(PSMSchema psmSchema)
        {
            this.PSMSchema = psmSchema;
        }

        public string SuggestName(PSMComponent node, bool topLevelPattern = false, bool contentPattern = false, bool attribute = false)
        {
            string nameBase = NormalizeTypeName(node);

            if (contentPattern)
            {
                return nameBase + "-content";
            }

            if (topLevelPattern)
            {
                return nameBase;
            }

            if (attribute)
            {
                return nameBase;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Declaration of a handler that returns name for an element. 
        /// </summary>
        public delegate string GetItemName<Element>(Element item);

        public Regex normalizationRegex;

        private string whitespaceReplacement = "-";

        /// String by which whitespace is replaced in element names (default is "-").
        /// </summary>
        /// <value><see cref="string"/></value>
        public string WhitespaceReplacement
        {
            get
            {
                return whitespaceReplacement;
            }
            set
            {
                if (normalizationRegex == null)
                {
                    normalizationRegex = new Regex("\\w", RegexOptions.CultureInvariant);
                }
                whitespaceReplacement = value;
            }
        }

        /// <summary>
        /// Normalizes the name of the type (replaces whitespaces with <see cref="WhitespaceReplacement"/>).
        /// </summary>
        /// <typeparam name="PSMComponent">Type of construct.</typeparam>
        /// <param name="element">The element whose name should be normalized.</param>
        /// <param name="nameGetter">The function (conviniently a lambda expression) that 
        /// returns name for <paramref name="element"/>.</param>
        /// <returns></returns>
        public string NormalizeTypeName(PSMComponent element)
        {
            string typeName = element.Name;
            if (normalizationRegex == null)
            {
                normalizationRegex = new Regex("\\s", RegexOptions.CultureInvariant);
            }
            if (normalizationRegex.IsMatch(typeName))
            {
                string replace = normalizationRegex.Replace(typeName, WhitespaceReplacement);
                if (element is PSMAttribute)
                    Log.AddWarning(string.Format("For the purpuses of Relax NG translation name of attribute '{0}' is treated as '{1}'.", element, replace));
                else
                    Log.AddWarning(string.Format("For the purpuses of Relax NG translation name of attribute '{0}' is treated as '{1}'.", element, replace));
                return replace;
            }
            else 
                return typeName;
        }

    }
}