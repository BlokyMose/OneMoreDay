using Encore.Serializables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    //[System.Serializable]
    //public class PlayerSkinData
    //{
    //    public string category;
    //    public string label;

    //    public PlayerSkinData(string c, string l)
    //    {
    //        category = string.Copy(c);
    //        label = string.Copy(l);
    //    }
    //}

    [System.Serializable]
    public class PlayerData
    {
        [SerializeField] private Dictionary<string, CharacterData.CharacterSkin> playerSkins;

        public PlayerData()
        {
            playerSkins = new Dictionary<string, CharacterData.CharacterSkin>();
        }

        public CharacterData.CharacterSkin GetSkin(string bodyPart)
        {
            if (!playerSkins.ContainsKey(bodyPart))
            {
                playerSkins.Add(bodyPart, null);
            }

            return playerSkins[bodyPart];
        }

        public Dictionary<string, CharacterData.CharacterSkin> GetAllSkins()
        {
            return playerSkins;
        }

        public void SetSkin(string bodyPart, CharacterData.CharacterSkin skinData)
        {
            if (!playerSkins.ContainsKey(bodyPart))
            {
                playerSkins.Add(bodyPart, skinData);
            }
            else
            {
                playerSkins[bodyPart] = skinData;
            }
        }
    }
}