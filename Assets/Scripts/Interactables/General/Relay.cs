using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Relay Interactable")]
    public class Relay : Interactable
    {
        [Title("Relay")]
        [InfoBox("Trigger targeted interactable when Relay is interacted", InfoMessageType.None), Required, SerializeField] 
        Interactable target;

        private void Start()
        {
            highlightedSRs = target.HighlightedSRs;
        }

        public override CursorImageManager.CursorImage GetCursorImage()
        {
            return target.GetCursorImage();
        }

        protected override void InteractModule(GameObject interactor)
        {
            target.Interact(interactor);
        }
    }
}

