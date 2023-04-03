using Conditions;
using System.Collections;
using UnityEngine;

namespace Encore.Conditions
{
    public class CheckIsObjectNull : Condition
    {
        [SerializeField]
        public Object target;

        public override bool CheckCondition()
        {
            return reverseReturn
                ? target == null ? true : false
                : target == null ? false : true;
        }
    }
}