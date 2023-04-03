using Encore.CharacterControllers;
using Encore.Interactables;
using Encore.VisualScripting;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Trigger Interscript Event")]
    public class TriggerInterscriptEventInteractable : Interactable
    {
        [Title("Interscript Event")]
        [SerializeField]
        string eventName = "eventName";

        [SerializeField, InlineButton(nameof(AddScriptMachineComponent), "Add", ShowIf = "@!" + nameof(scriptMachine))]
        ScriptMachine scriptMachine;

        [SerializeField, InlineButton(nameof(AddVariablesComponent), "Add", ShowIf = "@!" + nameof(parameters))]
        Variables parameters;

        [Title("Customizations:")]
        [SerializeField]
        CursorImageManager.CursorImage cursorImage = CursorImageManager.CursorImage.Normal;
        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return cursorImage;
        }

        [Button("Debug: Test()")]
        public void TriggerEvent()
        {
            EventBus.Trigger(EventNames.InterscriptEvent,
                new InterscriptEventParameters(eventName, parameters != null ? parameters.declarations : null, scriptMachine));
        }

        #region [Methods: Inspector]

        void AddVariablesComponent()
        {
            if (GetComponent<Variables>() == null)
                parameters = gameObject.AddComponent<Variables>();
            else
                parameters = gameObject.GetComponent<Variables>();
        }

        void AddScriptMachineComponent()
        {
            if (GetComponent<ScriptMachine>() == null)
                scriptMachine = gameObject.AddComponent<ScriptMachine>();
            else
                scriptMachine = gameObject.GetComponent<ScriptMachine>();
        }

        #endregion

        protected override void InteractModule(GameObject interactor)
        {
            TriggerEvent();
        }
    }
}