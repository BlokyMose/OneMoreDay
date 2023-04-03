using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using Encore.Inventory;

namespace Encore.Interactables
{
    /// <summary>
    /// 
    /// [GENERAL IDEA] <br></br>
    /// - Store items into a specified storage element with the same item<br></br>
    /// - SpriteRenderer's sprite can be different with its Item.sprite by assigning manually in inspector<br></br>
    /// 
    /// [FEATURES] <br></br>
    /// - Multiple positions: storage element's positions can be changed based on storage's current size <br></br>
    /// - Prefab/Sprite: specify which way to display item, instantiating its prefab or use its sprite <br></br>
    /// 
    /// </summary>

[AddComponentMenu("Encore/Interactables/Item Storage Specific")]
    public class ItemStorageSpecific : ItemStorage
    {
        [VerticalGroup("Properties"), SerializeField]
        List<StorageElementSpecific> storageElements = new List<StorageElementSpecific>();

        #region [Methods: Utilities]

        public override IList<StorageElement> GetElements()
        {
            var elements = new List<StorageElement>();
            foreach (var storageElement in storageElements)
                elements.Add(storageElement);
            return elements;
        }

        public override void AddElement(StorageElement element)
        {
            storageElements.Add(new StorageElementSpecific(element));
        }

        protected override StorageElement GetAvailableElement(Item clickedItem)
        {
            if (clickedItem == null) return null;

            foreach (var element in storageElements)
            {
                // Also checks if the clickedItem matches one capacity element's item
                if (!element.pickup.IsActive && clickedItem == element.useHook.Item)
                    return element;
            }
            return null;
        }

        #endregion

        public override string GetObjectName =>
            GameManager.Instance.InventoryManager?.GetClickedItem() != null
            ? storageElements.Find(x => x.pickupHook.Item == GameManager.Instance.InventoryManager.GetClickedItem()) != null
                ? "Store in " + objectName
                : "Cannot store here"
            : base.GetObjectName;
    }

}