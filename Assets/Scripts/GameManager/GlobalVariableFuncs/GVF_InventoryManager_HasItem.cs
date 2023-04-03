using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.GlobalVariableFuncs
{
    [CreateAssetMenu(menuName = "SO/Variables/Func: Inventory/Has Item", fileName = "varf_Inventory_HasItem")]
    public class GVF_InventoryManager_HasItem : GlobalVariableFunc
    {
        public override string FuncName => nameof(GameManager.Instance.InventoryManager) + "." + nameof(GameManager.Instance.InventoryManager.HasItem);

        public override string Invoke(string parameters)
        {
            return GameManager.Instance.InventoryManager.HasItem(parameters)?"true":"false";
        }

        protected override void OnEnable()
        {
            callExample = FuncName + "(itemName)";
            resultExample = "true";
        }
    }
}
