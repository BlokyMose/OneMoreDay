using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Conditions;

namespace Encore.Conditions
{
    [InlineEditor]
    [AddComponentMenu("Encore/Conditions/Condition Container")]
    public class ConditionContainer : MonoBehaviour
    {
        [SerializeField]
        Condition condition;
        public Condition Condition { get { return condition; } }

        public bool CheckCondition()
        {
            if (condition != null)
            {
                return condition.CheckCondition();
            }
            else
            {
                Debug.Log("Condition not found: "+gameObject.name);
                return true;
            }
        }

        #region [Buttons]

        [FoldoutGroup("Add", VisibleIf = "@!" + nameof(condition))]

        [HorizontalGroup("Add/1"), Button("AND Gate")]
        public void _AddAndGate() { condition = ScriptableObject.CreateInstance<CheckAndGate>(); condition.soName = "AND"; condition.name = "AND"; }

        [HorizontalGroup("Add/1"), Button("OR Gate")]
        public void _AddOrGate() { condition = ScriptableObject.CreateInstance<CheckOrGate>(); condition.soName = "OR"; condition.name = "OR"; }

        [HorizontalGroup("Add/1"), Button("Is Null")]
        public void _AddCheckExistence() { condition = ScriptableObject.CreateInstance<CheckIsObjectNull>(); condition.soName = "isNull"; condition.name = "isNull"; }

        [HorizontalGroup("Add/2"), Button("Bool")]
        public void _AddCheckBool() { condition = ScriptableObject.CreateInstance<CheckBool>(); condition.soName = "bool"; condition.name = "bool"; }

        [HorizontalGroup("Add/2"), Button("Item")]
        public void _AddCheckItem() { condition = ScriptableObject.CreateInstance<CheckItem>(); condition.soName = "item"; condition.name = "item"; }

        [HorizontalGroup("Add/2"), Button("Is Active")]
        public void _AddCheckIsActive() { condition = ScriptableObject.CreateInstance<CheckIsGOActive>(); condition.soName = "isActive"; condition.name = "isActive"; }

        #endregion
    }
}