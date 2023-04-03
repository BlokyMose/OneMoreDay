using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NodeCanvas.DialogueTrees;
using System;
using UnityEngine.EventSystems;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/Dialogue UI Player")]
    public class DialogueUIContainer_Player : DialogueUIContainer
    {
        public class MultipleChoiceData
        {
            public string text;
            public int choiceIndex;

            public MultipleChoiceData(string text, int choiceIndex)
            {
                this.text = text;
                this.choiceIndex = choiceIndex;
            }
        }

        [SerializeField]
        GameObject multipleChoiceCanvasPrefab;
        [SerializeField]
        GameObject choiceButtonPrefab;

        GameObject currentMultipleChoiceCanvas;

        public void CreateMultipleChoiceCanvas(string title, List<MultipleChoiceData> choices, Action<int> onSetPlayerChoice)
        {
            DestroyMultipleChoiceCanvas();

            List<TextMeshProUGUI> choiceTexts = new List<TextMeshProUGUI>();

            currentMultipleChoiceCanvas = Instantiate(multipleChoiceCanvasPrefab);
            var canvasAnimator = currentMultipleChoiceCanvas.GetComponent<Animator>();

            // Set Title's text
            var titleUI = currentMultipleChoiceCanvas.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            titleUI.text = title;

            // Create buttons for each choice
            var choicesPanel = currentMultipleChoiceCanvas.transform.Find("ChoicesPanel");
            foreach (var choice in choices)
            {
                var choiceButton = Instantiate(choiceButtonPrefab, choicesPanel);

                // Add click trigger
                var eventTrigger = choiceButton.GetComponent<EventTrigger>();
                EventTrigger.Entry entry_click = new EventTrigger.Entry();
                entry_click.eventID = EventTriggerType.PointerClick;
                entry_click.callback.AddListener((data) =>
                {
                    canvasAnimator.SetBool(boo_show, false);
                    onSetPlayerChoice(choice.choiceIndex);

                    StartCoroutine(Delay(0.5f));
                    IEnumerator Delay(float delay)
                    {
                        yield return new WaitForSeconds(delay);
                        DestroyMultipleChoiceCanvas();
                    }
                });
                eventTrigger.triggers.Add(entry_click);

                // Record Text UI to texts
                var textComp = choiceButton.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                textComp.text = choice.text;
                choiceTexts.Add(textComp);
            }

        }

        public void DestroyMultipleChoiceCanvas()
        {
            if (currentMultipleChoiceCanvas!=null) Destroy(currentMultipleChoiceCanvas);
        }
    }
}