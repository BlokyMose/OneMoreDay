using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.CharacterControllers
{
    [RequireComponent(typeof(Collider2D))]
    [AddComponentMenu("Encore/Character Controllers/Collider Detector")]
    public class ColliderDetector : MonoBehaviour
    {
        List<Collider2D> colliders = new List<Collider2D>();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!colliders.Contains(collision))
            {
                colliders.Add(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (colliders.Contains(collision))
            {
                colliders.Remove(collision);
            }
        }

        public List<Collider2D> GetColliders()
        {
            return colliders;
        }
    }
}