using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Transform/Always Rotate")]
    public class AlwaysRotate : MonoBehaviour
    {
        [SerializeField] Vector2 minMaxDegree = new Vector2(-60, 60);
        [SerializeField] float speed = 2.5f;
        float targetDegree;

        void Start()
        {
            targetDegree = Random.Range(minMaxDegree.x, minMaxDegree.y);
        }

        void Update()
        {
            transform.localEulerAngles = Vector3.LerpUnclamped(transform.localEulerAngles, new Vector3(0, 0, targetDegree), Time.deltaTime * speed);

            if (Mathf.Approximately(transform.localEulerAngles.z, targetDegree))
            {
                targetDegree = Random.Range(minMaxDegree.x, minMaxDegree.y);
            }
        }
    }
}