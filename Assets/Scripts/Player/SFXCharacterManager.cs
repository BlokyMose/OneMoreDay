using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.CharacterControllers
{
    [AddComponentMenu("Encore/Character Controllers/SFX Character")]
    public class SFXCharacterManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Set to False to prevent making sounds")]
        bool isMuted = false;

        [Title("Audio Sources")]
        [SerializeField, Required]
        AudioSource footstepsSource;

        [SerializeField, Required]
        AudioSource specialSource;


        [Title("Audio Clips")]
        [SerializeField]
        List<AudioClip> footstepsClips;
        [SerializeField]
        List<AudioClip> idleSpecialClips; // index must be based on animation's index


        // Data Handlers
        int footstepIndex = 0;
        bool randomFootstepIndex = true;

        /// <summary>Called by Animator in walk animation clip</summary>
        public void PlayFootstep()
        {
            if (isMuted) return;

            if (randomFootstepIndex)
            {
                footstepIndex = Random.Range(0, footstepsClips.Count);
            }
            else
            {
                footstepIndex++;
                if (footstepIndex == footstepsClips.Count) footstepIndex = 0;
            }

            footstepsSource.clip = footstepsClips[footstepIndex];
            footstepsSource.Play();
        }

        /// <summary>Called by Animator in special animation clip</summary>
        public void PlayIdleSpecial(int index)
        {
            if (isMuted) return;
            if (index >= idleSpecialClips.Count) return;

            specialSource.clip = idleSpecialClips[index];
            specialSource.Play();
        }
    }
}