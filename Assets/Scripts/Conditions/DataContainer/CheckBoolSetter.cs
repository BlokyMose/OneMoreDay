using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Conditions
{
    [System.Serializable]
    public class CheckBoolSetter
    {
        [SerializeField] CheckBool checkBool;

        public enum CheckBoolSetMode { OneTime, Toggle, Random }
        [SerializeField] CheckBoolSetMode mode = CheckBoolSetMode.OneTime;

        [SerializeField, ShowIf("@" + nameof(mode) + "== CheckBoolSetMode.OneTime")]
        bool setTo = true;

        public void SetCheckBool()
        {
            switch (mode)
            {
                case CheckBoolSetMode.OneTime:
                    checkBool.BoolValue = setTo;
                    break;
                case CheckBoolSetMode.Toggle:
                    checkBool.BoolValue = !checkBool.BoolValue;
                    break;
                case CheckBoolSetMode.Random:
                    checkBool.BoolValue = Random.Range(0, 2) == 0 ? false : true;
                    break;
                default:
                    break;
            }
        }
    }
}