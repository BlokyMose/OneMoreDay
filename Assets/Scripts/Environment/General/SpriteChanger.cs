using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("Encore/Environment/Sprite/Sprite Changer")]
    public class SpriteChanger : MonoBehaviour
    {
        [System.Serializable]
        public class SpriteChange
        {
            public Sprite sprite;
            [MinMaxSlider(0.1f, 10f, true)]
            public Vector2 randomDuration = new Vector2(5f, 7.5f);
            public bool inMinutes = false;

            public SpriteChange(Sprite sprite, Vector2 randomDuration, bool inMinutes)
            {
                this.sprite = sprite;
                this.randomDuration = randomDuration;
                this.inMinutes = inMinutes;
            }
        }

        [SerializeField] List<SpriteChange> sprites = new List<SpriteChange>();
        [SerializeField] float delay = 0;

        SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            StartCoroutine(Update());
            IEnumerator Update()
            {
                yield return new WaitForSeconds(delay);

                float time = 0;
                int index = 0;
                float duration = Random.Range(sprites[index].randomDuration.x, sprites[index].randomDuration.y);
                duration = sprites[index].inMinutes ? duration * 60 : duration;

                sr.sprite = sprites[index].sprite;

                while (true)
                {
                    if (time > duration)
                    {
                        index = index >= sprites.Count - 1 ? 0 : index + 1;
                        sr.sprite = sprites[index].sprite;
                        time = 0;
                        duration = Random.Range(sprites[index].randomDuration.x, sprites[index].randomDuration.y);
                    }

                    time += Time.deltaTime;
                    yield return null;
                }
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}