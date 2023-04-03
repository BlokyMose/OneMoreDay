using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    [RequireComponent(typeof(Collider2D))]
    [AddComponentMenu("Encore/Interactables/On Player Enter Interact")]
    public class OnPlayerEnterInteract : MonoBehaviour
    {
        [SerializeField, Tooltip("Get all interactables in this GO in runtime"), GUIColor("@"+nameof(autoDetect)+"?Color.green:Color.gray")]
        bool autoDetect = true;

        [SerializeField, EnableIf("@!"+nameof(autoDetect))]
        List<Interactable> interactables = new List<Interactable>();

        Collider2D col;

        private void Start()
        {
            col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (autoDetect)
            {
                interactables = new List<Interactable>(GetComponents<Interactable>());
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerController>() != null)
            {
                foreach (var interactable in interactables)
                {
                    if (interactable != null)
                        interactable.Interact(collision.gameObject);
                }
            }
        }
    }

}