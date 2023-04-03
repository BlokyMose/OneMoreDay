using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace Encore.VisualScripting
{
    [Inspectable]
    [System.Serializable]
    public class InterscriptEventParameters
    {
        [Inspectable]
        [SerializeField]
        string eventName;

        [Inspectable]
        public string EventName { get { return eventName; } set { eventName = value; } }

        [Inspectable]
        [SerializeField]
        VariableDeclarations parameters;

        [Inspectable]
        public VariableDeclarations Parameters { get { return parameters; } set { parameters = value; } }
        [Inspectable]
        [SerializeField]
        ScriptMachine scriptMachine;

        [Inspectable]
        public ScriptMachine ScriptMachine { get { return scriptMachine; } set { scriptMachine = value; } }

        public InterscriptEventParameters()
        {
        }        
        
        public InterscriptEventParameters(string eventName, VariableDeclarations parameters, ScriptMachine scriptMachine = null)
        {
            this.eventName = eventName;
            this.parameters = parameters;
            this.scriptMachine = scriptMachine;
        }

        public object GetParameter(string parameterName)
        {
            if (parameters == null) return null;

            object returnValue = null;
            foreach (var declaration in parameters)
            {
                if (declaration.name == parameterName)
                    returnValue = declaration.value;
            }
            return returnValue;
        }
    }
}