using System.Collections;
using UnityEngine;

namespace Encore.Locations
{
    [CreateAssetMenu(menuName = "SO/Location/Location Tag", fileName ="LocTag_")]
    public class LocationTag : ScriptableObject
    {
        [SerializeField]
        string tagName;

        [SerializeField, TextArea(3, 5)]
        string desc;

        [SerializeField]
        Sprite icon;

        [SerializeField]
        Color color;

        [SerializeField]
        Color accentColor;

        [SerializeField]
        Color textColor;

        public string TagName { get { return tagName; } }
        public string Desc { get { return desc; } }
        public Sprite Icon { get { return icon; } }
        public Color Color { get { return color; } }
        public Color AccentColor { get { return accentColor; } }
        public Color TextColor { get { return textColor; } }
    }
}