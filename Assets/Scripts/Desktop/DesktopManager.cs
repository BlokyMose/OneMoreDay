using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace Encore.Desktop
{
    [AddComponentMenu("Encore/Desktop/Desktop Manager")]
    public class DesktopManager : MonoBehaviour

    {
        [SerializeField] private GameObject lockScreenWindow;
        [SerializeField] private GameObject homeWindow;
        [SerializeField] private GameObject surfBrowserWindow;
        [SerializeField] private GameObject notebookWindow;
        [SerializeField] private GameObject universalAppsBox;

        [SerializeField] private Transform lockScreenTitle_Welcome;
        [SerializeField] private Transform lockScreenTitle_Back;
        [SerializeField] private string lockScreenPass;
        private bool canAnimateTitle = true;

        private float canvasScaleFactor;

        private GameObject currentOpenPanel;
        private bool allowPanelAnim = true;

        private bool showUniversalAppsBox = false;
        private bool isShowingUniversalAppsBox = false;
        private bool appsBoxIsAnimating = false;

        #region MonoBehaviour

        private void Start()
        {
            canvasScaleFactor = GetComponent<Canvas>().scaleFactor;
        }

        #endregion

        #region Universal

        public void ResetLayout()
        {
            homeWindow.SetActive(false);
            surfBrowserWindow.SetActive(false);
            notebookWindow.SetActive(false);
            universalAppsBox.SetActive(false);
            lockScreenWindow.SetActive(true);
            currentOpenPanel = lockScreenWindow;
        }

        public void Shutdown()
        {
            //gameObject.SetActive(false);
        }

        public void ShowUniversalAppsBox()
        {
            if (currentOpenPanel == homeWindow || currentOpenPanel == lockScreenWindow) return;

            showUniversalAppsBox = true;

            if (!isShowingUniversalAppsBox)
            {
                if (!appsBoxIsAnimating)
                    StartCoroutine(ShowAppsBox(true));
            }
        }

        public void HideUniversalAppsBox()
        {
            if (currentOpenPanel == homeWindow || currentOpenPanel == lockScreenWindow) return;

            showUniversalAppsBox = false;

            if (isShowingUniversalAppsBox)
            {
                if (!appsBoxIsAnimating)
                    StartCoroutine(ShowAppsBox(false));
            }
        }

        IEnumerator ShowAppsBox(bool b)
        {
            appsBoxIsAnimating = true;

            RectTransform rt = universalAppsBox.GetComponent<RectTransform>();
            float scaledRectHeight = rt.rect.height * canvasScaleFactor;

            if (!b)
            {
                yield return new WaitForSeconds(1);

                if (!showUniversalAppsBox)
                {
                    yield return rt.DOMoveY(0 - (scaledRectHeight / 2), 0.5f).SetEase(Ease.InCubic).WaitForCompletion();
                    universalAppsBox.SetActive(false);
                    isShowingUniversalAppsBox = false;
                }
            }
            else
            {
                appsBoxIsAnimating = true;
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0 - scaledRectHeight);
                universalAppsBox.SetActive(true);
                yield return rt.DOMoveY(0 + (scaledRectHeight / 2), 0.5f).SetEase(Ease.OutCubic).WaitForCompletion();
                isShowingUniversalAppsBox = true;
            }

            appsBoxIsAnimating = false;
        }

        #endregion

        #region Lock Screen Functions

        public void OnTitleClick()
        {
            if (canAnimateTitle)
            {
                canAnimateTitle = false;
                StartCoroutine(AnimateTitle());
            }
        }

        IEnumerator AnimateTitle()
        {
            yield return lockScreenTitle_Welcome.DOPunchPosition(new Vector3(0, 20, 0), 0.5f, 0, 0f).SetEase(Ease.InOutQuint).WaitForCompletion();
            yield return lockScreenTitle_Back.DOPunchPosition(new Vector3(0, 20, 0), 0.5f, 0, 0f).SetEase(Ease.InOutQuint).WaitForCompletion();
            canAnimateTitle = true;
        }

        public bool UnlockScreen(string pass)
        {
            if (lockScreenPass.Equals(pass))
            {
                homeWindow.SetActive(true);

                lockScreenWindow.transform.DOLocalMoveY(Screen.height, 0.5f).SetEase(Ease.OutSine);
                lockScreenWindow.GetComponent<CanvasGroup>().interactable = false;

                currentOpenPanel = homeWindow;

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Home Functions
        
        IEnumerator OpenPanelAnim(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            rt.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            cg.alpha = 0.3f;

            go.transform.SetSiblingIndex(transform.childCount - 4);
            go.SetActive(true);

            rt.DOScale(1f, 0.3f).SetEase(Ease.OutQuint);
            cg.DOFade(1f, 0.3f).SetEase(Ease.OutQuint).WaitForCompletion();

            currentOpenPanel = go;
            allowPanelAnim = true;

            yield break;
        }

        IEnumerator ClosePanelAnim(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            CanvasGroup cg = go.GetComponent<CanvasGroup>();

            rt.DOScale(0.9f, 0.3f).SetEase(Ease.OutQuint);
            yield return cg.DOFade(0f, 0.3f).SetEase(Ease.OutExpo).WaitForCompletion();

            go.SetActive(false);
            go.transform.SetSiblingIndex(1);
            
            for (int i = transform.childCount - 4; i >= 0; i--)
            {
                if (transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    currentOpenPanel = transform.GetChild(i).gameObject;
                    break;
                }
            }
            allowPanelAnim = true;
        }

        private void OpenPanel(GameObject go)
        {
            if (!allowPanelAnim) return;

            allowPanelAnim = false;
            if (currentOpenPanel == go)
            {
                StartCoroutine(ClosePanelAnim(go));
            }
            else
            {
                StartCoroutine(OpenPanelAnim(go));
            }
        }

        public void OpenSurfBowser()
        {
            OpenPanel(surfBrowserWindow);
        }

        public void OpenNotebook()
        {
            OpenPanel(notebookWindow);
        }

        #endregion
    }
}