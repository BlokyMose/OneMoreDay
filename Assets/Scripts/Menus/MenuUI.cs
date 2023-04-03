using Encore.Localisations;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Encore.Menus
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(TextsLocaliser))]

    public class MenuUI : MonoBehaviour
    {
        #region [Components]

        [Title("Buttons")]
        [SerializeField]
        protected Button exitBut;

        protected CanvasGroup canvasGroup;
        protected TextsLocaliser textsLocaliser;
        protected Animator animator;

        #endregion

        #region [Data Handlers]

        protected int boo_show;

        #endregion


        #region [Delegates]

        public Action OnClosed;

        #endregion


        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            textsLocaliser = GetComponent<TextsLocaliser>();
            animator = GetComponent<Animator>();
            boo_show = Animator.StringToHash(nameof(boo_show));

            #region [Buttons]

            exitBut.onClick.AddListener(OnClickExitBut);

            void OnClickExitBut()
            {
                Show(false);
            }

            #endregion
        }

        public virtual void Show(bool isShowing)
        {
            animator.SetBool(boo_show, isShowing);
            GameManager.Instance.Player.EnableAllInputs(!isShowing);
            canvasGroup.interactable = isShowing;
            canvasGroup.blocksRaycasts = isShowing;

            if (isShowing)
            {
                Setup();
            }
            else
            {
                OnClosed?.Invoke();
                OnClosed = null;
                StartCoroutine(DelayToDestroy(1f));
                IEnumerator DelayToDestroy(float delay)
                {
                    yield return new WaitForSeconds(delay);
                    Destroy(gameObject);
                }
            }
        }

        protected virtual void Setup()
        {

        }
    }
}