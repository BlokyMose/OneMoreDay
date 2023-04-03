using Encore.CharacterControllers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Encore.SceneMasters
{
    [AddComponentMenu("Encore/Scene Masters/Demo/Introduction")]
    public class Demo_Introduction : SceneMaster
    {
        #region [Classes]

        [System.Serializable]
        public class PhotoPair
        {
            public Sprite left;
            public Sprite right;
        }

        #endregion

        #region [Vars: Components]

        [SerializeField]
        List<PhotoPair> photos = new List<PhotoPair>();

        [SerializeField]
        Animator leftHand;

        [SerializeField]
        GameObject leftHandBack;

        [SerializeField]
        Animator rightHand;

        [SerializeField]
        SpriteRenderer leftPhoto;

        [SerializeField]
        SpriteRenderer rightPhoto;

        [SerializeField]
        GameObject cameraOnHand;

        [SerializeField]
        Button nextBut;

        #endregion

        #region [Vars: Data Handlers]

        int photoIndex = 0;

        int boo_show;

        Coroutine corNextBut;

        #endregion


        protected override void Awake()
        {
            base.Awake();
            boo_show = Animator.StringToHash(nameof(boo_show));
            nextBut.onClick.AddListener(NextPhoto);
        }

        void Start()
        {
            rightHand.gameObject.SetActive(false);
            cameraOnHand.SetActive(false);
            leftHand.gameObject.SetActive(false);
            leftHandBack.SetActive(false);
            rightPhoto.sprite = photos[photoIndex].right;
            leftPhoto.sprite = photos[photoIndex].left;
            nextBut.gameObject.SetActive(false);
        }

        public override void Init(InitialSettings settings)
        {
            base.Init(settings);

            GameManager.Instance.EnableUICornerTools(false, false);
        }

        public void StartShowingPhoto()
        {
            GameManager.Instance.Player.SetCanInteract(false, CursorImageManager.CursorImage.Normal);

            StartCoroutine(Delay());
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(2f);
                nextBut.gameObject.SetActive(true);
                yield return new WaitForSeconds(1f);
                NextPhoto();
            }
        }

        public void NextPhoto()
        {
            if (photoIndex < photos.Count)
            {
                if (corNextBut != null) return;

                corNextBut = StartCoroutine(Animate(0.33f));
                IEnumerator Animate(float delay)
                {
                    int photoIndex = this.photoIndex;
                    if (photoIndex != 0)
                    {
                        // Put hands down
                        leftHand.SetBool(boo_show, false);
                        yield return new WaitForSeconds(delay * Random.Range(0.33f, 0.66f));
                        rightHand.SetBool(boo_show, false);
                    }
                    else
                    {
                        rightHand.gameObject.SetActive(true);
                        leftHand.gameObject.SetActive(true);
                    }

                    // Wait for animation to finish
                    yield return new WaitForSeconds(1.2f);
                    rightPhoto.sprite = photos[photoIndex].right;
                    leftPhoto.sprite = photos[photoIndex].left;

                    // Put hands up
                    leftHand.SetBool(boo_show, true);
                    yield return new WaitForSeconds(delay * Random.Range(0.8f, 1.25f));
                    rightHand.SetBool(boo_show, true);

                    corNextBut = null;
                }

                photoIndex++;
            }
            else
            {
                nextBut.enabled = false;
                nextBut.animator.SetTrigger("Disabled");
                ActivateCameraMode();
            }
        }

        void ActivateCameraMode()
        {
            StartCoroutine(Delay());
            IEnumerator Delay()
            {
                // Put hands down
                leftHand.SetBool(boo_show, false);
                yield return new WaitForSeconds(0.33f * Random.Range(0.33f, 0.66f));
                rightHand.SetBool(boo_show, false);
                yield return new WaitForSeconds(1f);
                rightPhoto.gameObject.SetActive(false);
                leftPhoto.gameObject.SetActive(false);

                // Automatically triggers animator to play grab animation, which triggers an event which trigger Relay to deactivate Camera's GO
                leftHandBack.SetActive(true);
                yield return new WaitForSeconds(2.5f);

                // Bring up the camera using right hand
                cameraOnHand.SetActive(true);
                rightHand.SetBool(boo_show, true);
            }

        }
    }
}