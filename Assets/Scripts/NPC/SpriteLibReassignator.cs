using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace Encore.Utility.Sprites
{
    [AddComponentMenu("Encore/Utility/Sprite Lib Reassignator")]
    public class SpriteLibReassignator : MonoBehaviour
    {
        [InfoBox("You may delete this after assigning the new SpriteLibraryAsset")]
        public UnityEngine.U2D.Animation.SpriteLibraryAsset newAsset;
        public List<UnityEngine.U2D.Animation.SpriteLibrary> spriteLibraries;
        public string targetCategoryName = "default";

        [Button(ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.3f, 1f)]
        public void AssignNewAssetToSpriteLibs()
        {
            foreach (var spriteLib in spriteLibraries)
            {
                spriteLib.spriteLibraryAsset = newAsset;
                UnityEngine.U2D.Animation.SpriteResolver sr = spriteLib.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>();
                sr.SetCategoryAndLabel(targetCategoryName, sr.GetLabel());
            }
        }
    }
}