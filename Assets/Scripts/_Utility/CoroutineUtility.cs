using System.Collections;
using UnityEngine;

namespace Encore.Utility
{
    public static class CoroutineUtility
    {
        public static Coroutine RestartCoroutine(this MonoBehaviour go, IEnumerator routine)
        {
            if (routine != null) go.StopCoroutine(routine);
            return go.StartCoroutine(routine);
        }

        public static void StopCoroutineIfExists(this MonoBehaviour go, Coroutine routine)
        {
            if (routine != null) go.StopCoroutine(routine);
        }
    }
}