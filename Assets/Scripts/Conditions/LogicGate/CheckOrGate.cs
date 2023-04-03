using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Conditions
{
    [CreateAssetMenu(menuName = "SO/Interactable Conditions/OR Gate")]
    public class CheckOrGate : CheckLogicGate
    {
        public override bool CheckCondition()
        {
            int falseCount = 0;
            foreach (var item in conditions)
            {
                if (!item.CheckCondition()) falseCount++;
            }
            if (falseCount >= conditions.Count) return false;
            else return true;
        }
    }
}
