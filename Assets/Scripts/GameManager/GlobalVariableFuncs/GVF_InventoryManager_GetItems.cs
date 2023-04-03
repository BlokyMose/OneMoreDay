using System.Collections;
using UnityEngine;

namespace Encore.GlobalVariableFuncs
{
[CreateAssetMenu(menuName = "SO/Variables/Func: Inventory/Get Items", fileName ="varf_Inventory_GetItems")]
    public class GVF_InventoryManager_GetItems : GlobalVariableFunc
    {
        public override string FuncName { get { return nameof(GameManager.Instance.InventoryManager) + "." + nameof(GameManager.Instance.InventoryManager.GetItems); } }

        public override string Invoke(string parameters)
        {
            var result = "";
            foreach (var item in GameManager.Instance.InventoryManager.GetItems())
                result += item.ItemName + ", ";
            return result;
        }

        protected override void OnEnable()
        {
            callExample = FuncName + "()";
            resultExample = "item1, item2,";
        }
    }
}