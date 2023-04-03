using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Parallax To Camera")]
    public class ParallaxToCamera : MonoBehaviour
    {
        [SerializeField] [PropertyRange(0.001f, 1f)] float distanceRatio = 0.5f;

        // Data handlers
        Vector3 originalPos;

        private void OnEnable()
        {
            originalPos = transform.position;
        }

        private void Update()
        {
            Vector3 newPos = new Vector3(
                originalPos.x + (Camera.main.transform.position.x - transform.position.x) * distanceRatio,
                originalPos.y + (Camera.main.transform.position.y - transform.position.y) * distanceRatio,
                originalPos.z);

            transform.position = Vector3.LerpUnclamped(transform.position, newPos, Time.deltaTime * 10);
        }
    }
}