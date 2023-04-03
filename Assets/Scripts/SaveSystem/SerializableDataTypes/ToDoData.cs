using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class ToDoData
    {
        string id;
        int status;
        string text;
        string tag;
        string location;
        bool isKnownToPlayer;
        string csv;        
        
        public string ID { get { return id; } set { id = value; } }
        public int Status { get { return status; } set { status = value; } }
        /// <summary>Returns the text of ToDo or its localised version if exists</summary>
        public string Text { 
            get 
            {
                if (csv == null)
                {
                    return text; 
                }
                else
                {
                    var dictionary = Utility.CSVUtility.GetColumn(csv, GameManager.Instance.LanguageCode);
                    if (dictionary.Count == 0 || !dictionary.ContainsKey(nameof(text)))
                    {
                        return text;
                    }
                    else
                    {
                        return dictionary[nameof(text)];
                    }
                }
            } 
            set { text = value; } }
        public string Tag { get { return tag; } set { tag = value; } }
        public string Location { get { return location; } set { location = value; } }
        public bool IsKnownToPlayer { get { return isKnownToPlayer; } set { isKnownToPlayer = value; } }
        public string CSV { get { return csv; } set { csv = value; } }

        public ToDoData(string id, int status, string text, string tag, string location, bool isKnownToPlayer = false, string csv = "")
        {
            this.id = id;
            this.status = status;
            this.text = text;
            this.tag = tag;
            this.location = location;
            this.isKnownToPlayer = isKnownToPlayer;
            this.csv = csv;
        }

        public ToDoData(Phone.ToDo.ToDoData data, bool isKnownToPlayer)
        {
            id = data.ID;
            text = data.Text;
            status = (int)data.Status;
            tag = data.Tag.TagName;
            location = data.Location != null ? data.Location.SceneName : "";
            this.isKnownToPlayer = isKnownToPlayer;
            if (data.CSV!=null)
                this.csv = data.CSV.text;
        }

        
    }
}