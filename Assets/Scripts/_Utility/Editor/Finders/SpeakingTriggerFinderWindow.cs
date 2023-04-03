using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using Encore.Dialogues;
using Encore.Interactables;

public class SpeakingTriggerFinderWindow : OdinEditorWindow
{
    #region [Editor]

    [MenuItem("Tools/Finder/Speaking Trigger Finder")]
    public static void OpenWindow()
    {
        GetWindow<SpeakingTriggerFinderWindow>("ST Finder").Show();
    }

    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        Refresh();
    }

    #region [Speaking Triggers]

    [Title("Speaking Triggers")]

    [VerticalGroup("Tri")]
    public List<ISpeakingTrigger> speakingTriggers;

    [VerticalGroup("Tri")]
    public void FindAllSpeakingTriggers()
    {
        speakingTriggers = new List<ISpeakingTrigger>();

        speakingTriggers.AddRange(Resources.FindObjectsOfTypeAll<SpeakingTriggerCollider>());
        speakingTriggers.AddRange(Resources.FindObjectsOfTypeAll<SpeakingTriggerInteractable>());
        speakingTriggers.AddRange(Resources.FindObjectsOfTypeAll<DialogueTrigger>());
        speakingTriggers.AddRange(Resources.FindObjectsOfTypeAll<MonologueTrigger>());
        speakingTriggers.AddRange(Resources.FindObjectsOfTypeAll<ChatTrigger>());
    }

    #endregion

    #region [SpeakingHooks]

    [Title("Speaking Hooks")]

    [VerticalGroup("Hook"), EnumToggleButtons, OnValueChanged(nameof(FindAllSpeakingHooks)), HideLabel]
    public SpeakingHookFilter hookFilter = SpeakingHookFilter.All;
    public enum SpeakingHookFilter { All, Mono, MultiMono, Dia, Chat }

    [VerticalGroup("Hook")]
    public List<SpeakingHook> speakingHooks;

    [VerticalGroup("Hook")]
    public void FindAllSpeakingHooks()
    {
        switch (hookFilter)
        {
            case SpeakingHookFilter.All:
                speakingHooks = new List<SpeakingHook>(Resources.FindObjectsOfTypeAll<SpeakingHook>());
                break;
            case SpeakingHookFilter.Mono:
                speakingHooks = new List<SpeakingHook>(Resources.FindObjectsOfTypeAll<MonologueHook>());
                break;
            case SpeakingHookFilter.MultiMono:
                speakingHooks = new List<SpeakingHook>(Resources.FindObjectsOfTypeAll<MultiMonologueHook>());
                break;
            case SpeakingHookFilter.Dia:
                speakingHooks = new List<SpeakingHook>(Resources.FindObjectsOfTypeAll<DialogueHook>());
                break;
            case SpeakingHookFilter.Chat:
                speakingHooks = new List<SpeakingHook>(Resources.FindObjectsOfTypeAll<ChatHook>());
                break;
        }
    } 
    #endregion

    [HorizontalGroup("But"), Button(ButtonSizes.Large), GUIColor("@Color.green")]
    public void Refresh()
    {
        FindAllSpeakingTriggers();
        FindAllSpeakingHooks();
    }

    [HorizontalGroup("But"), Button("Ping Script", ButtonSizes.Large)]
    public void PingScript()
    {
        var scriptGUID = AssetDatabase.FindAssets(nameof(SpeakingTriggerFinderWindow) + " t:Script");
        Selection.activeObject = EditorGUIUtility.Load(AssetDatabase.GUIDToAssetPath(scriptGUID[0]));
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
