using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Global Variable Setter")]
    public class GlobalVariableSetter : Interactable
    {
        [SerializeField, OnValueChanged(nameof(_OnGlobalVariableSet))]
        GlobalVariable globalVariable;

        [SerializeField, DisableIf("@true")]
        GlobalVariable.VariableType varType;

        [SerializeField, ShowIf("@varType == GlobalVariable.VariableType.Bool"), LabelText("Previous value"), DisableIf("@true")]
        bool varBoolPrevious = true;
        [SerializeField, ShowIf("@varType == GlobalVariable.VariableType.String"), LabelText("Previous value"), DisableIf("@true")]
        string varStringPrevious = "";
        [SerializeField, ShowIf("@varType == GlobalVariable.VariableType.Integer"), LabelText("Previous value"), DisableIf("@true")]
        int varIntegerPrevious;

        [SerializeField, ShowIf("@varType == GlobalVariable.VariableType.Bool"), LabelText("Change to")]
        bool varBool = true;
        [SerializeField, ShowIf("@varType == GlobalVariable.VariableType.String"), LabelText("Change to")]
        string varString = "";
        [SerializeField, ShowIf("@varType == GlobalVariable.VariableType.Integer"), LabelText("Change to")]
        int varInteger;

        public void _OnGlobalVariableSet()
        {
            if (globalVariable != null)
            {
                varType = globalVariable.VarType;
                switch (varType)
                {
                    case GlobalVariable.VariableType.Bool: varBoolPrevious = bool.Parse(globalVariable.VarValue);
                        break;
                    case GlobalVariable.VariableType.String: varStringPrevious = globalVariable.VarValue;
                        break;
                    case GlobalVariable.VariableType.Integer: varIntegerPrevious = int.Parse(globalVariable.VarValue);
                        break;
                }
            }
        }

        protected override void InteractModule(GameObject interactor)
        {
            var newVarValue = "";
            switch (globalVariable.VarType)
            {
                case GlobalVariable.VariableType.Bool:
                    newVarValue = varBool.ToString();
                    break;
                case GlobalVariable.VariableType.String:
                    newVarValue = varString.ToString();
                    break;
                case GlobalVariable.VariableType.Integer:
                    newVarValue = varInteger.ToString();
                    break;
            }

            GameManager.Instance.ChangeGlobalVariable(globalVariable, newVarValue);
        }

    }

}
