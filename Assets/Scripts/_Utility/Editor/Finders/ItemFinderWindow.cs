using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Encore.Inventory;

public class ItemFinderWindow : OdinEditorWindow
{
    #region [Editor]

    [MenuItem("Tools/Finder/Item Finder %#i")]
    public static void OpenWindow()
    {   
        GetWindow<ItemFinderWindow>("Item Finder").Show();
    }

    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        Refresh();
        DeleteEmptyLocalItems();
        CheckSameSaveKey();
    }

    #region [Vars: Item]

    [Title("Item")]

    public enum ItemFinderFilter { All, Local, Project }
    [VerticalGroup("Item"), EnumToggleButtons, OnValueChanged(nameof(FindItems)), HideLabel]
    public ItemFinderFilter itemFilter = ItemFinderFilter.Local;

    [VerticalGroup("Item"), ListDrawerSettings(HideAddButton = true)]
    public List<Item> items;


    #endregion

    #region [Vars: ItemHook]

    [Title("Item Hook")]

    public enum ItemHookType { All, Use, Pickup }
    [VerticalGroup("Hook"), EnumToggleButtons, OnValueChanged(nameof(FindItemHooks)), HideLabel]
    public ItemHookType hookFilter = ItemHookType.All;

    [VerticalGroup("Hook"), ListDrawerSettings(HideAddButton = true)]
    public List<ItemHook> itemHooks;

    #endregion

    #region [Methods: Settings]

    [TitleGroup("Settings")]

    [HorizontalGroup("Settings/But"), Button(ButtonSizes.Large), GUIColor("@Color.green")]
    public void Refresh()
    {
        FindItems();
        FindItemHooks();
    }

    [HorizontalGroup("Settings/But"), Button("Ping Script", ButtonSizes.Large)]
    public void PingScript()
    {
        var scriptGUID = AssetDatabase.FindAssets(nameof(ItemFinderWindow) + " t:Script");
        Selection.activeObject = EditorGUIUtility.Load(AssetDatabase.GUIDToAssetPath(scriptGUID[0]));
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [TitleGroup("Settings"), Button, GUIColor(1f, 0.5f, 0.5f)]
    public void DeleteEmptyLocalItems()
    {
        itemFilter = ItemFinderFilter.Local;
        FindItems();
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(items[i].ItemName))
                ScriptableObject.DestroyImmediate(items[i]);
        }
    }

    [TitleGroup("Settings"), Button]
    public void CheckSameSaveKey()
    {
        itemFilter = ItemFinderFilter.All;
        FindItems();

        List<string> saveKeys = new List<string>();
        foreach (var item in items)
            saveKeys.Add(item.SaveKey);

        foreach (var saveKey in saveKeys)
        {
            int count = 0;
            foreach (var item in items)
            {
                if (item.SaveKey == saveKey)
                {
                    count++;
                    if (count >= 2) Debug.Log("Same key: [" + saveKey + "] in item: [" + item.ItemName + "]");
                }
            }
        }

        Debug.Log("Check same save key completed");
    }

    #endregion

    #region [Methods: Main}

    public void FindItems()
    {
        switch (itemFilter)
        {
            case ItemFinderFilter.All:
                items = new List<Item>(Resources.FindObjectsOfTypeAll<Item>());
                break;
            case ItemFinderFilter.Local:
                items = new List<Item>(FindObjectsOfType<Item>());
                break;
            case ItemFinderFilter.Project:
                var localItem = FindObjectsOfType<Item>();
                var projectItem = new List<Item>(Resources.FindObjectsOfTypeAll<Item>());
                foreach (var item in localItem) projectItem.Remove(item);
                items = new List<Item>(projectItem);
                break;
            default:
                break;
        }
    }

    public void FindItemHooks()
    {
        itemHooks = new List<ItemHook>();

        switch (hookFilter)
        {
            case ItemHookType.All:
                itemHooks = new List<ItemHook>(Resources.FindObjectsOfTypeAll<ItemHook>());
                break;
            case ItemHookType.Use:
                itemHooks = new List<ItemHook>(Resources.FindObjectsOfTypeAll<ItemUseHook>());
                break;
            case ItemHookType.Pickup:
                itemHooks = new List<ItemHook>(Resources.FindObjectsOfTypeAll<ItemPickupHook>());
                break;
        }
    }


    #endregion
}
