using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;
using Encore.Dialogues;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    /// <summary>
    /// 
    /// [GENERAL IDEA] 
    /// - Control one MonologueHook, so it may be interacted with
    /// - Recommended to use SpeakingTriggerInteractable instead
    /// 
    /// </summary>
    [RequireComponent(typeof(MonologueHook))]
    [AddComponentMenu("Encore/Interactables/Monologue Trigger")]
    public class MonologueTrigger : Interactable, ISpeakingTrigger
    {
        public List<SpeakingHook> SpeakingHooks { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SpeakingHooks = new List<SpeakingHook>() { GetComponent<MonologueHook>() };
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return CursorImageManager.CursorImage.Monologue;
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
