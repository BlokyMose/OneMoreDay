using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine.UI;
using Encore.Interactables;
using Encore.Inventory;
using Encore.Utility;

namespace Encore.CharacterControllers
{
    [AddComponentMenu("Encore/Character Controllers/Mouse Manager")]
    public class MouseManager : MonoBehaviour
    {
        #region [Classes]

        [Serializable]
        public class HighlightAppearance
        {
            public Material MatNormal { get { return matNormal; } }
            [SerializeField]
            private Material matNormal;

            public Material MatHighlight { get { return matHighlight; } }
            [SerializeField]
            private Material matHighlight;

            public Material MatStoring { get { return matStoring; } }
            [SerializeField]
            private Material matStoring;

            public ActionPreview ActionPreviewPrefab { get { return actionPreviewPrefab; } }
            [SerializeField]
            private ActionPreview actionPreviewPrefab;

        }

        #endregion

        #region [Properties]

        [SerializeField]
        HighlightAppearance highlightAppearance;

        [BoxGroup("Hover Object Text"), SerializeField, Required]
        private GameObject objectTextPrefab;
        [BoxGroup("Hover Object Text"), SerializeField]
        float textNameYOffset = 1f;
        [BoxGroup("Hover Object Text"), SerializeField]
        float textFontSize = 0.75f;


        [BoxGroup("Cursor Image Manager"), SerializeField, Required]
        private GameObject cursorCanvasPrefab;
        [BoxGroup("Cursor Image Manager"), SerializeField, Required]
        private CursorImageManager.CursorSprites cursorSprites;
        [BoxGroup("Cursor Image Manager"), SerializeField]
        private Vector2 cursorScale = new Vector2(1, 1);
        [BoxGroup("Cursor Image Manager"), SerializeField]
        private Vector2 cursorOffset = new Vector2(1f, -1f);
        [BoxGroup("Cursor Image Manager"), SerializeField]
        private Color cursorColor = new Color(1, 1, 1, 1);
        [BoxGroup("Cursor Image Manager"), SerializeField]
        private Vector2 itemSpriteScale = new Vector2(1f, 1f);

        #endregion

        #region [Data Handlers]

        CharacterBrain brain;
        CursorImageManager cursorImageManager;
        Interactable hoveredInteractable;
        int boo_showBelow, boo_show; // objectText's animator parameters
        private const string OBJECT_TEXT = "ObjectText";
        private const string ITEM_PREVIEW = "ItemPreview";
        Coroutine corUpdateSpriteByPlayerPosition;

        #endregion

        #region [Delegates]

        /// <summary>
        /// Returns true after a try to interact with hovered object;<br></br>
        /// Returns false after a try to interact with no hovered object
        /// </summary>
        public Action<bool> OnInteracted;

        public Func<List<Collider2D>> GetNearbyColliders;

        /// <summary>
        /// Left-Bottom is  (0,0)
        /// </summary>
        public Vector2 MouseScreenPos { get; private set; }

        bool canInteract = false;

        #endregion

        #region [Methods: Unity]

        private void Awake()
        {
            boo_showBelow = Animator.StringToHash(nameof(boo_showBelow));
            boo_show = Animator.StringToHash(nameof(boo_show));
        }

        public void Setup(CharacterBrain brain)
        {
            this.brain = brain;
            brain.OnSetCanInteract += SetCanInteract;

            GameObject cursorCanvas = Instantiate(cursorCanvasPrefab);
            cursorImageManager = cursorCanvas.GetComponentInChildren<CursorImageManager>();
            cursorImageManager.Setup(brain, this, cursorSprites, cursorScale, cursorOffset, cursorColor, itemSpriteScale);
            OnInteracted += ManageClickedInventoryItem;
        }

        private void OnDisable()
        {
            if (brain != null)
            {
                brain.OnSetCanInteract -= SetCanInteract;
                cursorImageManager.Setdown(brain);
            }
        }

        void Update()
        {
            MouseScreenPos = Mouse.current.position.ReadValue();

            if (GameManager.Instance.DialogueManager != null && GameManager.Instance.DialogueManager.IsInSpeaking)
                return;

            if (!canInteract) return;

            if (IsMouseBeyondScreen) return;

            // Raycast will go through Character layer 
            RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(MouseScreenPos), Vector2.zero, Mathf.Infinity, 9);

            // Hovering an interactable
            if (hitInfo)
            {
                Interactable hoveringInteractable = hitInfo.collider.GetComponent<Interactable>();
                if (hoveringInteractable != null)
                {
                    if (hoveredInteractable != hoveringInteractable)
                        ResetHighlightCurrentInteractable();

                    hoveredInteractable = hoveringInteractable;
                    hoveredInteractable.Highlight(highlightAppearance);
                    cursorImageManager.ShowTooltip(hoveredInteractable.GetObjectName);

                    ChangeCursorSprite(true);
                }
                // Hovering a non-interactable
                else
                {
                    ResetHighlightCurrentInteractable();
                }
            }

            // Hovering a non-colider
            else
            {
                ResetHighlightCurrentInteractable();
            }
        }


        public void ResetHighlightCurrentInteractable()
        {
            if (hoveredInteractable != null)
            {
                hoveredInteractable.Unhighlight();
                ChangeCursorSprite(false);
                hoveredInteractable = null;
                cursorImageManager.HighlightItemSprite(false);
                cursorImageManager.HideTooltip();
            }
        }

        #endregion

        #region [Methods: Interaction]

        public void Interact()
        {
            if (!hoveredInteractable || !canInteract)
            {
                OnInteracted(false);
                return;
            }

            if (GetNearbyColliders == null || GetNearbyColliders().Contains(hoveredInteractable.GetComponent<Collider2D>()))
            {
                List<Interactable> interactables = new List<Interactable>(hoveredInteractable.gameObject.GetComponents<Interactable>());
                foreach (var interactable in interactables) interactable.Interact(GameManager.Instance.Player.gameObject);
                OnInteracted(true);
            }
            else
            {
                // If object is too far away
                OnInteracted(false);
            }
        }

        void ManageClickedInventoryItem(bool hasInteracted)
        {
            InventoryManager inventory = GameManager.Instance.InventoryManager;
            if (!hasInteracted && inventory.GetClickedItem() && !inventory.IsCursorHovering && !inventory.GetClickedItem().IsGripOnly)
            {
                inventory.ResetClickedItem();
                inventory.Show(false);
            }
        }

        #endregion

        #region [Methods: Cursor Sprite]

        void ChangeCursorSprite(bool newInteractable)
        {
            if (!canInteract) return;
            if (GetNearbyColliders == null)
            {
                if (newInteractable)
                    cursorImageManager.SetSprite(hoveredInteractable.GetCursorImage());
                else
                    cursorImageManager.SetSprite(CursorImageManager.CursorImage.Normal);

                return;
            }

            // Hovering a new object
            if (newInteractable)
            {
                // If interactable is too far, wait until player is closer
                if (!GetNearbyColliders().Contains(hoveredInteractable.GetComponent<Collider2D>()))
                {
                    cursorImageManager.SetSprite(CursorImageManager.CursorImage.Far);
                    corUpdateSpriteByPlayerPosition = StartCoroutine(WaitUntilPlayerCloser());
                }
                else
                {
                    cursorImageManager.SetSprite(hoveredInteractable.GetCursorImage());
                    corUpdateSpriteByPlayerPosition = StartCoroutine(WaitUntilPlayerFarer());
                }
            }

            // Not Hovering
            else
            {
                cursorImageManager.SetSprite(CursorImageManager.CursorImage.Normal);
                if (corUpdateSpriteByPlayerPosition != null)
                    StopCoroutine(corUpdateSpriteByPlayerPosition);
            }

            IEnumerator WaitUntilPlayerCloser()
            {
                while (true)
                {
                    if (hoveredInteractable != null)
                    {
                        if (GetNearbyColliders().Contains(hoveredInteractable.GetComponent<Collider2D>()))
                        {
                            ChangeCursorSprite(hoveredInteractable.GetCursorImage());

                            if (corUpdateSpriteByPlayerPosition != null) StopCoroutine(corUpdateSpriteByPlayerPosition);
                            corUpdateSpriteByPlayerPosition = StartCoroutine(WaitUntilPlayerFarer());
                            break;
                        }
                    }
                    else break;

                    yield return null;
                }
            }

            IEnumerator WaitUntilPlayerFarer()
            {
                while (true)
                {
                    if (hoveredInteractable != null)
                    {
                        if (!GetNearbyColliders().Contains(hoveredInteractable.GetComponent<Collider2D>()))
                        {
                            ChangeCursorSprite(CursorImageManager.CursorImage.Far);

                            if (corUpdateSpriteByPlayerPosition != null) StopCoroutine(corUpdateSpriteByPlayerPosition);
                            corUpdateSpriteByPlayerPosition = StartCoroutine(WaitUntilPlayerCloser());
                            break;
                        }
                    }
                    else break;

                    yield return null;
                }
            }
        }

        public CursorImageManager CursorImageManager
        {
            get
            {
                return cursorImageManager;
            }
        }

        public void ChangeCursorSprite(CursorImageManager.CursorImage cursorImage)
        {
            cursorImageManager.SetSprite(cursorImage);

        }

        /// <param name="pos">Position in Screen; Left-Bottom? is (0,0)</param>
        public void SetCursorPosition(Vector2 pos, bool screenOffset = false)
        {
            pos = screenOffset 
                ? pos + new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2) 
                : pos;
            Mouse.current.WarpCursorPosition(pos);
            UnityEngine.InputSystem.LowLevel.InputState.Change(Mouse.current.position, pos);
        }

        #endregion

        #region [Methods: Utilities]

        void SetCanInteract(bool canInteract, CursorImageManager.CursorImage disabledCursorImage = CursorImageManager.CursorImage.Disabled)
        {
            this.canInteract = canInteract;

            if (!canInteract)
            {
                ChangeCursorSprite(disabledCursorImage);
                ResetHighlightCurrentInteractable();
            }
            else
            {
                ChangeCursorSprite(CursorImageManager.CursorImage.Normal);
            }
        }

        bool IsMouseBeyondScreen
        {
            get
            {
                var checkMousePos = Mouse.current.position.ReadValue();
                if (checkMousePos.x < -50 || checkMousePos.y < -50) return true;
                if (checkMousePos.x > Screen.width + 50 || checkMousePos.y > Screen.height + 50) return true;
                return false;
            }
        }


        #endregion

    }
}