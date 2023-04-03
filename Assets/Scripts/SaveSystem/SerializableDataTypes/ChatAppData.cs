using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class ChatAppData
    {
        public List<ChatContactData> contactsData = new List<ChatContactData>();

        public ChatAppData(List<ChatContactData> contactsData)
        {
            this.contactsData = contactsData;
        }
    }
}