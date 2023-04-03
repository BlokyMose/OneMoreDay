using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using Encore.Saves;
using Encore.Serializables;

public class SaveManagerEditor : OdinEditorWindow
{
    #region [Editor]

    [MenuItem("Tools/Save Manager Editor")]
    public static void OpenWindow()
    {
        GetWindow<SaveManagerEditor>("SaveManager").Show();
    }

    void OnFocus()
    {
        if (!UnityEngine.Application.isPlaying) isWatchingGameManager = false;
    }

    void OnEnable()
    {
        base.OnEnable();
        persistentPath = Path.Combine(Application.persistentDataPath + SerializationManager.folderName);
        GetSaveFileNames();
        LoadSettings();
        Refresh();

        if (saveFiles.Count > 0)
        {
            if (!string.IsNullOrEmpty(lastSaveFile))
                Load(lastSaveFile);
            else
                Load(saveFiles[0].Filename);
        }
    }

    void OnDisable()
    {
        SaveSettings();
    }

    void OnInspectorUpdate()
    {
        Refresh();
    }

    void LoadSettings()
    {
        // Find Settings file
        string thisFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        string thisFolderPath = thisFilePath.Substring(0, thisFilePath.Length - (GetType().Name + ".cs").Length);
        string thisFolderRelativePath = thisFolderPath.Substring(thisFolderPath.IndexOf("Assets"));

        var settingsFile = AssetDatabase.LoadAssetAtPath<TextAsset>(thisFolderRelativePath + "SaveManagerEditorSettings.json");
        if (settingsFile!=null)
        {
            // Load data based on key which is the variable's name
            Dictionary<string, object> settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsFile.text);
            if (settings == null) return;

            object isSavingResult, isNotSavingToFileResult, lastSaveFileResult;
            settings.TryGetValue(nameof(isSaving), out isSavingResult);
            settings.TryGetValue(nameof(isNotSavingToFile), out isNotSavingToFileResult);
            settings.TryGetValue(nameof(lastSaveFile), out lastSaveFileResult);
            isSaving = (bool)isSavingResult;
            isNotSavingToFile = (bool)isNotSavingToFileResult;
            lastSaveFile = (string)lastSaveFileResult;
            sceneName = (string)lastSaveFileResult;
        }
    }

    void SaveSettings()
    {
        // Find Settings file
        string thisFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        string thisFolderPath = thisFilePath.Substring(0, thisFilePath.Length - (GetType().Name + ".cs").Length);
        string thisFolderRelativePath = thisFolderPath.Substring(thisFolderPath.IndexOf("Assets"));
        string settingsFilePath = thisFolderRelativePath + "SaveManagerEditorSettings.json";

        // Serialize settings that are needed to be saved
        Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                {nameof(isSaving), isSaving },
                {nameof(isNotSavingToFile), isNotSavingToFile },
                {nameof(lastSaveFile), lastSaveFile}
            };
        string settingsJSON = JsonConvert.SerializeObject(settings, Formatting.Indented);

        if (File.Exists(settingsFilePath))
            File.WriteAllText(settingsFilePath, "");
        else
            File.CreateText(settingsFilePath);

        var file = File.OpenWrite(settingsFilePath);
        byte[] text = new UTF8Encoding(true).GetBytes(settingsJSON);
        file.Write(text, 0, text.Length);
        file.Close();

        AssetDatabase.Refresh();
    }

    #endregion

    #region [Vars: Data Handlers]

    [Serializable]
    public class SaveFileButton
    {
        string filename;
        public string Filename { get { return filename; } }
        [HideInInspector] public Action<string> OnClick;
        [HideInInspector] public Action RefreshList;

        public SaveFileButton(string name)
        {
            filename = name;
        }

        [Button("@"+nameof(filename)), GUIColor(nameof(GetColor))]
        void _Load()
        {
            if (string.IsNullOrEmpty(filename)) RefreshList();
            OnClick.Invoke(filename);
        }

        Color GetColor()
        {
            if (SaveManagerEditor.loadFileName == filename)
                return Color.green;
            else
                return Color.grey;
        }
    }

    [HideInInspector]
    public SaveData saveData;
    static string loadFileName;
    string sceneName;

    public static string GetLoadedFilename { get { return loadFileName; } }
    
    string persistentPath;

    bool isSaving = false;
    bool isNotSavingToFile = true;
    string lastSaveFile;

    #endregion

    #region [UI: Left]

    #region [UI]

    [HorizontalGroup("Main")]

    [Title("@" + nameof(SystemDataTitle) + "()")]

    [VerticalGroup("Main/Left"), DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout), GUIColor(1f, 0.996f, 0.680f)]
    public Dictionary<string, object> systemData;

    [Title("@"+nameof(GlobalDataTitle)+"()")]

    [VerticalGroup("Main/Left"), DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout), GUIColor(1f, 0.996f, 0.680f)]
    public Dictionary<string, object> globalData;

    [Title("@"+nameof(SceneNameTitle)+"()")]

    [VerticalGroup("Main/Left"), GUIColor(0.5f, 0.85f, 0.85f)]
    public Dictionary<string, object> objectsData;

    [VerticalGroup("Main/Left"), GUIColor(0.5f, 0.85f, 0.85f)]
    public List<GlobalStateData> globalStatesData;

    [VerticalGroup("Main/Left"), OnValueChanged(nameof(Search))]
    public string search;

    [VerticalGroup("Main/Left")]
    public Dictionary<string, object> searchResults;

    [VerticalGroup("Main/Left"), Button("Sort By Name", ButtonSizes.Large)]
    void SortByNameBut() { SortByName(); }

    #endregion

    #region [Methods]

    void Search()
    {
        if (!string.IsNullOrEmpty(search))
        {
            var sortedDict = from entry in objectsData where entry.Key.ToLower().Contains(search.ToLower()) select entry;

            searchResults = new Dictionary<string, object>();

            foreach (var l in sortedDict)
            {
                searchResults.Add(l.Key, l.Value);
            }
        }

        else
        {
            searchResults = null;
        }
    }

    void SortByName()
    {
        if (objectsData == null) return;

        var sortedDict = from entry in objectsData orderby entry.Key descending select entry;

        searchResults = new Dictionary<string, object>();

        foreach (var l in sortedDict)
        {
            searchResults.Add(l.Key, l.Value);
        }
    }

    string SystemDataTitle()
    {
        return isWatchingGameManager ? "[GM] System Data" : "System Data";
    }

    string GlobalDataTitle()
    {
        return isWatchingGameManager ? "[GM] Global Data" : "Global Data";
    }

    string SceneNameTitle()
    {
        return isWatchingGameManager ? "[GM] "+sceneName : sceneName;
    }

    #endregion

    #endregion

    #region [UI: Right]

    #region [Title: Save File]

    #region [UI]

    [Title("Save File")]
    [VerticalGroup("Main/Right"), HorizontalGroup("Main/Right/H",170,15), VerticalGroup("Main/Right/H/V")]
    [VerticalGroup("Main/Right/H/V/File"), OnValueChanged(nameof(CheckSaveName)), LabelWidth(40), LabelText("File"), SuffixLabel(".dat")]
    public string saveFileName;
    string saveButtonName;

    [VerticalGroup("Main/Right/H/V/File"), Button("@" + nameof(saveButtonName)), ShowIf(nameof(CheckIfDataLoaded))]
    [PropertyTooltip("Write saveData displayed on the left to this file")]
    void SaveOrCreateBut() { Save(); }

    [HorizontalGroup("Main/Right/H/V/File/H"), Button("Empty")]
    void EmptyBut() { Empty(); }

    [HorizontalGroup("Main/Right/H/V/File/H"), Button("@" + nameof(canDeleteButName)), GUIColor("@" + nameof(canDeleteButColor))]
    [EnableIf(nameof(CheckSaveName))]
    void DeleteSaveFileBut() { DeleteSaveFile(); }

    [Space(15)]

    [VerticalGroup("Main/Right/H/V"), ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true, IsReadOnly = true)]
    public List<SaveFileButton> saveFiles; 

    #endregion

    #region [Methods]

    string canDeleteButName = "Delete";
    Color canDeleteButColor = new Color32(255, 130, 130, 255);

    void Empty()
    {
        saveData = new SaveData();
        systemData = null;
        globalData = null;
        objectsData = null;
        globalStatesData = null;
        searchResults = null;

        ResetDeleteButton();
    }

    void DeleteSaveFile()
    {
        if (canDeleteButName.Equals("Delete"))
        {
            canDeleteButName = "C.Delete";
            canDeleteButColor = new Color32(255, 50, 50, 255);
        }
        else if (canDeleteButName.Equals("C.Delete"))
        {
            ResetDeleteButton();

            try
            {
                File.Delete(Path.Combine(persistentPath, saveFileName + ".dat"));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("SaveManagerEditor: " + e.Message);
            }

            Refresh();
        }
    }

    void ResetDeleteButton()
    {
        canDeleteButName = "Delete";
        canDeleteButColor = new Color32(255, 100, 100, 255);
    }

    bool CheckSaveName()
    {
        saveButtonName = "Create";

        if (saveFiles != null && saveFiles.Count > 0)
        {
            if (saveFiles.Exists(s => s.Filename == saveFileName))
            {
                saveButtonName = "Write to File";
                return true;
            }
        }

        return false;
    }

    #endregion

    #endregion

    #region [Title: Runtime]

    #region [UI]

    [TitleGroup("Main/Right/H/V/Runtime")]
    [PropertyTooltip("Write GM's saveData to a file when the game runs; or just keeps it temporarily in a cache file")]
    [Button("@" + nameof(_toggleSaveModeButName), ButtonSizes.Large), DisableIf("@!" + nameof(CheckIfDataLoaded) + "()"), GUIColor("@" + nameof(_toggleSaveModeButColor))]
    void ToggleSaveModeBut() { ToggleSaveMode(); }

    [TitleGroup("Main/Right/H/V/Runtime")]
    [PropertyTooltip("Force all cached data to be written to a save file")]
    [Button("Save Game"), HideInEditorMode]
    void SaveGame() { GameManager.Instance.SaveGame(); }

    [TitleGroup("Main/Right/H/V/Runtime")]
    [OnValueChanged(nameof(LoadFromGameManager)), HideInEditorMode, LabelText("Watch GM's saveData"), LabelWidth(135), GUIColor("@"+nameof(isWatchingGameManager)+"?Color.green:Color.gray")]
    public bool isWatchingGameManager = false;

    #endregion

    #region [Methods]

    string _toggleSaveModeButName = "is Caching to save.dat";
    Color _toggleSaveModeButColor = new Color(0.5f, 0.5f, 0.5f, 1);

    void RefreshToggleSaveModeButton()
    {
        if (isNotSavingToFile)
        {
            _toggleSaveModeButName = "is Caching to save.dat";
            _toggleSaveModeButColor = new Color(0.5f, 0.5f, 0.5f, 1);
            GameManager.saveFileName = GameManager.DEFAULT_SAVE_FILE_NAME;
        }

        else
        {
            _toggleSaveModeButName = "Saving to " + saveFileName + ".dat";
            _toggleSaveModeButColor = Color.yellow;
            GameManager.saveFileName = saveFileName;
        }
    }

    void LoadFromGameManager()
    {
        LoadSaveFromGameManager(SceneManager.GetActiveScene());
        SceneManager.activeSceneChanged -= OnSceneChange;
        SceneManager.activeSceneChanged += OnSceneChange;

        void OnSceneChange(Scene current, Scene next)
        {
            LoadSaveFromGameManager(next);
        }

        void LoadSaveFromGameManager(Scene scene)
        {
            if (isWatchingGameManager)
            {
                saveData = GameManager.Instance.currentSaveData;
                systemData = (Dictionary<string, object>)saveData.LoadKey(GameManager.SYSTEM_DATA_KEY);
                globalData = (Dictionary<string, object>)saveData.LoadKey(GameManager.GLOBAL_KEY);
                sceneName = scene.name;
                var sceneData = (Dictionary<string, object>)saveData.LoadKey(GameManager.SCENES_DATA_KEY);
                var scenes = (Dictionary<string, GameManager.GameAssets.ScenesData.SceneData>) sceneData[GameManager.SCENES_KEY];

                if (scenes.ContainsKey(sceneName))
                {
                    objectsData = scenes[sceneName].objectsData;
                }
                
            }
            else
            {
                Load(loadFileName);
            }
        }
    }

    void ToggleSaveMode()
    {
        isNotSavingToFile = !isNotSavingToFile;
        RefreshToggleSaveModeButton();
        SaveSettings();
    }

    #endregion

    #endregion

    #region [Title: Settings]

    #region [UI]

    [TitleGroup("Main/Right/H/V/Settings"), Button("Refresh", ButtonSizes.Large)]
    void RefreshBut() { Refresh(); }

    [HorizontalGroup("Main/Right/H/V/Settings/H"), VerticalGroup("Main/Right/H/V/Settings/H/0"), Button("See Folder")]
    void PingSaveFolderBut() { PingSaveFolder(); }

    [VerticalGroup("Main/Right/H/V/Settings/H/1"), Button("See Script")]
    void PingScriptBut() { PingScript(); } 

    #endregion

    #region [Methods]

    void PingSaveFolder()
    {
        if (Directory.Exists(persistentPath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = persistentPath,
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(startInfo);
        }
    }

    void PingScript()
    {
        var scriptGUID = AssetDatabase.FindAssets(nameof(SaveManagerEditor) + " t:Script");
        Selection.activeObject = EditorGUIUtility.Load(AssetDatabase.GUIDToAssetPath(scriptGUID[0]));
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    #endregion

    #endregion

    #endregion

    #region [Methods: Utils]

    string[] GetSaveFileNames()
    {
        return Directory.GetFiles(persistentPath, "*.dat", SearchOption.TopDirectoryOnly);
    }

    void Save()
    {
        if (string.IsNullOrEmpty(saveFileName))
        {
            SerializationManager.Save(Path.GetFileNameWithoutExtension(SaveManagerEditor.loadFileName), saveData);
        }
        else
        {
            SerializationManager.Save(saveFileName, saveData);
        }

        Refresh();
    }

    void Load(string filename)
    {
        lastSaveFile = filename;
        loadFileName = filename;
        saveFileName = filename;

        if (!isWatchingGameManager)
        {
            saveData = (SaveData)SerializationManager.Load(Path.GetFileNameWithoutExtension(loadFileName));
            systemData = (Dictionary<string, object>)saveData.LoadKey(GameManager.SYSTEM_DATA_KEY);
            globalData = (Dictionary<string, object>)saveData.LoadKey(GameManager.GLOBAL_KEY);

            var sceneData = (Dictionary<string, object>)saveData.LoadKey(GameManager.SCENES_DATA_KEY);
            var scenes = (Dictionary<string, GameManager.GameAssets.ScenesData.SceneData>)sceneData[GameManager.SCENES_KEY];
            sceneName = EditorSceneManager.GetActiveScene().name;
            if (scenes.ContainsKey(sceneName))
            {
                objectsData = scenes[sceneName].objectsData;
            }
        }

        SerializationManager.SetCache(saveData, filename);

        CheckSaveName();
        ResetDeleteButton();
        Refresh();
    }

    void Refresh()
    {
        saveFiles = new List<SaveFileButton>();
        ResetDeleteButton();

        foreach (string s in GetSaveFileNames())
        {
            if (Path.GetFileNameWithoutExtension(s) == GameManager.DEFAULT_SAVE_FILE_NAME) continue;
            SaveFileButton sfb = new SaveFileButton(Path.GetFileNameWithoutExtension(s));
            sfb.OnClick += Load;
            sfb.RefreshList += Refresh;
            saveFiles.Add(sfb);
        }

        RefreshToggleSaveModeButton();
    }

    bool CheckIfDataLoaded()
    {
        if (!string.IsNullOrEmpty(SaveManagerEditor.loadFileName)) return true;

        return false;
    }

    #endregion
}
