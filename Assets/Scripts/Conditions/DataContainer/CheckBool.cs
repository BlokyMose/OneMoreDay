using Conditions;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Encore.Conditions
{
    [CreateAssetMenu(menuName = "SO/Interactable Conditions/Bool")]
    public class CheckBool : Condition
    {
        public bool BoolValue { get { return boolValue; } set { boolValue = value; } }
        [SerializeField] bool boolValue = true;


        [SerializeField, RequireInterface(typeof(ICheckBool))]
        Object syncWith;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (syncWith != null)
            {
                if (syncWith is ICheckBool)
                    (syncWith as ICheckBool).SetBoolContainer(this);
                else
                    Debug.Log("not IBoolContainerSetter");
            }
        }

        public override bool CheckCondition()
        {
            return boolValue;
        }
    }
}