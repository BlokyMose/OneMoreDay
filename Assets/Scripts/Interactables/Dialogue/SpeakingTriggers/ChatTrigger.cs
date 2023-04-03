using Encore.Dialogues;
using Encore.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Interactables
{
    /// <summary>
    /// 
    /// [GENERAL IDEA] 
    /// - Control one ChatHook, so it may be interacted with
    /// - Recommended to use SpeakingTriggerInteractable instead
    /// 
    /// </summary>
    [AddComponentMenu("Encore/Interactables/Chat Trigger")]
    [RequireComponent(typeof(ChatHook))]
    public class ChatTrigger : Interactable, ISpeakingTrigger
    {
        public List<SpeakingHook> SpeakingHooks { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            SpeakingHooks = new List<SpeakingHook>() { GetComponent<ChatHook>() };
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


