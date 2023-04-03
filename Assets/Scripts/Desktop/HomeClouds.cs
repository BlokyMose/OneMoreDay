using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using Sirenix.OdinInspector;

namespace Encore.Desktop
{
    [AddComponentMenu("Encore/Desktop/Home Clouds")]
    public class HomeClouds : MonoBehaviour
    {
        [SerializeField] private RectTransform cloud1;
        [SerializeField] private RectTransform cloud2;
        [SerializeField] private RectTransform cloud3;
        [SerializeField] private RectTransform cloudSmall;

        [SerializeField] private float cloud1Speed; // pixels per second
        [SerializeField] private float cloud2Speed;
        [SerializeField] private float cloud3Speed;
        [SerializeField] private float cloudSmallSpeed;

        private void OnEnable()
        {
            Debug.Log("Start");
            StartCoroutine(AnimateCloud(cloud1, cloud1Speed));
            StartCoroutine(AnimateCloud(cloud2, cloud2Speed));
            StartCoroutine(AnimateCloud(cloud3, cloud3Speed));
            StartCoroutine(AnimateCloud(cloudSmall, cloudSmallSpeed));
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator AnimateCloud(RectTransform cloud, float speed)
        {
            while (true)
            {
                float rectWidth = cloud.rect.width;
                float screenWidth = Screen.width;
                float destination = screenWidth - (screenWidth - rectWidth) / 2;

                yield return cloud.DOLocalMoveX(destination, speed).SetEase(Ease.Linear).SetSpeedBased(true).WaitForCompletion();

                cloud.anchoredPosition = new Vector2(-destination, cloud.anchoredPosition.y);
            }
        }
    }
}