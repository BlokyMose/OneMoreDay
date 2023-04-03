using Encore.Serializables;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Locations
{
    public static class LocationUtility
    {
        public static string CalculateDistanceString(this Location from, Location to)
        {
            if (from == to)
                return "Here";
            else
            {
                var distance = CalculateDistanceInteger(from, to);
                if (distance < 1) return "Nearby";
                else return distance + " hm";
            }
        }        

        public static int CalculateDistanceInteger(this Location from, Location to)
        {
            return Mathf.RoundToInt(Vector2.Distance(from.Coordinate, to.Coordinate)/100);
        }

        public static LocationData AddTag(this LocationData locationData, string tagName)
        {
            if (!locationData.LocationTags.Contains(tagName))
            {
                var locationTags = new List<string>() { tagName };
                foreach (var t in locationData.LocationTags)
                    locationTags.Add(t);

                return new LocationData(locationData.SceneName, locationTags, locationData.IsUnlocked);
            }
            else
                return locationData;
        }        
        
        public static LocationData RemoveTag(this LocationData locationData, string tagName)
        {
            if (locationData.LocationTags.Contains(tagName))
            {
                var locationTags = new List<string>();
                foreach (var t in locationData.LocationTags)
                    if (t != tagName) locationTags.Add(t);

                return new LocationData(locationData.SceneName, locationTags, locationData.IsUnlocked);
            }
            else
                return locationData;
        }

        public static LocationData Unlock(this LocationData locationData, bool isUnlocked)
        {
            return new LocationData(locationData.SceneName, locationData.LocationTags, isUnlocked);
        }

        public static LocationTag GetLocationTag(this string tagName, List<LocationTag> tags)
        {
            return tags.Find(tag => tag.TagName == tagName);
        }
    }
}