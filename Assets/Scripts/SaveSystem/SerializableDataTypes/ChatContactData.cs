using Encore.Phone.Chat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class ChatContactData
    {
        public string ContactName;
        public int PhoneNumber;
        public string Status;
        public bool HasAlert;
        public List<ChatData> SavedDialogue;

        public ChatContactData(ChatContact data)
        {
            ContactName = data.ContactName;
            PhoneNumber = data.PhoneNumber;
            Status = data.Status;
            HasAlert = data.HasAlert;
            SavedDialogue = new List<ChatData>();

            foreach (ChatData chat in data.savedDialogue)
            {
                SavedDialogue.Add(new ChatData(chat.ActorType, chat.Say));
            }
        }
    }
}
