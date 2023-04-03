using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System;
using Encore.Utility;
using Encore.Interactables;
using Encore.Locations;
using Encore.CharacterControllers;
using static Encore.SceneMasters.SceneTransitionAnimationEvent;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Door")]
    public class Door : Interactable
    {
        [Title("Door")]
        [SerializeField]
        Location location;

        [SerializeField]
        bool isAutoAnimDirection = true;

        [SerializeField, ShowIf("@"+nameof(isAutoAnimDirection))]
        Transform locationPosition;

        [SerializeField, ShowIf("@!" + nameof(isAutoAnimDirection))]
        AnimationDirection animationDirection = AnimationDirection.FromRight;

        public override bool Interact(GameObject interactor)
        {
            if (GameManager.Instance.DialogueManager.IsInSpeaking) return false;
            return base.Interact(interactor);
        }

        protected override void InteractModule(GameObject interactor)
        {
            try
            {
                var direction = animationDirection;
                if (isAutoAnimDirection)
                {
                    var relativePos = locationPosition != null ? locationPosition.position : transform.position;
                    if (relativePos.y == transform.position.y)
                    {
                        direction = GameManager.Instance.Player.transform.position.x < relativePos.x
                            ? AnimationDirection.FromRight
                            : AnimationDirection.FromLeft;
                    }
                    else
                    {
                        direction = GameManager.Instance.Player.transform.position.y < relativePos.y
                            ? AnimationDirection.FromTop
                            : AnimationDirection.FromBottom;
                    }
                }
                GameManager.Instance.LoadScene(location.SceneName, direction);
            }
            catch (System.Exception)
            {
                Debug.LogWarning("No scene name: " + location.SceneName);
                throw;
            }
        }

        public override CursorImageManager.CursorImage GetCursorImage()
        {
            if (GameManager.Instance.DialogueManager.IsInSpeaking) return CursorImageManager.CursorImage.Disabled;
            return base.GetCursorImage();
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return CursorImageManager.CursorImage.Exit;
        }

        public override string GetObjectName => "To " + base.GetObjectName;

    }
}
