using Encore.CharacterControllers;
using Encore.Interactables;
using Encore.Inventory;
using Encore.Saves;
using Encore.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Item Storage")]
    public abstract class ItemStorage : Interactable, ISaveable
    {
        #region [Vars: Properties]

        [Title("Storage")]
        [VerticalGroup("Properties"), SerializeField]
        protected int maxSize = 1;

        #endregion

        #region [Vars: Data Handlers]

        protected int currentSize = 0;

        protected StorageElement highlightedElement;

        #endregion

        #region [Methods: Inspector]

        private void OnValidate()
        {
            existingItems.Clear();
            foreach (var item in new List<Item>(Resources.FindObjectsOfTypeAll<Item>()))
            {
                existingItems.Add(new StorageExistingItem()
                {
                    item = item,
                    onAdd = _AddElementExistingItem
                });

            }

            for (int i = GetElements().Count - 1; i >= 0; i--)
            {
                if ((GetElements()[i] as StorageElement).go == null) GetElements().RemoveAt(i);
            }
        }

        [Serializable]
        public class StorageExistingItem
        {
            public Action<Item> onAdd;
            [InlineButton(nameof(_Add), "Add")]
            public Item item;

            public void _Add()
            {
                onAdd(item);
            }
        }
        [FoldoutGroup("Add Element")]
        public List<StorageExistingItem> existingItems = new List<StorageExistingItem>();

        [HorizontalGroup("Add Element/But")]
        [Button("+ New Item", ButtonSizes.Large), GUIColor("@Color.green")]
        public void _AddElementNewItem()
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            _AddElementExistingItem(item);
        }

        [HorizontalGroup("Add Element/But")]
        [Button("+ No Item", ButtonSizes.Large), GUIColor("@Color.yellow")]
        public void _AddElementNoItem()
        {
            GameObject go = new GameObject();
            go.name = "Item_" + (GetElements().Count + 1).ToString();
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            StorageElement element = new StorageElement
            (
                go,
                go.AddComponent<ItemUseHook>(),
                go.AddComponent<ItemPickupHook>(),
                go.AddComponent<ItemPickup>(),
                go.AddComponent<SpriteRenderer>(),
                true
            );

            element.pickup.SetPickupHook(element.pickupHook);
            element.pickup.SetPostInteractionMode(PostInteractionMode.Deactivate);
            element.pickup.AddCapsuleCol();

            AddElement(element);

            #if UNITY_EDITOR
            // Utility
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
            #endif
        }

        [HorizontalGroup("Add Element/But")]
        [Button("+ Empty", ButtonSizes.Large), GUIColor(0.7f,0.4f,0.4f)]
        public void _AddElementEmpty()
        {
            GameObject go = new GameObject();
            go.name = "Empty_" + (GetElements().Count + 1).ToString();
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            StorageElement element = new StorageElement
            (
                go,
                go.AddComponent<ItemUseHook>(),
                go.AddComponent<ItemPickupHook>(),
                go.AddComponent<ItemPickup>(),
                go.AddComponent<SpriteRenderer>(),
                false
            );

            element.pickup.SetPickupHook(element.pickupHook);
            element.pickup.SetPostInteractionMode(PostInteractionMode.Deactivate);
            element.pickup.AddCapsuleCol();
            element.pickup.Activate(false);


            AddElement(element);
            
            #if UNITY_EDITOR

            // Utility
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            #endif
        }

        void _AddElementExistingItem(Item item)
        {
            GameObject go = new GameObject();
            go.name = "Item_" + (String.IsNullOrEmpty(item.ItemName) ? (GetElements().Count+1).ToString() : item.ItemName);
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            StorageElement element = new StorageElement
            (
                go,
                go.AddComponent<ItemUseHook>(),
                go.AddComponent<ItemPickupHook>(),
                go.AddComponent<ItemPickup>(),
                go.AddComponent<SpriteRenderer>(),
                true
            );

            element.sr.sprite = item.Sprite;
            element.useHook.Item = item;
            element.pickupHook.Item = item;

            element.pickup.SetPickupHook(element.pickupHook);
            element.pickup.SetObjectName(item.ItemName);
            element.pickup.SetPostInteractionMode(PostInteractionMode.Deactivate);
            element.pickup.AddCapsuleCol();

            AddElement(element);
            
            #if UNITY_EDITOR

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

            #endif
        }

        #endregion

        #region [Methods: Unity]
        protected override void Awake()
        {
            base.Awake();

            foreach (var element in GetElements())
            {
                var storageElement = element as StorageElement;
                highlightedSRs.Add(storageElement.sr);
                if (storageElement.pickup.IsActive) currentSize += storageElement.pickupHook.Item.StorageSize;
            }
        }

        private void OnEnable()
        {
            foreach (var element in GetElements())
            {
                StorageElement storageElement = element as StorageElement;
                storageElement.pickup.OnInteract += (succeed) => { if (succeed) ResetElement(storageElement); };
            }
        }

        private void OnDisable()
        {
            foreach (var element in GetElements())
            {
                StorageElement storageElement = element as StorageElement;
                storageElement.pickup.OnInteract -= (succeed) => { if (succeed) ResetElement(storageElement); };
            }
        }

        #endregion
        
        #region [Methods: Utilities]

        public abstract IList<StorageElement> GetElements();

        public abstract void AddElement(StorageElement element);

        protected abstract StorageElement GetAvailableElement(Item clickedItem);

        protected virtual void ResetElement(StorageElement element)
        {
            currentSize -= element.pickupHook.Item.StorageSize;
            element.Emptied();
        }

        #endregion

        #region [Methods: Interactables]

        public override string GetObjectName =>
            (GameManager.Instance.InventoryManager.GetClickedItem() ? "Store in " : "") + objectName;

        public override bool Interact(GameObject interactor)
        {
            var clickedItem = GameManager.Instance.InventoryManager.GetClickedItem();
            var element = GetAvailableElement(clickedItem);

            if (element != null && clickedItem.StorageSize + currentSize <= maxSize)
                return base.Interact(interactor);
            else
                return false;
        }

        protected override void InteractModule(GameObject interactor)
        {
            highlightedElement.sr.color = new Color(highlightedElement.sr.color.r, highlightedElement.sr.color.g, highlightedElement.sr.color.b, 1);
            highlightedElement = null;

            var clickedItem = GameManager.Instance.InventoryManager.GetClickedItem();
            var element = GetAvailableElement(clickedItem);
            if (element.Occupy(clickedItem))
            {
                currentSize += element.useHook.Item.StorageSize;
                foreach (var _element in GetElements())
                    _element.Move(currentSize);
            }
        }

        public override CursorImageManager.CursorImage GetCursorImage()
        {
            if (GameManager.Instance.InventoryManager == null)
                return CursorImageManager.CursorImage.Normal;

            var clickedItem = GameManager.Instance.InventoryManager.GetClickedItem();
            var element = GetAvailableElement(clickedItem);

            // Show disabled cursor if there's an unfulfilled condition
            if (conditionContainer != null && !conditionContainer.CheckCondition())
                return CursorImageManager.CursorImage.Disabled;
            else if (!isActive)
                return CursorImageManager.CursorImage.Disabled;
            else if (element == null || clickedItem.StorageSize + currentSize > maxSize)
                return CursorImageManager.CursorImage.Disabled;
            else
                return GetCursorImageModule();
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            GameManager.Instance.Player.MouseManager.CursorImageManager.HighlightItemSprite(true);
            return base.GetCursorImageModule();
        }

        protected override void HighlightModule(MouseManager.HighlightAppearance appearance)
        {
            var clickedItem = GameManager.Instance.InventoryManager?.GetClickedItem();
            highlightedElement = GetAvailableElement(clickedItem);

            if (highlightedElement != null && clickedItem.StorageSize + currentSize <= maxSize)
            {
                if (actionPreviews.Count == 0)
                {
                    var actionPreview = Instantiate(appearance.ActionPreviewPrefab, highlightedElement.go.transform, false);
                    var mat = GetHighlightMaterial(appearance.MatStoring);

                    highlightedElement.ChangeSprite(clickedItem);
                    actionPreview.SetupSprite(highlightedElement.sr, mat);
                    actionPreview.PlayAnimation(ActionPreview.AnimationMode.PutDown);
                    actionPreviews.Add(actionPreview);
                }
                else
                {
                    this.StopCoroutineIfExists(corDestroyingActionPreviews);
                    corDestroyingActionPreviews = null;
                    foreach (var actionPreview in actionPreviews)
                        actionPreview.PlayAnimation(ActionPreview.AnimationMode.PutDown);
                }
            }


        }

        #endregion

        #region [Methods: ISaveable]

        public void Save()
        {
            GOSaver saver = GetComponent<GOSaver>();
            if (saver == null) return;

            List<string> storedItems = new List<string>();
            foreach (var element in GetElements())
            {
                var _element = element as StorageElement;
                if (_element.isOccupied)
                    storedItems.Add(_element.pickupHook.Item.ItemName);
                else
                    storedItems.Add(StorageElement.EMPTY);
            }
            saver.AddProperty(GetType().ToString(), storedItems);
        }

        public void Load(GameManager.GameAssets gameAssets)
        {
            GOSaver saver = GetComponent<GOSaver>();
            if (saver == null) return;

            var storedItems = (List<string>) saver.GetProperty(GetType().ToString());

            if (storedItems == null) return;

            // Reset elementStorages
            foreach (var element in GetElements())
                ResetElement(element);

            // Apply savedItems
            for (int i = 0; i < GetElements().Count; i++)
            {
                var element = GetElements()[i];
                if (storedItems[i] != StorageElement.EMPTY)
                {
                    Item item = gameAssets.resources.Items.Find(item=>item.ItemName == storedItems[i]);
                    if (item == null)
                    {
                        Debug.Log("Can't find item: ["+storedItems[i]+"] in project");
                        continue;
                    }
                    element.SetupItem(item);
                    element.isOccupied = true;
                    currentSize += item.StorageSize;
                }
                else
                {
                    element.pickup.Activate(false);
                }
            }
        }

        #endregion
    }
}