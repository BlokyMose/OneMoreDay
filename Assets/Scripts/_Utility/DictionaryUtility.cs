using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Utility
{
    public static class DictionaryUtility
    {
        /// <summary>
        /// Check if the dictionary contains the key before returning
        /// </summary>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            else 
                return defaultValue;
        }

        public static bool AddIfHasnt<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }
            else
            {
                dictionary.Add(key, value);
                return true;
            }
        }
    }
}