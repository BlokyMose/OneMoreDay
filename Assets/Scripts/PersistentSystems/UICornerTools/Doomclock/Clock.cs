using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Doomclock
{
    public class Clock
    {
        private int hour, minute, second;
        private bool isTimer;

        public Clock() { }
        public Clock(int hour, int minute, int second)
        {
            SetTime(hour, minute, second);
        }
        public Clock(int hour, int minute, int second, bool isTimer)
        {
            SetTime(hour, minute, second);
            this.isTimer = isTimer;
        }

        public void SetTime(int hour, int minute, int second)
        {
            try
            {
                checkTime(hour, minute, second);

                this.hour = hour;
                this.minute = minute;
                this.second = second;
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError(string.Format("{0}: {1}", e.GetType().Name, e.Message));
            }
        }

        public void SetHour(int hour)
        {
            SetTime(hour, minute, second);
        }
        public void SetMinute(int minute)
        {
            SetTime(hour, minute, second);
        }
        public void SetSecond(int second)
        {
            SetTime(hour, minute, second);
        }

        public int GetHour()
        {
            return hour;
        }
        public int GetMinute()
        {
            return minute;
        }
        public int GetSecond()
        {
            return second;
        }
        public bool IsZero()
        {
            if (hour == 0 && minute == 0 && second == 0)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", hour.ToString("00"), minute.ToString("00"), second.ToString("00"));
        }

        private void checkTime(int hour, int minute, int second)
        {
            if (hour > 23 || hour < 0 || minute > 59 || minute < 0 || second > 59 || second < 0)
            {
                throw new System.ArgumentException("Invalid hour, minute, or second!");
            }
        }

        public Clock NextSecond()
        {
            if (second + 1 > 59)
            {
                second = 0;
                NextMinute();
            }
            else second++;

            return this;
        }

        public Clock NextMinute()
        {
            if (minute + 1 > 59)
            {
                minute = 0;
                NextHour();
            }
            else minute++;

            return this;
        }

        public Clock NextHour()
        {
            if (hour + 1 > 23) hour = 0;
            else hour++;

            return this;
        }

        public Clock PreviousSecond()
        {
            if (isTimer && IsZero())
            {
                return this;
            }

            if (second - 1 < 0)
            {
                second = 59;
                PreviousMinute();
            }
            else second--;

            return this;
        }

        public Clock PreviousMinute()
        {
            if (isTimer && IsZero())
            {
                return this;
            }

            if (minute - 1 < 0)
            {
                minute = 59;
                PreviousHour();
            }
            else minute--;

            return this;
        }

        public Clock PreviousHour()
        {
            if (isTimer && IsZero())
            {
                return this;
            }

            if (hour - 1 < 0) hour = 23;
            else hour--;

            return this;
        }
    }
}