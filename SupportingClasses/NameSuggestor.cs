using System.Collections.Generic;
using System.Linq;

namespace Exolutio.SupportingClasses
{
	/// <summary>
	/// Static methods of this class can check whether a name for a member type is 
	/// unique in a collection and suggest a unique name for a member of a collection.
	/// </summary>
	/// <typeparam name="Type"></typeparam>
	public class NameSuggestor<Type>
	{
		/// <summary>
		/// Delegate should return name of <paramref name="item"/>
		/// </summary>
		/// <param name="item">item</param>
		/// <returns>name of <paramref name="item"/></returns>
		public delegate string GetItemName(Type item);

		/// <summary>
		/// Checks whether <paramref name="name"/> is unique in <paramref name="collection"/>
		/// </summary>
		/// <param name="collection">collection</param>
		/// <param name="name">name</param>
		/// <param name="getItemName">delegate of a function that returns name of object of type  <typeparamref name="Type"/></param>
		/// <returns>true if <paramref name="name"/> is not used in <paramref name="collection"/></returns>
		public static bool IsNameUnique(IEnumerable<Type> collection, string name, GetItemName getItemName)
		{
			bool unique = true;
			foreach (Type item in collection)
			{
				if (getItemName(item) == name)
				{
					unique = false;
				}
			}
			return unique;
		}

        /// <summary>
        /// Checks whether <paramref name="name"/> is unique in <paramref name="collection"/>
        /// </summary>
        /// <param name="collection">collection</param>
        /// <param name="name">name</param>
        /// <param name="getItemName">delegate of a function that returns name of object of type  <typeparamref name="Type"/></param>
        /// <param name="excluded">item that is not considered, if found in collection</param>
        /// <returns>true if <paramref name="name"/> is not used in <paramref name="collection"/></returns>
        public static bool IsNameUnique(IEnumerable<Type> collection, string name, GetItemName getItemName, Type excluded)
        {
            bool unique = true;
            foreach (Type item in collection)
            {
                if (getItemName(item) == name && !excluded.Equals(item))
                {
                    unique = false;
                }
            }
            return unique;
        }

		/// <summary>
		/// Checks whether <paramref name="name"/> is unique in <paramref name="collection"/>.
		/// </summary>
		/// <param name="collection">collection</param>
		/// <param name="name">name</param>
		/// <param name="getItemName">delegate of a function that returns name of object of type  <typeparamref name="Type"/></param>
		/// <param name="conflictingItem">item whose name is in conflict if result is false</param>
		/// <returns>true if <paramref name="name"/> is not used in <paramref name="collection"/></returns>
		public static bool IsNameUnique(IEnumerable<Type> collection, string name, GetItemName getItemName, out Type conflictingItem)
		{
			bool unique = true;
			conflictingItem = default(Type);
			foreach (Type item in collection)
			{
				if (getItemName(item) == name)
				{
					unique = false;
					conflictingItem = item;
					break;
				}
			}
			return unique;
		}

		/// <summary>
		/// Returns name that is not used in <paramref name="collection"/> (returned name will be of {prefix}{number} pattern)
		/// </summary>
		/// <example>
		/// This returns a unique name for a class
		/// NameSuggestor&lt;Class&gt;.SuggestUniqueName(ActiveDiagramView.Controller.ModelController.Model.Classes, "Class", modelClass =&gt; modelClass.Name)
		/// </example>
		/// <param name="collection">collection</param>
		/// <param name="prefix">prefix of suggested name</param>
		/// <param name="getItemName">delegate of a function that returns name of object of type  <typeparamref name="Type"/></param>
		/// <param name="firstItemWithout1">if set to true, first returned name will not use number suffix</param>
		/// <param name="startWithCount">if set to <c>true</c> first suggestion will be {prefix}{number} where {number is count of 
		/// items in <paramref name="collection"/> + 1, otherwise first suggestion will be {prefix}1.</param>
		/// <returns>new unique name</returns>
		public static string SuggestUniqueName(IEnumerable<Type> collection, string prefix, GetItemName getItemName, bool firstItemWithout1, bool startWithCount)
		{
			int c = startWithCount ? collection.Count() + 1 : 1;
			while (!IsNameUnique(collection, (c != 1 || !firstItemWithout1) ? prefix + c : prefix, getItemName))
				c++;

			if (c == 1 && firstItemWithout1)
				return prefix;
			else
				return prefix + c; 
		}

		/// <summary>
		/// Returns name that is not used in <paramref name="collection"/> (returned name will be of {prefix}{number} pattern)
		/// </summary>
		/// <param name="collection">collection</param>
		/// <param name="prefix">prefix of suggested name</param>
		/// <param name="getItemName">delegate of a function that returns name of object of type  <typeparamref name="Type"/></param>
		/// <returns>new unique name</returns>
		/// <example>
		/// This returns a unique name for a class
		/// NameSuggestor&lt;Class&gt;.SuggestUniqueName(ActiveDiagramView.Controller.ModelController.Model.Classes, "Class", modelClass => modelClass.Name)
		/// </example>
		public static string SuggestUniqueName(IEnumerable<Type> collection, string prefix, GetItemName getItemName)
		{
			return SuggestUniqueName(collection, prefix, getItemName, false, true);
		}
	}
}