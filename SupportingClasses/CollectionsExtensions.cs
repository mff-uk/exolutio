using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Exolutio.SupportingClasses
{
    public static class CollectionsExtensions
    {
        public static object FirstOrDefault(this IEnumerable collection)
        {
            foreach (var obj in collection)
            {
                return obj;
            }
            return null; 
        }

        public static object FirstOrDefault(this IEnumerable collection, Func<object, bool> predicate)
        {
            foreach (var obj in collection)
            {
                if (predicate(obj))
                    return obj;
            }
            return null;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            return collection.Count() == 0;
        }

        public static void AddIfNotContained<T>(this ICollection<T> collection, T item)
        {
            if (!collection.Contains(item))
                collection.Add(item);
        }

        public static void AddOrMoveToFront<T>(this IList<T> collection, T item)
        {
            if (collection.Contains(item))
                collection.Remove(item);
            collection.Insert(0, item);
        }

        public static string ConcatWithSeparator<T>(this IEnumerable<T> collection, string op)
        {
            string result = collection.Aggregate(string.Empty, (current, item) => current + (item + op));
            if (result.Length > 0)
                result = result.Remove(result.Length - op.Length, op.Length);
            return result;
        }

        public static string ConcatWithSeparator<T>(this IEnumerable<T> collection, Func<T, string> stringConverter, string op)
        {
            string result = collection.Aggregate(string.Empty, (current, item) => current + (stringConverter(item) + op));
            if (result.Length > 0)
                result = result.Remove(result.Length - op.Length, op.Length);
            return result;
        }

        public static List<TValue> CreateSubCollectionIfNeeded<TKey, TValue>(this IDictionary<TKey, List<TValue>> collection, TKey key)
        {
            List<TValue> values;
            if (!collection.ContainsKey(key))
            {
                values = new List<TValue>();
                collection[key] = values;
            }
            else
            {
                if (collection[key] == null)
                {
                    values = new List<TValue>();
                    collection[key] = values;
                }
                else
                {
                    values = collection[key];
                }
            }

            return values;
        }

        public static KeyValuePair<TKey, TValue> FindByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)
            where TValue : class
        {
            KeyValuePair<TKey, TValue> result = dictionary.FirstOrDefault(kvp => kvp.Value == value);
            return result; 
        }

        public static TValue ValueOrNull<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue: class
        {
            TValue value = null;
            dictionary.TryGetValue(key, out value);
            return value; 
        }

        public static List<TValue> Flatten<TKey, TValue>(this IDictionary<TKey, List<TValue>> collection)
        {
            List<TValue> result = new List<TValue>();
            foreach (KeyValuePair<TKey, List<TValue>> keyValuePair in collection)
            {
                result.AddRange(keyValuePair.Value);
            }
            return result;
        }

        public static IEnumerable<TValue> Prepend<TValue>(this IEnumerable<TValue> items, TValue item)
        {
            TValue[] prep = new[] {item};
            return prep.Concat(items);
        }

        public static IEnumerable<TValue> Prepend<TValue>(this IEnumerable<TValue> items, params TValue[] moreItems)
        {
            return moreItems.Concat(items);
        }

        public static IEnumerable<TValue> Append<TValue>(this IEnumerable<TValue> items, TValue item)
        {
            TValue[] prep = new[] { item };
            return items.Concat(prep);
        }

        public static IEnumerable<TValue> Append<TValue>(this IEnumerable<TValue> items, params TValue[] moreItems)
        {
            return items.Concat(moreItems);
        }

        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                queue.Enqueue(value);
            }
        }

        public static void EnqueueIfNotContained<T>(this Queue<T> queue, T item)
        {
            if (!queue.Contains(item))
            {
                queue.Enqueue(item);
            }
        }

        public static bool IsEmpty<T>(this Queue<T> queue)
        {
            return queue.Count == 0;
        }


        #if SILVERLIGHT
        public static void RemoveAll<TValue>(this List<TValue> list, Func<TValue, bool> predicate)
        {
            List<TValue> tmp = list.Where(predicate).ToList();
            foreach (TValue item in tmp)
            {
                tmp.Remove(item);
            }
        }
        #endif
    }
}