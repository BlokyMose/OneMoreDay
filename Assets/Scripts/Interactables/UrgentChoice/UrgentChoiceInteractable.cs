using Encore.Interactables;
using Encore.MiniGames.UrgentChoice;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Urgent Choice")]
    public class UrgentChoiceInteractable : Interactable
    {
        [Title("Urgent Choice")]
        [SerializeField]
        UrgentChoiceManager.UrgentChoiceParameters parameter;

        [SerializeField, Tooltip("Assigned to parameter's choice callback according to list index")]
        List<UnityEvent> actions = new List<UnityEvent>();

        private void OnEnable()
        {
            parameter.OnSetPlayerChoice += (index) => { actions[index]?.Invoke(); };
        }

        private void OnDisable()
        {
            parameter.OnSetPlayerChoice -= (index) => { actions[index]?.Invoke(); };
        }

        protected override void InteractModule(GameObject interactor)
        {
            GameManager.Instance.CreateUrgentChoice(parameter);
        }
    }
}
