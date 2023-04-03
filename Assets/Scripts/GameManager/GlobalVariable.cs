using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "SO/Variables/Global Variable", fileName ="var_")]
public class GlobalVariable : ScriptableObject
{
    public string ID { get { return id; } }
    [SerializeField,EnableIf("@string.IsNullOrEmpty(" + nameof(id) + ")"),  InlineButton(nameof(_GetRandomID),ShowIf ="@string.IsNullOrEmpty("+nameof(id)+")")]
    string id;
    
    public string VarName { get { return varName; } }
    [SerializeField]
    string varName;

    public virtual string VarValue { 
        get 
        {
            return
                varType == VariableType.Bool ? varBool.ToString() :
                varType == VariableType.String ? varString :
                varType == VariableType.Integer ? varInteger.ToString() :
                "";
        } 
    }

    public VariableType VarType { get { return varType; } }
    [SerializeField]
    VariableType varType = VariableType.Bool;

    [SerializeField, ShowIf("@varType == VariableType.Bool"), LabelText("Var Value")]
    bool varBool = true;
    [SerializeField, ShowIf("@varType == VariableType.String"), LabelText("Var Value")]
    string varString = "";
    [SerializeField, ShowIf("@varType == VariableType.Integer"), LabelText("Var Value")]
    int varInteger;

    public enum VariableType { Bool, String, Integer }

    public GlobalVariable(string id, string varName, bool varValue)
    {
        this.id = id;
        this.varName = varName;
        this.varBool = varValue;
        this.varType = VariableType.Bool;
    }    
    
    public GlobalVariable(string id, string varName, string varValue)
    {
        this.id = id;
        this.varName = varName;
        this.varString = varValue;
        this.varType = VariableType.String;
    }    
    
    public GlobalVariable(string id, string varName, int varValue)
    {
        this.id = id;
        this.varName = varName;
        this.varInteger = varValue;
        this.varType = VariableType.Integer;
    }

    public void _GetRandomID()
    {
        if (id == null) id = System.Guid.NewGuid().ToString();
    }
}
