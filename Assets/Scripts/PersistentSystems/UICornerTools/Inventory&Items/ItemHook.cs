using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Encore.Inventory
{
    [AddComponentMenu("Encore/Inventory/Item Hook")]
    public class ItemHook : MonoBehaviour
    {
        [SerializeField] protected Item item;
        public Item Item { get { return item; } set { item = value; } }

        #region [Inspector Buttons]

        string saveFolderParentPath = "Assets/Contents/SOAssets/Items";


        [Button(ButtonSizes.Medium), GUIColor(0.3f, 0.9f, 0.3f), ShowIf("@" + nameof(item) + "==null")]
        public void MakeItemFromThis()
        {
            Item newItem = ScriptableObject.CreateInstance<Item>();
            newItem.Setup(
                GetComponent<SpriteRenderer>() != null ? GetComponent<SpriteRenderer>().sprite : null,
                gameObject.name,
                "",
                null);
            item = newItem;
        }

#if UNITY_EDITOR

        [ContextMenu("Save Item in Project")]
        public void SaveInProject()
        {
            string saveFolderPath = saveFolderParentPath + "/" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (!AssetDatabase.IsValidFolder(saveFolderPath))
                AssetDatabase.CreateFolder(saveFolderParentPath, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

            AssetDatabase.CreateAsset(item, saveFolderPath + "/" + item.ItemName + ".asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = item;
        }

#endif

        #endregion

    }
}