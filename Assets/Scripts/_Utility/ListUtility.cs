using System.Collections.Generic;

namespace Encore.Utility
{
    public static class ListUtility
    {
        /// <summary>
        /// Returns a member of the list by index; if index is not in range of the list's length, returns defaultValue
        /// </summary>
        public static T GetAt<T>(this IList<T> list, int index, T defaultValue = null) where T : class
        {
            if (index < 0)
                return defaultValue;
            else if (list.Count > index)
                return list[index];
            else
                return defaultValue;
        }



        /// <summary>
        /// Returns the last item in the list
        /// </summary>
        public static T GetLast<T>(this IList<T> list) where T : class
        {
            return list[list.Count - 1];
        }

        public static void AddIfHasnt<T>(this IList<T> list, T addValue)
        {
            if(!list.Contains(addValue)) list.Add(addValue);
        }

        public static void RemoveIfHas<T>(this IList<T> list, T addValue)
        {
            if (!list.Contains(addValue)) list.Remove(addValue);
        }

        public static void AddRangeUnique<T>(this IList<T> list, IList<T> otherList)
        {
            foreach (var item in otherList)
            {
                if (!list.Contains(item)) list.Add(item);
            }
        }
    }
}