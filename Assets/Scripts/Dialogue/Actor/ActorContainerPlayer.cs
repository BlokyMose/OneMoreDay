using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/Actor Container Player")]
    public class ActorContainerPlayer : ActorContainer
    {
        [SerializeField, Required]
        DialogueUIContainer_Player dialogueUIContainerPlayer;

        public override DialogueUIContainer GetDialogueUIContainer()
        {
            return dialogueUIContainerPlayer;
        }
    }
}