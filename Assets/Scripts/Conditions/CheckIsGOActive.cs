using Conditions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Conditions
{
    public class CheckIsGOActive : Condition
    {
        public List<GameObject> gos;
        public override bool CheckCondition()
        {
            bool returnValue = true;
            foreach (var item in gos)
                if (!item.activeInHierarchy) returnValue = false;

            return reverseReturn ? !returnValue : returnValue;
        }
    }
}