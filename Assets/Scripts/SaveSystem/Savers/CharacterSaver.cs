using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using Encore.Dialogues;
using Encore.Serializables;

namespace Encore.Saves
{
    [RequireComponent(typeof(ActorContainer))]
    [AddComponentMenu("Encore/Saves/Character Saver")]
    public class CharacterSaver : ObjectSaver
    {
        [Title("Character Saver")]

        [SerializeField]
        protected List<UnityEngine.U2D.Animation.SpriteResolver> playerSkins;

        protected ActorContainer actorContainer;

        private void Awake()
        {
            actorContainer = GetComponent<ActorContainer>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            saveKey = GetComponent<ActorContainer>().Actor.ActorName;
        }

        #region [Methods: ISaver]

        public override void SaveModule(GameManager.GameAssets gameAssets)
        {
            var characterData = gameAssets.globalData.GetCharacterData(actorContainer.Actor);

            // Save characterSkin
            foreach (UnityEngine.U2D.Animation.SpriteResolver s in playerSkins)
            {
                characterData.SetSkin(s.name, new CharacterData.CharacterSkin(s.GetCategory(), s.GetLabel()));
            }

            // Save currentScene
            characterData.CurrentScene = SceneManager.GetActiveScene().name;

            // Save currentPos
            characterData.CurrentPos = transform.position;

            // Save currentRot
            characterData.CurrentRot = transform.eulerAngles;
        }

        public override void LoadModule(GameManager.GameAssets gameAssets)
        {
            var characterData = gameAssets.globalData.GetCharacterData(actorContainer.Actor);

            // Apply characterSkin
            foreach (KeyValuePair<string, CharacterData.CharacterSkin> ps in characterData.GetAllSkins())
            {
                UnityEngine.U2D.Animation.SpriteResolver s = playerSkins.Find(p => p.name.Equals(ps.Key));

                if (s != null)
                {
                    s.SetCategoryAndLabel(ps.Value.category, ps.Value.label);
                }
            }

            // Apply loaded data if character already saved in the previous game
            if (!string.IsNullOrEmpty(characterData.CurrentScene))
            {
                // Apply currentScene
                if (CheckInstantiation(gameAssets))
                {
                    isDestroyed = true;
                    DestroyImmediate(gameObject);
                    return;
                }

                // Apply currentPos
                transform.position = characterData.CurrentPos;

                // Apply currentRot
                transform.eulerAngles = characterData.CurrentRot;

                // Apply rotation to dialogueUIContainer if exists
                if (actorContainer.GetDialogueUIContainer() != null)
                    actorContainer.GetDialogueUIContainer().Flip(transform.eulerAngles);
            }

        }

        #endregion

        protected virtual bool CheckInstantiation(GameManager.GameAssets gameAssets)
        {
            return SceneManager.GetActiveScene().name != gameAssets.globalData.GetCharacterData(actorContainer.Actor).CurrentScene;
        }
    }
}