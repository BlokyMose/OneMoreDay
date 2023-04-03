using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Saves
{
    [System.Serializable]
    public class SaveData
    {
        Dictionary<string, object> savedValues = new Dictionary<string, object>();

        public void SaveKeyValue(string k, object o)
        {
            if (savedValues.ContainsKey(k))
                savedValues[k] = o;
            else
                savedValues.Add(k, o);
        }

        public object LoadKey(string key)
        {
            object data;
            savedValues.TryGetValue(key, out data);
            return data;
        }

    }
}