using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Rain/Rain Trail")]
    public class RainTrailManager : MonoBehaviour
    {
        [SerializeField] List<GameObject> rainDropTrailPrefabs;
        [SerializeField] float radius = 5;
        [SerializeField] [SuffixLabel("sec")] int emissionPeriod = 1;

        [Header("Nudge")]
        [SerializeField] [MinMaxSlider(0, 5, true)] [SuffixLabel("sec")] Vector2 nudgeEvery = new Vector2(2, 5);
        [SerializeField] [MinMaxSlider(-10, 10, true)] Vector2 nudgeDistanceX = new Vector2(-3, 3);
        [SerializeField] [MinMaxSlider(-10, 10, true)] Vector2 nudgeDistanceY = new Vector2(-3, -0.5f);


        [SerializeField] float destroyAfter = 5;
        [SerializeField] bool playAtStart = true;

        Coroutine corRain;

        private void Start()
        {
            if (playAtStart)
                StartRaining();
        }

        [Button]
        public void StartRaining()
        {
            corRain = StartCoroutine(Raining());
        }

        [Button]
        public void StopRaining()
        {
            StopCoroutine(corRain);
        }

        IEnumerator Raining()
        {
            float time = 0;
            while (true)
            {
                if (time >= emissionPeriod)
                {
                    GameObject rainDrop = Instantiate(rainDropTrailPrefabs[Random.Range(0, rainDropTrailPrefabs.Count)], transform);
                    rainDrop.transform.localPosition = new Vector3(transform.position.x + Random.Range(-radius, radius), 0, 0);
                    StartCoroutine(Nudging(rainDrop));

                    Destroy(rainDrop, destroyAfter);
                    time = 0;
                }

                time += Time.deltaTime;
                yield return null;
            }


            IEnumerator Nudging(GameObject rainDrop)
            {
                GameObject _rainDrop = rainDrop;
                float time = 0;
                float period = Random.Range(nudgeEvery.x, nudgeEvery.y);

                while (true)
                {
                    if (!_rainDrop) break;

                    if (time >= period)
                    {
                        _rainDrop.transform.position = new Vector3(
                            _rainDrop.transform.position.x + Random.Range(nudgeDistanceX.x, nudgeDistanceX.y),
                            _rainDrop.transform.position.y + Random.Range(nudgeDistanceY.x, nudgeDistanceY.y),
                            _rainDrop.transform.position.z);
                        period = Random.Range(nudgeEvery.x, nudgeEvery.y);
                        time = 0;
                    }

                    time += Time.deltaTime;
                    yield return null;
                }
            }

        }
    }
}