using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/SFX Manager")]
    public class SFXManager : MonoBehaviour
    {
        #region [Basic Properties]

        [SerializeField]
        [Required]
        AudioSource audioSource;

        #region [Buttons]

        [Button(ButtonSizes.Medium), GUIColor(0, 1, 0)]
        [ShowIf("@audioSource==null"), PropertyOrder(-2)]
        public void AddAudioSourceHere()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        [Button(ButtonSizes.Medium), GUIColor(0, 1, 0)]
        [ShowIf("@audioSource==null"), PropertyOrder(-1)]
        public void AddAudioSourceChild()
        {
            GameObject child = new GameObject("SFX_AudioSource");
            child.transform.parent = transform;
            audioSource = child.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        #endregion

        [HorizontalGroup("Mode")]
        [SerializeField]
        bool useOnlySourceClip = true;
        public enum PlayAudioMode { OneShot, Loop }
        [HorizontalGroup("Mode")]
        [LabelText("Mode")]
        [LabelWidth(50)]
        [SerializeField]
        PlayAudioMode playAudioMode = PlayAudioMode.OneShot;

        [HorizontalGroup("AutoStart")]
        [SerializeField]
        bool autoStart = false;
        [HorizontalGroup("AutoStart")]
        [ShowIf("@autoStart")]
        [LabelText("Delay")]
        [LabelWidth(50)]
        [SerializeField]
        float delayStart = 0;

        #endregion

        #region [Multi-Clips Properties]

        [Title("Multi-Clips")]

        /// <summary>
        /// AllAtOnce: play every clip inside audioClips using PlayOneShot()<br></br>
        /// OneByOne: play one clip from audioClips based on clipIndex; incremented everytime PlaySFX called
        /// </summary>
        public enum ClipsMode { AllAtOnce, OneByOne }
        [SerializeField]
        [ShowIf("@!useOnlySourceClip")]
        ClipsMode clipsMode = ClipsMode.AllAtOnce;

        [SerializeField]
        [ShowIf("@clipsMode==ClipsMode.OneByOne")]
        [PropertyRange(1, "@audioClips.Count")]
        int clipIndex = 1;

        [SerializeField]
        [ShowIf("@!useOnlySourceClip")]
        List<AudioClip> audioClips = new List<AudioClip>();

        #endregion

        private void Start()
        {
            if (autoStart)
            {
                StartCoroutine(Delay(delayStart));
                IEnumerator Delay(float delay)
                {
                    yield return new WaitForSeconds(delay);
                    PlaySFX();
                }
            }
        }

        public void PlaySFX()
        {
            if (useOnlySourceClip)
            {
                if (playAudioMode == PlayAudioMode.OneShot)
                {
                    audioSource.PlayOneShot(audioSource.clip);
                }
                else if (playAudioMode == PlayAudioMode.Loop)
                {
                    audioSource.loop = true;
                    audioSource.Play();
                }

            }
            else
            {
                if (clipsMode == ClipsMode.AllAtOnce)
                {
                    foreach (var clip in audioClips)
                    {
                        audioSource.PlayOneShot(clip);
                    }
                }
                else if (clipsMode == ClipsMode.OneByOne)
                {
                    if (playAudioMode == PlayAudioMode.OneShot)
                    {
                        audioSource.PlayOneShot(audioClips[clipIndex - 1]);

                    }
                    else if (playAudioMode == PlayAudioMode.Loop)
                    {
                        audioSource.loop = true;
                        audioSource.clip = audioClips[clipIndex - 1];
                        audioSource.Play();
                    }

                    clipIndex = clipIndex >= audioClips.Count ? 1 : clipIndex + 1;
                }
            }
        }
    }
}