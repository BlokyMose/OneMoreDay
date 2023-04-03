using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class PhoneManagerData 
    {
        public List<NotificationData> notificationData;

        public PhoneManagerData(List<NotificationData> notificationData)
        {
            this.notificationData = notificationData;
        }
    }
}
