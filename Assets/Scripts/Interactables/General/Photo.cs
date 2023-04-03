using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Photo")]
    public class Photo : Interactable
    {
        [SerializeField, Title("External UI Component: ", "Require: CanvasPhoto, BG, photo, photo_back")]
        bool manuallyAssign;

        [ShowIf(nameof(manuallyAssign)),SerializeField]
        Animator CanvasPhoto;
        [ShowIf(nameof(manuallyAssign)),SerializeField]
        GameObject BG;
        [ShowIf(nameof(manuallyAssign)),SerializeField]
        GameObject photo;
        [ShowIf(nameof(manuallyAssign)),SerializeField]
        GameObject photo_back;
        [ShowIf(nameof(manuallyAssign)),SerializeField]
        Transform Content;
        [ShowIf(nameof(manuallyAssign)), SerializeField]
        Image zoomIn;
        [ShowIf(nameof(manuallyAssign)), SerializeField]
        Image zoomOut;

        [Title("Properties: ")]
        [SerializeField] bool spinnable = true;
        [SerializeField] bool zoomable = true;
        [SerializeField, ShowIf(nameof(zoomable))] Vector2 zoomMinMax = new Vector2(-1, 4);
        [SerializeField] bool followCursor = true;
        [Tooltip("Do not auto hide Photo UI if the value is below 0")]
        [SerializeField] float autoHideRadius = 15;

        // Data Handlers
        int boo_show, boo_front;
        bool canSpinPhoto = false;
        int speed = 10;
        float followCursorMargin = 0.03f;
        int currentZoom = 0;
        bool isShowing = false;
        private bool canShow = true;
        public bool CanShow { get { return canShow; } set { if (isShowing) Show(false); canShow = value; } }

        protected override void Awake()
        {
            base.Awake();
            if (!manuallyAssign)
            {
                CanvasPhoto = transform.Find(nameof(CanvasPhoto)).GetComponent<Animator>();
                BG = CanvasPhoto.transform.Find(nameof(BG)).gameObject;
                Content = CanvasPhoto.transform.Find("Photos").transform.GetChild(0).transform;
                photo = Content.transform.Find(nameof(photo)).gameObject;
                photo_back = Content.transform.Find(nameof(photo_back)).gameObject;

                zoomIn = CanvasPhoto.transform.Find("Buttons").Find(nameof(zoomIn)).GetComponent<Image>();
                zoomOut = CanvasPhoto.transform.Find("Buttons").Find(nameof(zoomOut)).GetComponent<Image>();
            }

            #region [Setup event triggers]

            #region [BG]

            EventTrigger bg_ET = BG.AddComponent<EventTrigger>();

            EventTrigger.Entry bg_entry_enter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            bg_entry_enter.callback.AddListener((data) =>
            {
                GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Exit);
            });
            bg_ET.triggers.Add(bg_entry_enter);

            EventTrigger.Entry bg_entry_click = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            bg_entry_click.callback.AddListener((data) => { Show(false); });
            bg_ET.triggers.Add(bg_entry_click);

            #endregion

            #region [Spinnable feature]

            if (spinnable)
            {
                // photo's EventTrigger
                EventTrigger photo_ET = photo.AddComponent<EventTrigger>();

                EventTrigger.Entry photo_entry_enter = new EventTrigger.Entry();
                photo_entry_enter.eventID = EventTriggerType.PointerEnter;
                photo_entry_enter.callback.AddListener((data) => { GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Spin); });
                photo_ET.triggers.Add(photo_entry_enter);

                EventTrigger.Entry photo_entry_click = new EventTrigger.Entry();
                photo_entry_click.eventID = EventTriggerType.PointerClick;
                photo_entry_click.callback.AddListener(((data) => { if (this.canSpinPhoto) CanvasPhoto.SetBool(boo_front, false); }));
                photo_ET.triggers.Add(photo_entry_click);

                // photo_back's EventTrigger
                EventTrigger photo_back_ET = photo_back.AddComponent<EventTrigger>();

                EventTrigger.Entry photo_back_entry_enter = new EventTrigger.Entry();
                photo_back_entry_enter.eventID = EventTriggerType.PointerEnter;
                photo_back_entry_enter.callback.AddListener((data) => { GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Spin); });
                photo_back_ET.triggers.Add(photo_back_entry_enter);

                EventTrigger.Entry photo_back_entry_click = new EventTrigger.Entry();
                photo_back_entry_click.eventID = EventTriggerType.PointerClick;
                photo_back_entry_click.callback.AddListener(((data) => { if (this.canSpinPhoto) CanvasPhoto.SetBool(boo_front, true); }));
                photo_back_ET.triggers.Add(photo_back_entry_click);
            }

            else
            {
                // photo's EventTrigger
                EventTrigger photo_ET = photo.AddComponent<EventTrigger>();

                EventTrigger.Entry photo_entry_enter = new EventTrigger.Entry();
                photo_entry_enter.eventID = EventTriggerType.PointerEnter;
                photo_entry_enter.callback.AddListener((data) => { GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Normal); });
                photo_ET.triggers.Add(photo_entry_enter);
            }

            #endregion

            #region [Zoomable feature]

            if (zoomable)
            {
                #region [ZoomIn]

                TextMeshProUGUI zoomIn_text = zoomIn.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                EventTrigger zoomIn_ET = zoomIn.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry zoomIn_entry_enter = new EventTrigger.Entry();
                zoomIn_entry_enter.eventID = EventTriggerType.PointerEnter;
                zoomIn_entry_enter.callback.AddListener((data) =>
                {
                    GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Normal);
                    ChangeButtonColor(zoomIn,zoomIn_text,true);
                });
                zoomIn_ET.triggers.Add(zoomIn_entry_enter);

                EventTrigger.Entry zoomIn_entry_click = new EventTrigger.Entry();
                zoomIn_entry_click.eventID = EventTriggerType.PointerClick;
                zoomIn_entry_click.callback.AddListener((data) =>
                {
                    if(currentZoom < zoomMinMax.y)
                    {
                        Content.localScale += new Vector3(0.15f, 0.15f, 0.15f);
                        followCursorMargin += 0.1f;
                        currentZoom++;
                    }
                });
                zoomIn_ET.triggers.Add(zoomIn_entry_click);

                EventTrigger.Entry zoomIn_entry_exit = new EventTrigger.Entry();
                zoomIn_entry_exit.eventID = EventTriggerType.PointerExit;
                zoomIn_entry_exit.callback.AddListener((data) =>
                {
                    ChangeButtonColor(zoomIn,zoomIn_text,false);
                });
                zoomIn_ET.triggers.Add(zoomIn_entry_exit);

                #endregion

                #region [ZoomOut]

                TextMeshProUGUI zoomOut_text = zoomOut.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                EventTrigger zoomOut_ET = zoomOut.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry zoomOut_entry_enter = new EventTrigger.Entry();
                zoomOut_entry_enter.eventID = EventTriggerType.PointerEnter;
                zoomOut_entry_enter.callback.AddListener((data) =>
                {
                    GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Normal);
                    ChangeButtonColor(zoomOut, zoomOut_text, true);
                });
                zoomOut_ET.triggers.Add(zoomOut_entry_enter);

                EventTrigger.Entry zoomOut_entry_click = new EventTrigger.Entry();
                zoomOut_entry_click.eventID = EventTriggerType.PointerClick;
                zoomOut_entry_click.callback.AddListener((data) =>
                {
                    if(currentZoom > zoomMinMax.x)
                    {
                        Content.localScale -= new Vector3(0.15f, 0.15f, 0.15f);
                        followCursorMargin -= 0.1f;
                        currentZoom--;
                    }
                });
                zoomOut_ET.triggers.Add(zoomOut_entry_click);

                EventTrigger.Entry zoomOut_entry_exit = new EventTrigger.Entry();
                zoomOut_entry_exit.eventID = EventTriggerType.PointerExit;
                zoomOut_entry_exit.callback.AddListener((data) =>
                {
                    ChangeButtonColor(zoomOut, zoomOut_text, false);
                });
                zoomOut_ET.triggers.Add(zoomOut_entry_exit);

                #endregion

                void ChangeButtonColor(Image button, TextMeshProUGUI text, bool isHovered)
                {
                    button.color = new Color(zoomIn.color.r, zoomIn.color.g, zoomIn.color.b, isHovered ? 0.8f : 0.33f);
                    text.color = new Color(zoomIn_text.color.r, zoomIn_text.color.g, zoomIn_text.color.b, isHovered ? 0.8f : 0.5f);
                }
            }

            else
            {
                zoomIn.gameObject.SetActive(false);
                zoomOut.gameObject.SetActive(false);
            }


            #endregion

            #endregion

            CanvasPhoto.gameObject.SetActive(true);
            boo_show = Animator.StringToHash(nameof(boo_show));
            boo_front = Animator.StringToHash(nameof(boo_front));
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return CursorImageManager.CursorImage.Look;
        }

        private void Start()
        {
            Show(false);
        }

        protected override void InteractModule(GameObject interactor)
        {
            // HARDCODE:    MouseClick can be detected after a few mili second(?)
            //              ShowCanvas right after a click will prompt PointerClick in Awake which executes ShowCanvas(false) as well;
            StartCoroutine(Delay(0.15f));
            IEnumerator Delay(float delay)
            {
                yield return new WaitForSeconds(delay);
                Show(true);
            }

            if (autoHideRadius > 0)
            {
                StartCoroutine(AutoHide());
                IEnumerator AutoHide()
                {
                    float leftBorder = 0, rightBorder = 0;
                    Vector3 interactedPos = GameManager.Instance.Player.transform.position;
                    if(interactedPos.x < transform.position.x)
                    {
                        leftBorder = interactedPos.x - autoHideRadius;
                        rightBorder = transform.position.x + autoHideRadius;
                    }
                    else
                    {
                        leftBorder = transform.position.x - autoHideRadius;
                        rightBorder = interactedPos.x + autoHideRadius;
                    }

                    while (true)
                    {
                        Vector3 playerPos = GameManager.Instance.Player.transform.position;
                        if (playerPos.x < leftBorder || playerPos.x > rightBorder )
                        {
                            Show(false);
                            break;
                        }

                        yield return null;
                    }
                }

            }
        }

        public void Show(bool isShowing)
        {
            if (!canShow) return;

            if (GameManager.Instance.Player != null)
            {
                GameManager.Instance.Player.SetCanInteract(!isShowing);

                if (isShowing)
                {
                    GameManager.Instance.Player.SetCanInteract(false);
                    ResetProperties();

                    if (followCursor) StartCoroutine(ContentFollowsMouse());
                    IEnumerator ContentFollowsMouse()
                    {
                        while (true)
                        {
                            yield return null;
                            Vector2 mousePos = GameManager.Instance.Player.MouseManager.CursorImageManager.transform.localPosition;

                            Content.localPosition = Vector3.Lerp(
                                Content.localPosition,
                                new Vector3(mousePos.x * followCursorMargin - currentZoom, mousePos.y * followCursorMargin - currentZoom, 0),
                                Time.deltaTime * speed);
                        }
                    }
                }
                else
                {
                    GameManager.Instance.Player.SetCanInteract(true);
                    StopAllCoroutines();
                }

            }

            CanvasPhoto.SetBool(boo_show, isShowing);
            BG.GetComponent<CanvasGroup>().blocksRaycasts = isShowing;
            photo.GetComponent<CanvasGroup>().blocksRaycasts = isShowing;
            photo_back.GetComponent<CanvasGroup>().blocksRaycasts = isShowing;


            StartCoroutine(DelayCanSpin(1f));
            IEnumerator DelayCanSpin(float delay)
            {
                yield return new WaitForSeconds(delay);
                canSpinPhoto = isShowing;
            }
        }

        void ResetProperties()
        {
            CanvasPhoto.SetBool(boo_front, true);
            currentZoom = 0;
            Content.localScale = new Vector3(1, 1, 1);
            followCursorMargin = 0.03f;
        }
    }
}
