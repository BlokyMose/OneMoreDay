using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using Encore.Inventory;

public class ItemStoragePlain : MonoBehaviour
{
    #region [Vars: Data Handlers]

    [SerializeField] List<Item> items = new List<Item>();

    #endregion

    #region [Methods: Inspector UI]

    [Button("Add Empty Item", 25), GUIColor(0.3f,0.8f,0.3f), PropertyOrder(-1)]
    public void AddEmptyItem()
    {
        Item newItem = ScriptableObject.CreateInstance<Item>();
        newItem.Setup(
            null,
            "",
            "",
            null);
        items.Add(newItem);

        saveFolderPath = saveFolderParentPath + "/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    string saveFolderParentPath = "Assets/Contents/SOAssets/Items";

    [FoldoutGroup("Save in Project"), SerializeField, OnValueChanged(nameof(ValidateItemsIndexToSave)), LabelText("Index in List")]
    int itemsIndexToSave = 0;

    [FoldoutGroup("Save in Project"), ShowInInspector, FolderPath, LabelText("Path")]
    string saveFolderPath;
    [FoldoutGroup("Save in Project"), ShowInInspector, LabelText("File Name"), SuffixLabel(".asset")]
    string saveFileName = "(no item)";

    public void ValidateItemsIndexToSave()
    {
        if (items.Count == 0) return;

        if (itemsIndexToSave < 0 || itemsIndexToSave >= items.Count)
        {
            itemsIndexToSave = 0;
        }

        if (!string.IsNullOrEmpty(items[itemsIndexToSave].ItemName))
            saveFileName = items[itemsIndexToSave].ItemName;
        else
            saveFileName = "newItem";
    }

    #if UNITY_EDITOR

    [FoldoutGroup("Save in Project"), Button("Save", 25), GUIColor(0.3f,0.8f,0.3f)]
    public void SaveItemInProject()
    {
        if (items.Count <= 0 || itemsIndexToSave >= items.Count || items[itemsIndexToSave] == null) return;

        if (!AssetDatabase.IsValidFolder(saveFolderPath))
            AssetDatabase.CreateFolder(saveFolderParentPath, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        AssetDatabase.CreateAsset(items[itemsIndexToSave], saveFolderPath + "/" + saveFileName + ".asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = items[itemsIndexToSave];
        saveFolderPath = null;
    }

    #endif

#endregion
}
