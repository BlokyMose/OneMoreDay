using System.Collections;
using UnityEngine;

namespace Encore.Utility
{
    public static class CalcUtility 
    {
        public static void CrossOp(out float xCurrent, float xTotal, float yCurrent, float yTotal)
        {
            xCurrent = xTotal * yCurrent / yTotal;
        }

        public static void CrossOp(float xCurrent, out float xTotal, float yCurrent, float yTotal)
        {
            xTotal = xCurrent * yTotal / yCurrent;
        }
    }
}