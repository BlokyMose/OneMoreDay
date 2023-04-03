using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using NodeCanvas.DialogueTrees;
using Encore.Dialogues;

public interface ISpeakingTrigger
{
    public List<SpeakingHook> SpeakingHooks { get; }
    public void TriggerAllHooks();
}
