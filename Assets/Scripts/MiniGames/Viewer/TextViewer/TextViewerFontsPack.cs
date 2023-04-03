using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Encore.MiniGames.Viewers
{
    [CreateAssetMenu(fileName = "fontsPack_", menuName = "SO/MiniGames/TextViewerFontsPack")]
    public class TextViewerFontsPack : ScriptableObject
    {
        public string fontsPackName;
        public TMP_FontAsset titleFont;
        public TMP_FontAsset headerFont;
        public TMP_FontAsset bodyFont;
    }
}