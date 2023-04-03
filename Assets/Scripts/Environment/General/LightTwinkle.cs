using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/Light Twinkle")]
    public class LightTwinkle : MonoBehaviour
    {
        [SerializeField] List<UnityEngine.Rendering.Universal.Light2D> lights;
        [SerializeField] [MinMaxSlider(0, 3, true)] Vector2 minMaxIntensity = new Vector2(0, 1);

        [SerializeField] bool isRandomPerLight = true;
        [SerializeField] [ShowIf("@isRandomPerLight")] [MinMaxSlider(0, 10, true)] Vector2 randomStartDelay = new Vector2(2, 5);
        [SerializeField] [ShowIf("@isRandomPerLight")] [MinMaxSlider(0.001f, 10, true)] Vector2 randomInterval = new Vector2(3, 5);

        [SerializeField] [ShowIf("@!isRandomPerLight")] float startDelay = 2;
        [SerializeField] [ShowIf("@!isRandomPerLight")] float interval = 3;


        private void OnEnable()
        {
            if (isRandomPerLight)
            {
                foreach (UnityEngine.Rendering.Universal.Light2D light in lights)
                {
                    float _startDelay = Random.Range(randomStartDelay.x, randomStartDelay.y);
                    float _interval = Random.Range(randomInterval.x, randomInterval.y);

                    StartCoroutine(Update(light, minMaxIntensity, _interval, _startDelay));
                }
            }
            else
            {
                foreach (UnityEngine.Rendering.Universal.Light2D light in lights)
                {
                    StartCoroutine(Update(light, minMaxIntensity, interval, startDelay));
                }
            }


            IEnumerator Update(UnityEngine.Rendering.Universal.Light2D light, Vector2 minMaxIntensity, float interval, float startDelay)
            {
                yield return new WaitForSeconds(startDelay);

                AnimationCurve curve = AnimationCurve.EaseInOut(0, minMaxIntensity.x, interval, minMaxIntensity.y + 0.1f);
                float time = 0;

                while (true)
                {
                    if (time >= interval)
                    {
                        if (light.intensity > minMaxIntensity.y)
                            curve = AnimationCurve.EaseInOut(0, light.intensity, interval, minMaxIntensity.x - 0.1f);
                        else if (light.intensity < minMaxIntensity.x)
                            curve = AnimationCurve.EaseInOut(0, light.intensity, interval, minMaxIntensity.y + 0.1f);
                        time = 0;
                    }

                    light.intensity = curve.Evaluate(time);
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