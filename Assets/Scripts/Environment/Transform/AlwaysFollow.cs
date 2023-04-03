using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Transform/Always Follow")]
    public class AlwaysFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0, 0, 0);
        public void Setup(Transform target, Vector3 offset)
        {
            this.target = target;
            this.offset = offset;
        }

        void Update()
        {
            if (target != null)
                transform.position = target.position + offset;
        }
    }
}