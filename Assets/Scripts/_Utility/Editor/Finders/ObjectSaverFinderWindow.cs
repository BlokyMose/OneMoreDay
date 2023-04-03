using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Encore.Saves;

public class ObjectSaverFinderWindow : OdinEditorWindow
{
    #region [Editor]

    [MenuItem("Tools/Finder/ObjectSaver Finder")]
    public static void OpenWindow()
    {   
        GetWindow<ObjectSaverFinderWindow>("ObjectSaver Finder").Show();
    }

    #endregion

    private void OnEnable()
    {
        Refresh();
        CheckSameSaverKey();
    }

    #region [Vars: Savers]

    [Title("Savers")]
    public enum SaversFinderFilter { GO, Character }
    [VerticalGroup("Savers"), EnumToggleButtons, OnValueChanged(nameof(FindSavers)), HideLabel]
    public SaversFinderFilter saverFilter = SaversFinderFilter.GO;

    [VerticalGroup("Savers"), ListDrawerSettings(HideAddButton = true)]
    public List<ObjectSaver> savers = new List<ObjectSaver>();

    #endregion

    #region [Vars: Saveables]

    [Title("Saveables")]
    public enum SaveablesFinderFilter { All, RB2D }
    [VerticalGroup("Savers"), EnumToggleButtons, OnValueChanged(nameof(FindSaveables)),HideLabel]
    public SaveablesFinderFilter saveableFilter = SaveablesFinderFilter.All;

    [VerticalGroup("Saveables"), ListDrawerSettings(HideAddButton = true)]
    public List<Component> saveables = new List<Component>();

    #endregion

    #region [Methods: Settings]

    [TitleGroup("Settings")]

    [HorizontalGroup("Settings/But"), Button(ButtonSizes.Large), GUIColor("@Color.green")]
    public void Refresh()
    {
        FindSavers();
        FindSaveables();
    }

    [HorizontalGroup("Settings/But"), Button("Ping Script", ButtonSizes.Large)]
    public void PingScript()
    {
        var scriptGUID = AssetDatabase.FindAssets(nameof(ObjectSaverFinderWindow) + " t:Script");
        Selection.activeObject = EditorGUIUtility.Load(AssetDatabase.GUIDToAssetPath(scriptGUID[0]));
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [TitleGroup("Settings"), Button]
    public void CheckSameSaverKey()
    {
        saverFilter = SaversFinderFilter.GO;
        FindSavers();

        List<string> saveKeys = new List<string>();
        foreach (var saver in savers)
            saveKeys.Add(saver.SaveKey);

        foreach (var saveKey in saveKeys)
        {
            int count = 0;
            foreach (var saver in savers)
            {
                if (saver.SaveKey == saveKey)
                {
                    count++;
                    if (count >= 2) Debug.Log("Same key: [" + saveKey + "] in GO: [" + saver.gameObject.name + "]");
                }
            }
        }

        Debug.Log("Check same save key completed");
    }

    #endregion

    #region [Methods: Main]

    void FindSavers()
    {
        savers.Clear();
        switch (saverFilter)
        {
            case SaversFinderFilter.GO: savers = new List<ObjectSaver>(FindObjectsOfType<GOSaver>(true));
                break;

            case SaversFinderFilter.Character: savers = new List<ObjectSaver>(FindObjectsOfType<CharacterSaver>(true));
                break;
        }
    }

    void FindSaveables()
    {
        saveables.Clear();
        switch (saveableFilter)
        {
            case SaveablesFinderFilter.All:
                var allComponents = new List<Component>(FindObjectsOfType<Component>(true));
                foreach (var component in allComponents)
                    if (component is ISaveable)
                        saveables.Add(component);
                break;
            case SaveablesFinderFilter.RB2D: saveables = new List<Component>(FindObjectsOfType<RB2DSaveable>(true));
                break;
        }
    }

    #endregion

}
