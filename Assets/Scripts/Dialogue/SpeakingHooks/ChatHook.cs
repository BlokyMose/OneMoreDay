using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/Chat Hook")]
    public class ChatHook : SpeakingHook
    {
        [SerializeField] [Required] List<DialogueTree> dialogueTrees = new List<DialogueTree>();

        protected override IList GetList() { return dialogueTrees; }

        protected override bool TriggerSpeaking(int index)
        {
            base.TriggerSpeaking(index);
            GameManager.Instance.PhoneManager.ChatApp.BeginDialogue(dialogueTrees[index]);
            return true;
        }
    }
}