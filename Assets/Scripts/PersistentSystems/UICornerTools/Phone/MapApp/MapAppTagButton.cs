using Encore.Locations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.Phone.Map
{
    [AddComponentMenu("Encore/Phone/Map App Tag Button")]
    public class MapAppTagButton : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI tagNameText;

        [SerializeField]
        Image icon;

        [SerializeField]
        Image bg;


        public void Setup(LocationTag tag, Action<LocationTag> onClickCallback, List<LocationTag> tags)
        {
            tagNameText.text = tag.TagName;
            icon.sprite = tag.Icon;
            bg.color = tag.Color;


            EventTrigger et = gameObject.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry_enter = new EventTrigger.Entry();
            entry_enter.eventID = EventTriggerType.PointerEnter;
            entry_enter.callback.AddListener((data) =>
            {
                transform.localScale = new Vector2(1.2f, 1.2f);
            });
            et.triggers.Add(entry_enter);            
            
            EventTrigger.Entry entry_exit = new EventTrigger.Entry();
            entry_exit.eventID = EventTriggerType.PointerExit;
            entry_exit.callback.AddListener((data) =>
            {
                transform.localScale = new Vector2(1, 1);
            });
            et.triggers.Add(entry_exit);

            EventTrigger.Entry entry_click = new EventTrigger.Entry();
            entry_click.eventID = EventTriggerType.PointerClick;
            entry_click.callback.AddListener((data) =>
            {
                onClickCallback(tag);
            });
            et.triggers.Add(entry_click);
        }
    }
}