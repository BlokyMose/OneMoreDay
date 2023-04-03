using Encore.Inventory;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemActionGOCreatorWindow : OdinEditorWindow
{
    #region [Editor]

    [MenuItem("GameObject/Create Item Action GO", false, 0)]
    public static void OpenWindow()
    {
        GetWindow<ItemActionGOCreatorWindow>("Item Action GO").Show();
    }

    private void OnEnable()
    {
        Refresh();
    }

    #endregion

    #region [Classes]

    [System.Serializable]
    public class ItemActionContainer
    {
        [HideLabel, InlineButton(nameof(InstantiateActionGO), "Create", ShowIf = nameof(canCreate))]
        public Item item;

        bool canCreate = false;

        public ItemActionContainer(Item item)
        {
            this.item = item;
            if (item.ActionPrefab != null) canCreate = true;
        }

        void InstantiateActionGO()
        {
            GameObjectMenuExtension.CreateItemActionGO(item);
        }
    }

    #endregion

    #region [Vars: Item]

    [Title("Item")]

    public enum ItemFinderFilter { All, Local, Project }
    [VerticalGroup("Item"), EnumToggleButtons, OnValueChanged(nameof(FindItems)), HideLabel]
    public ItemFinderFilter itemFilter = ItemFinderFilter.All;

    [VerticalGroup("Item"), ListDrawerSettings(HideAddButton = true)]
    public List<ItemActionContainer> items = new List<ItemActionContainer>();


    #endregion

    public void FindItems()
    {
        items.Clear();
        switch (itemFilter)
        {
            case ItemFinderFilter.All:
                var itemsInAll = new List<Item>(Resources.FindObjectsOfTypeAll<Item>());
                foreach (var item in itemsInAll) items.Add(new ItemActionContainer(item));
                break;
            case ItemFinderFilter.Local:
                var itemsInLocal = new List<Item>(FindObjectsOfType<Item>());
                foreach (var item in itemsInLocal) items.Add(new ItemActionContainer(item));
                break;
            case ItemFinderFilter.Project:
                var localItem = FindObjectsOfType<Item>();
                var projectItem = new List<Item>(Resources.FindObjectsOfTypeAll<Item>());
                foreach (var item in localItem) projectItem.Remove(item);
                var itemsInProject = new List<Item>(projectItem);
                foreach (var item in itemsInProject) items.Add(new ItemActionContainer(item));
                break;
            default:
                break;
        }
    }

    public void Refresh()
    {
        FindItems();
    }
}
