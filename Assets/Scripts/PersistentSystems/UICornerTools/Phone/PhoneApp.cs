using Encore.Phone.Chat;
using Encore.Phone.Map;
using Encore.Phone.ToDo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Phone
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PhoneApp : MonoBehaviour
    {
        public Sprite NotificationIcon { get { return notificationIcon; } }
        [SerializeField] protected Sprite notificationIcon;
        [SerializeField] protected AudioClip notificationSound;

        protected CanvasGroup canvasGroup;
        protected bool isShow = false;

        public Action<PhoneManager.NotificationData> OnAddNotification;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            Show(false);
        }

        public virtual void Show(bool isShow)
        {
            this.isShow = isShow;
            canvasGroup.interactable = isShow;
            canvasGroup.blocksRaycasts = isShow;
        }

    }
}