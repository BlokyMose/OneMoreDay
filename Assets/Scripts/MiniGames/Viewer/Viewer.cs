using Encore.CharacterControllers;
using Encore.Inventory;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.MiniGames.Viewers
{

    /*

    [GENERAL IDEA]
    Shows an isolated viewer of texts or images or anything; also pickupable

    [HOW TO]
    - Make a child class based on Viewer
    - Assign all components required
    - Make this gameObject as a prefab, then assign it to an item's actionPrefab
    - Use menu "GameObject - Create Item Action GO", then click create to the assigned item
    - To prevent Viewer to be pickuped, leave the pickupHook field null

    */

    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
    public abstract class Viewer : MonoBehaviour
    {
        #region [Vars: Components]

        [FoldoutGroup("Components"), SerializeField]
        protected Button exitBut;

        [FoldoutGroup("Components"), SerializeField]
        protected Button pickupBut;

        protected Animator animator;
        protected CanvasGroup canvasGroup;

        #endregion

        #region [Vars: Properties]

        [FoldoutGroup("Properties"), SerializeField]
        protected ItemPickupHook pickupHook;
        public ItemPickupHook PickupHook { get { return pickupHook; } set { pickupHook = value; } }

        #endregion

        #region [Vars: Events]

        [FoldoutGroup("Events"), SerializeField]
        protected UnityEvent onHide;

        #endregion

        #region [Vars: Data Handlers]

        protected Coroutine corDeactivating;
        protected int boo_show;
        protected bool isPicked = false;
        public void SetIsPicked(bool isPicked) { this.isPicked = isPicked; }

        #endregion

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();
            boo_show = Animator.StringToHash(nameof(boo_show));

            #region [Setup buttons]

            exitBut.onClick.AddListener(OnClickExitBut);
            void OnClickExitBut()
            {
                Show(false);
            }

            if (pickupHook != null)
            {
                EventTrigger pickupBut_et = pickupBut.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry pickupBut_entry_enter = new EventTrigger.Entry();
                pickupBut_entry_enter.eventID = EventTriggerType.PointerEnter;
                pickupBut_entry_enter.callback.AddListener((data) =>
                {
                    GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Grab);
                });
                pickupBut_et.triggers.Add(pickupBut_entry_enter);

                EventTrigger.Entry pickupBut_entry_exit = new EventTrigger.Entry();
                pickupBut_entry_exit.eventID = EventTriggerType.PointerExit;
                pickupBut_entry_exit.callback.AddListener((data) =>
                {
                    GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Normal);
                });
                pickupBut_et.triggers.Add(pickupBut_entry_exit);


                pickupBut.onClick.AddListener(OnClickPickupBut);

                void OnClickPickupBut()
                {
                    Pickup();
                }
            }
            else
            {
                pickupBut.gameObject.SetActive(false);
            }

            #endregion
        }

        protected virtual void Start()
        {

        }

        public virtual void Show(bool isShowing)
        {
            animator.SetBool(boo_show, isShowing);
            GameManager.Instance.Player.EnableAllInputs(!isShowing);
            canvasGroup.interactable = isShowing;
            canvasGroup.blocksRaycasts = isShowing;

            if (isShowing)
            {
                if (corDeactivating != null) StopCoroutine(corDeactivating);
                GameManager.Instance.EnableUICornerTools(false,false);
                if (GameManager.Instance.InventoryManager.GetClickedItem() != null)
                    GameManager.Instance.InventoryManager.ResetClickedItem();
            }
            else
            {
                onHide.Invoke();
                corDeactivating = StartCoroutine(Delay());
                IEnumerator Delay()
                {
                    yield return new WaitForSeconds(1);
                    GameManager.Instance.EnableUICornerTools(true, false);

                    yield return new WaitForSeconds(1);
                    corDeactivating = null;

                    gameObject.SetActive(false);
                    if (isPicked) Destroy(gameObject);
                }
            }
        }

        public virtual void Pickup()
        {
            if (pickupBut == null) return;

            if (pickupHook.Pickup())
            {
                isPicked = true;
                pickupBut.gameObject.SetActive(false);
                pickupBut = null;
                if (transform.parent != null)
                {
                    GameObject parent = transform.parent.gameObject;
                    transform.SetParent(null, false);
                    Destroy(parent);
                }
            }
        }
    }
}