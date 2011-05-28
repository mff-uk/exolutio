using System;
using System.Collections.Generic;

namespace Exolutio.Model.Versioning
{
    public interface IExolutioCloneable
    {
        /// <summary>
        /// Clones the component (creates a component of the same type in the model)
        /// </summary>
        /// <param name="projectVersion">The oldVersion where the copy is
        /// created.</param>
        /// <param name="createdCopies">The copies already created in this
        /// copying session.</param>
        /// <returns>Component of the same type</returns>
        /// <remarks>
        /// Should be overridden in all classes that can be instantiated, should
        /// not be  implemented for abstract classes (and base.Clone should
        /// never be called in any of the implementations). 
        /// </remarks>
        IExolutioCloneable Clone(ProjectVersion projectVersion, ElementCopiesMap createdCopies);

        /// <summary>
        /// Creates the copy of the component in the model in the appropriate
        /// location (all properties of the created component should correspond
        /// to the properties of the copied component). Can be easily
        /// implemented via the combination <see cref="Clone"/> & 
        /// <see cref="FillCopy"/>. 
        /// </summary>
        /// <param name="projectVersion">The oldVersion where the copy is
        /// created.</param>
        /// <param name="createdCopies">The copies already created in this
        /// copying session.</param>
        /// <returns>Copy of the component</returns>
        IExolutioCloneable CreateCopy(ProjectVersion projectVersion, ElementCopiesMap createdCopies);

        /// <summary>
        /// Fills all corresponding properties of 
        /// <paramref name="copyComponent"/> with properties  of this component.
        /// It is supposed to be called on instances created by 
        /// <see cref="Clone"/>.
        /// </summary>
        /// <param name="copyComponent">The filled component.</param>
        /// <param name="projectVersion">The oldVersion where the copy is
        /// created.</param>
        /// <param name="createdCopies">The copies already created in this
        /// copying session.</param>
        /// <remarks>
        /// Should always call base implementation like this: 
        /// <code>
        /// base.FillCopy(copyComponent, projectVersion, createdCopies);
        /// </code>
        /// </remarks>
        void FillCopy(IExolutioCloneable copyComponent, ProjectVersion projectVersion, ElementCopiesMap createdCopies);

        Guid ID { get; }
    }

    public static class IExolutioCloneableExt
    {
        public delegate void RegisterComponentDelegate<T>(T member) where T : IExolutioCloneable;

        /// <summary>
        /// Registers (already existing) copies of the members of <paramref name="sourceCollection"/> into <paramref name="targetCollection"/>.
        /// </summary>
        public static void CopyRefCollection<T>(this IExolutioCloneable cloneable, IEnumerable<T> sourceCollection, UndirectCollection<T> targetCollection,
            ProjectVersion projectVersion, ElementCopiesMap createdCopies, bool asGuid = false)
            where T : ExolutioObject
        {
            foreach (T collectionItem in sourceCollection)
            {
                Guid guid = createdCopies.GetGuidForCopyOf(collectionItem);
                if (!asGuid)
                {
                    targetCollection.Add(targetCollection.Project.TranslateComponent<T>(guid));
                }
                else
                {
                    targetCollection.AddAsGuid(guid);
                }
            }
        }
        
        /// <summary>
        /// Clones all the members of <paramref name="sourceCollection"/>, calls FillCopy for each cloned member and 
        /// </summary>
        public static void CopyCollection<T>(this IExolutioCloneable cloneable, UndirectCollection<T> sourceCollection, UndirectCollection<T> targetCollection, ProjectVersion projectVersion, ElementCopiesMap createdCopies)
            where T : ExolutioObject
        {
            foreach (T collectionItem in sourceCollection)
            {
                T itemCopy = (T)collectionItem.Clone(projectVersion, createdCopies);
                collectionItem.FillCopy(itemCopy, projectVersion, createdCopies);
                targetCollection.Add(itemCopy);
            }
        }

        /// <summary>
        /// Clones all the members of <paramref name="sourceCollection"/>, calls FillCopy for each cloned member and 
        /// </summary>
        public static void CopyDictionary<TKey, TValue>(this IExolutioCloneable cloneable, Dictionary<TKey, TValue> sourceDictionary,
            Dictionary<TKey, TValue> targetDictionary, ProjectVersion projectVersion, ElementCopiesMap createdCopies)
            where TKey : ExolutioObject
            where TValue: IExolutioCloneable
        {
            foreach (KeyValuePair<TKey, TValue> collectionItem in sourceDictionary)
            {
                TValue itemCopy = (TValue)collectionItem.Value.Clone(projectVersion, createdCopies);
                collectionItem.Value.FillCopy(itemCopy, projectVersion, createdCopies);
                Guid keyGuid = createdCopies.GetGuidForCopyOf(collectionItem.Key);
                targetDictionary.Add(projectVersion.Project.TranslateComponent<TKey>(keyGuid), itemCopy);
            }
        }

        public static T CreateTypedCopy<T>(this T ownedObject, ProjectVersion projectVersion, ElementCopiesMap createdCopies)
            where T : IExolutioCloneable
        {
            return (T) ownedObject.CreateCopy(projectVersion, createdCopies);
        }

    }
}