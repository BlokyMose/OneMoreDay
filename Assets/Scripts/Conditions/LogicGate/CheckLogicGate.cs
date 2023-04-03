using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Conditions;

namespace Encore.Conditions
{
    public abstract class CheckLogicGate : Condition
    {
        public List<Condition> conditions;

        #region [Buttons]

        [FoldoutGroup("Add")]

        [HorizontalGroup("Add/1"), Button("AND Gate")]
        public void _AddAndGate() { conditions.Add(CreateInstance<CheckAndGate>()); conditions[conditions.Count - 1].soName = "AND"; conditions[conditions.Count - 1].name = "AND"; }

        [HorizontalGroup("Add/1"), Button("OR Gate")]
        public void _AddOrGate() { conditions.Add(CreateInstance<CheckOrGate>()); conditions[conditions.Count - 1].soName = "OR"; conditions[conditions.Count - 1].name = "OR"; }

        [HorizontalGroup("Add/1"), Button("Is Null")]
        public void _AddCheckExistence() { conditions.Add(CreateInstance<CheckIsObjectNull>()); conditions[conditions.Count - 1].soName = "isNull"; conditions[conditions.Count - 1].name = "isNull"; }

        [HorizontalGroup("Add/2"), Button("Bool")]
        public void _AddCheckBool() { conditions.Add(CreateInstance<CheckBool>()); conditions[conditions.Count - 1].soName = "bool"; conditions[conditions.Count - 1].name = "bool"; }

        [HorizontalGroup("Add/2"), Button("Item")]
        public void _AddCheckItem() { conditions.Add(CreateInstance<CheckItem>()); conditions[conditions.Count - 1].soName = "item"; conditions[conditions.Count - 1].name = "item"; }

        [HorizontalGroup("Add/2"), Button("Is Active")]
        public void _AddCheckIsActive() { conditions.Add(CreateInstance<CheckIsGOActive>()); conditions[conditions.Count - 1].soName = "isActive"; conditions[conditions.Count - 1].name = "isActive"; }

        #endregion
    }
}
