using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Inventory
{
    [AddComponentMenu("Encore/Inventory/Item Use Hook")]
    public class ItemUseHook : ItemHook
    {
        public bool Use()
        {
            if (GameManager.Instance.InventoryManager.GetClickedItem() == item)
                return GameManager.Instance.InventoryManager.UseItem(item);
            else
                return false;
        }
    }
}