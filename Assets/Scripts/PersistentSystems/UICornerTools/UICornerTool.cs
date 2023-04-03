using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore
{
    public abstract class UICornerTool : MonoBehaviour, IPersistentSystem
    {
        [Title("UI Corner Tool")]
        [SerializeField] private Image hitShowPanel;
        [SerializeField] private CanvasGroup hitHidePanel;
        [SerializeField] private CanvasGroup maskHitHidePanel;
        [SerializeField] private Animator uiCornerIcon;

        protected Animator animator;

        #region [Data Handlers]

        protected CanvasGroup canvasGroup;

        public bool IsShowing { get { return isShowing; } }
        protected bool isShowing = false;
        public bool CanShow { get { return canShow; } }
        protected bool canShow = true;

        /// <summary>
        /// Check if mouse cursor is inside this UI; <br></br>Dont't set if unnecessary
        /// </summary>
        public bool IsCursorHovering { get { return isCursorHovering; } set { isCursorHovering = value; } }
        protected bool isCursorHovering = false;

        /// <param name="showTemporary"> Preview the tool for a second, before closing; Doesn't work if canShow is false</param>
        public void SetCanShow(bool canShow, bool showTemporary)
        {
            if (!canShow && isShowing) Show(false);
            this.canShow = canShow;
            canvasGroup.interactable = canShow;
            canvasGroup.blocksRaycasts = canShow;
            if (canShow && showTemporary) corShowTemporary = StartCoroutine(ShowTemporary(2.5f)); // Preview the tool that it can now be opened

            uiCornerIcon.SetBool(boo_show, canShow);
        }
        protected Coroutine corShowTemporary;

        // Animator's parameters
        protected int boo_show;

        #endregion

        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);

            canvasGroup = transform.GetChild(0).GetComponent<CanvasGroup>();
            animator = transform.GetChild(0).GetComponent<Animator>();
            boo_show = Animator.StringToHash(nameof(boo_show));

            #region [HitShowPanel: EventTriggers]

            if (!hitShowPanel) hitShowPanel = transform.GetChild(0).Find(nameof(hitShowPanel)).GetComponent<Image>();
            EventTrigger hitShowPanel_et = hitShowPanel.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry hitShowPanel_et_entry_enter = new EventTrigger.Entry();
            hitShowPanel_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            hitShowPanel_et_entry_enter.callback.AddListener((data) =>
            {
                IsCursorHovering = true;
                Show(true);
                if (corShowTemporary != null) { StopCoroutine(corShowTemporary); corShowTemporary = null; }
            });
            hitShowPanel_et.triggers.Add(hitShowPanel_et_entry_enter);

            #endregion

            #region [HitHidePanel & Mask: EventTriggers]

            if (!hitHidePanel) hitHidePanel = transform.transform.GetChild(0).Find(nameof(hitHidePanel)).GetComponent<CanvasGroup>();
            EventTrigger hitHidePanel_et = hitHidePanel.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry hitHidePanel_et_entry_enter = new EventTrigger.Entry();
            hitHidePanel_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            hitHidePanel_et_entry_enter.callback.AddListener((data) =>
            {
                IsCursorHovering = false;
                if (corShowTemporary == null) Show(false);
            });
            hitHidePanel_et.triggers.Add(hitHidePanel_et_entry_enter);

            if (!maskHitHidePanel) maskHitHidePanel = transform.transform.GetChild(0).Find(nameof(maskHitHidePanel)).GetComponent<CanvasGroup>();
            EventTrigger maskHitHidePanel_et = maskHitHidePanel.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry maskHitHidePanel_et_entry_enter = new EventTrigger.Entry();
            maskHitHidePanel_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            maskHitHidePanel_et_entry_enter.callback.AddListener((data) =>
            {
                IsCursorHovering = true;
                if (corShowTemporary != null) { StopCoroutine(corShowTemporary); corShowTemporary = null; }
            });
            maskHitHidePanel_et.triggers.Add(maskHitHidePanel_et_entry_enter);

            #endregion
        }

        protected virtual void Start()
        {
            //Show(false);
        }

        protected IEnumerator ShowTemporary(float delay)
        {
            Show(true);
            yield return new WaitForSeconds(delay);
            Show(false);

            corShowTemporary = null;
        }

        public virtual void Show(bool isShowing)
        {
            if (!canShow) return;

            this.isShowing = isShowing;

            hitHidePanel.interactable = isShowing;
            hitHidePanel.blocksRaycasts = isShowing;
            maskHitHidePanel.interactable = isShowing;
            maskHitHidePanel.blocksRaycasts = isShowing;

            animator.SetBool(boo_show, isShowing);
            uiCornerIcon.SetBool(boo_show, !isShowing);
        }

        public abstract void OnBeforeSceneLoad();
        public abstract void OnAfterSceneLoad();

    }
}