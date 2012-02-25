using System;
using System.Text.RegularExpressions;
using Exolutio.SupportingClasses;

namespace Exolutio.Model.PSM.Grammar.XSDTranslation
{
    public class NamingSupport
    {
        public PSMSchema PSMSchema { get; set; }

        public Log Log { get; set; }

        public void Initialize(PSMSchema psmSchema)
        {
            this.PSMSchema = psmSchema;
        }

        public string SuggestName(PSMComponent node, bool complexType = false, bool attributeGroup = false, bool contentGroup = false, bool optGroup = false, bool attribute = false)
        {
            string nameBase = NormalizeTypeName(node);

            if (attributeGroup)
            {
                return nameBase + "-att";
            }

            if (optGroup)
            {
                return nameBase + "-att-opt";
            }

            if (contentGroup)
            {
                return nameBase + "-elm";
            }

            if (complexType)
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
                    Log.AddWarning(string.Format("For the purpuses of XSD translation name of attribute '{0}' is treated as '{1}'.", element, replace));
                else
                    Log.AddWarning(string.Format("For the purpuses of XSD translation name of attribute '{0}' is treated as '{1}'.", element, replace));
                return replace;
            }
            else 
                return typeName;
        }

    }
}