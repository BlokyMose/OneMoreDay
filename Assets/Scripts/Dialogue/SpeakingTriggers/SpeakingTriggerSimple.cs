using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using NodeCanvas.DialogueTrees;
using Encore.Dialogues;

[RequireComponent(typeof(Collider2D))]
    [AddComponentMenu("Encore/Dialogues/Speaking Trigger Simple")]
public class SpeakingTriggerSimple : MonoBehaviour, ISpeakingTrigger
{
    #region [Buttons]

    [FoldoutGroup("Add Speaking Hook", expanded: true)]
    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("Monologue")]
    public void AddMonologueHook()
    {
        speakingHooks.Add(gameObject.AddComponent<MonologueHook>());
    }

    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("MultiMono")]
    public void AddMultiMonologueHook()
    {
        speakingHooks.Add(gameObject.AddComponent<MultiMonologueHook>());
    }

    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("Dialogue")]
    public void AddDialogueHook()
    {
        speakingHooks.Add(gameObject.AddComponent<DialogueHook>());
    }

    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("Chat")]
    public void AddChatHook()
    {
        speakingHooks.Add(gameObject.AddComponent<ChatHook>());
    }

    #endregion

    [Title("Trigger Properties")]
    public enum TriggerOrder { AllAtOnce, OneByOne }
    [SerializeField] TriggerOrder triggerOrder = TriggerOrder.AllAtOnce;

    public List<SpeakingHook> SpeakingHooks { get { return speakingHooks; } }
    [SerializeField] List<SpeakingHook> speakingHooks;
    [ShowIf("@triggerOrder==TriggerOrder.OneByOne")]
    [SerializeField] int currentHookIndex = 0;

    public void Speak()
    {
        switch (triggerOrder)
        {
            case TriggerOrder.AllAtOnce:
                TriggerAllHooks();
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
