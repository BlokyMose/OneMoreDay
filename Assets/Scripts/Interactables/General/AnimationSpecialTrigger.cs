using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Animation Special Trigger")]
    public class AnimationSpecialTrigger : Interactable
    {
        [Title("Animation Trigger")]
        [SerializeField] AnimationClip clip;
        [SerializeField] AnimationClip idleClip;

        protected override void InteractModule(GameObject interactor)
        {
            AnimationController ac = interactor.GetComponent<AnimationController>();
            if (ac != null)
            {
                ac.PlaySpecial(clip, idleClip);
            }
        }
    }
}

