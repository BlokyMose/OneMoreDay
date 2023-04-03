using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Utility/Self Destroy")]
    public class SelfDestroy : MonoBehaviour
    {
        [SerializeField] float delay = 5;

        void Start()
        {
            StartCoroutine(Delay());
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(delay);
                Destroy(gameObject);
            }
        }
    }
}