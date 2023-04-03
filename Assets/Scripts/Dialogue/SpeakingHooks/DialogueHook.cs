using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/Dialogue Hook")]
    public class DialogueHook : SpeakingHook
    {
        [SerializeField] bool actorsFacePlayer = true;
        [SerializeField] int overrideMaxCharacters = -1;
        [SerializeField] List<DialogueBundle> bundles = new List<DialogueBundle>();

        protected override IList GetList() { return bundles; }

        protected override bool TriggerSpeaking(int index)
        {
            base.TriggerSpeaking(index);
            return GameManager.Instance.DialogueManager.BeginDialogue(bundles[index]);
        }
    }
}