using Encore.Phone;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class NotificationData
    {
        public int appName;
        public string time;
        public string title;
        public string desc;

        public NotificationData(int appName, string time, string title, string desc)
        {
            this.appName = appName;
            this.time = time;
            this.title = title;
            this.desc = desc;
        }        
        
        public NotificationData(PhoneManager.NotificationData notificationData)
        {
            this.appName = (int)notificationData.AppName;
            this.time = notificationData.Time;
            this.title = notificationData.Title;
            this.desc = notificationData.Desc;
        }
    }
}
