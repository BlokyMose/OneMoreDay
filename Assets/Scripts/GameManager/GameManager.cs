using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using Encore.Utility;
using Encore.Dialogues;
using Encore.Localisations;
using Encore.Doomclock;
using Encore.Inventory;
using Encore.Phone;
using Encore.Phone.Chat;
using Encore.Phone.ToDo;
using Encore.CharacterControllers;
using Encore.Saves;
using Encore.Serializables;
using Encore.SceneMasters;
using Encore.MiniGames.UrgentChoice;
using Encore.Locations;
using Encore;
using static Encore.SceneMasters.SceneTransitionAnimationEvent;
using Encore.Menus;

/*[Notes]   

 [SaveSystem]
 - GameManager initiates the game by loading game assets required by scripts which need them to regain their saved data

 - Saving process: 
     1. Create empty gameAssets
     2. Pass empty gameAssets to SceneMaster, then to many scripts
     3. Scripts insert their data to gameAssets
     4. GameManager writes gameAssets to saveData
     
 - Loading process: 
     1. GameManager loads data from saveData
     2. Create gameAssets which hold loaded data
     3. Pass gameAssets to SceneMaster, then to many scripts
     4. Scripts read gameAssets to regain their data

 - Create new saveable data:
    1. Add a new Serializables script to contain the data
    2. Add a new field with the type of that script to GameAssets according to its purpose: System, Global, Scene, or Resources
    3. Loading:
        - In LoadGame(), load the data using LoadObjectFrom...(key)
        - In LoadGame(), insert the loaded data to gameAssets
        - In the script which needs the data, make Load(gameAssets), so it can get the data from gameAssets
        - In LoadGame(), call that script's load function, and pass the gameAssets
    4. Saving:
        - In the script which saves the data, make Save(gameAssets), so it can insert the data to gameAssets
        - In SaveGame(), make empty gameAssets
        - In SaveGame(), call the script's save function, and pass the empty gameAssets
        - In SaveGame(), save by calling SaveObjectTo...(key,data)
*/

public class GameManager : MonoBehaviour
{
    #region [Classes]

    [System.Serializable]
    public class GameAssets
    {
        #region [Classes]

        [System.Serializable]
        public class SystemData
        {
            public string currentScene;
            public InventoryData inventoryData;
            public ToDoAppData toDoAppData;
            public ChatAppData chatAppData;
            public PhoneManagerData phoneManagerData;

            public SystemData(
                string currentScene,
                InventoryData inventoryData,
                ToDoAppData toDoAppData,
                ChatAppData chatAppData,
                PhoneManagerData phoneManagerData
                )
            {
                this.currentScene = currentScene;
                this.inventoryData = inventoryData;
                this.toDoAppData = toDoAppData;
                this.chatAppData = chatAppData;
                this.phoneManagerData = phoneManagerData;
            }

            public SystemData()
            {
                inventoryData = null;
                currentScene = string.Empty;
            }
        }

        [System.Serializable]
        public class GlobalData
        {
            public Dictionary<string, GlobalVariableData> globalVariables;
            public Dictionary<string, CharacterData> charactersData;
            public Dictionary<string, LocationData> locationsData;
            public Dictionary<string, GlobalStateData> globalStatesData = new Dictionary<string, GlobalStateData>();


            public GlobalData(
                Dictionary<string, GlobalVariableData> globalVariables, 
                Dictionary<string,CharacterData> charactersData, 
                Dictionary<string, LocationData> locationsData,
                Dictionary<string, GlobalStateData> globalStatesData
                )
            {
                this.globalVariables = globalVariables;
                this.charactersData = charactersData;
                this.locationsData = locationsData;
                this.globalStatesData = globalStatesData;
            }

            public GlobalData()
            {
                this.globalVariables = new Dictionary<string, GlobalVariableData>();
                this.charactersData = new Dictionary<string,CharacterData>();
                this.locationsData = new Dictionary<string, LocationData>();
                this.globalStatesData = new Dictionary<string, GlobalStateData>();
            }

            public CharacterData GetCharacterData(Actor actor)
            {
                var foundCharacter = charactersData.Get(actor.ActorName, null);
                if (foundCharacter != null)
                {
                    return foundCharacter;
                }
                else
                {
                    var newCharacter = new CharacterData(actor);
                    charactersData.Add(newCharacter.ActorName, newCharacter);
                    return newCharacter;
                }
            }

            /// <summary>Find a state and modify it; If there's no such state, create a new one, and modify it</summary>
            public void ModifyState(GlobalStateSetter.GlobalStateModified stateModified)
            {
                var targetedState = globalStatesData.Get(stateModified.gs.stateName, null);
                if (targetedState != null)
                {
                    targetedState.ModifyData(stateModified);
                }
                else
                {
                    targetedState = new GlobalStateData(stateModified);
                    globalStatesData.Add(targetedState.stateName, targetedState);
                    Debug.Log("Adding StateIDData <b>[" + stateModified.gs.stateName + "]</b> ");
                }
            }

            /// <summary>The last state in the list will be applied in the end, and may override previously applied state</summary>
            public void SetAsLastState(GlobalState id)
            {
                // Since it's a dictionary, this function is useless
                var targetedState = globalStatesData.Get(id.stateName, null);
                if (targetedState != null)
                {
                    globalStatesData.Remove(targetedState.stateName);
                    globalStatesData.Add(targetedState.stateName, targetedState);
                }
            }
        }

        [System.Serializable]
        public class ScenesData
        {
            #region [Classes]

            [System.Serializable]
            public class SceneData
            {
                public string sceneName;
                public Dictionary<string, object> objectsData = new Dictionary<string, object>();// Key: sceneName; Value: sceneData of that state

                public SceneData(string sceneName)
                {
                    this.sceneName = sceneName;
                    this.objectsData = new Dictionary<string, object>();
                }

                public void SaveObjectToObjectData(string k, object o)
                {
                    if (objectsData.ContainsKey(k))
                        objectsData[k] = o;
                    else
                        objectsData.Add(k, o);
                }

                public object LoadObjectFromObjectData(string k)
                {
                    object o;
                    objectsData.TryGetValue(k, out o);
                    return o;
                }
            }

            #endregion

            public Dictionary<string, SceneData> scenes = new Dictionary<string, SceneData>();

            public ScenesData(Dictionary<string, SceneData> scenes)
            {
                this.scenes = scenes;
            }

            public ScenesData()
            {
                this.scenes = new Dictionary<string, SceneData>();
            }
        }

        [System.Serializable]
        public class Resources
        {
            public List<Item> Items { get; private set; }
            public List<GameObject> ActorPrefabs {get;private set;}
            public List<ChatContact> ChatContacts {get;private set;}
            public List<Location> Locations {get;private set;}
            public List<LocationTag> LocationTags {get;private set;}
            public List<GlobalVariableFunc> VarFuncs {get;private set;}
            public List<ToDoTag> ToDoTags { get; private set; }
            public List<GlobalState> GlobalStates { get; private set; }

            public Resources(
                List<Item> items, 
                List<GameObject> actorPrefabs, 
                List<ChatContact> chatContacts, 
                List<Location> locations, 
                List<LocationTag> locationTags, 
                List<GlobalVariableFunc> varFuncs, 
                List<ToDoTag> toDoTags,
                List<GlobalState> globalStates)
            {
                this.Items = items;
                this.ActorPrefabs = actorPrefabs;
                this.ChatContacts = chatContacts;
                this.Locations = locations;
                this.LocationTags = locationTags;
                this.VarFuncs = varFuncs;
                this.ToDoTags = toDoTags;
                this.GlobalStates = globalStates;
            }

            public Resources()
            {
                this.Items = new List<Item>();
                this.ActorPrefabs = new List<GameObject>();
                this.ChatContacts = new List<ChatContact>();
                this.Locations  = new List<Location>();
            }
        }

        #endregion

        public SystemData systemData;
        public GlobalData globalData;
        public ScenesData.SceneData currentSceneData;
        public Resources resources;

        public GameAssets(SystemData systemData, GlobalData globalData, ScenesData.SceneData currentSceneData, Resources resources)
        {
            this.systemData = systemData;
            this.globalData = globalData;
            this.currentSceneData = currentSceneData;
            this.resources = resources;
        }

        public GameAssets(string currentSceneName, Resources resources)
        {
            systemData = new SystemData();
            globalData = new GlobalData();
            currentSceneData = new ScenesData.SceneData(currentSceneName);
            this.resources = resources;
        }
    }

    #endregion

    #region [Vars]

    #region [Inspector]

    [Title("Player")]
    [Required, SerializeField] GameObject playerPrefab;
    public GameObject PlayerPrefab { get { return playerPrefab; } }
    [SerializeField] LocalisationSystem.Language language = LocalisationSystem.Language.ENG;
    public LocalisationSystem.Language Language { get { return language; } }
    public string LanguageCode { get { return language.ToString(); } }

    [Title("Persistent Systems")]
    [Required, SerializeField] GameObject dialoguePrefab;
    [Required, SerializeField] GameObject timePrefab;
    [Required, SerializeField] GameObject inventoryManagerPrefab;
    [Required, SerializeField] GameObject doomclockPrefab;
    [Required, SerializeField] GameObject phonePrefab;
    [Required, SerializeField] PauseMenu pauseMenuPrefab;
    [Required, SerializeField] UrgentChoiceManager urgentChoiceManagerPrefab;

    [Title("Scene Load")]
    [Required, SerializeField]
    SceneTransitionAnimationEvent sceneTransitionPrefab;

    public enum LoadSceneMode { CurrentScene, LastScene, FirstScene, MainMenu }
    [SerializeField, Tooltip("Play game from last saved scene")]
    LoadSceneMode loadSceneMode = LoadSceneMode.CurrentScene;

    [SerializeField]
    Location firstGameScene;

    [SerializeField]
    Location mainMenuScene;

    [TitleGroup("Saving")]
    public SaveData currentSaveData = null;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference itemsInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference charactersInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference chatContactsInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference locationsInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference locationTagsInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference dialogueAssetsCSVInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference toDoTagsInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference globalStatesInProjectRef;

    [FoldoutGroup("Resources References"), SerializeField]
    AssetLabelReference globalVariableFuncsInProjectRef;

    #endregion

    #region [Singleton]

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;

            return instance;
        }
    }
    public static bool HasInstance
    {
        get { return instance != null; }
    }

    #endregion

    #region [Keys]

    public const string DEFAULT_SAVE_FILE_NAME = "save";
    public static string saveFileName = "save";

    public static readonly string GLOBAL_KEY = "Global";
    public static readonly string SYSTEM_DATA_KEY = "SystemData";
    public static readonly string SCENES_DATA_KEY = "ScenesData";
    public static readonly string SCENES_KEY = "Scenes";
    public static readonly string SCENE_CURRENT_STATES_KEY = "SceneCurrentStates";
    public static readonly string SCENE_OBJECTS_DATA_KEY = "SceneObjectsData";
    public static readonly string GLOBAL_VARIABLES_KEY = "GlobalVariables";
    public static readonly string CHARACTERS_DATA_KEY = "CharactersData";
    public static readonly string LOCATIONS_DATA_KEY = "LocationsData";
    public static readonly string GLOBAL_STATE_DATA = "GlobalStateData";
    public static readonly string PLAYER_CURRENT_SCENE = "PlayerCurrentScene";

    #endregion

    #region [Current Data]

    public string PreviousSceneName { get; private set; }
    string currentSceneName;
    public string CurrentSceneName
    {
        get
        {
            return string.IsNullOrEmpty(currentSceneName) ? SceneManager.GetActiveScene().name : currentSceneName;
        }
        private set
        {
            currentSceneName = value;
        }
    }
    private bool currentlyLoadingScene = false;

    GameAssets gameAssets;

    public GameAssets GetGameAssets()
    {
        return gameAssets;
    }

    #endregion

    #region [Persistent Systems]

    public DialogueManagerNC DialogueManager { get; private set; }
    public TimeManager TimeManager { get; private set; }
    public InventoryManager InventoryManager { get; private set; }
    public DoomclockManager DoomclockManager { get; private set; }
    public PhoneManager PhoneManager { get; private set; }
    public PauseMenu PauseMenu { get; private set; }

    public List<IPersistentSystem> PersistentSystems { get; private set; }
    public List<UICornerTool> UICornerTools { get; private set; }

    #endregion

    #region [Player]

    PlayerBrain playerBrain;
    public PlayerBrain Player
    {
        get
        {
            return playerBrain;
        }
    }

    public void SetPlayer(PlayerBrain brain)
    {
        playerBrain = brain;
        playerBrain.Setup(DialogueManager);
    }

    #endregion

    #endregion

    #region [Methods: Unity]

    void Awake()
    {
        #region [Singleton]

        if (instance == null || instance == this)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        #endregion

        StartGameByLoadSceneMode();

    }

    #endregion

    #region [Methods: General]

    public void StartGameByLoadSceneMode()
    {
        // TODO: receive the name of the save data, then load it not from the cache
        currentSaveData = LoadSaveData();
        if (currentSaveData == null)
        {
            currentSaveData = CreateSaveData();
            WriteSaveDataToFile();
        }

        LoadGame(loadSceneMode);
    }

    public void NewGame()
    {
        // TODO: Create actual new save data file, not the cache
        currentSaveData = CreateSaveData();
        WriteSaveDataToFile();

        LoadGame(LoadSceneMode.FirstScene);
    }

    /// <param name="callback">Returns character's prefab</param>
    public void GetCharacterPrefab(string actorName, Action<GameObject> callback)
    {
        List<GameObject> characterPrefabs = new List<GameObject>();
        var opCharacters = Addressables.LoadAssetsAsync<GameObject>(
                charactersInProjectRef,
                (loadedCharacterPrefabs) =>
                {
                    characterPrefabs.Add(loadedCharacterPrefabs);
                });

        opCharacters.Completed += (obj) => 
        {
            bool isSuccess = false;
            foreach (var character in characterPrefabs)
            {
                CharacterSaver actorContainer = character.GetComponent<CharacterSaver>();
                if (actorContainer != null && actorContainer.SaveKey == actorName)
                {
                    callback(character);
                    isSuccess = true;
                }
            }
            if (!isSuccess) Debug.Log("Failed to load prefab : " + actorName);
            Addressables.Release(opCharacters);
        };
    }


    /// <summary> Enable UI Corner Tools to be shown when being hovered </summary>
    public void EnableUICornerTools(bool enable, bool showTemporary)
    {
        foreach (var tool in UICornerTools)
        {
            tool.SetCanShow(enable, showTemporary);
        }
    }

    #endregion

    #region [Methods: Save System]

    Action ReleaseAddressables;

    public void LoadGame(LoadSceneMode loadSceneMode)
    {
        var transition = Instantiate(sceneTransitionPrefab, transform, false);
        transition.StartCovering(() => { StartCoroutine(Load()); });

        IEnumerator Load()
        {
            // Load scene based on load mode
            switch (loadSceneMode)
            {
                case LoadSceneMode.LastScene:
                    var scene = (string)LoadObjectFromSystemData(PLAYER_CURRENT_SCENE);
                    if (scene != null)
                    {
                        yield return SceneManager.LoadSceneAsync(scene);
                        CurrentSceneName = scene;
                    }
                    break;
                case LoadSceneMode.FirstScene:
                    yield return SceneManager.LoadSceneAsync(firstGameScene.SceneName);
                    CurrentSceneName = firstGameScene.SceneName;
                    break;
                case LoadSceneMode.MainMenu:

                    Debug.Log("Scene exists: " + (mainMenuScene.Scene != null).ToString());
                    yield return SceneManager.LoadSceneAsync(mainMenuScene.SceneName);
                    CurrentSceneName = mainMenuScene.SceneName;
                    break;
                case LoadSceneMode.CurrentScene:
                    CurrentSceneName = SceneManager.GetActiveScene().name;
                    break;
            }

            // Load all required resources and save data, then apply it
            yield return StartCoroutine(LoadInitialGameAssets());
            InstantiatePersistenSystems();
            ApplyGameAssetsToSystems();

            var currentSceneMaster = SceneMaster.current != null ? SceneMaster.current : FindObjectOfType<SceneMaster>();
            currentSceneMaster.Init(new SceneMaster.InitialSettings(
                repositionPlayer: false,
                previousSceneName: "",
                onInitialized: () => { OnAfterSceneLoad(transition); }
                ));
        }

        IEnumerator LoadInitialGameAssets()
        {
            gameAssets = new GameAssets(LoadSystemData(), LoadGlobalData(), LoadSceneData(), null);
            yield return StartCoroutine(LoadAddressables(
                callback: (resources) =>
                {
                    gameAssets.resources = resources; SyncResourcesToGlobalData(gameAssets);
                },
                onRelease: ReleaseAddressables));


            GameAssets.SystemData LoadSystemData()
            {
                return new GameAssets.SystemData(
                    CurrentSceneName,
                    inventoryData: (InventoryData)LoadObjectFromSystemData(InventoryManager.INVENTORY_SAVE_KEY),
                    toDoAppData: (ToDoAppData)LoadObjectFromSystemData(ToDoApp.TODO_SAVE_KEY),
                    chatAppData: (ChatAppData)LoadObjectFromSystemData(ChatApp.CHAT_SAVE_KEY),
                    phoneManagerData: (PhoneManagerData)LoadObjectFromSystemData(PhoneManager.PHONE_MANAGER_SAVE_KEY)
                    );
            }

            GameAssets.GlobalData LoadGlobalData()
            {
                var globalVariables = LoadObjectFromGlobalData(GLOBAL_VARIABLES_KEY) as Dictionary<string, GlobalVariableData>;
                if (globalVariables == null) globalVariables = new Dictionary<string, GlobalVariableData>();

                var charactersData = LoadObjectFromGlobalData(CHARACTERS_DATA_KEY) as Dictionary<string, CharacterData>;
                if (charactersData == null) charactersData = new Dictionary<string, CharacterData>();

                var locationsData = LoadObjectFromGlobalData(LOCATIONS_DATA_KEY) as Dictionary<string, LocationData>;
                if (locationsData == null) locationsData = new Dictionary<string, LocationData>();

                var globalStateData = LoadObjectFromGlobalData(GLOBAL_STATE_DATA) as Dictionary<string, GlobalStateData>;
                if (globalStateData == null) globalStateData = new Dictionary<string, GlobalStateData>();

                return new GameAssets.GlobalData(globalVariables, charactersData, locationsData, globalStateData);
            }

            GameAssets.ScenesData.SceneData LoadSceneData()
            {
                return GetSceneData(CurrentSceneName);
            }

            IEnumerator LoadAddressables(Action<GameAssets.Resources> callback, Action onRelease)
            {
                List<Item> items = new List<Item>();
                var opItems = Addressables.LoadAssetsAsync<Item>(
                        itemsInProjectRef,
                        (loadedItem) =>
                        {
                            items.Add(loadedItem);
                        });
                if (!opItems.IsDone) yield return opItems;


                List<GameObject> characterPrefabs = new List<GameObject>();
                var opCharacters = Addressables.LoadAssetsAsync<GameObject>(
                        charactersInProjectRef,
                        (loadedCharacterPrefabs) =>
                        {
                            characterPrefabs.Add(loadedCharacterPrefabs);
                        });
                if (!opCharacters.IsDone) yield return opCharacters;

                List<ChatContact> chatContacts = new List<ChatContact>();
                var opChatContacts = Addressables.LoadAssetsAsync<ChatContact>(
                        chatContactsInProjectRef,
                        (loadedChatContact) =>
                        {
                            chatContacts.Add(loadedChatContact);
                        });
                if (!opChatContacts.IsDone) yield return opChatContacts;

                List<Location> locations = new List<Location>();
                var opLocations = Addressables.LoadAssetsAsync<Location>(
                        locationsInProjectRef,
                        (loadedLocation) =>
                        {
                            locations.Add(loadedLocation);
                        });
                if (!opLocations.IsDone) yield return opLocations;


                List<LocationTag> locationTags = new List<LocationTag>();
                var opLocationTags = Addressables.LoadAssetsAsync<LocationTag>(
                        locationTagsInProjectRef,
                        (loadedTag) =>
                        {
                            locationTags.Add(loadedTag);
                        });
                if (!opLocationTags.IsDone) yield return opLocationTags;


                List<GlobalVariableFunc> varFuncs = new List<GlobalVariableFunc>();
                var opGlobalVariableFuncs = Addressables.LoadAssetsAsync<GlobalVariableFunc>(
                            globalVariableFuncsInProjectRef,
                            (loadedGlobalVariableFunc) =>
                            {
                                varFuncs.Add(loadedGlobalVariableFunc);
                            });
                if (!opGlobalVariableFuncs.IsDone) yield return opGlobalVariableFuncs;



                List<ToDoTag> toDoTags = new List<ToDoTag>();
                var opToDoTags = Addressables.LoadAssetsAsync<ToDoTag>(
                        toDoTagsInProjectRef,
                        (loadedToDoTag) =>
                        {
                            toDoTags.Add(loadedToDoTag);
                        });
                if (!opToDoTags.IsDone) yield return opToDoTags;


                List<GlobalState> globalStates = new List<GlobalState>();
                var opGlobalStates = Addressables.LoadAssetsAsync<GlobalState>(
                        globalStatesInProjectRef,
                        (loadedGlobalState) =>
                        {
                            globalStates.Add(loadedGlobalState);
                        });
                if (!opGlobalStates.IsDone) yield return opGlobalStates;

                onRelease += () =>
                {
                    if (opItems.IsValid())
                        Addressables.Release(opItems);
                    if (opCharacters.IsValid())
                        Addressables.Release(opCharacters);
                    if (opChatContacts.IsValid())
                        Addressables.Release(opChatContacts);
                    if (opLocations.IsValid())
                        Addressables.Release(opLocations);
                    if (opLocationTags.IsValid())
                        Addressables.Release(opLocationTags);
                    if (opGlobalVariableFuncs.IsValid())
                        Addressables.Release(opGlobalVariableFuncs);                    
                };

                callback(new GameAssets.Resources(
                    items, 
                    characterPrefabs, 
                    chatContacts, 
                    locations, 
                    locationTags, 
                    varFuncs, 
                    toDoTags,
                    globalStates));
            }

            void SyncResourcesToGlobalData(GameAssets gameAssets)
            {
                foreach (var location in gameAssets.resources.Locations) // Sync locationData and Locations
                {
                    gameAssets.globalData.locationsData.AddIfHasnt(location.SceneName, new LocationData(location, new List<LocationTag>()));
                }

                foreach (var gs in gameAssets.resources.GlobalStates)
                {
                    var stage = gs.stages.GetAt(0, new GlobalState.Stage(""));
                    gameAssets.globalData.globalStatesData.AddIfHasnt(gs.stateName, new GlobalStateData(gs, stage.stageName));
                }
            }
        }

        void InstantiatePersistenSystems()
        {
            if (DialogueManager == null)
                DialogueManager = Instantiate(dialoguePrefab).GetComponent<DialogueManagerNC>();
            if (TimeManager == null)
                TimeManager = Instantiate(timePrefab).GetComponent<TimeManager>();
            if (InventoryManager == null)
                InventoryManager = Instantiate(inventoryManagerPrefab).GetComponent<InventoryManager>();
            if (DoomclockManager == null)
                DoomclockManager = Instantiate(doomclockPrefab).GetComponent<DoomclockManager>();
            if (PhoneManager == null)
                PhoneManager = Instantiate(phonePrefab).GetComponent<PhoneManager>();
            if (PauseMenu == null)
                PauseMenu = Instantiate(pauseMenuPrefab);

            PersistentSystems = new List<IPersistentSystem>()
            {
                DialogueManager, TimeManager, InventoryManager, DoomclockManager, PhoneManager
            };

            UICornerTools = new List<UICornerTool>()
            {
                InventoryManager, DoomclockManager, PhoneManager, PauseMenu
            };
        }
    }

    void ApplyGameAssetsToSystems()
    {
        // SystemData
        InventoryManager.Load(gameAssets);
        PhoneManager.Load(gameAssets);
        PhoneManager.MapApp.Load();
        PhoneManager.ToDoApp.Load(gameAssets);
        PhoneManager.ChatApp.Load(gameAssets);

        // SceneData
        SceneMaster.current.Load(gameAssets);

        #endregion

        Debug.Log("Loading game finished <b>[" + SceneManager.GetActiveScene().name + "]</b>");
    }

    /// <summary>Force every possible script to save, before writing to file</summary>
    public void SaveGame()
    {
        #region [Save scripts' data to GameAssets]

        // SystemData
        InventoryManager.Save(gameAssets);
        PhoneManager.Save(gameAssets);
        PhoneManager.ToDoApp.Save(gameAssets);
        PhoneManager.ChatApp.Save(gameAssets);

        // SceneData
        SceneMaster.current.Save(gameAssets);
        SaveSceneData(gameAssets.currentSceneData);

        #endregion

        // Save GameAssets.SystemData to SaveData
        SaveObjectToSystemData(InventoryManager.INVENTORY_SAVE_KEY, gameAssets.systemData.inventoryData);
        SaveObjectToSystemData(PhoneManager.PHONE_MANAGER_SAVE_KEY, gameAssets.systemData.phoneManagerData);
        SaveObjectToSystemData(ToDoApp.TODO_SAVE_KEY, gameAssets.systemData.toDoAppData);
        SaveObjectToSystemData(ChatApp.CHAT_SAVE_KEY, gameAssets.systemData.chatAppData);
        SaveObjectToSystemData(PLAYER_CURRENT_SCENE, CurrentSceneName);

        // Save GameAssets.GlobalData to SaveData
        SaveObjectToGlobalData(CHARACTERS_DATA_KEY, gameAssets.globalData.charactersData);
        SaveObjectToGlobalData(GLOBAL_VARIABLES_KEY, gameAssets.globalData.globalVariables);
        SaveObjectToGlobalData(LOCATIONS_DATA_KEY, gameAssets.globalData.locationsData);
        SaveObjectToGlobalData(GLOBAL_STATE_DATA, gameAssets.globalData.globalStatesData);

        WriteSaveDataToFile();
    }

    #region [GlobalData]

    void SaveObjectToGlobalData(string k, object o)
    {
        Dictionary<string, object> glb_save = GetDataFromSaveData(GLOBAL_KEY);

        if (glb_save.ContainsKey(k))
            glb_save[k] = o;
        else
            glb_save.Add(k, o);
    }

    object LoadObjectFromGlobalData(string k)
    {
        Dictionary<string, object> glb_save = GetDataFromSaveData(GLOBAL_KEY);

        glb_save.TryGetValue(k, out object o);
        return o;
    }

    #endregion

    #region [SystemData]

    void SaveObjectToSystemData(string k, object o)
    {
        Dictionary<string, object> systemData = GetDataFromSaveData(SYSTEM_DATA_KEY);

        if (systemData.ContainsKey(k))
            systemData[k] = o;
        else
            systemData.Add(k, o);
    }

    object LoadObjectFromSystemData(string k)
    {
        Dictionary<string, object> systemData = GetDataFromSaveData(SYSTEM_DATA_KEY);

        systemData.TryGetValue(k, out object o);
        return o;
    }

    #endregion

    #region [ScenesData]

    void SaveObjectToScenesData(string k, object o)
    {
        Dictionary<string, object> glb_save = GetDataFromSaveData(SCENES_DATA_KEY);

        if (glb_save.ContainsKey(k))
            glb_save[k] = o;
        else
            glb_save.Add(k, o);
    }

    object LoadObjectFromScenesData(string k)
    {
        Dictionary<string, object> glb_save = GetDataFromSaveData(SCENES_DATA_KEY);

        glb_save.TryGetValue(k, out object o);
        return o;
    }

    #endregion

    #region [SaveData]

    SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();
        return saveData;
    }

    /// <summary> Save cached currentSaveData to a data file </summary>
    void WriteSaveDataToFile()
    {
        if (currentSaveData != null)
        {
            SerializationManager.Save(saveFileName, currentSaveData);
        }
    }

    SaveData LoadSaveData()
    {
        //return (SaveData) SerializationManager.Load(saveFileName);
        return (SaveData)SerializationManager.LoadCache();
    }

    #endregion

    #region [Localisation]

    public IEnumerator LoadDialogueCSV(string filename, Action<DialogueCSVLoader> callback)
    {
        DialogueCSVLoader csvLoader = new DialogueCSVLoader();
        yield return StartCoroutine(csvLoader.LoadCSV(filename, callback));
    }
    #endregion

    Dictionary<string, object> GetDataFromSaveData(string key)
    {
        Dictionary<string, object> data = (Dictionary<string, object>)currentSaveData.LoadKey(key);
        if (data == null)
        {
            data = new Dictionary<string, object>();
            instance.currentSaveData.SaveKeyValue(key, data);
        }

        return data;
    }

    #region [Methods: Global Variables]

    public void AddGlobalVariable(GlobalVariable globalVariable)
    {
        AddGlobalVariable(new GlobalVariableData(globalVariable));
    }

    public void AddGlobalVariable(GlobalVariableData globalVariableData)
    {
        gameAssets.globalData.globalVariables.Add(globalVariableData.VarName, globalVariableData);
        SaveObjectToGlobalData(GLOBAL_VARIABLES_KEY, gameAssets.globalData.globalVariables);

        Debug.Log("GlobalVariable [<b>" + globalVariableData.VarName + " : " + globalVariableData.VarValue.ToString() + "]</b> added to save data");
    }

    /// <summary>
    /// Modify a global variable inside save data, or create a new one if it doesn't exits
    /// </summary>
    /// <param name="newGlobalVariable">This global variable will be added instead if the globalVariable cannot be found</param>
    public void ChangeGlobalVariable(GlobalVariable globalVariable, string newVarValue, GlobalVariable newGlobalVariable = null)
    {
        ChangeGlobalVariable(globalVariable.VarName, newVarValue, newGlobalVariable);
    }

    /// <summary>
    /// Modify a global variable inside save data, or create a new one if it doesn't exits
    /// </summary>
    /// <param name="newGlobalVariable">This global variable will be added instead if a global variable with varName cannot be found</param>
    public void ChangeGlobalVariable(string varName, string newVarValue, GlobalVariable newGlobalVariable = null)
    {
        var variable = GetGlobalVariable(varName);
        if (CheckIfGlobalVariableFunc(varName)) return;

        if (variable != null)
        {
            gameAssets.globalData.globalVariables[variable.VarName].VarValue = newVarValue;
            SaveObjectToGlobalData(GLOBAL_VARIABLES_KEY, gameAssets.globalData.globalVariables);
            Debug.Log("GlobalVariable <b>[" + variable.VarName + " : " + variable.VarValue + "]</b> updated in save data ");
        }
        else
        {
            if (newGlobalVariable != null)
            {
                AddGlobalVariable(newGlobalVariable);
                ChangeGlobalVariable(newGlobalVariable.VarName, newVarValue);
            }
        }
    }

    public GlobalVariableData GetGlobalVariable(string varName)
    {
        if (CheckIfGlobalVariableFunc(varName))
            return new GlobalVariableData(varName, varName, GetGlobalVariableFunc(varName), -1); // VariableFuncs shouldn't be saved to save data
        else
            return gameAssets.globalData.globalVariables.Get(varName, null);

    }    

    bool CheckIfGlobalVariableFunc(string varName)
    {
        return varName[^1] == ')';
    }
    

    /// <summary>
    /// Return the result of variableFunc's invocation
    /// </summary>
    /// <param name="code">example: <code>InventoryManager.HasItem(Towel)</code></param>
    public string GetGlobalVariableFunc(string code)
    {
        var funcNameAndParameters = code.SplitHalf("(", mode : StringUtility.SeparateByTokenMode.IncludeTokenInItem2);
        var funcName = funcNameAndParameters.Item1;
        var parameters = funcNameAndParameters.Item2.Extract("(", ")");
        var varFunc = gameAssets.resources.VarFuncs.Find(v=>v.FuncName.Equals(funcName, StringComparison.CurrentCultureIgnoreCase));

        if (varFunc != null)
            return varFunc.Invoke(parameters);
        else
            return null;
    }

    #endregion


    #region [Methods: Global GlobalStateData]

    public void ModifyGlobalStateData(GlobalStateSetter.GlobalStateModified stateModified)
    {
        gameAssets.globalData.ModifyState(stateModified);
    }

    public void SetAsLastGlobalState(GlobalState gs)
    {
        gameAssets.globalData.SetAsLastState(gs);
    }

    public Dictionary<string, GlobalStateData> GetAllGlobalStateData()
    {
        return gameAssets.globalData.globalStatesData;
    }

    public GlobalStateData GetGlobalStateData(GlobalState gs)
    {
        return gameAssets.globalData.globalStatesData.Get(gs.stateName, null);
    }

    #endregion


    #region [Methods: Locations Data]

    public void AddLocationData(Location location, List<LocationTag> currentTags)
    {
        if (gameAssets.globalData.locationsData.AddIfHasnt(location.SceneName, new LocationData(location,currentTags)))
            SaveObjectToGlobalData(LOCATIONS_DATA_KEY, gameAssets.globalData.locationsData);
    }

    public void ChangeLocationData(LocationData newData)
    {
        var locationDataInGM = gameAssets.globalData.locationsData.Get(newData.SceneName, null);
        if (locationDataInGM != null)
        {
            gameAssets.globalData.locationsData.Remove(locationDataInGM.SceneName);
            gameAssets.globalData.locationsData.Add(newData.SceneName, newData);
        }
    }

    public LocationData GetLocationData(Location location)
    {
        return gameAssets.globalData.locationsData.Get(location.SceneName, null);
    }

    public LocationData GetLocationData(string sceneName)
    {
        return gameAssets.globalData.locationsData.Get(sceneName, null);
    }

    #endregion

    #region [Method: SceneData]

    /// <summary>Get a SceneData from ScenesData.scenes if it exists, or create a new one</summary>
    public GameAssets.ScenesData.SceneData GetSceneData(string sceneName)
    {
        var scenes = GetAllSceneData();
        scenes.AddIfHasnt(sceneName, new GameAssets.ScenesData.SceneData(sceneName));

        return scenes[sceneName];
    }

    /// <summary>Edit (or add if it doesn't exist) a SceneData in ScenesData.scenes, then save it</summary>
    public void SaveSceneData(GameAssets.ScenesData.SceneData sceneData)
    {
        var scenes = GetAllSceneData();
        if (scenes.ContainsKey(sceneData.sceneName))
            scenes[sceneData.sceneName] = sceneData;
        else
            scenes.Add(sceneData.sceneName, sceneData);

        SaveObjectToScenesData(SCENES_KEY, scenes);
    }

    public Dictionary<string, GameAssets.ScenesData.SceneData> GetAllSceneData()
    {
        var scenes = LoadObjectFromScenesData(SCENES_KEY) as Dictionary<string, GameAssets.ScenesData.SceneData>;
        if (scenes == null)
            scenes = new Dictionary<string, GameAssets.ScenesData.SceneData>();

        return scenes;
    }

    #endregion

    #region [Methods: Scene Load]

    public void LoadScene(string sceneName, AnimationDirection animationDirection)
    {
        if (currentlyLoadingScene) return;
        currentlyLoadingScene = true;

        OnBeforeSceneLoad();
        SaveGame();
        PreviousSceneName = SceneMaster.current.Location.SceneName;
        CurrentSceneName = sceneName;

        #region [Transitioning]
        
        var transition = Instantiate(sceneTransitionPrefab, transform, false);
        transition.StartCovering(() => { StartCoroutine(Load()); }, animationDirection);
        IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync(sceneName);

            // Load only sceneData (gameAssets has been changed in SaveGame(), then apply it
            gameAssets.currentSceneData = GetSceneData(CurrentSceneName);
            ApplyGameAssetsToSystems();

            var currentSceneMaster = SceneMaster.current != null ? SceneMaster.current : FindObjectOfType<SceneMaster>();
            currentSceneMaster.Init(new SceneMaster.InitialSettings(
                repositionPlayer: true,
                previousSceneName: PreviousSceneName,
                onInitialized: ()=> { OnAfterSceneLoad(transition); }
                ));

        }

        #endregion
    }

    void OnBeforeSceneLoad()
    {
        foreach (var system in PersistentSystems)
        {
            system.OnBeforeSceneLoad();
        }

        Player.EnableAllInputs(false, CursorImageManager.CursorImage.Disabled, false);
        EnableUICornerTools(true, false);
    }

    void OnAfterSceneLoad(SceneTransitionAnimationEvent transition)
    {
        foreach (var system in PersistentSystems)
        {
            system.OnAfterSceneLoad();
        }

        Player.EnableAllInputs(true, CursorImageManager.CursorImage.Disabled, false);
        EnableUICornerTools(true, false);

        InventoryManager.OnAfterSceneLoad();

        transition.StartUncovering(AnimationUncoveredDone);
        currentlyLoadingScene = false;

        void AnimationUncoveredDone()
        {
            Destroy(transition.gameObject);
        }
    }

    #endregion

    #region [Methods: Urgent Choice]

    public UrgentChoiceManager CreateUrgentChoice(UrgentChoiceManager.UrgentChoiceParameters parameters)
    {
        var urgentChoice = Instantiate(urgentChoiceManagerPrefab);
        urgentChoice.Setup(parameters);

        return urgentChoice;
    }

    #endregion

    #region [Methods: Game Settings]

    public void ChangeLanguage(LocalisationSystem.Language language)
    {
        this.language = language;

        SceneMaster.current.LocaliseTexts();
    }

    #endregion

}