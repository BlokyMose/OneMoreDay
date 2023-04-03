using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;
using Encore.Dialogues;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    /// <summary>
    /// 
    /// [GENERAL IDEA] 
    /// - Control one DialogueHook, so it may be interacted with
    /// - Recommended to use SpeakingTriggerInteractable instead
    /// 
    /// </summary>
    [RequireComponent(typeof(DialogueHook))]
    [AddComponentMenu("Encore/Interactables/Dialogue Trigger")]
    public class DialogueTrigger : Interactable, ISpeakingTrigger
    {
        public List<SpeakingHook> SpeakingHooks { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SpeakingHooks = new List<SpeakingHook>() { GetComponent<DialogueHook>() };
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return CursorImageManager.CursorImage.Dialogue;
        }

        protected override void InteractModule(GameObject interactor)
        {
            TriggerAllHooks();
        }

        public void TriggerAllHooks()
        {
            SpeakingHooks[0].Speak();
        }
    }

}
