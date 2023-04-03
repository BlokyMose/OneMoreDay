using Encore.CharacterControllers;
using Encore.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.MiniGames.UrgentChoice
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Encore/MiniGames/Urgent Choice")]
    public class UrgentChoiceManager : MonoBehaviour
    {
        #region [Vars: Components]

        [SerializeField]
        GameObject choicePrefab;

        [SerializeField]
        TextMeshProUGUI titleText;

        [SerializeField]
        RectTransform initialDelayPercentage;

        [SerializeField]
        List<Transform> choicesRows;

        [SerializeField]
        UrgentChoiceParameters defaultParameters;

        Animator animator;

        #endregion

        #region [Vars: Data Handlers]

        public bool isActive { get; private set; }
        public bool hasChosen { get; private set; }

        List<UrgentChoice> choices = new List<UrgentChoice>();
        float initialAlphaFill = 0.5f;
        int currentChoiceIndex = 0;
        Vector3 fillPercentageCursorOffset = new Vector2(30, -30);
        Vector2 mouseMovementValidDistance = new Vector2(120, 120);

        int boo_show;
        const float animationShowDuration = 2;

        Action<int> OnChosen;

        #endregion

        #region [Classes]

        [Serializable]
        public class UrgentChoiceParameters
        {
            public string Title;
            /// <summary>Time for player to read all the options before game starts counting the time</summary>
            public float InitialDelay;
            public List<UrgentChoiceData> Choices;
            public Action<int> OnSetPlayerChoice;


            public UrgentChoiceParameters(string title, List<UrgentChoiceData> choices, float initialDelay, Action<int> onSetPlayerChoice)
            {
                this.Title = title;
                this.Choices = choices;
                this.InitialDelay = initialDelay;
                this.OnSetPlayerChoice = onSetPlayerChoice;
            }
        }

        public class UrgentChoice
        {
            public UrgentChoiceData data;
            public Image bg;
            public Image frame;
            public TextMeshProUGUI textChoice;
            public RectTransform percentageFill;

            const float bgAlphaMaxMargin = 0.075f;
            float percentage;
            public float initialAlphaFill;

            public bool isChosen { get; private set; }
            public Action<int> OnAutoChosen;

            public UrgentChoice(UrgentChoiceData data, Image bg, Image frame, TextMeshProUGUI textChoice, RectTransform percentageFill, float initialAlphaFill, Action<int> OnAutoChosen)
            {
                this.data = data;
                this.bg = bg;
                this.frame = frame;
                this.textChoice = textChoice;
                this.percentageFill = percentageFill;
                this.initialAlphaFill = initialAlphaFill;
                percentage = 0f;
                this.OnAutoChosen = OnAutoChosen;
            }

            public void AddPercentage()
            {
                float increment = data.speed * 0.1f * Time.deltaTime;
                if (percentage + increment < 1f)
                {
                    percentage += increment;
                    bg.color = data.color.ChangeAlpha((1 - initialAlphaFill) * percentage + initialAlphaFill - bgAlphaMaxMargin);
                    percentageFill.localScale = new Vector3(percentage, percentage, percentage);
                }
                else
                {
                    OnAutoChosen(data.choiceIndex);
                }
            }

            public void Hover(bool isHovering)
            {
                if (isHovering)
                {
                    AddPercentage();
                }
                else
                {
                    bg.color = new Color(0, 0, 0, 0.5f);
                }
            }

            /// <summary>
            /// Change UI appearance; Data.callback will be called in Close()
            /// </summary>
            public void Chosen()
            {
                if (isChosen) return;

                isChosen = true;
                bg.color = data.color.ChangeAlpha(1 - bgAlphaMaxMargin);
                percentageFill.localScale = new Vector3(1, 1, 1);
                percentageFill.GetComponent<Image>().color = Color.white;
            }


            public IEnumerator Dim(float duration)
            {
                Color imageDimColor = new Color(0, 0, 0, 0.5f);
                bg.color = imageDimColor;

                percentageFill.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

                AnimationCurve curve = AnimationCurve.EaseInOut(0, 1, duration, 0.25f);
                float time = 0;
                while (time < duration)
                {
                    time += Time.deltaTime;
                    textChoice.color = textChoice.color.ChangeAlpha(curve.Evaluate(time));
                    frame.color = frame.color.ChangeAlpha(curve.Evaluate(time));
                    yield return null;
                }
            }
        }

        [Serializable]
        public class UrgentChoiceData
        {
            [TextArea(1, 3), HideLabel]
            public string text;

            public int choiceIndex;

            /// <summary>Optimal from 2 to 5</summary>
            [PropertyRange(1, 10), LabelWidth(0.1f)]
            public int speed = 2;

            [LabelWidth(0.1f)]
            public Color color = new Color(0, 0, 0, 0.2f);

            public static Color DefaultColor = new Color(0, 0, 0, 0.9f);
            public static int DefaultSpeed = 2;


            public UrgentChoiceData(string text, int choiceIndex)
            {
                this.text = text;
                speed = 2;
                color = DefaultColor;
                this.choiceIndex = choiceIndex;
            }

            public UrgentChoiceData(string text, int choiceIndex, int speed)
            {
                this.text = text;
                this.speed = speed;
                color = DefaultColor;
                this.choiceIndex = choiceIndex;
            }

            public UrgentChoiceData(string text, int choiceIndex, int speed, Color color)
            {
                this.text = text;
                this.speed = speed;
                this.color = color;
                this.choiceIndex = choiceIndex;
            }
        }

        #endregion

        void Awake()
        {
            animator = GetComponent<Animator>();
            boo_show = Animator.StringToHash(nameof(boo_show));
        }

        public void Setup(UrgentChoiceParameters parameters)
        {
            choices = new List<UrgentChoice>();
            OnChosen = parameters.OnSetPlayerChoice;
            titleText.text = parameters.Title;

            int index = 0;
            foreach (var choice in parameters.Choices)
            {
                int _index = index;

                #region [Set Parent Choice Row]

                Transform parent = null;
                if (index < 2)
                    parent = choicesRows[0];
                else if (index < 4)
                    parent = choicesRows[1];
                else if (index < 6)
                    parent = choicesRows[2];

                #endregion

                GameObject choiceGO = Instantiate(choicePrefab, parent);

                UrgentChoice urgentChoice = new UrgentChoice(
                    choice,
                    bg: choiceGO.transform.Find("Fill").GetComponent<Image>(),
                    frame: choiceGO.transform.Find("Frame").GetComponent<Image>(),
                    textChoice: choiceGO.transform.Find("Text").GetComponent<TextMeshProUGUI>(),
                    percentageFill: choiceGO.transform.Find("Percentage").Find("PercentageFill").GetComponent<RectTransform>(),
                    initialAlphaFill: initialAlphaFill,
                    OnAutoChosen: Choose
                    );

                #region [Setup UI]

                urgentChoice.bg.color = new Color(0, 0, 0, 0.5f);
                urgentChoice.textChoice.text = choice.text;

                #endregion

                choices.Add(urgentChoice);

                index++;
            }

            foreach (var row in choicesRows)
            {
                if (row.childCount == 0) row.gameObject.SetActive(false);
                else row.gameObject.SetActive(true);
            }

            GameManager.Instance.Player.OnClick += OnPlayerControllerMouseClick;

            Begin(parameters);
        }

        void Begin(UrgentChoiceParameters settings)
        {
            isActive = true;
            hasChosen = false;
            StartCoroutine(Starting());
            IEnumerator Starting()
            {
                // Reset
                GameManager.Instance.Player.EnableAllInputs(false);
                GameManager.Instance.EnableUICornerTools(true, false);
                GameManager.Instance.Player.MouseManager.CursorImageManager.StopFollowingMouse();
                foreach (var choice in choices)
                {
                    choice.textChoice.alpha = 0.5f;
                    choice.frame.color = choice.frame.color.ChangeAlpha(0.25f);
                }
                initialDelayPercentage.localScale = new Vector2(0, 1);


                // Prepare to track Mouse
                Vector2 screenSize = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
                Vector2 screenCenterPos = screenSize / 2;
                GameManager.Instance.Player.MouseManager.CursorImageManager.SetCursorPosition(choices[currentChoiceIndex].percentageFill.position + fillPercentageCursorOffset);

                // Fade-In
                animator.SetBool(boo_show, true);
                yield return new WaitForSeconds(animationShowDuration);

                // Delay for reading time
                float time = settings.InitialDelay;
                while (time > 0)
                {
                    time -= Time.deltaTime;
                    initialDelayPercentage.localScale = new Vector2(time / settings.InitialDelay, 1);
                    yield return null;
                }
                initialDelayPercentage.localScale = new Vector2(0, 1);

                // Player can start choosing
                foreach (var choice in choices)
                {
                    choice.textChoice.alpha = 1f;
                    choice.frame.color = choice.frame.color.ChangeAlpha(1);
                }
                StartCoroutine(AddingPercentage());

                // Tracking mouse
                while (!hasChosen)
                {
                    var currentPos = GameManager.Instance.Player.MouseManager.MouseScreenPos;

                    #region [Check which area is hovered]

                    int rows =
                        choices.Count <= 2 ? 1 :
                        choices.Count <= 4 ? 2 :
                        choices.Count <= 6 ? 3 : 1;

                    if (Mathf.Abs(currentPos.x - screenCenterPos.x) > mouseMovementValidDistance.x)
                    {
                        if (currentPos.x > screenCenterPos.x + mouseMovementValidDistance.x)
                        {
                            if (rows == 1)
                            {
                                currentChoiceIndex = 1;
                            }
                            else if (rows == 2)
                            {
                                if (currentChoiceIndex == 0)
                                {
                                    currentChoiceIndex = 1;
                                }
                                else if (currentChoiceIndex == 2)
                                    currentChoiceIndex = 3;
                            }
                            else if (rows == 3)
                            {
                                if (currentChoiceIndex == 0)
                                    currentChoiceIndex = 1;
                                else if (currentChoiceIndex == 2)
                                    currentChoiceIndex = 3;
                                else if (currentChoiceIndex == 4)
                                    currentChoiceIndex = 5;
                            }
                        }

                        else if (currentPos.x < screenCenterPos.x - mouseMovementValidDistance.x)
                        {
                            if (rows == 1)
                            {
                                currentChoiceIndex = 0;
                            }
                            else if (rows == 2)
                            {
                                if (currentChoiceIndex == 1)
                                    currentChoiceIndex = 0;
                                else if (currentChoiceIndex == 3)
                                    currentChoiceIndex = 2;
                            }
                            else if (rows == 3)
                            {
                                if (currentChoiceIndex == 1)
                                    currentChoiceIndex = 0;
                                else if (currentChoiceIndex == 3)
                                    currentChoiceIndex = 2;
                                else if (currentChoiceIndex == 5)
                                    currentChoiceIndex = 4;
                            }
                        }
                    }

                    if (Mathf.Abs(currentPos.y - screenCenterPos.y) > mouseMovementValidDistance.y)
                    {
                        if (currentPos.y > screenCenterPos.y + mouseMovementValidDistance.y)
                        {
                            if (rows == 1)
                            {
                            }
                            else if (rows == 2)
                            {
                                if (currentChoiceIndex == 2)
                                    currentChoiceIndex = 0;
                                else if (currentChoiceIndex == 3)
                                    currentChoiceIndex = 1;
                            }
                            else if (rows == 3)
                            {
                                if (currentChoiceIndex == 2)
                                    currentChoiceIndex = 0;
                                else if (currentChoiceIndex == 3)
                                    currentChoiceIndex = 1;
                                else if (currentChoiceIndex == 4)
                                    currentChoiceIndex = 2;
                                else if (currentChoiceIndex == 5)
                                    currentChoiceIndex = 3;
                            }
                        }

                        else if (currentPos.y < screenCenterPos.y - mouseMovementValidDistance.y)
                        {
                            if (rows == 1)
                            {
                            }
                            else if (rows == 2)
                            {
                                if (currentChoiceIndex == 0)
                                    currentChoiceIndex = 2;
                                else if (currentChoiceIndex == 1)
                                    currentChoiceIndex = 3;
                            }
                            else if (rows == 3)
                            {
                                if (currentChoiceIndex == 0)
                                    currentChoiceIndex = 2;
                                else if (currentChoiceIndex == 1)
                                    currentChoiceIndex = 3;
                                else if (currentChoiceIndex == 2)
                                    currentChoiceIndex = 4;
                                else if (currentChoiceIndex == 3)
                                    currentChoiceIndex = 5;
                            }
                        }
                    }

                    #endregion

                    #region [Prevent over value]

                    if (currentChoiceIndex > choices.Count - 1)
                        currentChoiceIndex = choices.Count - 1;
                    else if (currentChoiceIndex < 0)
                        currentChoiceIndex = 0;

                    #endregion

                    GameManager.Instance.Player.MouseManager.CursorImageManager.SetCursorPosition(choices[currentChoiceIndex].percentageFill.position + fillPercentageCursorOffset);
                    GameManager.Instance.Player.MouseManager.SetCursorPosition(Vector2.zero, true);

                    yield return new WaitForSeconds(0.33f);
                }
            }

            IEnumerator AddingPercentage()
            {
                while (!hasChosen)
                {
                    for (int i = 0; i < choices.Count; i++)
                        if (i != currentChoiceIndex) choices[i].Hover(false);

                    choices[currentChoiceIndex].Hover(true);

                    yield return null;
                }
            }
        }

        void ChooseCurrentChoice()
        {
            StartCoroutine(Closing());
            IEnumerator Closing()
            {
                var chosenChoice = choices[currentChoiceIndex];
                // Display chosen choice
                GameManager.Instance.Player.MouseManager.CursorImageManager.SetCursorPosition(chosenChoice.percentageFill.position + fillPercentageCursorOffset);
                for (int i = 0; i < choices.Count; i++)
                    if (i != currentChoiceIndex) StartCoroutine(choices[i].Dim(1));

                yield return new WaitForSeconds(2f);

                isActive = false;

                animator.SetBool(boo_show, false);
                yield return new WaitForSeconds(animationShowDuration);

                OnChosen(chosenChoice.data.choiceIndex);
                GameManager.Instance.Player.EnableAllInputs(true);
                GameManager.Instance.EnableUICornerTools(true, false);
                GameManager.Instance.Player.MouseManager.CursorImageManager.StartFollowingMouse();
                GameManager.Instance.Player.OnClick -= OnPlayerControllerMouseClick;

                Destroy(gameObject);
            }
        }

        void OnPlayerControllerMouseClick(bool isClicked)
        {
            if (isClicked)
                Choose(currentChoiceIndex);
        }

        void Choose(int choiceIndex)
        {
            choices.Find(c=>c.data.choiceIndex == choiceIndex).Chosen();
            hasChosen = true;
            ChooseCurrentChoice();
        }
    }
}