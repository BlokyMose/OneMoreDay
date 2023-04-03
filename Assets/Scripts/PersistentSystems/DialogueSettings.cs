using System;
using UnityEngine;

namespace Encore.Dialogues
{
    [Serializable]
    public class DialogueSettings
    {
        [SerializeField]
        int overrideMaxCharacters = -1;

        [SerializeField]
        bool actorsAreFacingPlayer = true;

        public bool ActorsAreFacingPlayer { get => actorsAreFacingPlayer; private set => actorsAreFacingPlayer = value; }
        public int OverrideMaxCharacters { get => overrideMaxCharacters; private set => overrideMaxCharacters = value; }

        public DialogueSettings(bool actorsAreFacingPlayer, int overrideMaxCharacters)
        {
            this.actorsAreFacingPlayer = actorsAreFacingPlayer;
            this.overrideMaxCharacters = overrideMaxCharacters;
        }

        public DialogueSettings()
        {
            actorsAreFacingPlayer = true;
            overrideMaxCharacters = -1;
        }
    }
}