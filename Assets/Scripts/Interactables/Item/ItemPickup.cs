using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.CharacterControllers;
using Encore.Inventory;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Item Pickup")]
    public class ItemPickup : Interactable
    {
        #region [Vars]

        [Title("Item Pickup")]
        [SerializeField, InlineButton(nameof(_AddPickupHook), " Add ", ShowIf = "@!" + nameof(pickupHook))]
        ItemPickupHook pickupHook;

        #endregion

        #region [Methods: Inspector UI]

        public void _AddPickupHook()
        {
            pickupHook = gameObject.AddComponent<ItemPickupHook>();
        }

        #endregion

        public override string GetObjectName =>
            GameManager.Instance.InventoryManager != null
            ? GameManager.Instance.InventoryManager.CanTakeItem
                ? "Take: " + pickupHook.Item.ItemName
                : "Pocket's full for:\n"+pickupHook.Item.ItemName
            : pickupHook.Item.ItemName;

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            base.GetCursorImageModule();

            if (pickupHook.Item.IsGripOnly) return CursorImageManager.CursorImage.Grab;

            return GameManager.Instance.InventoryManager.CurrentSize < GameManager.Instance.InventoryManager.InventoryMaxSize
                ? CursorImageManager.CursorImage.Grab
                : CursorImageManager.CursorImage.Disabled;
        }

        public override bool Interact(GameObject interactor)
        {
            if (GameManager.Instance.InventoryManager.CurrentSize >= GameManager.Instance.InventoryManager.InventoryMaxSize)
                return false;
                
            return base.Interact(interactor);
        }

        protected override void InteractModule(GameObject interactor)
        {
            pickupHook.Pickup();
        }

        public void SetPickupHook(ItemPickupHook hook)
        {
            pickupHook = hook;
        }
    }
}
