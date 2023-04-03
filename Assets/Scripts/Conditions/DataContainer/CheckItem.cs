using Conditions;
using Encore.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Conditions
{
    [CreateAssetMenu(menuName = "SO/Interactable Conditions/Item")]
    public class CheckItem : Condition
    {
        [SerializeField]
        Item item;

        public override bool CheckCondition()
        {
            bool returnValue = false;

            if (GameManager.Instance.InventoryManager.GetClickedItem() == item)
                returnValue = true;

            if (reverseReturn) returnValue = !returnValue;
            return returnValue;
        }
    }
}
