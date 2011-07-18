using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System
{
    public static class CLRExtensions
    {
        public static bool IsAmong(this object item, IList values)
        {
            return values.Contains(item);
        }

        public static bool IsAmong<T>(this T item, IEnumerable<T> values)
        {
            return values.Contains(item);
        }

        public static bool IsAmong<T>(this T item, params T[] values)
        {
            return values.Contains(item);
        }

        public static bool IsOfType<T>(this T item, params Type[] values)
        {
            return values.Contains(item.GetType());
        }

        public static bool DownCastSatisfies<T>(this object item, Func<T, bool> predicate)
        {
            if (!(item is T))
                return false;

            return predicate((T) item);
        }
    }
}