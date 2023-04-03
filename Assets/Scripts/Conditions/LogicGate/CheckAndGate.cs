using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Conditions
{
    [CreateAssetMenu(menuName = "SO/Interactable Conditions/AND Gate")]
    public class CheckAndGate : CheckLogicGate
    {
        public override bool CheckCondition()
        {
            foreach (var item in conditions)
                if (!item.CheckCondition())
                    return false;

            return true;
        }
    }
}
