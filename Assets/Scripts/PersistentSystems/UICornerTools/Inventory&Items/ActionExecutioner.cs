using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Encore.Inventory
{

    /*NOTES:

    [GENERAL IDEA]:
    Acts as a bridge, so other component can call another component using string

    [For Item's Action Prefab]
    (Feb 25, 2022)
    - Add manually in item.actionPrefab's gameObject
    - InventoryManager creates actionButtons according to the list of actions here

    */

    [AddComponentMenu("Encore/Inventory/Action Executioner")]
    public class ActionExecutioner : MonoBehaviour
    {
        #region [Classes]

        [System.Serializable]
        public class ActionExecution
        {
            public string actionName;
            [HorizontalGroup(width: 0.8f)]
            public UnityEvent executeEvent;
            [HorizontalGroup, HideLabel, PreviewField]
            public Sprite icon;
        }

        #endregion

        #region [Vars: Properties]

        [SerializeField]
        List<ActionExecution> actions = new List<ActionExecution>();
        public List<ActionExecution> Actions { get { return actions; } }

        #endregion

        public void AddItemAction(ActionExecution itemAction)
        {
            actions.Add(itemAction);
        }

        public bool Execute(string actionName)
        {
            var foundEvent = actions.Find(x => x.actionName == actionName);
            if (foundEvent != null)
            {
                foundEvent.executeEvent.Invoke();
                return true;
            }
            else
                return false;
        }

        public void TryExecute(string actionName)
        {
            Execute(actionName);
        }
    }
}