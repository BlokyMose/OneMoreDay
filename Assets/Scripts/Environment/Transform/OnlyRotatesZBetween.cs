using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Transform/Only Rotates Z Between")]
    public class OnlyRotatesZBetween : MonoBehaviour
    {
        [SerializeField] Vector2 minMax = new Vector2(-180, 0);

        private void LateUpdate()
        {
            if (transform.localEulerAngles.z < minMax.x)
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, minMax.x);
            else if (transform.localEulerAngles.z > minMax.y)
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, minMax.y);
        }
    }
}