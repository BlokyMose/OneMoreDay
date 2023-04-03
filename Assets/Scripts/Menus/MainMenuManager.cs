using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using Encore.Doomclock;
using Encore.Inventory;
using Encore.CharacterControllers;
using Encore.Utility;
using System;
using Encore.Menus;

namespace Encore.Menus
{

    public class MenuItem
    {
        public Animator animator;
        public CanvasGroup canvasGroup;
        public EventTrigger eventTrigger;


        public MenuItem(GameObject go, Action onClick)
        {
            this.animator = go.GetComponent<Animator>();
            this.canvasGroup = go.GetComponent<CanvasGroup>();
            this.eventTrigger = go.GetComponent<EventTrigger>();

            int boo_hover = Animator.StringToHash(nameof(boo_hover));

            EventTrigger.Entry entry_enter = new EventTrigger.Entry();
            entry_enter.eventID = EventTriggerType.PointerEnter;
            entry_enter.callback.AddListener((data) =>
            {
                animator.SetBool(boo_hover, true);
            });
            eventTrigger.triggers.Add(entry_enter);

            EventTrigger.Entry entry_exit = new EventTrigger.Entry();
            entry_exit.eventID = EventTriggerType.PointerExit;
            entry_exit.callback.AddListener((data) =>
            {
                animator.SetBool(boo_hover, false);
            });
            eventTrigger.triggers.Add(entry_exit);

            EventTrigger.Entry entry_click = new EventTrigger.Entry();
            entry_click.eventID = EventTriggerType.PointerClick;
            entry_click.callback.AddListener((data) =>
            {
                onClick();
            });
            eventTrigger.triggers.Add(entry_click);


        }
    }

    [AddComponentMenu("Encore/Menu/Main Menu")]
    public class MainMenuManager : SceneMasters.SceneMaster
    {
        #region [Components]

        [TabGroup("Main Components"),SerializeField] Camera mainCamera;
        [TabGroup("Main Components"),SerializeField] MouseManager mouseManager;
        [TabGroup("Main Components"),SerializeField] Item sunflowerItem;
        [TabGroup("Main Components"),SerializeField] List<GameObject> sunflowerPrefabs;

        [TabGroup("Hints"),SerializeField] Animator hintMenus;
        [TabGroup("Hints"),SerializeField] Animator hintInteract;
        [TabGroup("Hints"),SerializeField] Animator hintMove;

        [TabGroup("Menu"),SerializeField] Animator menuPanel;
        [TabGroup("Menu"),SerializeField] Image hitPanelShowMenu;
        [TabGroup("Menu"),SerializeField] Image hitPanelHideMenu;

        [Header("Prefabs")]
        [TabGroup("Menu"), SerializeField] PlayGameMenu playGamePrefab;
        [TabGroup("Menu"),SerializeField] GameSettingsMenu gameSettingsPrefab;

        [Header("Menu Items")]
        [TabGroup("Menu"),SerializeField] Animator menuItemPlay;
        [TabGroup("Menu"),SerializeField] Animator menuItemSettings;
        [TabGroup("Menu"),SerializeField] Animator menuItemQuit;
        List<MenuItem> menuItems = new List<MenuItem>();

        [TabGroup("Clock"),SerializeField] CanvasGroup clockPanel;
        [TabGroup("Clock"),SerializeField] TextMeshProUGUI clockText;
        [TabGroup("Clock"),SerializeField] CanvasGroup hitPanelHideClock;
        [TabGroup("Clock"),SerializeField] CanvasGroup maskPanelHideClock;
        [TabGroup("Clock"),SerializeField] CanvasGroup hitPanelShowClock;

        #endregion

        #region [Data Handlers]

        int boo_show, boo_hover;
        float moveMax = 0.45f;
        Vector2 moveInput;

        #endregion

        protected override void Awake()
        {
            base.Awake();
            boo_show = Animator.StringToHash(nameof(boo_show));
            boo_hover = Animator.StringToHash(nameof(boo_hover));

            #region [Menu Items]

            menuItems = new List<MenuItem>() { 
                new MenuItem(menuItemPlay.gameObject, PlayGame),
                new MenuItem(menuItemSettings.gameObject, OpenSettings),
                new MenuItem(menuItemQuit.gameObject, QuitGame)
            };

            #endregion

            #region Hints: Setup event triggers

            EventTrigger et_menus = hintMenus.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_entry_enter = new EventTrigger.Entry();
            et_entry_enter.eventID = EventTriggerType.PointerEnter;
            et_entry_enter.callback.AddListener((data) => { hintMenus.SetBool(boo_show, true);});
            et_menus.triggers.Add(et_entry_enter);

            EventTrigger.Entry et_entry_exit = new EventTrigger.Entry();
            et_entry_exit.eventID = EventTriggerType.PointerExit;
            et_entry_exit.callback.AddListener((data) => { hintMenus.SetBool(boo_show, false); });
            et_menus.triggers.Add(et_entry_exit);

            // ---

            EventTrigger et_interact = hintInteract.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_interact_entry_enter = new EventTrigger.Entry();
            et_interact_entry_enter.eventID = EventTriggerType.PointerEnter;
            et_interact_entry_enter.callback.AddListener((data) => { hintInteract.SetBool(boo_show, true); });
            et_interact.triggers.Add(et_interact_entry_enter);

            EventTrigger.Entry et_interact_entry_exit = new EventTrigger.Entry();
            et_interact_entry_exit.eventID = EventTriggerType.PointerExit;
            et_interact_entry_exit.callback.AddListener((data) => { hintInteract.SetBool(boo_show, false); });
            et_interact.triggers.Add(et_interact_entry_exit);

            // ---

            EventTrigger et_move = hintMove.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_move_entry_enter = new EventTrigger.Entry();
            et_move_entry_enter.eventID = EventTriggerType.PointerEnter;
            et_move_entry_enter.callback.AddListener((data) => { hintMove.SetBool(boo_show, true); });
            et_move.triggers.Add(et_move_entry_enter);

            EventTrigger.Entry et_move_entry_exit = new EventTrigger.Entry();
            et_move_entry_exit.eventID = EventTriggerType.PointerExit;
            et_move_entry_exit.callback.AddListener((data) => { hintMove.SetBool(boo_show, false); });
            et_move.triggers.Add(et_move_entry_exit);

            #endregion

            #region MenuPanel: Setup event triggers of hitPanels

            EventTrigger et_showMenu = hitPanelShowMenu.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_entry_showMenu = new EventTrigger.Entry();
            et_entry_showMenu.eventID = EventTriggerType.PointerEnter;
            et_entry_showMenu.callback.AddListener((data) => { ShowMenuPanel(true); });
            et_showMenu.triggers.Add(et_entry_showMenu);

            // ---

            EventTrigger et_hideMenu = hitPanelHideMenu.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_entry_hideMenu = new EventTrigger.Entry();
            et_entry_hideMenu.eventID = EventTriggerType.PointerEnter;
            et_entry_hideMenu.callback.AddListener((data) => { ShowMenuPanel(false); });
            et_hideMenu.triggers.Add(et_entry_hideMenu);

            #endregion

            #region ClockPanel: Setup event triggers of hitPanels

            Coroutine corClockAlpha = null;

            EventTrigger et_showClock = hitPanelShowClock.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_entry_showClock = new EventTrigger.Entry();
            et_entry_showClock.eventID = EventTriggerType.PointerEnter;
            et_entry_showClock.callback.AddListener((data) => { ShowClock(true); });
            et_showClock.triggers.Add(et_entry_showClock);

            // ---

            EventTrigger et_hideClock = hitPanelHideClock.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_entry_hideClock = new EventTrigger.Entry();
            et_entry_hideClock.eventID = EventTriggerType.PointerEnter;
            et_entry_hideClock.callback.AddListener((data) => { ShowClock(false); });
            et_hideClock.triggers.Add(et_entry_hideClock);

            void ShowClock(bool isShowing)
            {
                corClockAlpha = this.RestartCoroutine(AnimateClockAlphaTo(isShowing, 1f, 0f, 1.5f));

                hitPanelHideClock.interactable = isShowing;
                hitPanelHideClock.blocksRaycasts = isShowing;
                maskPanelHideClock.interactable = isShowing;
                maskPanelHideClock.blocksRaycasts = isShowing;

                IEnumerator AnimateClockAlphaTo(bool toMax, float maxAlpha, float minAlpha, float duration)
                {
                    var targetAlpha = toMax ? maxAlpha : minAlpha;

                    CalcUtility.CrossOp(out float time, duration, clockPanel.alpha, maxAlpha);
                    if (!toMax) time = duration - time;

                    AnimationCurve curve = AnimationCurve.EaseInOut(time,clockPanel.alpha, duration, targetAlpha);

                    while (time < duration-0.1f)
                    {
                        clockPanel.alpha = curve.Evaluate(time);
                        time += Time.deltaTime;
                        yield return null;
                    }

                    corClockAlpha = null;
                }
            }
            #endregion
        }

        void Update()
        {
            if (moveInput.x > 0)
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(moveMax, 0, -10), Time.deltaTime * 2);
            }
            else if (moveInput.x < 0)
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(-moveMax, 0, -10), Time.deltaTime * 3);
            }
            else
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, new Vector3(0, 0, -10), Time.deltaTime * 2);
            }
        }

        public override void Init(InitialSettings settings)
        {
            base.Init(settings);

            GameManager.Instance.TimeManager.OnSecondPassed += EverySecondPassed;
            GameManager.Instance.TimeManager.StartClock();

            GameManager.Instance.EnableUICornerTools(false, false);

            ShowHintsAndMenuPanelTemporarily();

            void ShowHintsAndMenuPanelTemporarily()
            {
                StartCoroutine(Delay());
                IEnumerator Delay()
                {
                    GameManager.Instance.Player.EnableAllInputs(false, CursorImageManager.CursorImage.Invisible, false);
                    yield return new WaitForSeconds(3f);

                    hintMenus.SetBool(boo_show, true);
                    yield return new WaitForSeconds(1f);

                    hintInteract.SetBool(boo_show, true);
                    yield return new WaitForSeconds(1f);

                    hintMove.SetBool(boo_show, true);
                    yield return new WaitForSeconds(3f);

                    hintMenus.SetBool(boo_show, false);
                    hintInteract.SetBool(boo_show, false);
                    hintMove.SetBool(boo_show, false);

                    yield return new WaitForSeconds(1.5f);

                    ShowMenuPanel(true);
                    yield return new WaitForSeconds(1.5f);

                    var mousePos = Camera.main.WorldToScreenPoint(hitPanelShowMenu.transform.position) + new Vector3(200,100,0);
                    GameManager.Instance.Player.MouseManager.SetCursorPosition(mousePos);
                    GameManager.Instance.Player.EnableAllInputs(true, CursorImageManager.CursorImage.Normal, false);
                }
            }

        }

        public void PlayGame()
        {
            ShowMenuPanel(false);
            var playGame = Instantiate(playGamePrefab);
            playGame.Show(true);
        }

        public void OpenSettings()
        {
            ShowMenuPanel(false);
            var gameSettings = Instantiate(gameSettingsPrefab);
            gameSettings.Show(true);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        void ShowMenuPanel(bool isShowing)
        {
            const float MENU_ITEM_SHOW_DELAY = 0.15f;
            menuPanel.SetBool(boo_show, isShowing);
            StartCoroutine(ShowAllMenuItems(isShowing));

            IEnumerator ShowAllMenuItems(bool isShowing)
            {
                foreach (var menuItem in menuItems)
                {
                    menuItem.animator.SetBool(boo_show, isShowing);
                    menuItem.canvasGroup.interactable = isShowing;
                    menuItem.canvasGroup.blocksRaycasts = isShowing;

                    yield return new WaitForSeconds(MENU_ITEM_SHOW_DELAY);
                }
            }
        }

        void EverySecondPassed(object sender, Clock clock)
        {
            string hour = (clock.GetHour() < 10 ? "0":"") + clock.GetHour().ToString();
            string minute = (clock.GetMinute() < 10 ? "0" : "") + clock.GetMinute().ToString();

            clockText.text =
                hour +
                "<alpha=#" + (clock.GetSecond() % 2 == 0 ? "AA" : "44") + ">:<alpha=#FF>" +
                minute;
        }


        #region [Methods: Deprecated]

        public void MoveInput(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        public void PrimaryInput(InputAction.CallbackContext context)
        {
            if(context.started)
            if(GameManager.Instance.InventoryManager.GetClickedItem() == sunflowerItem)
            {
                GameObject sunflower = Instantiate(sunflowerPrefabs[UnityEngine.Random.Range(0,sunflowerPrefabs.Count)], transform, true);
                sunflower.transform.SetParent(null);
                sunflower.transform.position = mouseManager.CursorImageManager.transform.position;
                Destroy(sunflower, 10f);

                    GameManager.Instance.InventoryManager.UseItem(sunflowerItem);

                    StartCoroutine(Delay(5f));
                    IEnumerator Delay(float delay)
                    {
                        yield return new WaitForSeconds(delay);
                        GameManager.Instance.InventoryManager.AddItem(sunflowerItem);
                    }
                }

        }

        #endregion


    }
}

