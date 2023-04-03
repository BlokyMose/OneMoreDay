using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Conditions
{
    [InlineEditor]
    public abstract class Condition : ScriptableObject
    {
        public string soName;
        public bool reverseReturn = false;
        public abstract bool CheckCondition();

        protected virtual void OnValidate()
        {
            name = soName;
        }
    }

}
