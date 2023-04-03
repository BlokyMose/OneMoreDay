using Encore.Dialogues;
using Encore.Saves;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Encore.CharacterControllers
{
    [RequireComponent(typeof(MouseManager))]
    [RequireComponent(typeof(ActorContainer))]
    [AddComponentMenu("Encore/Character Controllers/Character Brain")]
    public class CharacterBrain : MonoBehaviour
    {
        #region [Vars: Components]

        protected CharacterController controller;
        public virtual CharacterController Controller { get => controller; protected set => controller = value; }
        public ActorContainer ActorContainer { get; protected set; }

        #endregion

        #region [Vars: Data Handlers]
        public bool IsClicking { get; private set; }
        protected bool isLoaded = false;
        protected bool canMoveInput = false;
        protected bool canClickInput = false;
        protected bool canInteract = false;
        int setCanInteractQueue = 0;
        public int SetCanInteractQueue { get { return setCanInteractQueue; } }
        public void ResetCanInteractQueue() { setCanInteractQueue = 0; }
        
        #endregion

        #region [Getters]

        public bool GetCanMoveInput { get { return canMoveInput; } }
        public bool GetCanClickInput { get { return canClickInput; } }
        public bool GetCanInteract { get { return canInteract; } }

        #endregion

        #region [Setters]

        public void SetCanClickInput(bool canClickInput)
        {
            this.canClickInput = canClickInput;
            if (!canClickInput) IsClicking = false;
        }

        public void SetCanInteract(string actorName, bool isSpeaking)
        {
            if (actorName == ActorContainer.Actor.ActorName) SetCanInteract(!isSpeaking);
        }

        public void SetCanInteract(bool canInteract, CursorImageManager.CursorImage disabledCursorImage = CursorImageManager.CursorImage.Disabled, bool isQueued = true)
        {
            if (isQueued)
            {
                if (setCanInteractQueue == 0)
                {
                    Toggle();
                    setCanInteractQueue = canInteract ? 1 : -1;
                }
                else
                {
                    setCanInteractQueue = canInteract ? setCanInteractQueue + 1 : setCanInteractQueue - 1;
                    if (setCanInteractQueue == 0) Toggle();
                }
            }
            else
            {
                ResetCanInteractQueue();
                Toggle();
            }

            void Toggle()
            {
                this.canInteract = canInteract;
                OnSetCanInteract?.Invoke(canInteract, disabledCursorImage);
            }
        }

        public void SetCanMoveInput(bool canMoveInput)
        {
            this.canMoveInput = canMoveInput;
            if (!canMoveInput)
                Move?.Invoke(Vector2.zero);
        }

        /// <summary>
        /// Use this to enable or prevent player from playing directly the main character<br></br>
        /// - Preferably used when playing mini-game or in cutscene<br></br>
        /// - Queue this if both enabling & disabling is expected; Don't queue if player input must be enabled
        /// </summary>
        public void EnableAllInputs(bool isEnabled, CursorImageManager.CursorImage disabledCursorImage = CursorImageManager.CursorImage.Disabled, bool isQueued = true)
        {
            SetCanMoveInput(isEnabled);
            SetCanClickInput(isEnabled);
            SetCanInteract(isEnabled, disabledCursorImage, isQueued);
            Controller.AnimationController.SetCanIdleSpecial(isEnabled);
        }

        #endregion

        #region [Delegates]

        public Action<Vector2> Move;
        public Action<InputAction.CallbackContext> Click;
        public Action<bool> OnClick;
        public Action<bool, CursorImageManager.CursorImage> OnSetCanInteract;

        #endregion

        #region [Methods: Unity]
        
        protected virtual void Awake()
        {
            ActorContainer = GetComponent<ActorContainer>();
            Controller = GetComponent<CharacterController>();
        }

        protected virtual void OnLoaded()
        {
            if (isLoaded) return;
            isLoaded = true;
            GameManager.Instance.DialogueManager.OnActorSpeaking += SetCanInteract;
        }

        protected virtual void OnEnable()
        {
            CharacterSaver saver = GetComponent<CharacterSaver>();
            if (saver != null)
            {
                saver.OnLoaded += OnLoaded;
            }
            else
            {
                if (GameManager.HasInstance && GameManager.Instance.DialogueManager != null)
                    GameManager.Instance.DialogueManager.OnActorSpeaking += SetCanInteract;
            }

            if (Controller != null)
            {
                var colliderController = Controller.ColliderController;
                if (colliderController != null)
                    colliderController.gameObject.layer = 2;
            }
        }

        protected virtual void OnDisable()
        {
            if (GameManager.HasInstance && GameManager.Instance.DialogueManager != null)
                    GameManager.Instance.DialogueManager.OnActorSpeaking -= SetCanInteract;
        }

        #endregion

        #region [Methods: Input]
        
        public virtual void MoveInput(InputAction.CallbackContext context)
        {
            if (!canMoveInput) return;

            var moveDirection = context.ReadValue<Vector2>();
            Move?.Invoke(moveDirection);

            if (moveDirection.x > 0)
            {
                ActorContainer.GetDialogueUIContainer().Flip(Vector3.zero);
            }
            else if (moveDirection.x < 0)
            {
                ActorContainer.GetDialogueUIContainer().Flip(new Vector3(0, 180, 0));
            }
        }

        public virtual void ClickInput(InputAction.CallbackContext context)
        {
            if (!canClickInput) return;

            IsClicking = context.ReadValueAsButton();
            OnClick?.Invoke(context.performed);
            Click?.Invoke(context);
        }

        #endregion
    }
}