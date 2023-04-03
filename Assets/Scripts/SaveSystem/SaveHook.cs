using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Saves
{
    [AddComponentMenu("Encore/Saves/Save Hook")]
    public class SaveHook : MonoBehaviour
    {
        public string key;

        public void SaveKey(object o)
        {
            Debug.LogWarning(nameof(SaveHook) + ".cs has been deprecated");
            //SceneMaster.current.SaveKey(key, o);
        }

        public object LoadKey()
        {
            Debug.LogWarning(nameof(SaveHook) + ".cs has been deprecated");
            return null;
            //return SceneMaster.current.LoadKey(key);
        }
    }
}