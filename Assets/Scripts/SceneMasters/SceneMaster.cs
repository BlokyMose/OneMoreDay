using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;
using Encore.Utility;
using Encore.Interactables;
using Encore.Locations;
using Encore.CharacterControllers;
using Encore.Saves;

namespace Encore.SceneMasters
{
    [AddComponentMenu("Encore/Scene Masters/Scene Master")]
    public class SceneMaster : MonoBehaviour
    {
        #region [Classes]

        [Serializable]
        public class InitialSettings
        {
            public bool repositionPlayer;
            public string previousSceneName;
            public Action OnInitialized;

            public InitialSettings(bool repositionPlayer, string previousSceneName, Action onInitialized)
            {
                this.repositionPlayer = repositionPlayer;
                this.previousSceneName = previousSceneName;
                this.OnInitialized = onInitialized;
            }
        }

        [Serializable]
        public class StartPosData
        {
            public Location location;
            public Transform pos;
            public bool isFacingRight = true;
            public bool instantiateOnFloor = true;

            public StartPosData(Transform pos, Location location, bool isFacingRight = true, bool instantiateOnFloor = true)
            {
                this.pos = pos;
                this.location = location;
                this.isFacingRight = isFacingRight;
                this.instantiateOnFloor = instantiateOnFloor;
            }

            public Vector3 GetStartPos(Collider2D floorCol, Collider2D col)
            {
                return instantiateOnFloor
                        ? new Vector3(pos.position.x, floorCol.bounds.max.y + col.bounds.extents.y - col.offset.y, 0)
                        : pos.position;
            }
        }

        #endregion

        #region [Vars: Properties]

        [SerializeField] bool usePlayerInScene = true;

        [SerializeField, Required] Location location;
        public Location Location { get { return location; } }

        #region [Start Pos]

        [FoldoutGroup("Start Pos"), SerializeField]
        protected List<StartPosData> startPos;

        [FoldoutGroup("Start Pos"), SerializeField]
        protected BoxCollider2D floorCol;

        [FoldoutGroup("Start Pos"), SerializeField]
        StartPosData startPosDefault;

        #endregion

        #endregion

        #region [Vars: Data Handlers]

        protected StartPosData startPosCurrent;

        public static SceneMaster current;

        protected List<string> loadedObjectSaveKeys = new List<string>();

        #endregion

        #region [Methods: Inspector]

        [FoldoutGroup("Start Pos")]
        [ShowInInspector, InlineButton(nameof(_AddStartPos), " + ", ShowIf = "@" + nameof(existingLocation)), LabelText("Add"), LabelWidth(35)]
        Location existingLocation;

        void _AddStartPos()
        {
            var sceneName = existingLocation.SceneName;

            GameObject go = new GameObject();
            go.name = "startPos_" + sceneName;
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;

            startPos.Add(new StartPosData(go.transform, existingLocation));

#if UNITY_EDITOR

            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);

#endif
        }



        #endregion

        #region [Methods: Unity]

        protected virtual void Awake()
        {
            current = this;
        }

        public virtual void Init(InitialSettings settings)
        {
            #region [Instantiate Player]

            PlayerBrain player = null;

            if (usePlayerInScene) // Find a player in this scene
            {
                player = FindObjectOfType<PlayerBrain>();
                if (player == null)
                {
                    Debug.Log("Cannot find PlayerBrain, instantiating new player");
                    player = InstantiateAndSetPlayer(Vector3.zero, Vector3.zero);
                }
                else
                {
                    GameManager.Instance.SetPlayer(player);
                }
            }
            else // Create new player from GameManager.PlayerPrefab
            {
                player = InstantiateAndSetPlayer(Vector3.zero, Vector3.zero);
            }

            GameManager.Instance.SetPlayer(player);

            #region [Set StartPos]

            if (settings.repositionPlayer)
            {
                if (startPos.Count > 0)
                {
                    startPosCurrent = startPos.Find(data => settings.previousSceneName == data.location.SceneName);
                    if (startPosCurrent == null) startPosCurrent = startPosDefault;
                }
                else startPosCurrent = startPosDefault;

                player.transform.position = startPosCurrent.GetStartPos(floorCol, player.GetComponent<Collider2D>());
                player.transform.localEulerAngles = startPosCurrent.isFacingRight ? Vector3.zero : new Vector3(0, 180, 0);
            }

            #endregion

            #endregion

            GameManager.Instance.SaveGame();

            ApplyAllStatesCurrentStage();
            LocaliseTexts();

            settings.OnInitialized?.Invoke();
        }

        #endregion

        #region [Methods: Utility]

        public PlayerBrain InstantiateAndSetPlayer(Vector3 position, Vector3 rotation)
        {
            var player = Instantiate(GameManager.Instance.PlayerPrefab, null);
            var playerBrain = player.GetComponent<PlayerBrain>();
            player.transform.position = position;
            player.transform.localEulerAngles = rotation;

            GameManager.Instance.SetPlayer(playerBrain);

            return playerBrain;
        }

        public void InstantiateNPC(string actorName, Vector3 position, Vector3 rotation)
        {
            Action<GameObject> instantiation = (prefab) =>
            {
                var characterGO = Instantiate(prefab);
                characterGO.transform.position = position;
                characterGO.transform.localEulerAngles = rotation;
            };
            GameManager.Instance.GetCharacterPrefab(actorName, instantiation);
        }

        public void LocaliseTexts()
        {
            // Localise all texts
            var textLocalisers = FindObjectsOfType<Localisations.TextsLocaliser>();
            foreach (var textLocaliser in textLocalisers)
            {
                textLocaliser.LocaliseTexts();
            }
        }

        #endregion

        #region [Methods: GlobalStates]

        public void ApplyAllStatesCurrentStage()
        {
            var allGSOContainers = FindObjectsOfType<GlobalStateObjectContainer>(true);
            var allGlobalStateData = GameManager.Instance.GetAllGlobalStateData();

            // Instead of passing all GSData, just send GSData that is used by GSOContainer in this scene
            var requiredGlobalStateData = new Dictionary<string, Serializables.GlobalStateData>();
            foreach (var container in allGSOContainers)
                foreach (var act in container.gso.activations)
                {
                    var state = allGlobalStateData.Get(act.gs.stateName, new Serializables.GlobalStateData(act.gs.stateName));
                    requiredGlobalStateData.AddIfHasnt(act.gs.stateName, state);
                }

            foreach (var gso in allGSOContainers)
                gso.Evaluate(requiredGlobalStateData);
        }

        public void ApplyState(GlobalState gs, string currentStage)
        {
            var allGSOContainers = FindObjectsOfType<GlobalStateObjectContainer>(true);
            foreach (var container in allGSOContainers)
                container.Evaluate(gs.stateName, currentStage);
        }

        #endregion

        #region [Methods: Save System]

        public void Load(GameManager.GameAssets gameAssets)
        {
            // Find and load all objectSavers
            loadedObjectSaveKeys.Clear();
            var objectSavers = FindObjectsOfType<ObjectSaver>(true);
            foreach (var objectSaver in objectSavers)
            {
                objectSaver.Load(gameAssets);
                loadedObjectSaveKeys.Add(objectSaver.SaveKey);
            }

            // Instantiate NPCs in this scene
            var alreadyExistNPCs = new List<CharacterSaver>(FindObjectsOfType<CharacterSaver>());
            foreach (var characterPair in gameAssets.globalData.charactersData)
            {
                var character = characterPair.Value;
                if (character.CurrentScene == SceneManager.GetActiveScene().name)
                {
                    if (alreadyExistNPCs.Find(npc => npc.SaveKey == character.ActorName) == null)
                        InstantiateNPC(character.ActorName, character.CurrentPos, character.CurrentRot);
                }
            }

            // Enable all input for CharacterBrains
            var brains = FindObjectsOfType<CharacterBrain>();
            foreach (var brain in brains)
            {
                if (brain is not PlayerBrain)
                    brain.EnableAllInputs(true);
            }

        }

        public void Save(GameManager.GameAssets gameAssets)
        {
            // Find and load all objectSavers
            var savedObjectKeys = new List<string>();
            var objectSavers = FindObjectsOfType<ObjectSaver>(true);

            foreach (var objectSaver in objectSavers)
            {
                objectSaver.Save(gameAssets);
                savedObjectKeys.Add(objectSaver.SaveKey);
            }

            {
#if UNITY_EDITOR
                var _savedGOs = new List<GOSaver>();
                var _savedCharacters = new List<CharacterSaver>();

                foreach (var objectSaver in objectSavers)
                {
                    if (objectSaver is GOSaver) _savedGOs.Add((GOSaver)objectSaver);
                    else if (objectSaver is CharacterSaver) _savedCharacters.Add((CharacterSaver)objectSaver);
                }

                string _savedGOsKeys = "";
                foreach (var goSaver in _savedGOs) _savedGOsKeys += goSaver.SaveKey + "\n";

                string _savedCharactersKeys = "";
                foreach (var characterSaver in _savedCharacters) _savedCharactersKeys += characterSaver.SaveKey + ", ";

                Debug.Log(
                    "<b>CharacterSavers</b> are saved [" + _savedCharacters.Count + "] \n" + _savedCharactersKeys + "\n.\n" +
                    "<b>GOSavers</b> are saved [" + _savedGOs.Count + "] \n" + _savedGOsKeys
                    );
#endif
            }

            // Find loadedKeys which haven't been saved because they're destroyed (only applies for GOSaver)
            var _loadedObjectSaveKeys = new List<string>(loadedObjectSaveKeys);
            foreach (var savedKey in savedObjectKeys)
                _loadedObjectSaveKeys.Remove(savedKey);

            foreach (var destroyedSaverKey in _loadedObjectSaveKeys)
                gameAssets.currentSceneData.SaveObjectToObjectData(destroyedSaverKey, new Serializables.GOSaverData() { isDestroyed = true });
        }

        #endregion
    }
}