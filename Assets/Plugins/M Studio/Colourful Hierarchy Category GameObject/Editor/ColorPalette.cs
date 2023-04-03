using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MStudio
{
    /// <summary>
    /// Details of custom design
    /// </summary>
    [System.Serializable]
    public class ColorDesign
    {
        public string token;
        public string newName;
        public Color textColor;
        public Color backgroundColor = Color.white;
        public TextAnchor textAlignment = TextAnchor.UpperLeft;
        public FontStyle fontStyle = FontStyle.Normal;

        public Vector2Int textOffset = new Vector2Int(18, 0);
        public Vector2 backgroundRectOffset = new Vector2(16, 0);
    }

    /// <summary>
    /// ScriptableObject:Save list of ColorDesign
    /// </summary>
    public class ColorPalette : ScriptableObject
    {
        public List<ColorDesign> colorDesigns = new List<ColorDesign>();
    }
}