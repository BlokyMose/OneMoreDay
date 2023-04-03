using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Encore.MiniGames.Viewers
{
    [AddComponentMenu("Encore/MiniGames/Viewers/Image Viewer")]
    public class ImageViewer : Viewer
    {
        #region [Classes]

        [System.Serializable]
        public class ImageContainer
        {
            [HideLabel]
            public Sprite sprite;

            [HideInInspector]
            public Image image;

            public ImageContainer(Sprite sprite, Image image)
            {
                this.sprite = sprite;
                this.image = image;
            }
        }

        #endregion

        #region [Vars: Components]

        [FoldoutGroup("Components"), SerializeField]
        Transform imagesParent;

        [FoldoutGroup("Components"), SerializeField]
        Button nextBut;

        [FoldoutGroup("Components"), SerializeField]
        Button backBut;

        [FoldoutGroup("Components"), SerializeField]
        Slider zoomSlider;

        #endregion

        #region [Vars: Properties]

        [FoldoutGroup("Properties"), SerializeField]
        bool isFollowingCursor = true;

        [FoldoutGroup("Properties"), SerializeField]
        List<ImageContainer> images;

        #endregion

        #region [Vars: Data Handlers]

        const int SPACE_BETWEEN_IMAGES = 1920;
        int currentImageIndex = 0;
        const int SPEED = 10;
        const float FOLLOW_CURSOR_MARGIN_MAX = 0.1f;
        const float FOLLOW_CURSOR_MARGIN_MIN = 0.03f;
        ImageContainer currentImage;
        Tweener moveImagesParentTweener;
        Coroutine corHideLastImage;
        Coroutine corShowCurrentImage;
        Coroutine corFollowingCursor;
        float followCursorRatio;

        #endregion

        #region [Methods: Unity]

        protected override void Awake()
        {
            base.Awake();

            #region [Setup Buttons]

            nextBut.onClick.AddListener(OnClickNextBut);
            backBut.onClick.AddListener(OnClickBackBut);
            zoomSlider.onValueChanged.AddListener(OnValueChangedZoomSlider);

            void OnClickNextBut()
            {
                GoToImage(1);
            }

            void OnClickBackBut()
            {
                GoToImage(-1);
            }

            void OnValueChangedZoomSlider(float value)
            {
                Zoom(value);
            }

            #endregion

            // Clean parent
            for (int i = imagesParent.childCount - 1; i >= 0; i--)
                Destroy(imagesParent.GetChild(i).gameObject);

            InstantiateImageContainers();
        }

        #endregion

        #region [Methods: Main]

        public override void Show(bool isShowing)
        {
            base.Show(isShowing);

            if (isShowing)
            {
                if (GameManager.Instance.Player == null) return;

                ResetProperties();
                GoToImage(0);
                if (isFollowingCursor)
                    corFollowingCursor = StartCoroutine(ContentFollowsMouse());
                IEnumerator ContentFollowsMouse()
                {
                    while (true)
                    {
                        yield return null;
                        Vector2 mousePos = GameManager.Instance.Player.MouseManager.CursorImageManager.transform.localPosition;

                        currentImage.image.transform.localPosition = Vector3.Lerp(
                            currentImage.image.transform.localPosition,
                            new Vector3(mousePos.x * followCursorRatio, mousePos.y * followCursorRatio, 0),
                            Time.deltaTime * SPEED);
                    }
                }
            }
            else
            {
                if (corFollowingCursor != null) StopCoroutine(corFollowingCursor);
            }
        }

        void InstantiateImageContainers()
        {
            int index = 0;
            foreach (var image in images)
            {
                int _index = index;
                GameObject imageParent = new GameObject(_index.ToString());
                imageParent.transform.parent = imagesParent;
                imageParent.transform.localPosition = new Vector2(SPACE_BETWEEN_IMAGES * _index, 0);

                GameObject imageGO = new GameObject("Image");
                imageGO.transform.parent = imageParent.transform;
                imageGO.transform.localPosition = Vector3.zero;

                Image imageComponent = imageGO.AddComponent<Image>();
                imageComponent.sprite = image.sprite;
                imageComponent.preserveAspect = true;
                image.image = imageComponent;

                RectTransform rect = imageGO.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(1270, 920);

                index++;
            }
        }

        void ResetProperties()
        {
            // Reset settings
            foreach (var image in images) image.image.color = new Color(1, 1, 1, 0);
            imagesParent.localPosition = Vector3.zero;
            zoomSlider.value = 0;

            // Reset data handlers
            currentImage = null;
            currentImageIndex = 0;
            followCursorRatio = FOLLOW_CURSOR_MARGIN_MIN;

            // Reset coroutines
            corShowCurrentImage = null;
            corHideLastImage = null;
            corFollowingCursor = null;
            corDeactivating = null;

            if (moveImagesParentTweener != null && moveImagesParentTweener.IsActive())
                moveImagesParentTweener.Kill();

        }

        void GoToImage(int addIndex)
        {
            currentImageIndex += addIndex;

            // Validate
            if (currentImageIndex < 0)
            {
                currentImageIndex = 0;
                return;
            }
            else if (currentImageIndex > images.Count - 1)
            {
                currentImageIndex = images.Count - 1;
                return;
            }

            // Activation of back and next buttons
            if (images.Count == 1)
            {
                backBut.gameObject.SetActive(false);
                nextBut.gameObject.SetActive(false);
            }
            else
            {
                if (currentImageIndex == 0)
                {
                    backBut.gameObject.SetActive(false);
                    nextBut.gameObject.SetActive(true);
                }
                else if (currentImageIndex == images.Count - 1)
                {
                    backBut.gameObject.SetActive(true);
                    nextBut.gameObject.SetActive(false);
                }
                else
                {
                    backBut.gameObject.SetActive(true);
                    nextBut.gameObject.SetActive(true);
                }
            }

            // Reset old image, set new image
            if (currentImage != null)
            {
                if (corHideLastImage != null)
                {
                    StopCoroutine(corHideLastImage);
                    foreach (var image in images)
                    {
                        image.image.color = new Color(image.image.color.r, image.image.color.g, image.image.color.b, 0);
                    }
                }
                if (corShowCurrentImage != null)
                {
                    StopCoroutine(corShowCurrentImage);
                    currentImage.image.color = new Color(currentImage.image.color.r, currentImage.image.color.g, currentImage.image.color.b, 1);
                }
                currentImage.image.transform.localPosition = Vector3.zero;
            }

            currentImage = images[currentImageIndex];

            corHideLastImage = StartCoroutine(HideImagesExcept(currentImage.image));
            corShowCurrentImage = StartCoroutine(ShowImage(currentImage.image));

            // Move parent
            if (moveImagesParentTweener != null && moveImagesParentTweener.IsActive())
                moveImagesParentTweener.Kill();
            moveImagesParentTweener = imagesParent.DOLocalMoveX(-1 * currentImageIndex * SPACE_BETWEEN_IMAGES, 1);
            moveImagesParentTweener.SetEase(Ease.OutQuad);

            IEnumerator HideImagesExcept(Image exceptImage)
            {
                AnimationCurve curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
                float time = 0;
                while (time < 1)
                {
                    foreach (var image in images)
                    {
                        if (image.image != exceptImage)
                        {
                            image.image.color = new Color(image.image.color.r, image.image.color.g, image.image.color.b, curve.Evaluate(time));
                        }
                    }
                    time += Time.deltaTime;
                    yield return null;
                }
            }

            IEnumerator ShowImage(Image image)
            {
                AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                float time = 0;
                while (image.color.a < 1)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, curve.Evaluate(time));
                    time += Time.deltaTime;
                    yield return null;
                }
            }
        }

        void Zoom(float addZoom)
        {
            followCursorRatio = addZoom / zoomSlider.maxValue * FOLLOW_CURSOR_MARGIN_MAX;
            if (followCursorRatio <= 0) followCursorRatio = FOLLOW_CURSOR_MARGIN_MIN;

            foreach (var image in images)
            {
                image.image.transform.localScale = new Vector3(1f + addZoom, 1f + addZoom, 1);
            }
        }

        #endregion

    }
}
