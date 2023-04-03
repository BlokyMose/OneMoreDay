using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Relay Action")]
    public class RelayAction : Interactable
    {
        [Title("Relay Action")]
        [SerializeField] 
        UnityEvent executeEvent = new UnityEvent();
        public UnityEvent ExecuteEvent { get { return executeEvent; } }

        [SerializeField] 
        CursorImageManager.CursorImage cursorImage = CursorImageManager.CursorImage.Normal;

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return cursorImage;
        }

        protected override void InteractModule(GameObject interactor)
        {
            executeEvent.Invoke();
        }
    }
}
