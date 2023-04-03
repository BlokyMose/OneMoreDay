using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Encore.Interactables;
using Encore.CharacterControllers;
using Encore.Inventory;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Item Use")]
    public class ItemUse : Interactable
    {
        [Title("Item Use")]
        [SerializeField, InlineButton(nameof(_AddUseHook), " Add ", ShowIf = "@!"+nameof(useHook))]
        ItemUseHook useHook;

        #region [Methods: Inspector UI]

        public void _AddUseHook()
        {
            useHook = gameObject.AddComponent<ItemUseHook>();
        }

        #endregion

        public override string GetObjectName => 
            (GameManager.Instance.InventoryManager.GetClickedItem()
            ? "Store in: " : "") + base.GetObjectName;

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            if (useHook.Item == GameManager.Instance.InventoryManager.GetClickedItem())
            {
                GameManager.Instance.Player.MouseManager.CursorImageManager.HighlightItemSprite(true);
                return base.GetCursorImageModule();
            }
            else
            {
                return CursorImageManager.CursorImage.Disabled;
            }
        }

        protected override void InteractModule(GameObject interactor)
        {
            useHook.Use();
        }

        public void SetUseHook(ItemUseHook hook)
        {
            useHook = hook;
        }
    }
}
