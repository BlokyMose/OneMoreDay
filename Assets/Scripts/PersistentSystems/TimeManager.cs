using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Doomclock
{
    [AddComponentMenu("Encore/Doomclock")]
    public class TimeManager : MonoBehaviour, IPersistentSystem
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);

            clock = new Clock(startHour, startMinute, startSecond);
            timeInString = clock.ToString();

        }

        [TitleGroup("Starting Time", GroupID = "StartTime")]
        [SerializeField]
        [HorizontalGroup("StartTime/Time", LabelWidth = 40)]
        [LabelText("Hour")]
        [Wrap(0, 24)]
        [DisableInPlayMode]
        private int startHour = 0;
        [SerializeField]
        [HorizontalGroup("StartTime/Time")]
        [LabelText("Min")]
        [Wrap(0, 60)]
        [DisableInPlayMode]
        private int startMinute = 0;
        [SerializeField]
        [HorizontalGroup("StartTime/Time")]
        [LabelText("Sec")]
        [Wrap(0, 60)]
        [DisableInPlayMode]
        private int startSecond = 0;

        [TitleGroup("Current Time", GroupID = "CurrentTime")]
        [SerializeField]
        [HideLabel]
        [ReadOnly]
        private string timeInString;

        public Clock clock { get; private set; }

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

        Coroutine clockCoroutine;

        public event EventHandler<Clock> OnSecondPassed;

        [DisableInEditorMode]
        [HideIf("_started")]
        [Button("Start Clock")]
        public void StartClock()
        {
            if (started == false)
            {

            }
            clockCoroutine = StartCoroutine(ClockCoroutine());

            started = true;
        }

        [DisableInEditorMode]
        [ShowIf("_started")]
        [Button("Stop Clock")]
        public void StopClock()
        {
            StopCoroutine(clockCoroutine);

            started = false;
        }

        IEnumerator ClockCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                clock.NextSecond();
                timeInString = clock.ToString();
                //OnSecondPassed?.BeginInvoke(this, clock, null, null);
                OnSecondPassed?.Invoke(this, clock);
            }
        }

        public void OnBeforeSceneLoad()
        {
        }

        public void OnAfterSceneLoad()
        {
        }
    }
}