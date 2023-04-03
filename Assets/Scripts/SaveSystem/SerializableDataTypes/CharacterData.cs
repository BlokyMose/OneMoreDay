using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Dialogues;

namespace Encore.Serializables
{
    [System.Serializable]
    public class CharacterData
    {
        #region [Vars: Properties]

        public string ActorName { get { return actorName; } }
        [ShowInInspector] string actorName;

        public string CurrentScene { get { return currentScene; } set { currentScene = value; } }
        [ShowInInspector] string currentScene;
        public Vector3 CurrentPos { get { return currentPos; } set { currentPos = value; } }
        [ShowInInspector] Vector3 currentPos;
        public Vector3 CurrentRot { get { return currentRot; } set { currentRot = value; } }
        [ShowInInspector] Vector3 currentRot;

        [SerializeField]
        Dictionary<string, CharacterSkin> characterSkins = new Dictionary<string, CharacterSkin>();

        #endregion

        #region [Classes]

        [System.Serializable]
        public class CharacterSkin
        {
            public string category;
            public string label;

            public CharacterSkin(string c, string l)
            {
                category = c;
                label = l;
            }
        }

        #endregion

        public CharacterData(Actor actor)
        {
            actorName = actor.ActorName;
            characterSkins = new Dictionary<string, CharacterSkin>();
        }

        #region [Methods: Skin]

        public CharacterSkin GetSkin(string bodyPart)
        {
            if (!characterSkins.ContainsKey(bodyPart))
            {
                characterSkins.Add(bodyPart, null);
            }

            return characterSkins[bodyPart];
        }

        public Dictionary<string, CharacterSkin> GetAllSkins()
        {
            return characterSkins;
        }

        public void SetSkin(string bodyPart, CharacterSkin skinData)
        {
            if (!characterSkins.ContainsKey(bodyPart))
            {
                characterSkins.Add(bodyPart, skinData);
            }
            else
            {
                characterSkins[bodyPart] = skinData;
            }
        }

        #endregion
    }
}