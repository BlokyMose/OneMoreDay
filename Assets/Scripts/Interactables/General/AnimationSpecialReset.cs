using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.CharacterControllers;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Animation Special Reset")]
    public class AnimationSpecialReset : Interactable
    {
        [Title("Reset Special")]
        [SerializeField] AnimationClip clip;
        [SerializeField, Tooltip("Use False to use State with Speed -1")] 
        bool speedPositive = false;

        protected override void InteractModule(GameObject interactor)
        {
            AnimationController ac = interactor.GetComponent<AnimationController>();
            if (ac != null)
            {
                ac.ResetSpecial(clip, speedPositive);
            }
        }
    }
}
