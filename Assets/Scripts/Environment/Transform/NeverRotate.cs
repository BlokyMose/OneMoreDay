using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Transform/Never Rotate")]
    public class NeverRotate : MonoBehaviour
    {
        [SerializeField] Vector3 targetRotation = new Vector3(0, 0, 0);

        void Update()
        {
            transform.localEulerAngles = targetRotation;
        }
    }
}