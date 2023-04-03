using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Encore.Animations
{
    [RequireComponent(typeof(EventTrigger))]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Encore/Animations/Animation Parameer Event")]
    public class AnimatorParameterEventModifier : MonoBehaviour
    {
        #region [Classes]

        [System.Serializable]
        public class AnimatorEvent
        {
            public enum AnimatorSet { SetTrigger, SetInteger, SetFloat, SetBool }

            [HorizontalGroup("1"), HideLabel]
            public EventTriggerType eventType;

            [HorizontalGroup("1"), HideLabel]
            public AnimatorSet animatorSet;

            [LabelText("Param Name")]
            public string parameterName;

            [LabelText("Value"), ShowIf("@animatorSet==AnimatorSet.SetInteger")]
            public int valueInteger;

            [LabelText("Value"), ShowIf("@animatorSet==AnimatorSet.SetFloat")]
            public float valueFloat;

            [LabelText("Value"), ShowIf("@animatorSet==AnimatorSet.SetBool")]
            public bool valueBool;


            public void ExecuteEventSet(Animator animator)
            {
                switch (animatorSet)
                {
                    case AnimatorSet.SetTrigger:
                        animator.SetTrigger(parameterName);
                        break;
                    case AnimatorSet.SetInteger:
                        animator.SetInteger(parameterName, valueInteger);
                        break;
                    case AnimatorSet.SetFloat:
                        animator.SetFloat(parameterName, valueFloat);
                        break;
                    case AnimatorSet.SetBool:
                        animator.SetBool(parameterName, valueBool);
                        break;
                }
            }

        }

        #endregion

        #region [Vars: DataHandlers]

        public List<AnimatorEvent> events = new List<AnimatorEvent>();

        #endregion

        #region [Vars: Components]

        public EventTrigger EventTrigger { get; private set; }
        public Animator Animator { get; private set; }

        #endregion

        private void Awake()
        {
            EventTrigger = GetComponent<EventTrigger>();
            Animator = GetComponent<Animator>();

            foreach (var animatorEvent in events)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = animatorEvent.eventType;
                entry.callback.AddListener((data) =>
                {
                    animatorEvent.ExecuteEventSet(Animator);
                });
                EventTrigger.triggers.Add(entry);
            }
        }

    }
}