using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Dialogues;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    /// <summary>
    /// 
    /// [GENERAL IDEA]
    /// Control all SpeakingHooks in this gameObject, instead of just one
    /// 
    /// [FLOW]
    /// - Add all hooks in this gameObject to a list
    /// - Trigger all hooks once interacted
    /// 
    /// </summary>
    [AddComponentMenu("Encore/Interactables/Speaking Trigger")]
    public class SpeakingTriggerInteractable : Interactable, ISpeakingTrigger
    {
        #region [Buttons]

        private void OnValidate()
        {
            if (speakingHooks.Count > 0)
                for (int i = speakingHooks.Count-1; i >= 0; i--)
                {
                    if (speakingHooks[i] == null) speakingHooks.Remove(speakingHooks[i]);
                }
        }

        [FoldoutGroup("Add Speaking Hook", expanded: true)]
        [HorizontalGroup("Add Speaking Hook/Buttons_1")]
        [Button("Mono")]
        public void AddMonologueHook()
        {
            speakingHooks.Add(gameObject.AddComponent<MonologueHook>());
        }

        [HorizontalGroup("Add Speaking Hook/Buttons_1")]
        [Button("MultiMono")]
        public void AddMultiMonologueHook()
        {
            speakingHooks.Add(gameObject.AddComponent<MultiMonologueHook>());
        }

        [HorizontalGroup("Add Speaking Hook/Buttons_1")]
        [Button("Dia")]
        public void AddDialogueHook()
        {
            speakingHooks.Add(gameObject.AddComponent<DialogueHook>());
        }

        [HorizontalGroup("Add Speaking Hook/Buttons_2")]
        [Button("Chat")]
        public void AddChatHook()
        {
            speakingHooks.Add(gameObject.AddComponent<ChatHook>());
        }

        [HorizontalGroup("Add Speaking Hook/Buttons_2")]
        [Button("DSyntax")]
        public void AddDSyntaxHook()
        {
            speakingHooks.Add(gameObject.AddComponent<DSyntaxHook>());
        }

        #endregion

        public enum TriggerOrder { AllAtOnce, OneByOne }
        [Title("Trigger Properties")]
        [SerializeField] TriggerOrder triggerOrder = TriggerOrder.AllAtOnce;

        public List<SpeakingHook> SpeakingHooks { get { return speakingHooks; } }
        [SerializeField] List<SpeakingHook> speakingHooks = new List<SpeakingHook>();
        [ShowIf("@triggerOrder==TriggerOrder.OneByOne")]
        [SerializeField] int currentHookIndex = 0;

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return SpeakingHooks[currentHookIndex] is DialogueHook
                ? CursorImageManager.CursorImage.Dialogue
                : SpeakingHooks[currentHookIndex] is MonologueHook
                ? CursorImageManager.CursorImage.Monologue
                : SpeakingHooks[currentHookIndex] is MultiMonologueHook
                ? CursorImageManager.CursorImage.MultiMonologue
                : CursorImageManager.CursorImage.Normal;
        }

        protected override void InteractModule(GameObject interactor)
        {
            switch (triggerOrder)
            {
                case TriggerOrder.AllAtOnce: TriggerAllHooks();
                    break;
                case TriggerOrder.OneByOne:

                    // Try trigger enabled Hook by Priority order
                    int preventInfiniteLoop = SpeakingHooks.Count;

                    // Try a triggerable hook, or keep looking
                    while (!TriggerHook(currentHookIndex) && preventInfiniteLoop > 0)
                    {
                        currentHookIndex = currentHookIndex >= SpeakingHooks.Count - 1 ? 0 : currentHookIndex + 1;
                        preventInfiniteLoop--;
                    }
                    currentHookIndex = currentHookIndex >= SpeakingHooks.Count - 1 ? 0 : currentHookIndex + 1;
                    break;
                default:
                    break;
            }
        }

        public void TriggerAllHooks()
        {
            for (int i = 0; i < SpeakingHooks.Count; i++)
            {
                TriggerHook(i);
            }
        }

        /// <param name="index">SpeakingHooks index</param>
        /// <returns>Succeed to trigger enabled SpeakingHook</returns>
        public bool TriggerHook(int index)
        {
            if (SpeakingHooks[index].enabled)
            {
                return SpeakingHooks[index].Speak();
            }
            return false;
        }
    }
}

