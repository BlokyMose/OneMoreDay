using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TVContent_Hospital : MonoBehaviour
{
    [SerializeField] List<string> sectionSubtitles;
    [SerializeField] TextMeshProUGUI subtitle;
    [SerializeField] Animator newsPanelAnimator;

    int boo_show, boo_showSubtitle;

    int currentIndex = -1;

    private void Awake()
    {
        boo_show = Animator.StringToHash(nameof(boo_show));
        boo_showSubtitle = Animator.StringToHash(nameof(boo_showSubtitle));
        subtitle.text = sectionSubtitles[0];

    }

    public void ChangeSubtitleText()
    {
        // NOTE: currentIndex also signify the begininning and end of NewsPanel; so the index includes the last index + 1

        if (currentIndex == -1)
        {
            newsPanelAnimator.SetBool(boo_show, true);
        }
        else if(currentIndex == sectionSubtitles.Count-1)
        {
            newsPanelAnimator.SetBool(boo_show, false);
        }
        else
        {
            newsPanelAnimator.SetBool(boo_showSubtitle, false);
            StartCoroutine(Delay(1));
            IEnumerator Delay(float delay)
            {
                yield return new WaitForSeconds(delay);
                subtitle.text = sectionSubtitles[currentIndex];
                newsPanelAnimator.SetBool(boo_showSubtitle, true);
            }
        }


        currentIndex = currentIndex >= sectionSubtitles.Count-1 ? -1 : currentIndex + 1;
    }

}
