using Encore.Locations;
using Encore.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.Phone.Map
{
    [AddComponentMenu("Encore/Phone/Map App")]
    public class MapApp : PhoneApp
    {
        #region [Classes]

        public class FocusPanelData
        {
            public string title;
            public string subtitle;
            public Sprite icon;

            public FocusPanelData(string title, string subtitle, Sprite icon)
            {
                this.title = title;
                this.subtitle = subtitle;
                this.icon = icon;
            }
        }

        #endregion

        #region [Vars: Components]

        #region [Properties]

        [Title("Properties")]

        [SerializeField]
        MapAppInfoCard infoCardPrefab;

        [SerializeField]
        MapAppTagButton tagButtonPrefab;

        #endregion


        #region [Overview]

        [Title("Overview Card")]

        [SerializeField]
        Sprite pointIcon;

        [SerializeField]
        Color pointHeaderColor;

        [SerializeField]
        Color pointTextColor;

        [SerializeField]
        TextMeshProUGUI pointNameText;

        [SerializeField]
        Image pointHeaderBGImage;

        [SerializeField]
        Image pointIconImage;

        [SerializeField]
        TextMeshProUGUI pointDescText;

        [SerializeField]
        Transform pointImagesParent;

        #endregion



        #region [HomeCard]

        [Title("Home Card")]

        [SerializeField]
        Transform infoCardsParent;

        [SerializeField]
        TextMeshProUGUI currentLocationText;

        [SerializeField]
        TextMeshProUGUI currentDistrictText;

        [SerializeField]
        Transform locationsByTagParent;

        [SerializeField]
        Image goCurrentLocationBut;

        #endregion




        #region [Map]

        [Title("Map")]

        [SerializeField]
        MapManager mapManager;

        [SerializeField]
        RectTransform zoomPort;

        [SerializeField]
        Vector2 mapCenterOffset = new Vector2(0, 0);

        #endregion


        #region [Controls]

        [Title("Controls")]

        [SerializeField]
        Image nextBut;

        [SerializeField]
        Image backBut;

        [SerializeField]
        Image toggleHomeInfoCardBut;

        [SerializeField]
        Sprite homeIcon;

        [SerializeField]
        Sprite infoCardIcon;

        [SerializeField]
        Image hideBut;

        [SerializeField]
        Image zoomBut;

        [SerializeField]
        Slider zoomSlider;

        #endregion

        #region [FocusParent]

        [Title("Focus Panel")]

        [SerializeField]
        Animator focusParentAnimator;

        [SerializeField]
        Image focusParent;

        [SerializeField]
        TextMeshProUGUI focusTitleText;

        [SerializeField]
        TextMeshProUGUI focusSubtitleText;

        [SerializeField]
        Image focusTitleIcon;

        [SerializeField]
        Transform focusPanelsParent;

        [SerializeField]
        Image focusHideBut;

        [SerializeField]
        GameObject focusPanelPrefab;

        #endregion

        Animator animator;

        #endregion

        #region [Vars: Data Handlers]

        Coroutine corTransitioningInfoCard;
        Coroutine corMovingMapToPoint;

        const float transitioningInfoCardDuration = 0.5f;

        const int transitionDistance = 480;

        int currentInfoCardIndex = -1;

        List<Location> currentShowingLocations = new List<Location>();

        Location currentLocation;

        public enum ViewMode { HomeCard, InfoCards }
        ViewMode currentViewMode = ViewMode.HomeCard;

        int boo_show,boo_showControls, int_mode;

        bool isShowingControls = true;

        Coroutine corLoadingResources;

        const float defaultZoom = 150f;
        const float normalZoom = 100f;

        public List<LocationTag> AllTags { get { return GameManager.Instance.GetGameAssets().resources.LocationTags; } }

        #endregion

        #region [Methods: Phone Apps Overrides]

        override protected void Awake()
        {
            base.Awake();

            animator = GetComponent<Animator>();
            boo_showControls = Animator.StringToHash(nameof(boo_showControls));
            int_mode = Animator.StringToHash(nameof(int_mode));
            boo_show = Animator.StringToHash(nameof(boo_show));

            #region [Next But]

            EventTrigger nextBut_et = nextBut.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry nextBut_entry_enter = new EventTrigger.Entry();
            nextBut_entry_enter.eventID = EventTriggerType.PointerEnter;
            nextBut_entry_enter.callback.AddListener((data) =>
            {
                nextBut.transform.localScale = new Vector2(1.33f, 1.33f);
            });
            nextBut_et.triggers.Add(nextBut_entry_enter);

            EventTrigger.Entry nextBut_entry_exit = new EventTrigger.Entry();
            nextBut_entry_exit.eventID = EventTriggerType.PointerExit;
            nextBut_entry_exit.callback.AddListener((data) =>
            {
                nextBut.transform.localScale = new Vector2(1f, 1f);
            });
            nextBut_et.triggers.Add(nextBut_entry_exit);

            EventTrigger.Entry nextBut_entry_click = new EventTrigger.Entry();
            nextBut_entry_click.eventID = EventTriggerType.PointerClick;
            nextBut_entry_click.callback.AddListener((data) =>
            {
                NextInfoCard();
            });
            nextBut_et.triggers.Add(nextBut_entry_click);

            #endregion

            #region [Back But]

            EventTrigger backBut_et = backBut.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry backBut_entry_enter = new EventTrigger.Entry();
            backBut_entry_enter.eventID = EventTriggerType.PointerEnter;
            backBut_entry_enter.callback.AddListener((data) =>
            {
                backBut.transform.localScale = new Vector2(1.33f, 1.33f);
            });
            backBut_et.triggers.Add(backBut_entry_enter);

            EventTrigger.Entry backBut_entry_exit = new EventTrigger.Entry();
            backBut_entry_exit.eventID = EventTriggerType.PointerExit;
            backBut_entry_exit.callback.AddListener((data) =>
            {
                backBut.transform.localScale = new Vector2(1f, 1f);
            });
            backBut_et.triggers.Add(backBut_entry_exit);

            EventTrigger.Entry backBut_entry_click = new EventTrigger.Entry();
            backBut_entry_click.eventID = EventTriggerType.PointerClick;
            backBut_entry_click.callback.AddListener((data) =>
            {
                PreviousInfoCard();
            });
            backBut_et.triggers.Add(backBut_entry_click);

            #endregion

            #region [Home But]

            EventTrigger toggleHomeInfoCardBut_et = toggleHomeInfoCardBut.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry homeBut_entry_enter = new EventTrigger.Entry();
            homeBut_entry_enter.eventID = EventTriggerType.PointerEnter;
            homeBut_entry_enter.callback.AddListener((data) =>
            {
                toggleHomeInfoCardBut.transform.localScale = new Vector2(1.33f, 1.33f);
            });
            toggleHomeInfoCardBut_et.triggers.Add(homeBut_entry_enter);

            EventTrigger.Entry toggleHomeInfoCardBut_entry_exit = new EventTrigger.Entry();
            toggleHomeInfoCardBut_entry_exit.eventID = EventTriggerType.PointerExit;
            toggleHomeInfoCardBut_entry_exit.callback.AddListener((data) =>
            {
                toggleHomeInfoCardBut.transform.localScale = new Vector2(1f, 1f);
            });
            toggleHomeInfoCardBut_et.triggers.Add(toggleHomeInfoCardBut_entry_exit);

            EventTrigger.Entry toggleHomeInfoCardBut_entry_click = new EventTrigger.Entry();
            toggleHomeInfoCardBut_entry_click.eventID = EventTriggerType.PointerClick;
            toggleHomeInfoCardBut_entry_click.callback.AddListener((data) =>
            {
                ChangeMode((ViewMode)(((int)currentViewMode + 1) % Enum.GetNames(typeof(ViewMode)).Length));
            });
            toggleHomeInfoCardBut_et.triggers.Add(toggleHomeInfoCardBut_entry_click);

            #endregion

            #region [Hide But]

            EventTrigger hideBut_et = hideBut.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry hideBut_entry_enter = new EventTrigger.Entry();
            hideBut_entry_enter.eventID = EventTriggerType.PointerEnter;
            hideBut_entry_enter.callback.AddListener((data) =>
            {
                hideBut.transform.localScale = new Vector2(1.33f, 1.33f);
            });
            hideBut_et.triggers.Add(hideBut_entry_enter);

            EventTrigger.Entry hideBut_entry_exit = new EventTrigger.Entry();
            hideBut_entry_exit.eventID = EventTriggerType.PointerExit;
            hideBut_entry_exit.callback.AddListener((data) =>
            {
                hideBut.transform.localScale = new Vector2(1f, 1f);
            });
            hideBut_et.triggers.Add(hideBut_entry_exit);

            EventTrigger.Entry hideBut_entry_click = new EventTrigger.Entry();
            hideBut_entry_click.eventID = EventTriggerType.PointerClick;
            hideBut_entry_click.callback.AddListener((data) =>
            {
                ToggleShowControls();
            });
            hideBut_et.triggers.Add(hideBut_entry_click);

            #endregion

            #region [Zoom But]

            EventTrigger zoomBut_et = zoomBut.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry zoomBut_entry_enter = new EventTrigger.Entry();
            zoomBut_entry_enter.eventID = EventTriggerType.PointerEnter;
            zoomBut_entry_enter.callback.AddListener((data) =>
            {
                zoomBut.transform.localScale = new Vector2(1.33f, 1.33f);
            });
            zoomBut_et.triggers.Add(zoomBut_entry_enter);

            EventTrigger.Entry zoomBut_entry_exit = new EventTrigger.Entry();
            zoomBut_entry_exit.eventID = EventTriggerType.PointerExit;
            zoomBut_entry_exit.callback.AddListener((data) =>
            {
                zoomBut.transform.localScale = new Vector2(1f, 1f);
            });
            zoomBut_et.triggers.Add(zoomBut_entry_exit);

            EventTrigger.Entry zoomBut_entry_click = new EventTrigger.Entry();
            zoomBut_entry_click.eventID = EventTriggerType.PointerClick;
            zoomBut_entry_click.callback.AddListener((data) =>
            {
                SetZoom(defaultZoom);
            });
            zoomBut_et.triggers.Add(zoomBut_entry_click);

            #endregion

            #region [goCurrentLocation But]

            EventTrigger goCurrentLocationBut_et = goCurrentLocationBut.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry goCurrentLocationBut_entry_enter = new EventTrigger.Entry();
            goCurrentLocationBut_entry_enter.eventID = EventTriggerType.PointerEnter;
            goCurrentLocationBut_entry_enter.callback.AddListener((data) =>
            {
                goCurrentLocationBut.transform.localScale = new Vector2(1.33f, 1.33f);
            });
            goCurrentLocationBut_et.triggers.Add(goCurrentLocationBut_entry_enter);

            EventTrigger.Entry goCurrentLocationBut_entry_exit = new EventTrigger.Entry();
            goCurrentLocationBut_entry_exit.eventID = EventTriggerType.PointerExit;
            goCurrentLocationBut_entry_exit.callback.AddListener((data) =>
            {
                goCurrentLocationBut.transform.localScale = new Vector2(1f, 1f);
            });
            goCurrentLocationBut_et.triggers.Add(goCurrentLocationBut_entry_exit);

            EventTrigger.Entry goCurrentLocationBut_entry_click = new EventTrigger.Entry();
            goCurrentLocationBut_entry_click.eventID = EventTriggerType.PointerClick;
            goCurrentLocationBut_entry_click.callback.AddListener((data) =>
            {
                ShowCurrentLocation();
            });
            goCurrentLocationBut_et.triggers.Add(goCurrentLocationBut_entry_click);

            #endregion

            #region [FocusParent]

            focusHideBut.color = new Color(1, 1, 1, 0.66f);

            var focusHideBut_et = focusHideBut.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry focusParent_entry_enter = new EventTrigger.Entry();
            focusParent_entry_enter.eventID = EventTriggerType.PointerEnter;
            focusParent_entry_enter.callback.AddListener((data) =>
            {
                focusHideBut.color = new Color(1, 1, 1, 1f);
            });
            focusHideBut_et.triggers.Add(focusParent_entry_enter);

            EventTrigger.Entry focusParent_entry_exit = new EventTrigger.Entry();
            focusParent_entry_exit.eventID = EventTriggerType.PointerExit;
            focusParent_entry_exit.callback.AddListener((data) =>
            {
                focusHideBut.color = new Color(1, 1, 1, 0.66f);
            });
            focusHideBut_et.triggers.Add(focusParent_entry_exit);

            EventTrigger.Entry focusParent_entry_click = new EventTrigger.Entry();
            focusParent_entry_click.eventID = EventTriggerType.PointerClick;
            focusParent_entry_click.callback.AddListener((data) =>
            {
                HideFocusPanels();
            });
            focusHideBut_et.triggers.Add(focusParent_entry_click);


            #endregion


            zoomSlider.onValueChanged.AddListener(SetZoom);
            zoomSlider.value = 150f;

            focusParent.gameObject.SetActive(false);
        }

        public override void Show(bool isShow)
        {
            base.Show(isShow);

            if (isShow)
            {
                // Destroy current tag buttons
                for (int i = locationsByTagParent.childCount - 1; i >= 0; i--)
                    Destroy(locationsByTagParent.GetChild(i).gameObject);

                // Setup LocationTags
                foreach (LocationTag tag in AllTags)
                {
                    if (tag.TagName == "NonFavs") continue;
                    var tagBut = Instantiate(tagButtonPrefab, locationsByTagParent);
                    tagBut.Setup(tag, ShowLocationsByTag, AllTags);
                }

                SetupMapPoints();

                // Setup MapPoints
                ShowCurrentLocation();
                ShowOverViewCard();
                ChangeMode(ViewMode.HomeCard);

                HideFocusPanels();
            }

            else
            {
                foreach (var point in mapManager.Points)
                {
                    point.Setdown();
                }
            }
        }

        #endregion

        #region [Methods: Setups]

        void SetupMapPoints()
        {
            var points = mapManager.Points;
            foreach (var point in points)
            {
                var thisPoint = point;
                thisPoint.Setup(AllTags, OnPointSelected);

                void OnPointSelected(bool isSelected, List<Location> locations)
                {
                    if (isSelected)
                    {
                        ResetInfoCards();

                        foreach (var location in locations)
                            AddInfoCard(location);

                        SetOverviewCard(thisPoint, pointIcon, pointHeaderColor, pointTextColor);
                        ShowOverViewCard();
                        ChangeMode(ViewMode.InfoCards);
                    }
                }
            }

            SetZoom(150f);

        }

        public void Load()
        {

        }

        #endregion

        #region [Methods: Navigations]

        void ShowOverViewCard()
        {
            currentInfoCardIndex = -1;

            backBut.enabled = false;
            nextBut.enabled = currentInfoCardIndex != currentShowingLocations.Count - 1;

            corTransitioningInfoCard = this.RestartCoroutine(TransitioningInfoCard(currentInfoCardIndex));
            corMovingMapToPoint = this.RestartCoroutine(MovingMapToMapPoint(GetCurrentIndexMapPoint()));
        }

        void SetOverviewCard(LocationTag tag)
        {
            pointNameText.text = tag.TagName;
            pointDescText.text = tag.Desc;
            pointIconImage.sprite = tag.Icon;
            pointHeaderBGImage.color = tag.AccentColor;
            pointNameText.color = tag.TextColor;


            // Remove all old images
            for (int i = pointImagesParent.childCount - 1; i >= 0; i--)
                Destroy(pointImagesParent.GetChild(i).gameObject);
        }

        void SetOverviewCard(MapPoint point, Sprite icon, Color headerColor, Color nameColor)
        {
            pointNameText.text = point.PointName;
            pointDescText.text = point.PointDesc;
            pointIconImage.sprite = icon;
            pointHeaderBGImage.color = headerColor;
            pointNameText.color = nameColor;

            // Remove all old images
            for (int i = pointImagesParent.childCount - 1; i >= 0; i--)
                Destroy(pointImagesParent.GetChild(i).gameObject);

            // Add new images
            int index = 0;
            foreach (var image in point.PointImages)
            {
                var imageGO = new GameObject("image_" + index);
                imageGO.transform.parent = pointImagesParent.transform;
                var imageComp = imageGO.AddComponent<Image>();
                imageComp.preserveAspect = true;
                imageComp.sprite = image;
                imageComp.SetNativeSize();
            }
        }

        void ShowFirstInfoCard()
        {
            currentInfoCardIndex = 0;

            backBut.enabled = true;
            nextBut.enabled = currentInfoCardIndex != currentShowingLocations.Count - 1;

            corTransitioningInfoCard = this.RestartCoroutine(TransitioningInfoCard(currentInfoCardIndex));
            corMovingMapToPoint = this.RestartCoroutine(MovingMapToMapPoint(GetCurrentIndexMapPoint()));
        }

        void NextInfoCard()
        {
            currentInfoCardIndex += 1;

            backBut.enabled = true;
            nextBut.enabled = currentInfoCardIndex != currentShowingLocations.Count - 1;

            corTransitioningInfoCard = this.RestartCoroutine(TransitioningInfoCard(currentInfoCardIndex));
            corMovingMapToPoint = this.RestartCoroutine(MovingMapToMapPoint(GetCurrentIndexMapPoint()));
        }

        void PreviousInfoCard()
        {
            currentInfoCardIndex -= 1;
            backBut.enabled = currentInfoCardIndex != -1;
            nextBut.enabled = true;

            corTransitioningInfoCard = this.RestartCoroutine(TransitioningInfoCard(currentInfoCardIndex));
            corMovingMapToPoint = this.RestartCoroutine(MovingMapToMapPoint(GetCurrentIndexMapPoint()));
        }

        #endregion

        #region [Methods: Show info cards]

        void ShowCurrentLocation()
        {
            currentLocation = SceneMasters.SceneMaster.current.Location;
            GetMapPoint(currentLocation).Select(true);
        }

        void ShowLocationsByTag(LocationTag tag)
        {
            ResetInfoCards();
            var locations = GameManager.Instance.GetGameAssets().resources.Locations;
            foreach (var location in locations)
            {
                var locationData = GameManager.Instance.GetLocationData(location);

                if (
                locationData.SceneName == location.SceneName &&
                locationData.IsUnlocked &&
                locationData.LocationTags.Contains(tag.TagName))
                {
                    AddInfoCard(location);
                }
            }

            SetOverviewCard(tag);
            ShowOverViewCard();
            ChangeMode(ViewMode.InfoCards);
            
        }

        public void ChangeMode(ViewMode mode)
        {
            ShowControls(true);
            currentViewMode = mode;

            switch (mode)
            {
                case ViewMode.HomeCard:
                    toggleHomeInfoCardBut.sprite = homeIcon;
                    break;
                case ViewMode.InfoCards:
                    toggleHomeInfoCardBut.sprite = infoCardIcon;
                    break;
                default:
                    break;
            }

            animator.SetInteger(int_mode, (int)currentViewMode);
        }

        #endregion

        #region [Methods: Utility]

        IEnumerator TransitioningInfoCard(int toIndex)
        {
            // HomeCard is actully in the zero index's location, so add +1
            float fromPos = infoCardsParent.transform.localPosition.x;
            float toPos = -transitionDistance * (toIndex + 1);
            AnimationCurve curve = AnimationCurve.EaseInOut(0, fromPos, transitioningInfoCardDuration, toPos);
            float time = 0;

            while (time < transitioningInfoCardDuration)
            {
                time += Time.deltaTime;
                infoCardsParent.transform.localPosition = new Vector2(curve.Evaluate(time), infoCardsParent.transform.localPosition.y);
                yield return null;
            }
        }

        IEnumerator MovingMapToMapPoint(MapPoint mapPoint)
        {
            if (mapPoint == null) yield break;

            var time = 0f;
            var duration = 0.5f;
            var mapParent = mapManager.transform.parent;
            var targetPos = new Vector2(-mapPoint.transform.localPosition.x, -mapPoint.transform.localPosition.y);
            targetPos += mapCenterOffset;

            AnimationCurve curveX = AnimationCurve.EaseInOut(0, mapParent.localPosition.x, duration, targetPos.x);
            AnimationCurve curveY = AnimationCurve.EaseInOut(0, mapParent.localPosition.y, duration, targetPos.y);


            while (time < duration)
            {
                mapParent.localPosition = new Vector2(curveX.Evaluate(time), curveY.Evaluate(time));
                time += Time.deltaTime;
                yield return null;
            }
        }

        void ToggleShowControls()
        {
            ShowControls(!isShowingControls);
        }

        void ShowControls(bool isShowingControls)
        {
            this.isShowingControls = isShowingControls;
            animator.SetBool(boo_showControls, isShowingControls);
        }

        void SetZoom(float zoomValue)
        {
            zoomPort.localScale = new Vector3(zoomValue / normalZoom, zoomValue / normalZoom, zoomValue / normalZoom);

            foreach (var point in mapManager.Points)
            {
                point.SetZoom(zoomValue, 200f);
            }
        }
        
        void ResetInfoCards()
        {
            currentShowingLocations.Clear();
            for (int i = infoCardsParent.childCount - 1; i >= 1; i--) // Except HomeCard
                Destroy(infoCardsParent.GetChild(i).gameObject);

            foreach (var point in mapManager.Points)
                if (point.IsSelected)
                {
                    point.Select(false);
                }

            ShowOverViewCard();
        }

        void AddInfoCard(Location location)
        {
            var infoCard = Instantiate(infoCardPrefab, infoCardsParent);
            var mapPoint = mapManager.Points.Find(point => point.Locations.Contains(location));
            infoCard.Setup(location, GameManager.Instance.GetLocationData(location), mapPoint, AllTags);
            currentShowingLocations.Add(location);
        }

        MapPoint GetMapPoint(Location location)
        {
            if (location == null) return null;
            return mapManager.Points.Find(point => point.Locations.Contains(location));
        }

        MapPoint GetCurrentIndexMapPoint()
        {
            if (currentInfoCardIndex == -1)
                    return GetMapPoint(currentShowingLocations.GetAt(0));
            else
                return GetMapPoint(currentShowingLocations[currentInfoCardIndex]);
        }

        public void RemoveTagFromLocation(Location location, LocationTag tag)
        {
            var mapPointWithLocation = mapManager.Points.Find(point => point.Locations.Contains(location));
            if (mapPointWithLocation != null)
                mapPointWithLocation.RemoveTagImage(tag);
        }

        #endregion

        #region [Methods: Show Focus Panels]

        public void ShowFocusPanels(string title, Sprite titleIcon, string subtitle, List<FocusPanelData> data)
        {
            focusTitleText.text = title;
            focusTitleIcon.sprite = titleIcon;
            focusSubtitleText.text = subtitle;

            for (int i = focusPanelsParent.childCount - 1; i >= 0; i--)
                Destroy(focusPanelsParent.GetChild(i).gameObject);

            foreach (var datum in data)
            {
                var focusPanel = Instantiate(focusPanelPrefab, focusPanelsParent);
                var titleText = focusPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                titleText.text = datum.title;
                var subtitleText = focusPanel.transform.Find("Subtitle").GetComponent<TextMeshProUGUI>();
                subtitleText.text = datum.subtitle;
                var iconImage = focusPanel.transform.Find("Icon").GetComponent<Image>();
                iconImage.sprite = datum.icon;
            }

            focusParent.gameObject.SetActive(true);
            focusParentAnimator.SetBool(nameof(boo_show), true);
        }

        public void HideFocusPanels()
        {
            focusParentAnimator.SetBool(nameof(boo_show), false);

            StartCoroutine(Delay(0.66f));
            IEnumerator Delay(float delay)
            {
                yield return new WaitForSeconds(delay);
                focusParent.gameObject.SetActive(false);
            }
        }

        #endregion

    }
}