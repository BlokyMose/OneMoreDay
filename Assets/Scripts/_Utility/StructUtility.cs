using System.Collections;
using UnityEngine;

namespace Encore.Utility
{
    public enum BoolStatus { False, True, AsIs }

    public static class BoolStatusExtensions
    {
        public static bool GetBool(this BoolStatus status, bool defaultValue = false)
        {
            switch (status)
            {
                case BoolStatus.False: return false;
                case BoolStatus.True: return true;
                case BoolStatus.AsIs: return defaultValue;
                default: return defaultValue;
            }
        }
    }

    public struct Padding
    {
        public float left;
        public float top;
        public float right;
        public float bottom;

        public Padding(float left, float top, float right, float bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }



    public static class StructUtility 
    {
    }
}