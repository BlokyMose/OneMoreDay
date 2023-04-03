using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Dialogues;
using Encore.Interactables;
using Encore.Inventory;

namespace Encore.Interactables
{
    [RequireComponent(typeof(ItemPickupHook))]
    [RequireComponent(typeof(MonologueHook))]
    [AddComponentMenu("Encore/Interactables/Test")]
    public class Testing : Interactable
    {
        ItemPickupHook itemHook;
        MonologueHook monologueHook;

        protected override void Awake()
        {
            base.Awake();
            itemHook = GetComponent<ItemPickupHook>();
            monologueHook = GetComponent<MonologueHook>();
        }

        protected override void InteractModule(GameObject interactor)
        {
             
            if (itemHook.Pickup())
            {
                monologueHook.Speak(0);
            }
            else
            {

            }
        }
    }
}
