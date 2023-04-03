using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Encore.CharacterControllers;

namespace Encore.Doomclock
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members

    [AddComponentMenu("Encore/Doomclock/Doomclock Manager")]
    public class DoomclockManager : UICornerTool
    {
        [TitleGroup("Countdown Starting Time", GroupID = "StartCountdown")]

        [HorizontalGroup("StartCountdown/CD", LabelWidth = 40)]
        [SerializeField]
        [LabelText("Hour")]
        [Wrap(0, 24)]
        [DisableInPlayMode]
        private int startHour = 0;

        [HorizontalGroup("StartCountdown/CD")]
        [SerializeField]
        [LabelText("Min")]
        [Wrap(0, 60)]
        [DisableInPlayMode]
        private int startMinute = 0;

        [HorizontalGroup("StartCountdown/CD")]
        [SerializeField]
        [LabelText("Sec")]
        [Wrap(0, 60)]
        [DisableInPlayMode]
        private int startSecond = 0;

        [TitleGroup("Remaining Time", GroupID = "RemainingTime")]
        [SerializeField]
        [HideLabel]
        [ReadOnly]
        private string countdownInString;

        public Clock countdownClock { get; private set; }

        [TitleGroup("Properties", GroupID = "Properties")]
        [ShowInInspector]
        [ReadOnly]
        private bool _started;
        public bool started
        {
            get
            {
                return _started;
            }
            private set
            {
                _started = value;
            }
        }

        [Title("UI Objects")]
        [SerializeField]
        [HideInPlayMode]
        private RectTransform hourHand;
        [SerializeField]
        [HideInPlayMode]
        private RectTransform minuteHand;
        [SerializeField]
        [HideInPlayMode]
        private TextMeshProUGUI countdownText;

        [Title("Time Manager")]
        [SerializeField]
        private bool subscribeToTimeManagerAtStart = true;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            if (subscribeToTimeManagerAtStart) SubscribeToTimeManager();
        }

        public override void Show(bool isShowing)
        {
            base.Show(isShowing);
            if (!canShow) return;

            GameManager.Instance.Player.SetCanInteract(!isShowing, CursorImageManager.CursorImage.Normal);
        }

        [Title("Debug")]
        [DisableInEditorMode]
        [HideIf("_started")]
        [Button("Start Countdown")]
        public void StartCountdown()
        {
            started = true;
        }

        [DisableInEditorMode]
        [ShowIf("_started")]
        [Button("Stop Countdown")]
        public void StopCountdown()
        {
            started = false;
        }

        [Button("Subscribe")]
        public void SubscribeToTimeManager()
        {
            GameManager.Instance.TimeManager.OnSecondPassed += EverySecondPassed;
        }

        public void UnsubscribeToTimeManager()
        {
            GameManager.Instance.TimeManager.OnSecondPassed -= EverySecondPassed;
        }

        public void EverySecondPassed(object sender, Clock clock)
        {
            if (started)
            {
                if (countdownClock.PreviousSecond().IsZero())
                {
                    Debug.LogWarning("WAKTU MU HABIS CINTA!");
                }
            }

            UpdateClockUI(clock.GetHour(), clock.GetMinute());
        }

        public void UpdateClockUI(int _h, int _m)
        {
            float hourRotationRatio = 360f / 12f;
            float minRotationRatio = 360f / 60f;

            float hourRotation = _h * hourRotationRatio;
            float addedHourRotation = _m / 60f * hourRotationRatio;
            float minRotation = _m * minRotationRatio;

            if (countdownClock != null)
            {
                /*
                int _cdH = Mathf.FloorToInt(_cd / 3600);
                int _cdM = Mathf.FloorToInt(_cd % 3600 / 60);
                int _cdS = Mathf.FloorToInt(_cd % 3600 % 60);

                countdownText.text = $"{_cdH:D2}:{_cdM:D2}:{_cdS:D2}";
                */

                countdownText.text = countdownClock.ToString();
            }
            else
            {
                countdownText.text = "00:00:00";
            }

            hourHand.localEulerAngles = new Vector3(0, 0, -hourRotation - addedHourRotation);
            minuteHand.localEulerAngles = new Vector3(0, 0, -minRotation);
        }


        #region [Methods: UI Corner Tool]
        public override void OnBeforeSceneLoad()
        {
        }

        public override void OnAfterSceneLoad()
        {
        }



        #endregion
    }
}