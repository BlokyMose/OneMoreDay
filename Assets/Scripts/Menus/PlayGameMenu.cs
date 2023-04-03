using Encore.Localisations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.Menus
{
    [AddComponentMenu("Encore/Menu/Play Game Menu")]
    public class PlayGameMenu : MenuUI
    {
        #region [Classes]

        class PlayGamePanel
        {
            public Animator animator;
            public EventTrigger eventTrigger;
            public Image screenshotImage;
            public TextMeshProUGUI titleText;
            public TextMeshProUGUI descText;
            public TextsLocaliser localiser;

            int boo_show, boo_hover;

            public PlayGamePanel(GameObject go)
            {
                this.animator = go.GetComponent<Animator>();
                this.eventTrigger = go.GetComponent<EventTrigger>();
                this.localiser = go.GetComponent<TextsLocaliser>();
                this.screenshotImage = go.transform.GetChild(0).Find("Screenshot").GetComponent<Image>();
                this.titleText = go.transform.GetChild(0).Find("Title").GetComponent<TextMeshProUGUI>();
                this.descText = go.transform.GetChild(0).Find("Desc").GetComponent<TextMeshProUGUI>();
            }



            public class DescData
            {
                public int doomclockHours;
                public int doomclockMinutes;
                public string locationName;
                public string todoTitle;
                public int timePlayedHours;
                public int timePlayedMinutes;

                public DescData(int doomclockHours, int doomclockMinutes, string locationName, string todoTitle, int timePlayedHours, int timePlayedMinutes)
                {
                    this.doomclockHours = doomclockHours;
                    this.doomclockMinutes = doomclockMinutes;
                    this.locationName = locationName;
                    this.todoTitle = todoTitle;
                    this.timePlayedHours = timePlayedHours;
                    this.timePlayedMinutes = timePlayedMinutes;
                }

                public Dictionary<string, string> GetDictionary()
                {
                    return new Dictionary<string, string>()
                    {
                        { nameof(doomclockHours), doomclockHours.ToString() },
                        { nameof(doomclockMinutes), doomclockMinutes.ToString() },
                        { nameof(locationName), locationName.ToString() },
                        { nameof(todoTitle), todoTitle.ToString() },
                        { nameof(timePlayedHours), timePlayedHours.ToString() },
                        { nameof(timePlayedMinutes), timePlayedMinutes.ToString() },
                    };
                }
            }

            public void Setup(Sprite screenshot, string title, DescData descData, Action onClickPlayBut)
            {
                titleText.text = title;
                screenshotImage.sprite = screenshot != null ? screenshot : screenshotImage.sprite;
                localiser.SetFormatDataText(descText.gameObject.name, descData.GetDictionary());

                boo_show = Animator.StringToHash(nameof(boo_show));
                boo_hover = Animator.StringToHash(nameof(boo_hover));

                EventTrigger.Entry go_entry_enter = new EventTrigger.Entry();
                go_entry_enter.eventID = EventTriggerType.PointerEnter;
                go_entry_enter.callback.AddListener((data) =>
                {
                    animator.SetBool(boo_hover, true);
                });
                eventTrigger.triggers.Add(go_entry_enter);

                EventTrigger.Entry go_entry_exit = new EventTrigger.Entry();
                go_entry_exit.eventID = EventTriggerType.PointerExit;
                go_entry_exit.callback.AddListener((data) =>
                {
                    animator.SetBool(boo_hover, false);
                });
                eventTrigger.triggers.Add(go_entry_exit);

                EventTrigger.Entry go_entry_click = new EventTrigger.Entry();
                go_entry_click.eventID = EventTriggerType.PointerClick;
                go_entry_click.callback.AddListener((data) =>
                {
                    onClickPlayBut();
                });
                eventTrigger.triggers.Add(go_entry_click);

            }
        }

        #endregion

        #region [Components]

        [SerializeField]
        Transform playGamePanelsParent;

        [SerializeField]
        GameObject playGamePanelPrefab;

        #endregion

        #region [Data Handlers]

        List<PlayGamePanel> playGamePanels = new List<PlayGamePanel>();

        #endregion

        protected override void Setup()
        {
            base.Setup();

            #region [PlayGamePanels]

            for (int i = playGamePanelsParent.childCount - 1; i >= 0; i--)
                Destroy(playGamePanelsParent.GetChild(i).gameObject);

            // TODO: Get all save data from GameManager, then instantiate panels accordingly

            #endregion

            #region [Demo]

            var demoPanelGO = Instantiate(playGamePanelPrefab, playGamePanelsParent);

            var descData = new PlayGamePanel.DescData(
                doomclockHours: 0,
                doomclockMinutes: 0,
                locationName: "n/a",
                todoTitle: "n/a",
                timePlayedHours: 0,
                timePlayedMinutes: 0
                );

            var demoPanel = new PlayGamePanel(demoPanelGO);

            demoPanel.Setup(
                screenshot: null,
                title: "Chapter: Demo",
                descData: descData,
                onClickPlayBut: () => { GameManager.Instance.NewGame(); }
                );

            playGamePanels.Add(demoPanel);

            #endregion
        }
    }
}