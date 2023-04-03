using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using NodeCanvas.DialogueTrees;
using System;

namespace Encore.CharacterControllers
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SFXCharacterManager))] // Some animation clips require to call methods of this SFXCharactermanager
    [AddComponentMenu("Encore/Character Controllers/Animation Controller")]
    public class AnimationController : MonoBehaviour
    {
        #region [Vars: Properties]

        [SerializeField, Tooltip("For NPCs, assign animator override controller here too")]
        AnimatorOverrideController animatorOverrideDefault;

        [Header("Walk")]
        [SerializeField, SuffixLabel("Matching speed: 7.5"), Tooltip("Walk animation matches 7.5 speed unless there's a major change in the animation")]
        float defaultWalkSpeed = 7.5f;

        [Header("Face")]
        [SerializeField, MinMaxSlider(0f, 2.9f, true), Tooltip("Randomly choose a delay between Min-Max")]
        Vector2 randomBlinkingDelay = new Vector2(0, 2.9f);

        [Header("Idle Specials")]
        [SerializeField]
        bool doIdleSpecial = true;
        [SerializeField, ShowIf("@doIdleSpecial")]
        float durationToIdleSpecial = 10f;
        [SerializeField, ShowIf("@doIdleSpecial"), Tooltip("Change according to available idle special animations in the animator")]
        int idleSpecialMaxIndex = 0;

        [Header("Special")]
        [SerializeField]
        AnimationClip specialEmptyClip;
        [SerializeField]
        AnimationClip specialIdleEmptyClip;
        [SerializeField]
        AnimationClip specialResetEmptyClip;

        #endregion

        #region [Vars: Components]



        #endregion

        #region [Vars: Data Handlers]



        #endregion


        // Data Handlers
        [FoldoutGroup("Debugging")]
        [ShowInInspector, PropertyRange(0, nameof(idleSpecialMaxIndex)), InlineButton(nameof(PlayIdleSpecial), "Play")]
        int currentIdleSpecialIndex;
        Animator animator;
        float timeToIdleSpecial;
        SFXCharacterManager sFX;
        [FoldoutGroup("Debugging")]
        [ShowInInspector]
        [ReadOnly]
        bool canIdleSpecial = true;
        public AnimationClip CurrentSpecialClip { get; private set; }

        AnimatorOverrideController animatorOverride;

        // Parameters
        int boo_walk, tri_idleSpecial, int_idleSpecial, flo_walkSpeed, tri_expression, int_expression, boo_special, int_specialResetSpeed;

        // Delegates
        public Action<bool> OnSpecial;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            boo_walk = Animator.StringToHash(nameof(boo_walk));
            tri_idleSpecial = Animator.StringToHash(nameof(tri_idleSpecial));
            int_idleSpecial = Animator.StringToHash(nameof(int_idleSpecial));
            flo_walkSpeed = Animator.StringToHash(nameof(flo_walkSpeed));
            int_expression = Animator.StringToHash(nameof(int_expression));
            tri_expression = Animator.StringToHash(nameof(tri_expression));
            boo_special = Animator.StringToHash(nameof(boo_special));
            int_specialResetSpeed = Animator.StringToHash(nameof(int_specialResetSpeed));

            sFX = GetComponent<SFXCharacterManager>();

            animatorOverride = animatorOverrideDefault == null ? new AnimatorOverrideController(animator.runtimeAnimatorController) : animatorOverrideDefault;
            animator.runtimeAnimatorController = animatorOverride;
        }

        void OnEnable()
        {
            // Play Idle Special if no movement every few seconds
            if (doIdleSpecial)
                StartCoroutine(CorPlayIdleSpecial());
            IEnumerator CorPlayIdleSpecial()
            {
                timeToIdleSpecial = durationToIdleSpecial;
                while (true)
                {
                    timeToIdleSpecial -= Time.deltaTime;
                    if (timeToIdleSpecial < 0)
                    {
                        currentIdleSpecialIndex = UnityEngine.Random.Range(0, idleSpecialMaxIndex + 1);
                        PlayIdleSpecial();
                        timeToIdleSpecial = durationToIdleSpecial;
                    }
                    yield return null;
                }
            }
        }

        void OnDisable()
        {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController)
            {
                playerController.OnWalk -= PlayWalk;
                playerController.OnWalkSpeedChanged -= ChangeWalkSpeed;
            }

            StopAllCoroutines();
        }

        void Start()
        {
            StartCoroutine(Delay(UnityEngine.Random.Range(randomBlinkingDelay.x, randomBlinkingDelay.y)));
            IEnumerator Delay(float delay)
            {
                yield return new WaitForSeconds(delay);
                animator.SetBool("boo_blinkingDelayDone", true);
            }
        }

        public void Setup(CharacterController characterController)
        {
            Awake();
            characterController.OnWalk += PlayWalk;
            characterController.OnWalkSpeedChanged += ChangeWalkSpeed;
            ChangeWalkSpeed(characterController.speed);
        }

        void ChangeWalkSpeed(float physicalSpeed)
        {
            animator.SetFloat(flo_walkSpeed, physicalSpeed / defaultWalkSpeed);
        }

        void PlayWalk(Vector2 value)
        {
            animator.SetBool(boo_walk, value.x != 0 ? true : false);
            timeToIdleSpecial = durationToIdleSpecial;
        }

        public void PlayIdleSpecial()
        {
            if (!canIdleSpecial) return;
            animator.SetInteger(int_idleSpecial, currentIdleSpecialIndex);
            animator.SetTrigger(tri_idleSpecial);
        }

        public void PlayAudioIdleSpecial()
        {
            if (sFX) sFX.PlayIdleSpecial(currentIdleSpecialIndex);
        }

        public void PlayExpression(Expression expression)
        {
            animator.SetInteger(int_expression, (int)expression);
            if (expression != Expression.None)
                animator.SetTrigger(tri_expression);
        }

        public void PlayGesture(Gesture gesture)
        {
            animator.SetInteger("int_gesture", (int)gesture);
            if (gesture != Gesture.None)
            {
                animator.SetTrigger("tri_gesture");
                timeToIdleSpecial += 5;
            }
        }

        public void SetCanIdleSpecial(bool isSpeaking)
        {
            canIdleSpecial = !isSpeaking;
            timeToIdleSpecial = durationToIdleSpecial;
        }

        public void PlaySpecial(AnimationClip clip, AnimationClip idleClip)
        {
            animatorOverride[specialEmptyClip] = clip;
            animatorOverride[specialIdleEmptyClip] = idleClip;
            animator.SetBool(boo_special, true);
            CurrentSpecialClip = clip;
            OnSpecial(true);
        }

        public void ResetSpecial(AnimationClip clip, bool speedPositive)
        {
            if (CurrentSpecialClip != null)
            {
                animator.SetInteger(int_specialResetSpeed, speedPositive ? 1 : -1);
                animatorOverride[specialResetEmptyClip] = clip;
                animator.SetBool(boo_special, false);

                StartCoroutine(Delay(clip.length));
                IEnumerator Delay(float delay)
                {
                    yield return new WaitForSeconds(delay);
                    animatorOverride[CurrentSpecialClip] = specialEmptyClip;
                    animatorOverride[clip] = specialResetEmptyClip;
                    animator.SetInteger(int_specialResetSpeed, 0);
                    CurrentSpecialClip = null;
                    OnSpecial(false);
                }
            }
        }
    }
}