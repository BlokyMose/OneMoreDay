using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Encore.Locations
{
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("Encore/Locations/Map District")]
    public class MapDistrict : MonoBehaviour
    {
        [SerializeField]
        District district;

        [SerializeField]
        TextMeshProUGUI districtNameText;

        [SerializeField]
        List<TextMeshProUGUI> otherTexts = new List<TextMeshProUGUI>();

        [SerializeField]
        Image maskImage;

        CanvasGroup canvasGroup;

        public District District { get { return district; } }

        float fadeDuration = 0.5f;

        Coroutine corFadeIn;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            maskImage.enabled = true;
        }

        public void Select(bool isSelected)
        {
            if (isSelected)
            {
                if (corFadeIn != null) StopCoroutine(corFadeIn);
                corFadeIn = StartCoroutine(Fading(1f));
            }
            else
            {
                if (corFadeIn != null) StopCoroutine(corFadeIn);
                corFadeIn = StartCoroutine(Fading(0f));

            }

            IEnumerator Fading(float toValue)
            {
                float time = 0;
                AnimationCurve curve = AnimationCurve.EaseInOut(0, canvasGroup.alpha, fadeDuration, toValue);
                while (time < fadeDuration)
                {
                    canvasGroup.alpha = curve.Evaluate(time);
                    time+=Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
}
