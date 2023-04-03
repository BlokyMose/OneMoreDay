using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Encore.Inventory
{
    [AddComponentMenu("Encore/Inventory/Inventory Manager")]
    public class InventoryManager : UICornerTool
    {
        #region [Classes]

        class ItemInventory
        {
            public ItemHolder ItemHolder;
            public Item item;

            public ItemInventory(ItemHolder itemHolder, Item item)
            {
                ItemHolder = itemHolder;
                this.item = item;
            }
        }

        #endregion

        #region [Vars: External Components]

        [Title("Inventory Manager")]
        [SerializeField] GameObject itemHolderPrefab;
        [SerializeField] GameObject inventoryUI;

        #endregion

        #region [Vars: Properties]

        private Color colorHighlight = new Color(1, 1, 1, 1);
        private Color colorNormal = new Color(1, 1, 1, 0.75f);
        private Color colorClickedNormal = new Color(1, 1, 0, 0.5f);
        private Color colorClickedHighlight = new Color(1, 1, 0, 1);

        public int InventoryMaxSize { get { return inventoryMaxSize; } }
        private int inventoryMaxSize = 5;

        #endregion

        #region [Vars: Data Handlers]

        public static readonly string INVENTORY_SAVE_KEY = "Inventory";
        ItemInventory clickedItem;
        bool keepInventoryOpen = false;
        List<ItemInventory> items = new List<ItemInventory>();

        public int CurrentSize { get { return currentSize; } }
        int currentSize = 0;

        #endregion

        public bool CanTakeItem
        {
            get
            {
                return currentSize < inventoryMaxSize;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // Clean parent
            for (int i = inventoryUI.transform.childCount - 1; i >= 0; i--)
                Destroy(inventoryUI.transform.GetChild(i).gameObject);
        }

        protected override void Start()
        {
            base.Start();
            currentSize = items.Count;
        }

        #region [Methods: UICornerTool]

        public override void Show(bool isShowing)
        {
            if (!isShowing) if (keepInventoryOpen) return;
            base.Show(isShowing);
        }

        public void Save(GameManager.GameAssets gameAssets)
        {
            List<string> itemsSaveKey = new List<string>();
            foreach (var item in items)
                itemsSaveKey.Add(item.item.SaveKey);


            var saveData = new Serializables.InventoryData(
                items: itemsSaveKey,
                inventoryMaxSize: inventoryMaxSize,
                clickedItem: clickedItem!=null ? clickedItem.item.SaveKey : null
                );
            gameAssets.systemData.inventoryData = saveData;
        }

        public void Load(GameManager.GameAssets gameAssets)
        {
            // Clear items when loading a new scene
            for (int i = items.Count - 1; i >= 0; i--)
                Destroy(items[i].ItemHolder.gameObject);
            items.Clear();
            currentSize = 0;

            var saveData = gameAssets.systemData.inventoryData;
            if (saveData == null) return;

            foreach (var itemSaveKey in saveData.items)
            {
                var foundItem = gameAssets.resources.Items.Find(_item => _item.SaveKey == itemSaveKey);
                if (foundItem != null)
                {
                    AddItem(foundItem, false);
                }
                else
                {
                    Debug.Log("Cannt find item: "+itemSaveKey);
                }
            }

            // Grip gripOnly Item
            if (saveData.clickedItem != null)
            {
                var grippingItem = gameAssets.resources.Items.Find(_item => _item.SaveKey == saveData.clickedItem);
                clickedItem = new ItemInventory(null, grippingItem);
            }
        }

        public override void OnAfterSceneLoad()
        {
            if (clickedItem != null && clickedItem.item.IsGripOnly)
            {
                AddItem(clickedItem.item);
            }
        }

        public override void OnBeforeSceneLoad()
        {
            if (clickedItem != null && clickedItem.ItemHolder != null)
            {
                ResetClickedItem();
            }
        }

        #endregion

        #region [Methods: Main]

        public bool AddItem(Item item, bool doShowTemporary = true)
        {
            // Don't add item to inventory if gripOnly
            if (!item.IsGripOnly)
            {
                // Check inventory size to item size
                if (CanTakeItem)
                {
                    #region [Instantiation]

                    GameObject go = Instantiate(itemHolderPrefab, inventoryUI.transform, false);
                    ItemHolder itemHolder = go.GetComponent<ItemHolder>();
                    itemHolder.Item = item;
                    itemHolder.SetText(item.ItemName);

                    if (itemHolder.Item.ActionPrefab != null)
                    {
                        // Get data from ActionExecutioner which currently exists in project folder; not instantiated yet
                        var actionExecutioner = itemHolder.Item.ActionPrefab.GetComponent<ActionExecutioner>();
                        if (actionExecutioner != null)
                        {
                            foreach (var action in actionExecutioner.Actions)
                                itemHolder.AddAction(action, itemHolder.Item.ActionPrefab);
                        }
                        else
                        {
                            Debug.Log(itemHolder.Item.ItemName + ": ActionPrefab requires ActionExecutioner");
                        }
                    }

                    Image image = go.GetComponent<Image>();
                    image.color = new Color(1, 1, 1, 0.5f);

                    #region [Event Trigger]

                    EventTrigger et = go.AddComponent<EventTrigger>();

                    EventTrigger.Entry entry_click = new EventTrigger.Entry();
                    entry_click.eventID = EventTriggerType.PointerClick;
                    entry_click.callback.AddListener((data) => { OnItemClicked(itemHolder); });
                    et.triggers.Add(entry_click);

                    EventTrigger.Entry entry_enter = new EventTrigger.Entry();
                    entry_enter.eventID = EventTriggerType.PointerEnter;
                    entry_enter.callback.AddListener((data) =>
                    {
                        itemHolder.ShowText(true);

                        if (keepInventoryOpen && clickedItem.ItemHolder == itemHolder)
                        {
                            image.color = colorClickedHighlight;
                        }
                        else
                        {
                            image.color = colorHighlight;
                        }
                    });
                    et.triggers.Add(entry_enter);

                    EventTrigger.Entry entry_exit = new EventTrigger.Entry();
                    entry_exit.eventID = EventTriggerType.PointerExit;
                    entry_exit.callback.AddListener((data) =>
                    {
                        itemHolder.ShowText(false);
                        if (keepInventoryOpen && clickedItem.ItemHolder == itemHolder)
                        {
                            image.color = colorClickedNormal;
                        }
                        else
                        {
                            image.color = colorNormal;
                        }
                    });
                    et.triggers.Add(entry_exit);

                    #endregion

                    #endregion

                    currentSize++;
                    var itemInventory = new ItemInventory(itemHolder, itemHolder.Item);
                    items.Add(itemInventory);

                    // Add animation
                    if (doShowTemporary)
                    {
                        if (corShowTemporary != null) StopCoroutine(corShowTemporary);
                        corShowTemporary = StartCoroutine(ShowTemporary(2.5f));
                    }
                }
            }

            // Grip only mode
            else
            {
                clickedItem = new ItemInventory(null, item);

                GameManager.Instance.Player.MouseManager.CursorImageManager.SetItemSprite(item.Sprite);
                GameManager.Instance.Player.Controller.RigController?.GripItem(item);
                GameManager.Instance.EnableUICornerTools(false, false);
            }

            return true;
        }

        public bool UseItem(Item item)
        {
            var foundItemHolder = items.Find(itemInventory => itemInventory.item == item);
            if (foundItemHolder != null)
            {
                // Reset data handlers
                keepInventoryOpen = false;
                currentSize--;
                clickedItem = null;
                animator.SetBool(boo_show, false);

                // Destroy components
                foundItemHolder.ItemHolder.DestroyActionButtons();
                Destroy(foundItemHolder.ItemHolder.gameObject);
                items.Remove(foundItemHolder);

                // Reset external components
                GameManager.Instance.Player.MouseManager.CursorImageManager.RemoveItemSprite();
                GameManager.Instance.EnableUICornerTools(true, false);
                GameManager.Instance.Player.Controller.RigController?.UngripAllItems(false);
                return true;
            }
            else if (item.IsGripOnly && clickedItem != null && item == clickedItem.item)
            {
                ResetClickedItem();
                return true;
            }
            return false;
        }

        public List<Item> GetItems()
        {
            var _items = new List<Item>();
            foreach (var item in items) _items.Add(item.item);
            return _items;
        }

        public bool HasItem(string itemName)
        {
            return items.Find(i => i.item.ItemName == itemName) != null ? true : false;
        }

        #endregion

        #region [Methods: ClickedItem]

        public Item GetClickedItem()
        {
            return clickedItem == null ? null : clickedItem.item;
        }

        void OnItemClicked(ItemHolder itemHolder)
        {
            // Click an inventory item after reset
            if (clickedItem == null)
            {
                SetClickedItem(itemHolder);
            }
            else
            {
                // Click the same inventory item while holding the item to reset
                if (clickedItem.ItemHolder == itemHolder)
                {
                    ResetClickedItem();
                }

                // Click another inventory item while holding an item
                else
                {
                    if (clickedItem.ItemHolder != null)
                        clickedItem.ItemHolder.GetComponent<Image>().color = colorNormal;
                    SetClickedItem(itemHolder);
                }
            }
        }

        void SetClickedItem(ItemHolder itemHolder)
        {
            keepInventoryOpen = true;
            clickedItem = new ItemInventory(itemHolder, itemHolder.Item);
            clickedItem.ItemHolder.GetComponent<Image>().color = colorClickedHighlight;
            GameManager.Instance.Player.MouseManager.CursorImageManager.SetItemSprite(itemHolder.Item.Sprite);
            GameManager.Instance.EnableUICornerTools(false, false);
            IsCursorHovering = false;

            var rigController = GameManager.Instance.Player.Controller.RigController;
            if (rigController != null)
            {
                rigController.GripItem(itemHolder.Item);
                Transform targetArmLeft = GameManager.Instance.Player.Controller.RigController.TargetArmleft;
                GameManager.Instance.Player.MouseManager.SetCursorPosition(Camera.main.WorldToScreenPoint(targetArmLeft.position));
            }
        }

        public void ResetClickedItem()
        {
            if (clickedItem == null) return; // in case being accessed by force

            keepInventoryOpen = false;
            if (clickedItem.ItemHolder != null)
                clickedItem.ItemHolder.GetComponent<Image>().color = colorHighlight;
            clickedItem = null; 
            GameManager.Instance.Player.MouseManager.CursorImageManager.RemoveItemSprite();
            GameManager.Instance.Player.Controller.RigController?.UngripAllItems(true);
            GameManager.Instance.EnableUICornerTools(true, false);
        }

        #endregion
    }
}