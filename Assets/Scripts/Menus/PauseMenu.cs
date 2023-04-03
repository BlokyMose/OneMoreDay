using Encore.CharacterControllers;
using Encore.Locations;
using Encore.SceneMasters;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Encore.SceneMasters.SceneTransitionAnimationEvent;

namespace Encore.Menus
{
    [AddComponentMenu("Encore/Menu/Pause Menu")]
    public class PauseMenu : UICornerTool
    {

        #region [Components]

        [Title("Pause Menu")]
        [SerializeField] CanvasGroup menuPanel;
        [SerializeField] GameObject menuItemSettings;
        [SerializeField] GameObject menuItemQuit;
        [SerializeField] GameSettingsMenu gameSettingsPrefab;

        [Header("Prefabs")]
        [SerializeField] Location mainMenu;

        #endregion


        #region [Data Handlers]

        List<MenuItem> menuItems = new List<MenuItem>();

        #endregion



        protected override void Awake()
        {
            base.Awake();
            #region [Menu Items]

            menuItems = new List<MenuItem>() {
                new MenuItem(menuItemSettings.gameObject, OpenSettings),
                new MenuItem(menuItemQuit.gameObject, QuitGame)
            };

            #endregion
        }

        public void OpenSettings()
        {
            Show(false);
            var gameSettings = Instantiate(gameSettingsPrefab);
            gameSettings.OnClosed = () => 
            {
                StartCoroutine(Delay(1f));
                IEnumerator Delay(float delay)
                {
                    yield return new WaitForSeconds(delay);
                    GameManager.Instance.EnableUICornerTools(true, false); 
                }
            };
            gameSettings.Show(true);
            GameManager.Instance.EnableUICornerTools(false, false);
        }

        public override void Show(bool isShowing)
        {
            base.Show(isShowing);
            GameManager.Instance.Player.SetCanInteract(!isShowing, CursorImageManager.CursorImage.Normal);

            menuPanel.blocksRaycasts = isShowing;
            menuPanel.interactable = isShowing;

            ShowMenuItems(isShowing);
            void ShowMenuItems(bool isShowing)
            {
                const float MENU_ITEM_SHOW_DELAY = 0.15f;
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
        }

        public void QuitGame()
        {
            GameManager.Instance.LoadScene(mainMenu.SceneName, AnimationDirection.FromBottom);
        }

        public override void OnAfterSceneLoad()
        {
        }

        public override void OnBeforeSceneLoad()
        {
        }
    }
}
