using Encore.Serializables;
using Encore.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.Locations
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Encore/Locations/Map Point")]
    public class MapPoint : MonoBehaviour
    {
        #region [Vars: Properties]

        [Title("Properties")]

        [SerializeField]
        string pointName;
        public string PointName { get { return pointName; } }

        [SerializeField, TextArea (3,5)]
        string pointDesc;
        public string PointDesc { get { return pointDesc; } }

        [SerializeField]
        List<Sprite> pointImages;
        public List<Sprite> PointImages { get { return pointImages; } }

        [SerializeField]
        List<Location> locations = new List<Location>();

        public List<Location> Locations { get { return locations; } }

        #endregion

        #region [Vars: Components]

        [Title("Components")]

        [SerializeField]
        Image pointImage;        
        
        [SerializeField]
        Transform pointParent;

        [SerializeField]
        Image hitArea;

        [SerializeField]
        Transform tagsParent;

        Image circleImage;
        Animator animator;

        #endregion

        #region [Vars: Data Handlers]

        int boo_hovered, boo_clicked;
        bool isSelected = false;
        public bool IsSelected { get { return isSelected; } }
        Color selectedColor = new Color(1, 0.282f, 0.282f);
        Color unselectedColor = new Color(0.5f, 0.5f, 0.5f);
        List<LocationTag> currentTags = new List<LocationTag>();

        // Tags Transition Animation
        const float tagTransitionDistance = 24;
        const float tagTransitionDuration = 0.5f;

        Coroutine corAnimatingTags;

        float currentZoom = 1f;

        #endregion

        #region [Delegates]

        public Action<bool,List<Location>> OnSelected;

        #endregion

        #region [Methods: Inspector]

        [Title("Utility")]
        [Button(ButtonSizes.Large)]
        void SyncCoordinate()
        {
            foreach (var loc in locations)
                loc.Coordinate = transform.localPosition;
        }

        #endregion

        void Awake()
        {
            animator = GetComponent<Animator>();
            boo_hovered = Animator.StringToHash(nameof(boo_hovered));
            boo_clicked = Animator.StringToHash(nameof(boo_clicked));
            circleImage = GetComponent<Image>();

            #region [Setup Event Trigger]

            EventTrigger et = hitArea.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry _entry_enter = new EventTrigger.Entry();
            _entry_enter.eventID = EventTriggerType.PointerEnter;
            _entry_enter.callback.AddListener((data) =>
            {
                animator.SetBool(boo_hovered, true);
            });
            et.triggers.Add(_entry_enter);

            EventTrigger.Entry _entry_exit = new EventTrigger.Entry();
            _entry_exit.eventID = EventTriggerType.PointerExit;
            _entry_exit.callback.AddListener((data) =>
            {
                animator.SetBool(boo_hovered, false);
            });
            et.triggers.Add(_entry_exit);

            EventTrigger.Entry _entry_click = new EventTrigger.Entry();
            _entry_click.eventID = EventTriggerType.PointerClick;
            _entry_click.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)((data) =>
            {
                ToggleSelect();
            }));
            et.triggers.Add(_entry_click);

            #endregion
        }

        public void Setup(List<LocationTag> tags, Action<bool, List<Location>> onSelected)
        {
            // Bundle Location and its raw data
            var locationAndData = new List<(Location, LocationData)>();
            foreach (var loc in locations)
                locationAndData.Add((loc, GameManager.Instance.GetLocationData(loc)));

            #region [Hide this if there's no unlocked location]

            if (locationAndData.Find(locNData=>locNData.Item2.IsUnlocked == true) == (null,null))
            {
                gameObject.SetActive(false);
                return;
            }

            #endregion

            #region [Setup tags]

            // Clear tags
            for (int i = tagsParent.childCount - 1; i >= 0; i--)
                Destroy(tagsParent.GetChild(i).gameObject);


            // Find all location tags to be displayed in MapPoint
            currentTags = new List<LocationTag>();
            foreach (var loc in locationAndData)
            {
                if (loc.Item2.IsUnlocked)
                {
                    foreach (var tag in loc.Item2.LocationTags)
                    {
                        currentTags.AddIfHasnt(tag.GetLocationTag(tags));
                    }
                }
            }

            foreach (var tag in currentTags)
                CreateTagImage(tag);

            if (currentTags.Count > 0)
                corAnimatingTags = StartCoroutine(AnimatingTags());

            #endregion

            this.OnSelected += onSelected;
        }

        public void Setdown()
        {
            if (animator == null) return; // has not been awaken

            OnSelected = null;
            if (corAnimatingTags != null) StopCoroutine(corAnimatingTags);

            isSelected = false;
            pointImage.color = unselectedColor;

            circleImage.color = unselectedColor;
            animator.SetBool(boo_clicked, false);
            animator.SetBool(boo_hovered, false);
        }

        /// <summary>Change image's size by zoom; Default zoomValue is 1, the bigger the zoom is, the smaller the image scale</summary>
        /// <param name="zoomValue"></param>
        public void SetZoom(float zoomValue, float normalZoom)
        {
            if (zoomValue < 150f) zoomValue = 150f;
            currentZoom = normalZoom / zoomValue;

            pointParent.transform.localScale = new Vector2(currentZoom,currentZoom);
            for (int i = 0; i < tagsParent.childCount; i++)
                tagsParent.GetChild(i).transform.localScale = new Vector2(currentZoom, currentZoom);
        }

        public void Select(bool isSelected)
        {
            OnSelected?.Invoke(isSelected, locations);

            this.isSelected = isSelected;
            animator.SetBool(boo_clicked, isSelected);
            if (corAnimatingTags != null) StopCoroutine(corAnimatingTags);

            if (isSelected)
            {
                pointImage.color = selectedColor;
                circleImage.color = selectedColor;
            }
            else
            {
                if (currentTags.Count == 0)
                {
                    pointImage.color = unselectedColor;
                    circleImage.color = unselectedColor;
                }
                else
                {
                    corAnimatingTags = StartCoroutine(AnimatingTags());
                }
            }

        }

        public void ToggleSelect()
        {
            Select(!isSelected);
        }

        IEnumerator AnimatingTags()
        {
            while (true)
            {
                int index = 0;
                foreach (var tag in currentTags)
                {
                    float time = 0;
                    var curve = AnimationCurve.EaseInOut(0, tagsParent.localPosition.x, tagTransitionDuration, -tagTransitionDistance * index);

                    while (time < tagTransitionDuration)
                    {
                        tagsParent.localPosition = new Vector2(curve.Evaluate(time), tagsParent.localPosition.y);
                        pointImage.color = Color.Lerp(pointImage.color, tag.Color, time/tagTransitionDuration);
                        circleImage.color = pointImage.color;

                        time += Time.deltaTime;
                        yield return null;
                    }

                    yield return new WaitForSeconds(2.5f) ;

                    index++;
                }
            }

        }

        /// <summary> Quickly add TagImage to the MapPoint; This function doesn't alter LocationTag in the GameManager </summary>
        public void AddTagImage(LocationTag tag)
        {
            if (currentTags.Contains(tag)) return;
            if(corAnimatingTags != null) StopCoroutine(corAnimatingTags);
            
            currentTags.Add(tag);
            CreateTagImage(tag);
            
            if(!isSelected)
                corAnimatingTags = StartCoroutine(AnimatingTags());
        }

        /// <summary> Quickly remove TagImage to the MapPoint; This function doesn't alter LocationTag in the GameManager </summary>
        public void RemoveTagImage(LocationTag tag)
        {
            if (!currentTags.Contains(tag)) return;
            if(corAnimatingTags != null) StopCoroutine(corAnimatingTags);

            currentTags.Remove(tag);
            for (int i = tagsParent.childCount - 1; i >= 0; i--)
            {
                if (tagsParent.GetChild(i).name == tag.TagName)
                    Destroy(tagsParent.GetChild(i).gameObject);
            }

            if (currentTags.Count > 0 && !isSelected)
                corAnimatingTags = StartCoroutine(AnimatingTags());
        }

        void CreateTagImage(LocationTag tag)
        {
            var imageGO = new GameObject(tag.TagName);
            imageGO.transform.parent = tagsParent;
            imageGO.transform.localScale = new Vector2(currentZoom, currentZoom);

            var imageRT = imageGO.AddComponent<RectTransform>();
            imageRT.sizeDelta = new Vector2(24, 24);

            var imageComp = imageGO.AddComponent<Image>();
            imageComp.sprite = tag.Icon;
            imageComp.raycastTarget = false;
        }

    }
}
