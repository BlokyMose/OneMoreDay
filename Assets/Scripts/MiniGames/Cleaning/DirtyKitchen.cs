using Encore.CharacterControllers;
using Encore.Conditions;
using Encore.Dialogues;
using Encore.Interactables;
using Encore.Inventory;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.MiniGames.DIrtyKitchen
{
    [AddComponentMenu("Encore/Interactables/MiniGame/Dirty Kitchen/Dirty Kitchen")]
    public class DirtyKitchen : Interactable
    {
        [Title("MG: Dirty Kitchen")]

        [SerializeField]
        float cleanerScaleMultiplier = 0.75f;

        #region [Vars: Components]

        [SerializeField] Camera zoomInCamera;
        [SerializeField] Button buttonExit;
        [SerializeField] List<Dirt> dirts;
        [SerializeField] List<CheckBoolSetter> checkBoolSetters;

        #region [Callbacks]

        [System.Serializable]
        public class MGCallbacks
        {
            [Header("Interactables")]
            public List<Interactable> onStart;
            public List<Interactable> onQuit;
            public List<Interactable> onWon;
            public List<Interactable> onLost;

            [Header("SpeakingHooks")]
            public List<SpeakingHook> shCannotPlay;
            public List<SpeakingHook> shBeforePlay;
            public List<SpeakingHook> shQuit;
            public List<SpeakingHook> shWon;
            public List<SpeakingHook> shLost;
            public List<SpeakingHook> shPostWon;
            public List<SpeakingHook> shPostLost;
        }

        [SerializeField]
        MGCallbacks callbacks = new MGCallbacks();

        #endregion

        #endregion

        #region [Vars: Data Handlers]

        public bool IsInGame { get { return isInGame; } }
        bool isInGame = false;
        public bool IsClean { get { return isClean; } }
        bool isClean = false;
        public int CleanedDirtCount { get { return cleanedDirtCount; } }
        int cleanedDirtCount = 0;
        public int UncleanedDirtCount { get { return uncleanedDirtCount; } }
        int uncleanedDirtCount = 0;

        const string TOWEL = "TOWEL";
        GameObject towel;
        GameObject prefabTowel;

        #endregion

        Coroutine corInGame;

        protected override void Awake()
        {
            base.Awake();
            buttonExit.onClick.AddListener(() =>
            {
                QuitGame();
                foreach (var item in callbacks.shQuit) item.Speak();

            });
        }

        protected void Start()
        {
            zoomInCamera.gameObject.SetActive(false);
            foreach (var dirt in dirts)
            {
                highlightedSRs.Add(dirt.sr);
                dirt.Setup(TOWEL, false);
                dirt.OnClean += AddCleanedDirt;
            }
            uncleanedDirtCount = dirts.Count;
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return CursorImageManager.CursorImage.Normal;
        }

        public override bool Interact(GameObject interactor)
        {
            var returnValue = false;
            if (!IsClean)
            {
                Item clickedItem = GameManager.Instance.InventoryManager.GetClickedItem();
                if (clickedItem != null) prefabTowel = clickedItem.Prefab;
                returnValue = base.Interact(interactor);
                if (returnValue)
                    foreach (var item in callbacks.shBeforePlay) item.Speak();
                else
                    foreach (var item in callbacks.shCannotPlay) item.Speak();
            }
            else
            {
                foreach (var item in callbacks.shPostWon) item.Speak();
            }
            return returnValue;
        }

        protected override void InteractModule(GameObject interactor)
        {
            StartGame();
        }

        public void StartGame()
        {
            isInGame = true;
            zoomInCamera.gameObject.SetActive(true);
            towel = Instantiate(prefabTowel);
            towel.transform.localScale *= cleanerScaleMultiplier;
            towel.name = TOWEL;
            towel.AddComponent<Rigidbody2D>();
            foreach (var dirt in dirts)
            {
                dirt.EnableCols(true);
            }


            if (corInGame != null) StopCoroutine(corInGame);
            corInGame = StartCoroutine(InGame());
            IEnumerator InGame()
            {
                GameManager.Instance.InventoryManager.ResetClickedItem();
                GameManager.Instance.Player.EnableAllInputs(false);
                GameManager.Instance.EnableUICornerTools(true, false);
                GameManager.Instance.Player.MouseManager.ResetHighlightCurrentInteractable();

                while (true)
                {
                    Vector2 pos = zoomInCamera.ScreenToWorldPoint(GameManager.Instance.Player.MouseManager.MouseScreenPos);
                    towel.transform.position = pos;
                    yield return null;
                }
            }
        }

        public void QuitGame()
        {
            isInGame = false;
            zoomInCamera.gameObject.SetActive(false);
            if (towel != null) Destroy(towel);
            foreach (var dirt in dirts)
            {
                dirt.EnableCols(false);
            }

            GameManager.Instance.Player.EnableAllInputs(true);
            GameManager.Instance.EnableUICornerTools(true, false);
            if (corInGame != null) StopCoroutine(corInGame);
        }

        void AddCleanedDirt()
        {
            cleanedDirtCount++;
            if (cleanedDirtCount >= dirts.Count)
                AllCleaned();
        }

        void AllCleaned()
        {
            QuitGame();
            isClean = true;
            foreach (var item in checkBoolSetters) item.SetCheckBool();
            foreach (var item in callbacks.shWon) item.Speak();
            foreach (var item in callbacks.onWon) item.Interact(GameManager.Instance.Player.gameObject);
        }
    }
}


