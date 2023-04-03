using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using Encore.Interactables;
using Encore.Inventory;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Item Mono Storage")]
    public class ItemMonoStorage : Interactable
    {
        [Title("Item Mono Storage")]

        #region [Vars: Components]

        [SerializeField, InlineButton(nameof(_AddPickupHook)," Add ", ShowIf = "@!"+nameof(pickupHook))]
        ItemPickupHook pickupHook;

        [SerializeField, InlineButton(nameof(_AddUseHook)," Add ", ShowIf = "@!"+nameof(useHook))]
        ItemUseHook useHook;

        #endregion

        #region [Vars: Properties]

        [SerializeField, InlineButton(nameof(_SyncFromPickupHook), "Sync from Pickup" ,ShowIf = nameof(pickupHook))]
        Item item;

        [Serializable]
        public class ItemCapacityElement
        {
            public SpriteRenderer sr;
        }

        [SerializeField]
        List<ItemCapacityElement> capacityElements = new List<ItemCapacityElement>();

        [SerializeField]
        int initialCapacity = 0;

        [SerializeField, PropertyTooltip("Auto: -1")]
        int maxCapacity = -1;

        #endregion

        #region [Vars: Data Handlers]

        int currentCapacity = 0;

        #endregion

        #region [Methods: Inspector UI]

        private void OnValidate()
        {
            if (initialCapacity < 0 || (capacityElements.Count > 0 && initialCapacity > capacityElements.Count)) 
                initialCapacity = 0;
        }

        public void _AddPickupHook()
        {
            pickupHook = gameObject.AddComponent<ItemPickupHook>();
        }

        public void _AddUseHook()
        {
            useHook = gameObject.AddComponent<ItemUseHook>();
        }

        public void _SyncFromPickupHook()
        {
            if (pickupHook) item = pickupHook.Item;
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();

            currentCapacity = initialCapacity;
            foreach (var item in capacityElements)
            {
                highlightedSRs.Add(item.sr);
                item.sr.enabled = false;
            }

            for (int i = 0; i < initialCapacity; i++)
            {
                capacityElements[i].sr.enabled = true;
            }
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            Item grippedItem = GameManager.Instance.InventoryManager.GetClickedItem();
            if (grippedItem == null)
                return currentCapacity > 0 ? CursorImageManager.CursorImage.Grab : CursorImageManager.CursorImage.Disabled;
            else if (grippedItem != null && grippedItem == item)
                return CursorImageManager.CursorImage.Normal;
            else
                return CursorImageManager.CursorImage.Normal;
        }

        public override bool Interact(GameObject interactor)
        {
            Item grippedItem = GameManager.Instance.InventoryManager.GetClickedItem();

            // Pickup Item
            if (grippedItem == null)
            {
                if (currentCapacity <= 0) return false;
                return base.Interact(interactor);
            }

            // Store Item
            else if (grippedItem != null && grippedItem == item)
            {
                if (currentCapacity >= capacityElements.Count) return false;
                if (maxCapacity > 0 && currentCapacity >= maxCapacity) return false;
                return base.Interact(interactor);
            }

            // Gripped Item doesn't match this item
            else
            {
                return false;
            }
        }

        protected override void InteractModule(GameObject interactor)
        {
            Item grippedItem = GameManager.Instance.InventoryManager.GetClickedItem();

            // Pickup Item
            if (grippedItem == null)
            {
                currentCapacity--;
                pickupHook.Pickup();
                capacityElements[currentCapacity].sr.enabled = false;
            }

            // Store Item
            else if (grippedItem != null && grippedItem == item)
            {
                currentCapacity++;
                useHook.Use();
                capacityElements[currentCapacity - 1].sr.enabled = true;
            }

            // Gripped Item doesn't match this item
            else
            {
                Debug.Log("Item doesn't match");
            }
        }

        protected override bool ValidateHighlight()
        {
            if (base.ValidateHighlight())
                if (GameManager.Instance.InventoryManager.GetClickedItem() == item)
                    return true;

            return false;
        }

        protected override void HighlightModule(MouseManager.HighlightAppearance appearance)
        {
            base.HighlightModule(appearance);
            capacityElements[currentCapacity].sr.enabled = true;
        }

        public override void Unhighlight()
        {
            base.Unhighlight();
            capacityElements[currentCapacity].sr.enabled = false;
        }
    }

}
