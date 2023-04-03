using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Encore.Locations;
using UnityEngine.UI;
using Encore.Serializables;
using UnityEngine.EventSystems;

namespace Encore.Phone.Map
{
    [AddComponentMenu("Encore/Phone/Map App Info Card")]
    public class MapAppInfoCard : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI locationNameText;

        [SerializeField]
        TextMeshProUGUI districtNameText;

        [SerializeField]
        TextMeshProUGUI distanceText;        
        
        [SerializeField]
        TextMeshProUGUI descText;

        [SerializeField]
        Transform imagesParent;

        [Header("Tags")]

        [SerializeField]
        Transform tagsParent;

        [SerializeField]
        LocationTag nonFavsTag;        
        
        [SerializeField]
        LocationTag favsTag;

        MapPoint mapPoint;
        LocationData locationData;
        Location location;

        public void Setup(Location location, LocationData locationData, MapPoint mapPoint, List<LocationTag> tagsInProject)
        {
            this.mapPoint = mapPoint;
            this.locationData = locationData;
            this.location = location;
            var currentLocation = SceneMasters.SceneMaster.current.Location;

            locationNameText.text = location.LocationDisplayName;
            districtNameText.text = location.District.DistrictName;
            distanceText.text = currentLocation.CalculateDistanceString(location);
            descText.text = location.Desc;

            #region [Images]

            // Reset images
            for (int i = imagesParent.childCount - 1; i >= 0; i--)
                Destroy(imagesParent.GetChild(i).gameObject);

            // Add new images
            int index = 0;
            foreach (var image in location.Images)
            {
                var imageGO = new GameObject("image_" + index);
                imageGO.transform.parent = imagesParent.transform;
                var imageComp = imageGO.AddComponent<Image>();
                imageComp.preserveAspect = true;
                imageComp.sprite = image;
                imageComp.SetNativeSize();
            }

            #endregion

            #region [Tags]

            // Reset tags
            for (int i = tagsParent.childCount - 1; i >= 0; i--)
                Destroy(tagsParent.GetChild(i).gameObject);

            // Add Favs or NonFavs button
            if (locationData.LocationTags.Find(tag => tag == "Favs") != null) AddTag(favsTag);
            else AddTag(nonFavsTag);

            // Add tags depending on locationData
            foreach (var tag in locationData.LocationTags)
                if (tag != "Favs")
                    AddTag(tagsInProject.Find(x=>x.TagName==tag));


            #endregion
        }

        void AddTag(LocationTag tag, int siblingIndex = -1)
        {
            #region [Add components]

            var tagGO = new GameObject("tag_" + tag.TagName);
            tagGO.transform.parent = tagsParent.transform;
            if (siblingIndex != -1)
                tagGO.transform.SetSiblingIndex(siblingIndex);

            var tagRT = tagGO.AddComponent<RectTransform>();
            tagRT.sizeDelta = new Vector2(48, 48);

            var tagImage = tagGO.AddComponent<Image>();
            tagImage.sprite = tag.Icon;

            #endregion

            #region [EventTriggers]

            EventTrigger tagGO_et = tagGO.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry tagGO_entry_enter = new EventTrigger.Entry();
            tagGO_entry_enter.eventID = EventTriggerType.PointerEnter;
            tagGO_entry_enter.callback.AddListener((data) =>
            {
                tagGO.transform.localScale = new Vector2(1.2f, 1.2f);
            });
            tagGO_et.triggers.Add(tagGO_entry_enter);

            EventTrigger.Entry tagGO_entry_exit = new EventTrigger.Entry();
            tagGO_entry_exit.eventID = EventTriggerType.PointerExit;
            tagGO_entry_exit.callback.AddListener((data) =>
            {
                tagGO.transform.localScale = new Vector2(1, 1);
            });
            tagGO_et.triggers.Add(tagGO_entry_exit);

            EventTrigger.Entry tagGO_entry_click = new EventTrigger.Entry();
            tagGO_entry_click.eventID = EventTriggerType.PointerClick;
            tagGO_entry_click.callback.AddListener((data) =>
            {
                OnClickTag();
            });
            tagGO_et.triggers.Add(tagGO_entry_click);

            #endregion

            // Different functionalities depending on the tag
            void OnClickTag()
            {
                switch (tag.TagName)
                {
                    case "Favs": OnFavsTag();
                        break;

                    case "NonFavs": OnNonFavsTag();
                        break;

                    case "ToDo": OnToDoTag(tag);
                        break;
                    case "New":
                        break;
                    case "Events":
                        break;

                }
            }
        }

        void OnNonFavsTag()
        {
            var newLocationDataFavs = locationData.AddTag(favsTag.TagName);
            GameManager.Instance.ChangeLocationData(newLocationDataFavs);
            mapPoint.AddTagImage(favsTag);
            Destroy(tagsParent.GetChild(0).gameObject);
            AddTag(favsTag, 0);
        }

        void OnFavsTag()
        {
            var newLocationDataNonFavs = locationData.RemoveTag(favsTag.TagName);
            GameManager.Instance.ChangeLocationData(newLocationDataNonFavs);
            mapPoint.RemoveTagImage(favsTag);
            Destroy(tagsParent.GetChild(0).gameObject);
            AddTag(nonFavsTag, 0);
        }

        void OnToDoTag(LocationTag tag)
        {
            var todos = new List<MapApp.FocusPanelData>();
            foreach (var todo in GameManager.Instance.PhoneManager.ToDoApp.GetToDos())
            {
                var todoTag = GameManager.Instance.PhoneManager.ToDoApp.AllTags.Find(t=>t.TagName == todo.Tag);
                todos.Add(new MapApp.FocusPanelData(todo.Text, todoTag.TagName, todoTag.Image));
            }

            GameManager.Instance.PhoneManager.MapApp.ShowFocusPanels(
                title: tag.TagName,
                titleIcon: tag.Icon,
                subtitle: "at " + location.LocationDisplayName,
                data: todos
                );
        }
    }
}
