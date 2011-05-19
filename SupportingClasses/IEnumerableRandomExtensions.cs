using System;
using System.Collections.Generic;
using System.Linq;

namespace EvoX.SupportingClasses
{
    public static class IEnumerableRandomExtensions
    {
        public static T ChooseOneRandomly<T>(this IEnumerable<T> items)
        {
            int count = items.Count();
            if (count > 0)
            {
                uint i = RandomGenerator.Next((uint)count);

                uint index = 0;
                foreach (T item in items)
                {
                    if (index == i)
                    {
                        return item;
                    }
                    index++;
                }

            }

            throw new ArgumentException("Collection is empty", "items");
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            T[] retArray = new T[items.Count()];
            Array.Copy(items.ToArray(), retArray, retArray.Length);

            for (int i = 0; i < retArray.Length; i += 1)
            {
                int swapIndex = RandomGenerator.Next(i, retArray.Length);
                if (swapIndex != i)
                {
                    T temp = retArray[i];
                    retArray[i] = retArray[swapIndex];
                    retArray[swapIndex] = temp;
                }
            }

            return retArray;
        }

        public static IEnumerable<T> RandomDelete<T>(this IEnumerable<T> items)
        {
            List<T> result = new List<T>();
            foreach (T item in items)
            {
                if (RandomGenerator.Toss(3, 1))
                {
                    result.Add(item);   
                }
            }
            return result;
        }
    }
}