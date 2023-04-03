using Encore.Locations;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class LocationData
    {
        [SerializeField]
        string sceneName;
        [SerializeField]
        List<string> locationTags;
        [SerializeField]
        bool isUnlocked;
        [SerializeField]
        bool isInitialized;

        public string SceneName { get { return sceneName; } }
        public List<string> LocationTags { get { return locationTags; } }
        public bool IsUnlocked { get { return isUnlocked; } }

        public LocationData(string sceneName, List<string> locationTags, bool isUnlocked)
        {
            this.sceneName = sceneName;
            this.locationTags = locationTags;
            this.isUnlocked = isUnlocked;
        }        
        
        public LocationData(Location location, List<LocationTag> currentTags)
        {
            this.sceneName = location.SceneName;

            var tagsStrings = new List<string>();
            foreach (var tag in currentTags)
                tagsStrings.Add(tag.TagName);

            this.locationTags = tagsStrings;
            this.isUnlocked = location.InitialIsUnlocked;
        }

        public void ChangeData(LocationData newData)
        {
            this.locationTags = new List<string>();
            foreach (var tag in newData.locationTags)
                this.locationTags.Add(tag);

            this.isUnlocked = newData.isUnlocked;
        }

        
    }
}