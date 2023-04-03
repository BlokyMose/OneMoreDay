using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using NodeCanvas.DialogueTrees;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/Dialogue UI Container")]
    public class DialogueUIContainer : MonoBehaviour
    {
        public string actorName;

        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;

        protected Animator animator;
        protected int boo_show;

        bool isSpeaking = false;

        protected void Awake()
        {
            animator = GetComponent<Animator>();
            boo_show = Animator.StringToHash(nameof(boo_show));
        }

        public virtual void Flip(Vector3 v)
        {
            nameText.transform.parent.GetComponent<RectTransform>().localEulerAngles = v;
            dialogueText.transform.parent.GetComponent<RectTransform>().localEulerAngles = v;

            if (v == Vector3.zero)
            {
                nameText.alignment = TextAlignmentOptions.Left;
                dialogueText.alignment = TextAlignmentOptions.Left;
            }
            else
            {
                nameText.alignment = TextAlignmentOptions.Right;
                dialogueText.alignment = TextAlignmentOptions.Right;
            }
        }

        void PlayShowSpeechBubbleAnimation(bool toShow)
        {
            animator.SetBool(boo_show, toShow);
        }

        /// <summary>
        /// Show Speech Bubble
        /// </summary>
        /// <param name="singleText">Used for dialogue or monologue for simple sentences</param>
        /// <param name="actorName">This speaking actor's name</param>
        /// <param name="closeAfter">Automatically close speech bubble after seconds</param>
        /// <returns>Succeed to call speech bubble; returns false if currently speaking</returns>
        public bool ShowSpeechBubble(string singleText, string actorName, float closeAfter = -1)
        {
            if (isSpeaking) return false;
            isSpeaking = true;

            dialogueText.text = singleText;
            nameText.text = actorName;
            dialogueText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(singleText));
            nameText.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(actorName));

            Canvas.ForceUpdateCanvases();
            GetComponent<VerticalLayoutGroup>().enabled = false;
            GetComponent<VerticalLayoutGroup>().enabled = true;

            PlayShowSpeechBubbleAnimation(true);

            if (closeAfter > 0) StartCoroutine(DelayClose());
            IEnumerator DelayClose()
            {
                yield return new WaitForSeconds(closeAfter);
                HideSpeechBubble();
            }

            return true;
        }

        public void HideSpeechBubble()
        {
            PlayShowSpeechBubbleAnimation(false);
            isSpeaking = false;
        }
    }
}