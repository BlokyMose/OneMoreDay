using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("Encore/Environment/Sprite/Sprite Alpha Twinkle")]
    public class SpriteAlphaTwinkle : MonoBehaviour
    {
        [SerializeField]
        float originAlpha = 0;

        [SerializeField]
        float targetAlpha = 1;

        [SerializeField]
        bool isLoop = true;

        [SerializeField]
        float incrementBy = 0.001f;

        [SerializeField]
        float incrementEverySecond = 1.0f;

        [SerializeField]
        float decrementBy = 0.001f;

        [SerializeField]
        float decrementEverySecond = 1.0f;

        SpriteRenderer sr;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();

            float _temp;
            if (originAlpha > targetAlpha)
            {
                _temp = originAlpha;
                originAlpha = targetAlpha;
                targetAlpha = _temp;
            }

            StartCoroutine(Update());
            IEnumerator Update()
            {
                bool isIncrementing = true;
                while (true)
                {
                    if (sr.color.a >= targetAlpha)
                    {
                        isIncrementing = false;
                    }
                    else if (sr.color.a <= originAlpha)
                    {
                        isIncrementing = true;
                    }

                    if (isIncrementing)
                    {
                        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a + incrementBy);
                        yield return new WaitForSeconds(incrementEverySecond);

                    }
                    else
                    {
                        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a - decrementBy);
                        yield return new WaitForSeconds(decrementEverySecond);
                    }

                }
            }
        }
    }
}