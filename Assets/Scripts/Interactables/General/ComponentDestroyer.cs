using Encore.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Component Destroyer")]
    public class ComponentDestroyer : Interactable
    {
        [SerializeField] List<Object> components = new List<Object> ();

        protected override void InteractModule(GameObject interactor)
        {
            foreach (var component in components)
            {
                Destroy(component);
            }
        }
    }
}

