using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Serializables
{
    [System.Serializable]
    public class GlobalVariableData
    {
        public string ID { get { return id; } }
        [SerializeField]
        string id;

        public string VarName { get { return varName; } }
        [SerializeField]
        string varName;

        public string VarValue { get { return varValue; } set { varValue = value; } }
        [SerializeField]
        string varValue;

        public int VarType { get { return varType; } }
        [SerializeField]
        int varType;

        public GlobalVariableData(string id, string varName, string varValue, int varType = 1)
        {
            this.id = id;
            this.varName = varName;
            this.varValue = varValue;
            this.varType = varType;
        }

        public GlobalVariableData(global::GlobalVariable globalVariable)
        {
            this.id = globalVariable.ID;
            this.varName = globalVariable.VarName;
            this.varValue = globalVariable.VarValue;
            this.varType = (int)globalVariable.VarType;
        }

        public const string time = "time";

    }
}
