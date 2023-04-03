using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Dialogues
{
    [CreateAssetMenu(menuName = "SO/Actor", fileName = "actor_")]
    [InlineEditor]
    [System.Serializable]
    public class Actor : ScriptableObject
    {
        [SerializeField]
        string actorName;
        public string ActorName { get { return actorName; } }

        public Actor(string actorName)
        {
            this.actorName = actorName;
        }
    }
}