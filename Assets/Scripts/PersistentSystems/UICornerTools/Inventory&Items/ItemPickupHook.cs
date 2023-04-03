using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Inventory
{
    [AddComponentMenu("Encore/Inventory/Item Pickup Hook")]
    public class ItemPickupHook : ItemHook
    {
        public bool Pickup()
        {
            if (item != null)
            {
                return GameManager.Instance.InventoryManager.AddItem(item);
            }

            return false;
        }

    }
}