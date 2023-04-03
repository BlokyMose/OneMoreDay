using UnityEngine;
using NodeCanvas.DialogueTrees;
using System;

namespace Encore.Dialogues
{
    [Serializable]
    public class Monologue
    {
        [Serializable]
        public class MonologueSettings
        {
            [SerializeField]
            int overrideMaxCharacters = -1;

            public int OverrideMaxCharacters { get => overrideMaxCharacters; private set => overrideMaxCharacters = value; }

            public MonologueSettings(int overrideMaxCharacters)
            {
                this.overrideMaxCharacters = overrideMaxCharacters;
            }
        }

        [SerializeField] Actor actor;
        [SerializeField] Statement statement;
        [SerializeField] MonologueSettings settings;

        public Actor Actor { get => actor; set => actor = value; }
        public Statement Statement { get => statement; set => statement = value; }
        public MonologueSettings Settings { get => settings; set => settings = value; }

        public Monologue(Actor actor, Statement statement, MonologueSettings settings)
        {
            this.actor = actor;
            this.statement = statement;
            this.settings = settings;
        }
    }
}