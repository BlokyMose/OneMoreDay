using Encore.Dialogues;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Encore.CharacterControllers
{
    [RequireComponent(typeof(ActorContainerPlayer))]
    [AddComponentMenu("Encore/Character Controllers/Character Brain")]
    public class PlayerBrain : CharacterBrain
    {
        public static readonly string PLAYER_NAME = "Zach";
        public PlayerController PlayerController { get; private set; }
        public override CharacterController Controller { get => PlayerController; protected set => PlayerController = value as PlayerController; }
        public MouseManager MouseManager { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            MouseManager = GetComponent<MouseManager>();
        }

        protected override void OnLoaded()
        {
            if (isLoaded) return;
            base.OnLoaded();
            MouseManager.Setup(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Controller != null && Controller.ColliderController != null)
                MouseManager.GetNearbyColliders += Controller.ColliderController.GetColliders;
        }

        public void Setup(DialogueManagerNC dialoguemanager)
        {
            if (ActorContainer == null) Awake();
            dialoguemanager.DialogueUIContainer_Player = (ActorContainer as ActorContainerPlayer).GetDialogueUIContainer() as DialogueUIContainer_Player;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Controller != null && Controller.ColliderController != null)
                MouseManager.GetNearbyColliders -= Controller.ColliderController.GetColliders;

        }

        public override void MoveInput(InputAction.CallbackContext context)
        {
            base.MoveInput(context);
        }

        public override void ClickInput(InputAction.CallbackContext context)
        {
            if (!canClickInput) return;

            base.ClickInput(context);

            if (context.performed)
            {
                if (GameManager.Instance.DialogueManager.IsInSpeaking)
                {
                    GameManager.Instance.DialogueManager.NextBubble();
                }
                else
                {
                    if (canInteract) MouseManager.Interact();
                }

            }
        }
    }
}