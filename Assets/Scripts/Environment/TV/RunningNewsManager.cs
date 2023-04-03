using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Encore.Environment
{
    [AddComponentMenu("Encore/Environment/TV/Running News")]
    public class RunningNewsManager : MonoBehaviour
    {
        [Header("External Components")]
        [SerializeField] RectTransform parent;
        [SerializeField] GameObject runningNewsContainerPrefab;

        [Header("Properties")]
        [SerializeField] List<string> news;
        [SerializeField] string separator = "  |  ";
        [SerializeField] float speed = 100;

        // Data Handlers
        RectTransform currentRunningNews;
        int newsIndex = 0;

        List<RectTransform> allRunningNews = new List<RectTransform>();

        private void Start()
        {
            CreateNewRunningNews();
        }

        private void Update()
        {
            for (int i = allRunningNews.Count - 1; i > -1; i--)
            {
                allRunningNews[i].localPosition = new Vector3(
                    allRunningNews[i].localPosition.x - speed * Time.deltaTime,
                    allRunningNews[i].localPosition.y,
                    allRunningNews[i].localPosition.z);

                if (currentRunningNews == allRunningNews[i] && allRunningNews[i].localPosition.x < -allRunningNews[i].rect.width)
                    CreateNewRunningNews();

                if (allRunningNews[i] && allRunningNews[i].localPosition.x < -(allRunningNews[i].rect.width + parent.rect.width))
                {
                    Destroy(allRunningNews[i].gameObject);
                    allRunningNews.RemoveAt(i);
                }
            }
        }

        void CreateNewRunningNews()
        {
            currentRunningNews = Instantiate(runningNewsContainerPrefab, parent).GetComponent<RectTransform>();
            currentRunningNews.localPosition = Vector3.zero;
            currentRunningNews.GetComponentInChildren<TextMeshProUGUI>().text = separator + news[newsIndex];
            newsIndex = newsIndex >= news.Count - 1 ? 0 : newsIndex + 1;

            allRunningNews.Add(currentRunningNews);
        }
    }
}