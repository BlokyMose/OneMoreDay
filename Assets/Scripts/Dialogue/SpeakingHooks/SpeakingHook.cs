using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.VisualScripting;
using System;

namespace Encore.Dialogues
{
    public abstract class SpeakingHook : MonoBehaviour
    {
        [HorizontalGroup]
        [SuffixLabel("triggers")]
        [Tooltip("Prevent triggering this hook after being triggered x times; If 0, will never be disabled")]
        [LabelText("Disable After")]
        [LabelWidth(80)]
        [SerializeField] protected int disableAfter = 0;
        public int DisableAfter { get { return disableAfter; } }

        [HorizontalGroup]
        [PropertyTooltip("Delay triggering the dialogue; If delay > 0, cannot verify if the trigger succeed or not")]
        [LabelWidth(40)]
        [SerializeField] protected float delay = 0;

        [SerializeField, InlineButton(nameof(_AddTriggerInterscript), "Add", ShowIf ="@!"+nameof(triggerInterscript))]
        TriggerInterscriptEvent triggerInterscript;

        // Data handlers
        protected int currentIndex = 0;
        protected int triggerCount = 0;
        public Action<bool> OnSpeaking;
        bool isSpoken = false;
        const string isDialogueManagerSpeaking = "isDialogueManagerSpeaking";

        void OnEnable()
        {
            if (triggerInterscript != null)
            {
                OnSpeaking += OnSpeakingTriggerInterScriptHandler;
            }
        }

        void OnDisable()
        {
            if (triggerInterscript != null)
            {
                OnSpeaking -= OnSpeakingTriggerInterScriptHandler;
            }

            // TODO: Not elegant; Clean the DM.OnSpeaking's delegates after scene transition
            if (OnSpeaking != null)
            {
                GameManager.Instance.DialogueManager.OnSpeaking += OnSpeakingHandler;
            }
        }

        void OnSpeakingTriggerInterScriptHandler(bool isSpeaking)
        {
            triggerInterscript.Parameters.declarations.Set(isDialogueManagerSpeaking, isSpeaking);
            triggerInterscript.TriggerEvent();
        }

        private void OnSpeakingHandler(bool isSpeaking)
        {
            if (!isSpoken)
            {
                OnSpeaking?.Invoke(isSpeaking);
                if (!isSpeaking) isSpoken = true;
            }
        }

        /// <summary> Start speaking using preferred index<br></br> </summary>
        /// <param name="alsoSetCurrentIndex">Set Hook's currentIndex to index</param>
        /// <returns>Succeed to BeginDialogue; If delay > 0, then returns true</returns>
        public virtual bool Speak(int index = 0, bool alsoSetCurrentIndex = false)
        {
            #region Checking errors

            if (GetList().Count < 0)
                Debug.LogError("There are no list");

            if (index < 0 || index >= GetList().Count)
                Debug.LogWarning("Index is larger than list.Count");

            if (!GameManager.Instance.DialogueManager)
                Debug.LogWarning("DialogueManagerNC doesn't exist");

            #endregion

            // Manage index
            if (disableAfter > 0 && triggerCount >= disableAfter)
            {
                enabled = false;
                return false;
            }
            if (alsoSetCurrentIndex) currentIndex = index;

            // Try begin dialogue
            if (delay <= 0)
            {
                if (TriggerSpeaking(index))
                {
                    triggerCount++;
                    return true;
                }
                return false;
            }
            else
            {
                StartCoroutine(DelayBeginDialogue());
                return true;
            }

            IEnumerator DelayBeginDialogue()
            {
                yield return new WaitForSeconds(delay);
                TriggerSpeaking(index);
            }
        }

        /// <summary>Start speaking using hook's currentIndex</summary>
        /// <returns>Succeed to StartSpeaking or not; return true if delay > 0</returns>
        public bool Speak()
        {
            bool returnValue = Speak(currentIndex);
            if (returnValue)
                currentIndex = currentIndex >= GetList().Count - 1 ? 0 : currentIndex + 1;
            return returnValue;
        }

        protected virtual bool TriggerSpeaking(int index)
        {
            // TODO: Not elegant; Clean the DM.OnSpeaking's delegates after scene transition
            if (OnSpeaking != null)
            {
                GameManager.Instance.DialogueManager.OnSpeaking += OnSpeakingHandler;
            }
            return false; // This will be ignored if overridden
        }

        protected abstract IList GetList();


        #region [Methods: Inspector]

        void _AddTriggerInterscript()
        {
            triggerInterscript = GetComponent<TriggerInterscriptEvent>();
            if (triggerInterscript == null)
            {
                triggerInterscript = gameObject.AddComponent<TriggerInterscriptEvent>();
                triggerInterscript.AddScriptMachineComponent();
                triggerInterscript.AddVariablesComponent();
            }

            triggerInterscript.Parameters.declarations.Set(isDialogueManagerSpeaking, false);
        }

        #endregion

    }
}