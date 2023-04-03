using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.MiniGames.DIrtyKitchen
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Encore/Interactables/MiniGame/Dirty Kitchen/Dirt Cleaner")]
    public class DirtCleaner : MonoBehaviour
    {
        Collider2D col;

        [SerializeField, Range(1, 100)]
        int strength = 1;

        [SerializeField]
        ParticleSystem ps;

        [SerializeField]
        List<AudioClip> audioClips;


        AudioSource audioSource;
        float pitchInit;
        Coroutine corPlayPS;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            col.isTrigger = true;

            audioSource = GetComponent<AudioSource>();
            pitchInit = audioSource.pitch;
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            var dirt = collision.GetComponent<Dirt>();

            if (dirt != null)
            {
                // Clicking while rubbing give +30% strength
                if (GameManager.Instance.Player.IsClicking)
                {
                    dirt.Scrubbed(strength + (int)(strength * 0.33f));
                    audioSource.pitch = pitchInit + 1.5f;

                    if (corPlayPS != null) StopCoroutine(corPlayPS);
                    corPlayPS = StartCoroutine(PlayAndStopPS(2f));
                    IEnumerator PlayAndStopPS(float delay)
                    {
                        ps.Play();
                        yield return new WaitForSeconds(delay);
                        ps.Stop();
                    }
                }

                // Normal rubbing
                else
                {
                    dirt.Scrubbed(strength);
                    audioSource.pitch = pitchInit;
                }
                audioSource.clip = audioClips[Random.Range(0, audioClips.Count)];
                audioSource.Play();
            }
        }
    }
}