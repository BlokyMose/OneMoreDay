using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Interactable Modifier")]
    public class InteractableModifier : Interactable
    {
        [System.Serializable]
        public class InteractableModifiedData 
        {
            [LabelWidth(1)]
            public Interactable interactable;
            public bool isActive = true;
        }

        [Title("Interactable Modifier")]
        [SerializeField, ListDrawerSettings(DraggableItems = false)]
        List<InteractableModifiedData> interactables = new List<InteractableModifiedData> ();

        protected override void InteractModule(GameObject interactor)
        {
            foreach (var modifyInteractable in interactables)
            {
                modifyInteractable.interactable.Activate(modifyInteractable.isActive);
            }
        }
    }

}