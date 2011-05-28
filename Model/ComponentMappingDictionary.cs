using System;
using System.Collections.Generic;
using Exolutio.Model.Resources;
using Exolutio.SupportingClasses;

namespace Exolutio.Model
{
    public class ComponentMappingDictionary
    {
        private ExolutioDictionary<Guid, ExolutioObject> internalDictionary = new ExolutioDictionary<Guid, ExolutioObject>();

        #region delegations to internal dictionary

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.Dictionary`2.KeyCollection"/> containing the keys in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </returns>
        public Dictionary<Guid, ExolutioObject>.KeyCollection Keys
        {
            get { return internalDictionary.Keys; }
        }

        /// <summary>
        /// Gets a collection containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.Dictionary`2.ValueCollection"/> containing the values in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </returns>
        public Dictionary<Guid, ExolutioObject>.ValueCollection Values
        {
            get { return internalDictionary.Values; }
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </summary>
        /// <returns>
        /// The number of key/value pairs contained in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </returns>
        public int Count
        {
            get { return internalDictionary.Count; }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException"/>, and a set operation creates a new element with the specified key.
        /// </returns>
        /// <param name="key">The key of the value to get or set.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> does not exist in the collection.</exception>
        public ExolutioObject this[Guid key]
        {
            get
            {
                TestGuidNonEmpty(key);
                return internalDictionary[key];
            }
            set
            {
                TestGuidNonEmpty(key); 
                TestValueNonEmpty(value);
                internalDictionary[key] = value;
            }
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </summary>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false.  This method returns false if <paramref name="key"/> is not found in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </returns>
        /// <param name="key">The key of the element to remove.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool Remove(Guid key)
        {
            TestGuidNonEmpty(key);
            return internalDictionary.Remove(key);
        }

        /// <summary>
        /// Removes all keys and values from the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </summary>
        public void Clear()
        {
            internalDictionary.Clear();
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.Dictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key of the value to get.</param><param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(Guid key, out ExolutioObject value)
        {
            TestGuidNonEmpty(key);
            return internalDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param><param name="value">The value of the element to add. The value can be null for reference types.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception><exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</exception>
        public void Add(Guid key, ExolutioObject value)
        {
            TestGuidNonEmpty(key);
            TestValueNonEmpty(value);
            internalDictionary.Add(key, value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2"/> contains the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.Dictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.Dictionary`2"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(Guid key)
        {
            TestGuidNonEmpty(key);
            return internalDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.Dictionary`2"/> contains an element with the specified value; otherwise, false.
        /// </returns>
        /// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.Dictionary`2"/>. The value can be null for reference types.</param>
        public bool ContainsValue(Component value)
        {
            TestValueNonEmpty(value);
            return internalDictionary.ContainsValue(value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.Dictionary`2.Enumerator"/> structure for the <see cref="T:System.Collections.Generic.Dictionary`2"/>.
        /// </returns>
        public Dictionary<Guid, ExolutioObject>.Enumerator GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        #endregion

        private static void TestGuidNonEmpty(Guid key)
        {
            if (key == Guid.Empty)
            {
                throw new ExolutioModelException("Key guid must not be empty. ");
            }
        }

        private static void TestValueNonEmpty(ExolutioObject component)
        {
            if (component == null)
            {
                throw new ExolutioModelException("Component must not be empty. ");
            }
        }

        public T FindComponent<T>(Guid key)
            where T : ExolutioObject
        {
            ExolutioObject component;
            TestGuidNonEmpty(key);
            TryGetValue(key, out component);
            if (component == null)
            {
                throw new ExolutioModelException(string.Format(Exceptions.Lookup_for_component_of_type___0___failed__key___1___not_found_in_the_component_dictionary_, typeof(T).Name, key));
            }
            return (T)component;
        }

        public ExolutioObject FindComponent(Guid key)
        {
            ExolutioObject component;
            TestGuidNonEmpty(key);
            TryGetValue(key, out component);
            if (component == null)
            {
                throw new ExolutioModelException(string.Format(Exceptions.Lookup_for_component_of_type___0___failed__key___1___not_found_in_the_component_dictionary_, "(not specified)", key));
            }
            return component;
        }

        public bool VerifyComponentType<T>(Guid key)
        {
            ExolutioObject component;
            TestGuidNonEmpty(key);
            TryGetValue(key, out component);
            if (component == null)
            {
                throw new ExolutioModelException(string.Format(Exceptions.Lookup_for_component_of_type___0___failed__key___1___not_found_in_the_component_dictionary_, typeof(T).Name, key));
            }
            if (component is T)
            {
                return true;
            }
            else
            {
                return false; 
            }
        }
    }
}