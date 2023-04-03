using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Encore.Phone.Chat
{
    [CreateAssetMenu(menuName = "SO/ChatApp/Chat Contact Data", fileName = "ChatContact_")]
    [Serializable]
    public class ChatContact : ScriptableObject
    {
        [HorizontalGroup("row0")]
        [SerializeField, VerticalGroup("row0/col1")]
        string contactName;
        [SerializeField, VerticalGroup("row0/col1")]
        int phoneNumber;
        [SerializeField, VerticalGroup("row0/col1")]
        string status = "Online";
        [SerializeField, VerticalGroup("row0/col0", order: -1)]
        [PreviewField(Alignment = ObjectFieldAlignment.Left)]
        [LabelWidth(0.1f)]
        Sprite photo;

        public string ContactName { get { return contactName; } }
        public int PhoneNumber { get { return phoneNumber; } }
        public string Status { get { return status; } }
        public Sprite Photo { get { return photo; } }

        public bool HasAlert { get; set; }

        public List<ChatData> savedDialogue = new List<ChatData>();

        public ChatContact(string contactName, int phoneNumber, string status, Sprite photo)
        {
            this.contactName = contactName;
            this.phoneNumber = phoneNumber;
            this.status = status;
            this.photo = photo;
        }
    }
}