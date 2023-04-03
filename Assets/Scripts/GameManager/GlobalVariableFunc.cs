using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore
{
    public abstract class GlobalVariableFunc : ScriptableObject
    {
        public abstract string FuncName { get; }
        public abstract string Invoke(string parameters);


        [ShowInInspector, LabelWidth(1), Header("Call example:")]
        protected string callExample;

        [ShowInInspector, LabelWidth(1), Header("Result example:")]
        protected string resultExample;

        protected abstract void OnEnable();

    }
}
