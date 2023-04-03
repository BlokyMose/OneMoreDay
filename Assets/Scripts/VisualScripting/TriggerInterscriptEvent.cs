using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Sirenix.OdinInspector;

namespace Encore.VisualScripting
{
    [AddComponentMenu("Encore/Visual Scripting/Trigger Interscript Event")]
    public class TriggerInterscriptEvent : MonoBehaviour
    {
        [SerializeField]
        string eventName = "eventName";

        [SerializeField, InlineButton(nameof(AddScriptMachineComponent), "Add", ShowIf = "@!" + nameof(scriptMachine))]
        ScriptMachine scriptMachine;

        [SerializeField, InlineButton(nameof(AddVariablesComponent), "Add", ShowIf = "@!" + nameof(parameters))]
        Variables parameters;
        public Variables Parameters { get { return parameters; } }

        [Button("Debug: Test()")]
        public void TriggerEvent()
        {
            EventBus.Trigger(EventNames.InterscriptEvent, 
                new InterscriptEventParameters(eventName, parameters != null ? parameters.declarations : null , scriptMachine)) ;
        }

        #region [Methods: Inspector]

        public void AddVariablesComponent()
        {
            if (GetComponent<Variables>() == null)
                parameters = gameObject.AddComponent<Variables>();
            else
                parameters = gameObject.GetComponent<Variables>();
        }

        public void AddScriptMachineComponent()
        {
            if (GetComponent<ScriptMachine>() == null)
                scriptMachine = gameObject.AddComponent<ScriptMachine>();
            else
                scriptMachine = gameObject.GetComponent<ScriptMachine>();

        }

        #endregion
    }
}