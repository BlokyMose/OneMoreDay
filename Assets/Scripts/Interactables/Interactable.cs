using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using Encore.Conditions;
using Encore.CharacterControllers;
using Encore.Environment;
using Encore.Utility;

namespace Encore.Interactables
{
    public abstract class Interactable : MonoBehaviour
    {
        #region [Vars: General Properties]

        [FoldoutGroup("Interactable")]
        // Object Name
        [SerializeField, Required, InlineButton(nameof(_SetObjectNameAsGOName), "GO's Name", ShowIf = "@string.IsNullOrWhiteSpace(objectName)")]
        protected string objectName = string.Empty;

        public virtual string GetObjectName { get { return objectName; } }
        public void SetObjectName(string newName) { objectName = newName; }

        // Activation
        public bool IsActive { get { return isActive; } }
        [FoldoutGroup("Interactable"), SerializeField, GUIColor("@" + nameof(isActive) + "? Color.green : new Color(1f, 0.66f, 0.66f)")]
        protected bool isActive = true;

        // PostInteractionMode
        public enum PostInteractionMode { None, Remove, Deactivate }
        [FoldoutGroup("Interactable"), SerializeField]
        protected PostInteractionMode postInteractionMode = PostInteractionMode.None;
        public PostInteractionMode GetInteractionMode { get { return postInteractionMode; } }
        public void SetPostInteractionMode(PostInteractionMode mode) { postInteractionMode = mode; }

        [FoldoutGroup("Interactable"), SerializeField, ShowIf("@postInteractionMode==PostInteractionMode.Remove")]
        protected bool removeColliderAlso = true;

        // HighlightMode
        public enum HighlightCondition { Always, IfActive, IfActiveAndCondition }
        [FoldoutGroup("Interactable"), SerializeField]
        protected HighlightCondition highlightCondition = HighlightCondition.Always;
        public HighlightCondition GetHighlightMode { get { return highlightCondition; } }

        // Other
        [FoldoutGroup("Interactable"), SerializeField, InlineButton(nameof(_AddConditionContainer), " Add ", ShowIf = "@!" + nameof(conditionContainer))]
        protected ConditionContainer conditionContainer;

        [FoldoutGroup("Interactable"), SerializeField, InlineButton(nameof(_AddSFXManager), " Add ", ShowIf = "@!" + nameof(sfx))]
        protected SFXManager sfx;

        [FoldoutGroup("Interactable"), SerializeField, InlineButton(nameof(_AddStandingPos), " Add ", ShowIf = "@!" + nameof(standingPos))]
        Transform standingPos;
        public enum FaceDirection { FaceThis, AsIs, Right, Left }
        [FoldoutGroup("Interactable"), SerializeField, ShowIf(nameof(standingPos))]
        FaceDirection faceDirection = FaceDirection.FaceThis;


        #endregion

        #region [Vars: Overrider]

        [FoldoutGroup("Interactable"), SerializeField]
        protected bool overrideMouseMats = false;

        [Serializable]
        public class MouseMaterialsOverrider
        {
            public string OverlayOpacityName { get { return overlayOpacityName; } }
            [SerializeField]
            protected string overlayOpacityName = "_OverlayOpacity";

            public float OverlayOpacity { get { return overlayOpacity; } }
            [SerializeField, PropertyRange(0, 1)]
            protected float overlayOpacity = 0.35f;

            public string OverlayColorName { get { return overlayColorName; } }
            [SerializeField]
            protected string overlayColorName = "_OverlayColor";

            public Color OverlayColor { get { return overlayHighlightColor; } }
            [SerializeField]
            protected Color overlayHighlightColor = new Color(0.9967995f, 1, 0.2018868f, 1);
        }

        [FoldoutGroup("Interactable"), SerializeField, ShowIf(nameof(overrideMouseMats))]
        protected MouseMaterialsOverrider mouseMatsOverrider;

        public bool OverrideObjectText { get { return overrideObjectText; } }
        [FoldoutGroup("Interactable"), SerializeField]
        protected bool overrideObjectText = false;

        [Serializable]
        public class ObjectTextTransformOverrider
        {
            public Vector2 Pos { get { return pos; } }
            [SerializeField]
            Vector2 pos = new Vector2(0, 0);

            public Vector2 LocalScale { get { return localScale; } }
            [SerializeField]
            Vector2 localScale = new Vector2(1, 1);

            public enum AnimationShow { Auto, FromTop, FromBottom }
            public AnimationShow AnimShow { get { return animationShow; } }
            [SerializeField]
            AnimationShow animationShow = AnimationShow.Auto;
        }

        public ObjectTextTransformOverrider ObjectTextOverrider { get { return objectTextOverrider; } }
        [FoldoutGroup("Interactable"), SerializeField, ShowIf(nameof(overrideObjectText))]
        ObjectTextTransformOverrider objectTextOverrider;

        #endregion

        #region [Vars: Components]

        [Title("Add Collider (Optional)"), VerticalGroup("Interactable/Col")]
        [PropertyTooltip("A collider must exist in this GO so Mouse can interact with this"), HideLabel]
        [SerializeField, ShowIf("@!" + nameof(col)), InlineButton(nameof(AssignExistingCol), " Get ")]
        protected Collider2D col;

        public List<SpriteRenderer> HighlightedSRs { get { return highlightedSRs; } }
        protected List<SpriteRenderer> highlightedSRs = new List<SpriteRenderer>();
        [HideInEditorMode]
        public GameObject objectText;

        #endregion

        #region [Methods: Inspector]

        void _SetObjectNameAsGOName()
        {
            objectName = gameObject.name;
        }

        void _AddSFXManager()
        {
            sfx = gameObject.AddComponent<SFXManager>();
        }

        [ButtonGroup("Interactable/Col/But"), Button("Box"), ShowIf("@!" + nameof(col)), GUIColor(0.3f, 0.8f, 0.3f)]
        public void AddBoxCol()
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

        [ButtonGroup("Interactable/Col/But"), Button("Capsule"), ShowIf("@!" + nameof(col)), GUIColor(0.3f, 0.8f, 0.3f)]
        public void AddCapsuleCol()
        {
            col = gameObject.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
        }

        [ButtonGroup("Interactable/Col/But"), Button("Circle"), ShowIf("@!" + nameof(col)), GUIColor(0.3f, 0.8f, 0.3f)]
        public void AddCirlceCol()
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        [ButtonGroup("Interactable/Col/But"), Button("Polygon"), ShowIf("@!" + nameof(col)), GUIColor(0.3f, 0.8f, 0.3f)]
        public void AddPolygonCol()
        {
            col = gameObject.AddComponent<PolygonCollider2D>();
            col.isTrigger = true;
        }

        public void AssignExistingCol()
        {
            if (GetComponent<Collider2D>() != null)
            {
                col = GetComponent<Collider2D>();
                col.isTrigger = true;
            }
        }

        void _AddConditionContainer()
        {
            conditionContainer = gameObject.AddComponent<ConditionContainer>();
        }

        void _AddStandingPos()
        {
            GameObject standingPosGO = new GameObject("StandingPos");
            standingPosGO.transform.parent = transform;
            standingPosGO.transform.localPosition = Vector3.zero;
            standingPos = standingPosGO.transform;
        }

        [PropertyOrder(-1)]
        [Button("Quick Setup", ButtonSizes.Large),GUIColor("@Color.green"), ShowIf("@string.IsNullOrEmpty("+nameof(objectName)+")")]
        void _QuickSetup()
        {
            _SetObjectNameAsGOName();
            AddCapsuleCol();
            if (GetComponent<SpriteRenderer>() == null)
            {
                (col as CapsuleCollider2D).size = new Vector2(5, 15);
            }
        }

        #endregion

        #region [Vars: Data Handlers]

        protected List<ActionPreview> actionPreviews = new List<ActionPreview>();

        public Action<bool> OnInteract;

        protected Coroutine corDestroyingActionPreviews;

        #endregion

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(objectName)) objectName = gameObject.name;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr!=null) highlightedSRs.Add(sr);
        }

        /// <summary>Checking some conditions then call InteractModule after c; also plays SFX</summary>
        /// <returns>Succeed to execute <see cref="InteractModule(GameObject)"/></returns>
        public virtual bool Interact(GameObject interactor)
        {
            #region [Check interactability]

            if (!isActive ||
                conditionContainer != null && !conditionContainer.CheckCondition())
            {
                OnInteract?.Invoke(false);
                return false;
            }

            OnInteract?.Invoke(true);

            #endregion

            InteractModule(interactor);

            #region [Settings]

            if (postInteractionMode == PostInteractionMode.Remove)
            {
                SelfDestroy();
            }
            else if (postInteractionMode == PostInteractionMode.Deactivate)
            {
                Activate(false);
            }

            sfx?.PlaySFX();

            if (standingPos != null)
            {
                StartCoroutine(WalkPlayerToStandingPos());
                IEnumerator WalkPlayerToStandingPos()
                {
                    PlayerController player = GameManager.Instance.Player.Controller as PlayerController;
                    while (true)
                    {
                        if (Mathf.Abs(player.transform.position.x - standingPos.position.x) > 1.5f)
                        {
                            if (player.transform.position.x < standingPos.position.x) player.Move(Vector2.right);
                            else player.Move(Vector2.left);
                        }
                        else break;
                        yield return null;
                    }

                    player.Move(Vector2.zero);
                    switch (faceDirection)
                    {
                        case FaceDirection.FaceThis:
                            if (player.transform.position.x < transform.position.x) player.transform.localEulerAngles = Vector3.zero;
                            else player.transform.localEulerAngles = new Vector3(0, 180, 0);
                            break;

                        case FaceDirection.Right:
                            player.transform.localEulerAngles = Vector3.zero;
                            break;
                        case FaceDirection.Left:
                            player.transform.localEulerAngles = new Vector3(0, 180, 0);
                            break;

                        case FaceDirection.AsIs:
                        default:
                            break;
                    }
                }
            }

            #endregion

            Unhighlight();

            return true;
        }

        /// <summary>Pure interaction function based on Interactable child</summary>
        protected abstract void InteractModule(GameObject interactor);

        /// <summary>Check conditions, then return cursor image</summary>
        public virtual CursorImageManager.CursorImage GetCursorImage()
        {
            // Show disabled cursor if there's an unfulfilled condition
            if (conditionContainer != null && !conditionContainer.CheckCondition())
                return CursorImageManager.CursorImage.Disabled;
            else if (!isActive)
                return CursorImageManager.CursorImage.Disabled;
            else
                return GetCursorImageModule();
        }

        /// <summary>Doesn't check conditions before return cursor image</summary>
        protected virtual CursorImageManager.CursorImage GetCursorImageModule()
        {
            return CursorImageManager.CursorImage.Normal;
        }

        /// <summary>
        /// Change sprite renderer's material when being hovered
        /// </summary>
        /// <param name="isHighlighting">"False" to dehighligh</param>
        /// <param name="appearance">Interactable will process which material to be used</param>
        public virtual void Highlight(MouseManager.HighlightAppearance appearance)
        {
            if (!ValidateHighlight()) return;
            HighlightModule(appearance);
        }

        protected virtual void HighlightModule(MouseManager.HighlightAppearance appearance)
        {
            if (actionPreviews.Count == 0)
            {
                foreach (var sr in highlightedSRs)
                {
                    var actionPreview = Instantiate(appearance.ActionPreviewPrefab, transform, false);
                    var mat = GetHighlightMaterial(appearance.MatHighlight);

                    actionPreview.SetupSprite(sr, mat);
                    actionPreview.PlayAnimation(ActionPreview.AnimationMode.None);
                    actionPreviews.Add(actionPreview);
                }
            }
            else
            {
                this.StopCoroutineIfExists(corDestroyingActionPreviews);
                corDestroyingActionPreviews = null;
                foreach (var actionPreview in actionPreviews)
                    actionPreview.PlayAnimation(ActionPreview.AnimationMode.None);
            }
        }

        protected virtual bool ValidateHighlight()
        {
            switch (highlightCondition)
            {
                case HighlightCondition.IfActive:
                    if (!isActive) return false;
                    break;
                case HighlightCondition.IfActiveAndCondition:
                    if (!isActive || !conditionContainer.CheckCondition()) return false;
                    break;
            }

            return true;
        }

        public virtual void Unhighlight()
        {
            foreach (var actionPreview in actionPreviews)
            {
                actionPreview.PlayAnimation(ActionPreview.AnimationMode.Hide);
            }

            corDestroyingActionPreviews = this.RestartCoroutine(Delay(3f));
            IEnumerator Delay(float delay)
            {
                yield return new WaitForSeconds(delay);
                for (int i = actionPreviews.Count - 1; i >= 0; i--)
                    Destroy(actionPreviews[i].gameObject);
                actionPreviews.Clear();
                corDestroyingActionPreviews = null;
            }
        }

        /// <summary>Get material for interactable's sprite after checking overrideMouseMats</summary>
        protected Material GetHighlightMaterial(Material fallbackMaterial)
        {
            var mat = fallbackMaterial;
            if (overrideMouseMats)
            {
                mat = Instantiate(fallbackMaterial);
                mat.SetColor(mouseMatsOverrider.OverlayColorName, mouseMatsOverrider.OverlayColor);
                mat.SetFloat(mouseMatsOverrider.OverlayOpacityName, mouseMatsOverrider.OverlayOpacity);
            }
            return mat;
        }

        /// <summary>Enable interaction, enable collider, and show sprites</summary>
        public void Activate(bool isActive)
        {
            this.isActive = isActive;
            foreach (var sr in highlightedSRs) sr.enabled = isActive;
            if (col != null) col.enabled = isActive;
        }

        protected void SelfDestroy()
        {
            if (objectText) Destroy(objectText, 0.1f);
            if (removeColliderAlso) Destroy(col, 0.125f);
            Destroy(this, 0.1f);
        }

    }
}