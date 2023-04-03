using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using Encore.Locations;
using Encore.Phone.Chat;
using Encore.Phone.ToDo;
using Encore.SceneMasters;
using Encore.CharacterControllers;
using Encore.Phone.Map;

namespace Encore.Phone
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Encore/Phone/Phone Manager")]
    public class PhoneManager : UICornerTool
    {
        #region [Classes]

        public enum PhoneAppName { None, Chat, ToDo, Social, Camera, Map, Settings, Quit }

        public class NotificationData
        {
            PhoneAppName appName;
            string time;
            string title;
            string desc;
            Sprite image;
            AudioClip sound;

            public PhoneAppName AppName { get { return appName; } }
            public string Time { get { return time; } }
            public string Title { get { return title; } }
            public string Desc { get { return desc; } }
            public Sprite Image { get { return image; } }
            public AudioClip Sound { get { return sound; } }

            public NotificationData(PhoneAppName appName, string time, string title, string desc, Sprite image, AudioClip sound)
            {
                this.appName = appName;
                this.time = time;
                this.title = title;
                this.desc = desc;
                this.image = image;
                this.sound = sound;
            }
        }

        class NotificationDocker
        {
            public Serializables.NotificationData notificationData;
            public GameObject go;
            public GameObject notificationMini;

            public NotificationDocker(Serializables.NotificationData notificationData, GameObject go, GameObject notificationMini)
            {
                this.notificationData = notificationData;
                this.go = go;
                this.notificationMini = notificationMini;
            }
        }

        #endregion

        #region [Vars: Components]

        [SerializeField, SuffixLabel("Find PhoneBody")]
        CanvasGroup phoneBody;

        [Header("Homescreen Icons")]
        [SerializeField] Image chatIcon;
        [SerializeField] Image todoIcon;
        [SerializeField] Image socialIcon;
        [SerializeField] Image cameraIcon;
        [SerializeField] Image mapIcon;
        [SerializeField] Image settingsIcon;
        [SerializeField] Image quitIcon;

        [Header("Apps")]
        [SerializeField]
        List<PhoneApp> apps = new List<PhoneApp>();

        public ChatApp ChatApp
        {
            get
            {
                foreach (var app in apps) if (app is ChatApp) return app as ChatApp;
                return null;
            } 
        }

        public ToDoApp ToDoApp
        {
            get
            {
                foreach (var app in apps) if (app is ToDoApp) return app as ToDoApp;
                return null;
            }
        }

        public MapApp MapApp
        {
            get
            {
                foreach (var app in apps) if (app is MapApp) return app as MapApp;
                return null;
            }
        }

        [SerializeField] CanvasGroup socialCanvas;
        [SerializeField] CanvasGroup cameraCanvas;
        [SerializeField] CanvasGroup settingsCanvas;
        [SerializeField] CanvasGroup quitCanvas;

        [Header("Docker")]
        [SerializeField] Animator dockerAnimator;
        [SerializeField] Image dockerHeader;
        [SerializeField] Image dockerDragger;
        [SerializeField] Image dockerHitHidePanel;
        [SerializeField] Transform notificationDockerList;
        [SerializeField] Transform notificationMiniList;

        [Header("Doker Header")]
        [SerializeField] TextMeshProUGUI providerText;
        [SerializeField] Image networkStatusImage;
        [SerializeField] TextMeshProUGUI locationText;
        [SerializeField] TextMeshProUGUI batteryText;
        [SerializeField] Image batteryIcon;

        [Header("Other")]
        [SerializeField] Image backButton;
        [SerializeField] Transform notificationList;

        [Header("Prefabs")]
        [SerializeField] GameObject notificationPanelPrefab;
        [SerializeField] GameObject notificationDockerPanelPrefab;

        #endregion

        #region [Vars: Data Handlers]

        public static readonly string PHONE_MANAGER_SAVE_KEY = "Notification";

        AudioSource audioSource;
        PhoneAppName currentAppName = PhoneAppName.None;

        float notificationHideDelay = 5;
        int int_app, boo_show_notif, boo_hovered, int_notif_count;

        const int NOTIF_DOCKER_MAX_COUNT = 5;

        List<NotificationDocker> notifDockers = new List<NotificationDocker>();

        Dictionary<PhoneAppName, Sprite> appNotifIcons = new Dictionary<PhoneAppName, Sprite>();

        #endregion

        #region [Methods: Unity]

        protected override void Awake()
        {
            base.Awake();

            #region [Get external components]

            audioSource = GetComponent<AudioSource>();
            if (!phoneBody) phoneBody = transform.Find("CanvasPhone").GetComponent<CanvasGroup>();

            #endregion

            #region [Get appNotifIcons]

            foreach (var app in apps)
                appNotifIcons.Add(GetAppName(app), app.NotificationIcon);

            #endregion

            #region [Animator's parameters]

            int_app = Animator.StringToHash(nameof(int_app));
            boo_show_notif = Animator.StringToHash(nameof(boo_show));
            int_notif_count = Animator.StringToHash(nameof(int_notif_count));
            boo_hovered = Animator.StringToHash(nameof(boo_hovered));

            ShowDocker(false);

            #endregion

            #region [Homescreen : Event Triggers]

            #region [Chat]

            EventTrigger chat_et = chatIcon.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry chat_et_entry_enter = new EventTrigger.Entry();
            chat_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            chat_et_entry_enter.callback.AddListener((data) => { MakeBig(chatIcon, true); });
            chat_et.triggers.Add(chat_et_entry_enter);

            EventTrigger.Entry chat_et_entry_exit = new EventTrigger.Entry();
            chat_et_entry_exit.eventID = EventTriggerType.PointerExit;
            chat_et_entry_exit.callback.AddListener((data) => { MakeBig(chatIcon, false); });
            chat_et.triggers.Add(chat_et_entry_exit);


            EventTrigger.Entry chat_et_entry_click = new EventTrigger.Entry();
            chat_et_entry_click.eventID = EventTriggerType.PointerClick;
            chat_et_entry_click.callback.AddListener((data) => { MakeBig(chatIcon, false); OpenApp(PhoneAppName.Chat); });
            chat_et.triggers.Add(chat_et_entry_click);

            #endregion

            #region [ToDo]

            EventTrigger todo_et = todoIcon.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry todo_et_entry_enter = new EventTrigger.Entry();
            todo_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            todo_et_entry_enter.callback.AddListener((data) => { MakeBig(todoIcon, true); });
            todo_et.triggers.Add(todo_et_entry_enter);

            EventTrigger.Entry todo_et_entry_exit = new EventTrigger.Entry();
            todo_et_entry_exit.eventID = EventTriggerType.PointerExit;
            todo_et_entry_exit.callback.AddListener((data) => { MakeBig(todoIcon, false); });
            todo_et.triggers.Add(todo_et_entry_exit);


            EventTrigger.Entry todo_et_entry_click = new EventTrigger.Entry();
            todo_et_entry_click.eventID = EventTriggerType.PointerClick;
            todo_et_entry_click.callback.AddListener((data) => { MakeBig(todoIcon, false); OpenApp(PhoneAppName.ToDo); });
            todo_et.triggers.Add(todo_et_entry_click);

            #endregion

            #region [Map]

            EventTrigger map_et = mapIcon.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry map_et_entry_enter = new EventTrigger.Entry();
            map_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            map_et_entry_enter.callback.AddListener((data) => { MakeBig(mapIcon, true); });
            map_et.triggers.Add(map_et_entry_enter);

            EventTrigger.Entry map_et_entry_exit = new EventTrigger.Entry();
            map_et_entry_exit.eventID = EventTriggerType.PointerExit;
            map_et_entry_exit.callback.AddListener((data) => { MakeBig(mapIcon, false); });
            map_et.triggers.Add(map_et_entry_exit);


            EventTrigger.Entry map_et_entry_click = new EventTrigger.Entry();
            map_et_entry_click.eventID = EventTriggerType.PointerClick;
            map_et_entry_click.callback.AddListener((data) => { MakeBig(mapIcon, false); OpenApp(PhoneAppName.Map); });
            map_et.triggers.Add(map_et_entry_click);

            #endregion

            #region [Social]

            EventTrigger social_et = socialIcon.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry social_et_entry_enter = new EventTrigger.Entry();
            social_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            social_et_entry_enter.callback.AddListener((data) => { MakeBig(socialIcon, true); });
            social_et.triggers.Add(social_et_entry_enter);

            EventTrigger.Entry social_et_entry_exit = new EventTrigger.Entry();
            social_et_entry_exit.eventID = EventTriggerType.PointerExit;
            social_et_entry_exit.callback.AddListener((data) => { MakeBig(socialIcon, false); });
            social_et.triggers.Add(social_et_entry_exit);


            EventTrigger.Entry social_et_entry_click = new EventTrigger.Entry();
            social_et_entry_click.eventID = EventTriggerType.PointerClick;
            social_et_entry_click.callback.AddListener((data) => { MakeBig(socialIcon, false); });
            social_et.triggers.Add(social_et_entry_click);

            #endregion

            #region [Camera]

            EventTrigger camera_et = cameraIcon.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry camera_et_entry_enter = new EventTrigger.Entry();
            camera_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            camera_et_entry_enter.callback.AddListener((data) => { MakeBig(cameraIcon, true); });
            camera_et.triggers.Add(camera_et_entry_enter);

            EventTrigger.Entry camera_et_entry_exit = new EventTrigger.Entry();
            camera_et_entry_exit.eventID = EventTriggerType.PointerExit;
            camera_et_entry_exit.callback.AddListener((data) => { MakeBig(cameraIcon, false); });
            camera_et.triggers.Add(camera_et_entry_exit);


            EventTrigger.Entry camera_et_entry_click = new EventTrigger.Entry();
            camera_et_entry_click.eventID = EventTriggerType.PointerClick;
            camera_et_entry_click.callback.AddListener((data) => { MakeBig(cameraIcon, false); });
            camera_et.triggers.Add(camera_et_entry_click);

            #endregion

            #region [Settings]

            EventTrigger settings_et = settingsIcon.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry settings_et_entry_enter = new EventTrigger.Entry();
            settings_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            settings_et_entry_enter.callback.AddListener((data) => { MakeBig(settingsIcon, true); });
            settings_et.triggers.Add(settings_et_entry_enter);

            EventTrigger.Entry settings_et_entry_exit = new EventTrigger.Entry();
            settings_et_entry_exit.eventID = EventTriggerType.PointerExit;
            settings_et_entry_exit.callback.AddListener((data) => { MakeBig(settingsIcon, false); });
            settings_et.triggers.Add(settings_et_entry_exit);


            EventTrigger.Entry settings_et_entry_click = new EventTrigger.Entry();
            settings_et_entry_click.eventID = EventTriggerType.PointerClick;
            settings_et_entry_click.callback.AddListener((data) => { MakeBig(settingsIcon, false); });
            settings_et.triggers.Add(settings_et_entry_click);

            #endregion

            #region [Quit]

            EventTrigger quit_et = quitIcon.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry quit_et_entry_enter = new EventTrigger.Entry();
            quit_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            quit_et_entry_enter.callback.AddListener((data) => { MakeBig(quitIcon, true); });
            quit_et.triggers.Add(quit_et_entry_enter);

            EventTrigger.Entry quit_et_entry_exit = new EventTrigger.Entry();
            quit_et_entry_exit.eventID = EventTriggerType.PointerExit;
            quit_et_entry_exit.callback.AddListener((data) => { MakeBig(quitIcon, false); });
            quit_et.triggers.Add(quit_et_entry_exit);


            EventTrigger.Entry quit_et_entry_click = new EventTrigger.Entry();
            quit_et_entry_click.eventID = EventTriggerType.PointerClick;
            quit_et_entry_click.callback.AddListener((data) => { MakeBig(quitIcon, false); });
            quit_et.triggers.Add(quit_et_entry_click);

            #endregion

            #endregion

            #region [BackButton : Event Triggers]

            EventTrigger backButton_et = backButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry backButton_et_entry_enter = new EventTrigger.Entry();
            backButton_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            backButton_et_entry_enter.callback.AddListener((data) => { MakeBig(backButton, true); });
            backButton_et.triggers.Add(backButton_et_entry_enter);

            EventTrigger.Entry backButton_et_entry_exit = new EventTrigger.Entry();
            backButton_et_entry_exit.eventID = EventTriggerType.PointerExit;
            backButton_et_entry_exit.callback.AddListener((data) => { MakeBig(backButton, false); });
            backButton_et.triggers.Add(backButton_et_entry_exit);


            EventTrigger.Entry backButton_et_entry_click = new EventTrigger.Entry();
            backButton_et_entry_click.eventID = EventTriggerType.PointerClick;
            backButton_et_entry_click.callback.AddListener((data) => { MakeBig(backButton, false); Back(); });
            backButton_et.triggers.Add(backButton_et_entry_click);

            #endregion

            #region [Docker : EventTriggers]

            #region [Header]

            EventTrigger docker_header_et = dockerHeader.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry docker_entry_enter = new EventTrigger.Entry();
            docker_entry_enter.eventID = EventTriggerType.PointerEnter;
            docker_entry_enter.callback.AddListener((data) =>
            {
                dockerAnimator.SetBool(boo_hovered, true);
            });
            docker_header_et.triggers.Add(docker_entry_enter);

            EventTrigger.Entry docker_header_entry_exit = new EventTrigger.Entry();
            docker_header_entry_exit.eventID = EventTriggerType.PointerExit;
            docker_header_entry_exit.callback.AddListener((data) =>
            {
                dockerAnimator.SetBool(boo_hovered, false);
            });
            docker_header_et.triggers.Add(docker_header_entry_exit);

            EventTrigger.Entry docker_header_entry_click = new EventTrigger.Entry();
            docker_header_entry_click.eventID = EventTriggerType.PointerClick;
            docker_header_entry_click.callback.AddListener((data) =>
            {
                if (dockerAnimator.GetBool(boo_hovered))
                    ShowDocker(!dockerAnimator.GetBool(boo_show));
            });
            docker_header_et.triggers.Add(docker_header_entry_click);

            #endregion

            #region [Dragger]

            EventTrigger dockerDragger_header_et = dockerDragger.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry dockerDragger_entry_click = new EventTrigger.Entry();
            dockerDragger_entry_click.eventID = EventTriggerType.PointerClick;
            dockerDragger_entry_click.callback.AddListener((data) =>
            {
                ShowDocker(false);
            });
            dockerDragger_header_et.triggers.Add(dockerDragger_entry_click);

            EventTrigger.Entry dockerDragger_entry_enter = new EventTrigger.Entry();
            dockerDragger_entry_enter.eventID = EventTriggerType.PointerEnter;
            dockerDragger_entry_enter.callback.AddListener((data) =>
            {
                dockerDragger.color = new Color(0, 0, 0, 0.3f);
            });
            dockerDragger_header_et.triggers.Add(dockerDragger_entry_enter);

            EventTrigger.Entry dockerDragger_entry_exit = new EventTrigger.Entry();
            dockerDragger_entry_exit.eventID = EventTriggerType.PointerExit;
            dockerDragger_entry_exit.callback.AddListener((data) =>
            {
                dockerDragger.color = new Color(0, 0, 0, 0);
            });
            dockerDragger_header_et.triggers.Add(dockerDragger_entry_exit);

            #endregion

            #region [Hit Hide Panel]

            EventTrigger dockerHitHidePanel_header_et = dockerHitHidePanel.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry dockerHitHidePanel_entry_click = new EventTrigger.Entry();
            dockerHitHidePanel_entry_click.eventID = EventTriggerType.PointerClick;
            dockerHitHidePanel_entry_click.callback.AddListener((data) =>
            {
                ShowDocker(false);
            });
            dockerHitHidePanel_header_et.triggers.Add(dockerHitHidePanel_entry_click);

            #endregion

            #endregion

            void MakeBig(Image image, bool toBig)
            {
                if (toBig)
                    image.transform.localScale = new Vector2(1.15f, 1.15f);
                else
                    image.transform.localScale = new Vector2(1f, 1f);
            }
        }

        void OnEnable()
        {
            foreach (var app in apps)
                app.OnAddNotification += AddNotification;
        }

        void OnDisable()
        {
            foreach (var app in apps)
                app.OnAddNotification -= AddNotification;
        }

        #endregion

        #region [Methods: Main]

        void ShowDocker(bool isShowing)
        {
            dockerAnimator.SetBool(boo_show, isShowing);
            dockerHitHidePanel.gameObject.SetActive(isShowing);
            if (!isShowing) dockerDragger.color = new Color(0, 0, 0, 0);
        }

        void OpenApp(PhoneAppName app)
        {
            currentAppName = app;
            animator.SetInteger(int_app, (int)currentAppName);

            if (app == PhoneAppName.None)
            {
                backButton.raycastTarget = false;
                backButton.color = new Color(1, 1, 1, 0);
                foreach (var _app in apps) _app.Show(false);
                socialCanvas.interactable = false;
                socialCanvas.blocksRaycasts = false;
                cameraCanvas.interactable = false;
                cameraCanvas.blocksRaycasts = false;
                settingsCanvas.interactable = false;
                settingsCanvas.blocksRaycasts = false;
                quitCanvas.interactable = false;
                quitCanvas.blocksRaycasts = false;
            }
            else
            {
                CurrentApp.Show(true);
                StartCoroutine(Delay(0.33f));
            }

            IEnumerator Delay(float delay)
            {
                yield return new WaitForSeconds(delay);
                backButton.raycastTarget = true;
                backButton.color = Color.white;
            }

        }

        void Back()
        {
            if(currentAppName != PhoneAppName.None)
            {
                CurrentApp.Show(false);
            }

            OpenApp(PhoneAppName.None);
        }

        public override void Show(bool isShowing)
        {
            base.Show(isShowing);

            if (!canShow) return;

            GameManager.Instance.Player.SetCanInteract(!isShowing, CursorImageManager.CursorImage.Normal);
            var controller = GameManager.Instance.Player.PlayerController;
            if (controller != null)
                controller.RigControllerPlayer?.ActivatePhone(isShowing);

            phoneBody.interactable = isShowing;
            phoneBody.blocksRaycasts = isShowing;

            if (!isShowing)
            {
                ShowDocker(false);
                OpenApp(PhoneAppName.None);
            }
        }

        void AddNotification(NotificationData data)
        {
            #region [Instantiate Notification]

            GameObject notificationGO = Instantiate(notificationPanelPrefab, notificationList);
            notificationGO.transform.Find("HeaderPanel").Find("Icon").GetComponent<Image>().sprite = GetAppNotifIcon(data.AppName);
            notificationGO.transform.Find("HeaderPanel").Find("AppName").GetComponent<TextMeshProUGUI>().color = GetAppColor(data.AppName);
            notificationGO.transform.Find("HeaderPanel").Find("AppName").GetComponent<TextMeshProUGUI>().text = data.AppName.ToString();
            notificationGO.transform.Find("HeaderPanel").Find("Time").GetComponent<TextMeshProUGUI>().text = data.Time;

            notificationGO.transform.Find("BodyPanel").Find("Image").GetComponent<Image>().sprite = data.Image;
            notificationGO.transform.Find("BodyPanel").Find("Texts").Find("Title").GetComponent<TextMeshProUGUI>().text = data.Title;
            notificationGO.transform.Find("BodyPanel").Find("Texts").Find("Desc").GetComponent<TextMeshProUGUI>().text = data.Desc;

            #endregion

            AddNotificationDocker(new Serializables.NotificationData(data));

            audioSource.clip = data.Sound;
            audioSource.Play();

            StartCoroutine(DelayDeletion(notificationHideDelay));
            IEnumerator DelayDeletion(float delay)
            {
                yield return new WaitForSeconds(delay);
                notificationGO.GetComponent<Animator>().SetBool(boo_show_notif, false);
                yield return new WaitForSeconds(1f);
                Destroy(notificationGO);
            }
        }

        void AddNotificationDocker(Serializables.NotificationData data)
        {
            if (notifDockers.Count >= NOTIF_DOCKER_MAX_COUNT)
            {
                Destroy(notifDockers[0].go);
                Destroy(notifDockers[0].notificationMini);
                notifDockers.RemoveAt(0);
            }

            // Instantiate Notification Docker
            GameObject notificationDockerGO = Instantiate(notificationDockerPanelPrefab, notificationDockerList);
            notificationDockerGO.transform.Find("HeaderPanel").Find("Icon").GetComponent<Image>().sprite = GetAppNotifIcon((PhoneAppName)data.appName);
            notificationDockerGO.transform.Find("HeaderPanel").Find("AppName").GetComponent<TextMeshProUGUI>().color = GetAppColor((PhoneAppName)data.appName);
            notificationDockerGO.transform.Find("HeaderPanel").Find("AppName").GetComponent<TextMeshProUGUI>().text = ((PhoneAppName)data.appName).ToString();
            notificationDockerGO.transform.Find("HeaderPanel").Find("Time").GetComponent<TextMeshProUGUI>().text = data.time;
            notificationDockerGO.transform.Find("HeaderPanel").Find("Title").GetComponent<TextMeshProUGUI>().text = data.title;
            notificationDockerGO.transform.Find("BodyPanel").Find("Desc").GetComponent<TextMeshProUGUI>().text = data.desc;

            // Instantiate Notification Mini
            GameObject notificationMiniGO = new GameObject(((PhoneAppName)data.appName).ToString());
            notificationMiniGO.transform.parent = notificationMiniList;
            Image notificationMiniImage = notificationMiniGO.AddComponent<Image>();
            notificationMiniImage.sprite = GetAppNotifIcon((PhoneAppName)data.appName);
            notificationMiniImage.color = new Color(1, 1, 1, 0.66f);
            notificationMiniImage.rectTransform.sizeDelta = new Vector2(20, 20);
            notificationMiniImage.raycastTarget = false;

            notifDockers.Add(new NotificationDocker(data, notificationDockerGO, notificationMiniGO));
            dockerAnimator.SetInteger(int_notif_count, notifDockers.Count);
        }

        public void ChangeLocation(Location location)
        {
            locationText.text = location.LocationDisplayName;
        }

        #endregion

        #region [Methods: UI Corner Tool]

        public void Save(GameManager.GameAssets gameAssets)
        {
            List<Serializables.NotificationData> notificationData = new List<Serializables.NotificationData>();
            foreach (var notif in notifDockers)
                notificationData.Add(notif.notificationData);

            var saveData = new Serializables.PhoneManagerData(notificationData);

            gameAssets.systemData.phoneManagerData = saveData;
        }

        public void Load(GameManager.GameAssets gameAssets)
        {
            for (int i = notifDockers.Count - 1; i >= 0; i--)
            {
                Destroy(notifDockers[i].go);
                Destroy(notifDockers[i].notificationMini);
                notifDockers.RemoveAt(i);
            }

            var saveData = gameAssets.systemData.phoneManagerData;
            if (saveData == null) return;

            foreach (var notif in saveData.notificationData)
                AddNotificationDocker(notif);

            ChangeLocation(SceneMaster.current.Location);

        }

        public override void OnBeforeSceneLoad()
        {
        }

        public override void OnAfterSceneLoad()
        {
        }

        #endregion

        #region [Methods: Utility]

        Sprite GetAppNotifIcon(PhoneAppName app)
        {
            appNotifIcons.TryGetValue(app, out Sprite icon);
            if (icon == null) return null;
            else return icon;
        }

        Color GetAppColor(PhoneAppName appName)
        {
            switch (appName)
            {
                case PhoneAppName.None:
                    return Color.white;
                case PhoneAppName.Chat:
                    return new Color(0.2f, 1f, 0.2f, 1f);
                case PhoneAppName.ToDo:
                    return new Color(0.5f, 0.8f, 0.9f, 1f);
                case PhoneAppName.Social:
                    return new Color(1f, 0.2f, 0.2f, 1f);
                case PhoneAppName.Camera:
                    return new Color(0.8f, 0.2f, 0.5f, 1f);
                case PhoneAppName.Map:
                    return new Color(0.85f, 0.35f, 0.15f, 1f);
                case PhoneAppName.Settings:
                    return new Color(0.5f, 0.5f, 0.5f, 1f);                
                case PhoneAppName.Quit:
                    return new Color(0.5f, 0.5f, 0.5f, 1f);
                default:
                    return Color.white;
            }
        }

        public PhoneApp CurrentApp
        {
            get
            {
                switch (currentAppName)
                {
                    case PhoneAppName.None:
                        return null;

                    case PhoneAppName.Chat:
                        return ChatApp;

                    case PhoneAppName.ToDo:
                        return ToDoApp;

                    case PhoneAppName.Map:
                        return MapApp;

                    case PhoneAppName.Social:
                        return null;

                    case PhoneAppName.Camera:
                        return null;

                    case PhoneAppName.Settings:
                        return null;

                    case PhoneAppName.Quit:
                        return null;

                    default:
                        return null;
                }
            }
        }

        PhoneAppName GetAppName(PhoneApp app)
        {
            if (app is ChatApp)
                return PhoneAppName.Chat;
            else if (app is ToDoApp)
                return PhoneAppName.ToDo;
            else if (app is MapApp)
                return PhoneAppName.Map;
            else
                return PhoneAppName.None;
        }




        #endregion
    }
}