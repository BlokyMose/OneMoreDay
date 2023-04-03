using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.SceneMasters
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Encore/Scene Masters/Transitions/Scene Transition Animation Event")]
    public class SceneTransitionAnimationEvent : MonoBehaviour
    {
        public enum AnimationDirection { FromTop, FromRight, FromBottom, FromLeft}

        // External components
        public Animator animator;

        // Delegates
        public Action onCovered, onUncovered;

        // Animation parameters
        int int_direction; // 0: fromTop, 1: fromRight, 2: fromBottom, 3: fromLeft
        int tri_transitioning;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            int_direction = Animator.StringToHash(nameof(int_direction));
            tri_transitioning = Animator.StringToHash(nameof(tri_transitioning));
        }

        /// <summary>
        /// Play animation to cover the entire screen before loading
        /// </summary>
        /// <param name="coveredCallback">Call this delegate once animation is done</param>
        /// <param name="direction">0: fromTop, 1: fromRight, 2: fromBottom, 3: fromLeft</param>
        public void StartCovering(Action coveredCallback, AnimationDirection direction = AnimationDirection.FromTop)
        {
            onCovered += coveredCallback;
            animator.SetInteger(int_direction, (int)direction);
            animator.SetTrigger(tri_transitioning);
            onUncovered = null;
        }

        /// <summary>
        /// Play animation to uncover the entire screen after loading
        /// </summary>
        /// <param name="coveredCallback">Call this delegate once animation is done</param>
        public void StartUncovering(Action uncoveredCallback)
        {
            onUncovered += uncoveredCallback;
            animator.SetTrigger(tri_transitioning);
            onCovered = null;
        }

        /// <summary>
        /// Called by Animator when covering animation is about to over
        /// </summary>
        public void Covered()
        {
            onCovered?.Invoke();
        }

        /// <summary>
        /// Called by Animator when uncovering animation is about to over
        /// </summary>
        public void Uncovered()
        {
            onUncovered?.Invoke();
        }
    }
}