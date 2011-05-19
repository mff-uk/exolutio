using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Model.PIM;
using EvoX.SupportingClasses;
using EvoX.Translation;

namespace EvoX.DataGenerator
{
	public class NamingSupport
	{
		/// <summary>
		/// Initializes the class, resets all lists and counters
		/// </summary>
		public void Initialize()
		{
			if (typeNameSuggestions == null)
				typeNameSuggestions = new TypeNameSuggestions();
			else
				typeNameSuggestions.Clear();
		}

		private string whitespaceReplacement = "-";

		/// <summary>
		/// String by which whitespace is replaced in element names (default is "-").
		/// </summary>
		/// <value><see cref="String"/></value>
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

		private Regex normalizationRegex;

		/// <summary>
		/// Logs where errors and warnings are written.
		/// </summary>
		/// <value><see cref="Log"/></value>
		public Log Log { get; set; }


		/// <summary>
		/// Normalizes the name of the type (replaces white spaces with <see cref="WhitespaceReplacement"/>).
		/// </summary>
        /// <typeparam name="TComponent">The type of the element.</typeparam>
		/// <param name="component">The element whose name should be normalized.</param>
		/// <param name="nameGetter">The function (conveniently a lambda expression) that 
		/// returns name for <paramref name="component"/>.</param>
		/// <returns></returns>
        public string NormalizeTypeName<TComponent>(TComponent component, GetItemName<TComponent> nameGetter = null)
            where TComponent: Component
		{
			string typeName = nameGetter != null ? nameGetter(component) : component.Name;
			if (normalizationRegex == null)
			{
				normalizationRegex = new Regex("\\s", RegexOptions.CultureInvariant);
			}
			if (normalizationRegex.IsMatch(typeName))
			{
				string replace = normalizationRegex.Replace(typeName, WhitespaceReplacement);
				if (component is PSMAttribute)
					Log.AddWarning(string.Format(LogMessages.XS_TRANSLATED_ATTRIBUTE_ALIAS, component.Name, replace));
				else
					Log.AddWarning(string.Format(LogMessages.XS_TRANSLATED_CLASS_NAME, component, replace));
				return replace;
			}
			else return typeName;
		}

        /// <summary>
        /// Contains suggestion for type names. 
        /// </summary>
        public TypeNameSuggestions typeNameSuggestions;

        /// <summary>
        /// Declaration of a handler that returns name for an element. 
        /// </summary>
        public delegate string GetItemName<TComponent>(TComponent item);

		public class TypeNameSuggestions : Dictionary<PSMClass, string>
		{
			
		}
	}
}