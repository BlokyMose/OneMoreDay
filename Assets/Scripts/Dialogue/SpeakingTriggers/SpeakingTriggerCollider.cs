using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using NodeCanvas.DialogueTrees;
using Encore.Dialogues;
using Encore.Interactables;

[RequireComponent(typeof(Collider2D))]
[AddComponentMenu("Encore/Dialogues/Speaking Trigger Collider")]
public class SpeakingTriggerCollider : MonoBehaviour, ISpeakingTrigger
{
    #region [Buttons]

    [FoldoutGroup("Add Speaking Hook", expanded: true)]
    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("Monologue")]
    public void AddMonologueHook()
    {
        gameObject.AddComponent<MonologueHook>();
    }

    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("MultiMono")]
    public void AddMultiMonologueHook()
    {
        gameObject.AddComponent<MultiMonologueHook>();
    }

    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("Dialogue")]
    public void AddDialogueHook()
    {
        gameObject.AddComponent<DialogueHook>();
    }

    [HorizontalGroup("Add Speaking Hook/Buttons")]
    [Button("Chat")]
    public void AddChatHook()
    {
        gameObject.AddComponent<ChatHook>();
    }

    #endregion

    #region [Vars: Properties]

    [SerializeField]
    Actor collidingActorName;

    [SerializeField, Range(0, 1f)]
    float chanceToTrigger = 1f;

    public enum RemovalMode { DestroyAfter, RemoveAfter }
    [HorizontalGroup("Mode"), SerializeField, HideLabel]
    RemovalMode removalMode = RemovalMode.DestroyAfter;

    [HorizontalGroup("Mode"), SerializeField, SuffixLabel("triggers"), Tooltip("To never destroy, write -1"),HideLabel]
    int removalCount = 1;

    [SerializeField]
    SpeakingTriggerInteractable.TriggerOrder triggerOrder = SpeakingTriggerInteractable.TriggerOrder.AllAtOnce;

    [ShowIf("@"+nameof(triggerOrder)+ "==SpeakingTriggerInteractable.TriggerOrder.OneByOne")]
    [SerializeField] int currentHookIndex = 0;

    #endregion

    #region [Vars: Data Handlers]

    Collider2D col;
    int triggerCount = 0;

    #endregion

    public List<SpeakingHook> SpeakingHooks { get; private set; }

    void Awake()
    {
        SpeakingHooks = new List<SpeakingHook>(GetComponents<SpeakingHook>());
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDialogueActor actor = collision.gameObject.GetComponent<IDialogueActor>();
        if (actor != null)
        {
            if (Random.Range(0, 1f) > chanceToTrigger) return;
            if (actor.name == collidingActorName.ActorName) ProcessTrigger();
        }
    }

    public void ProcessTrigger()
    {
        triggerCount++;

        switch (triggerOrder)
        {
            case SpeakingTriggerInteractable.TriggerOrder.AllAtOnce:
                TriggerAllHooks();
                break;
            case SpeakingTriggerInteractable.TriggerOrder.OneByOne:
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
        }

        if (removalCount > 0 && triggerCount >= removalCount)
        {
            SpeakingHooks.Clear();

            if(removalMode == RemovalMode.DestroyAfter)
                Destroy(gameObject,0.5f);
            else if (removalMode == RemovalMode.RemoveAfter)
                Destroy(this, 0.5f);
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
