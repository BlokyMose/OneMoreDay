using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.Saves;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/GO Destroyer")]
    public class GODestroyer : Interactable
    {
        [Title("GO Destroyer")]
        [SerializeField] List<GameObject> targets;

        protected override void InteractModule(GameObject interactor)
        {
            foreach (var target in targets)
            {
                Destroy(target);
            }
        }

        [Button("Add SaveGOState To Targets"), GUIColor("@Color.green")]
        void _AddSaveGOStateToTargets()
        {
            foreach (var go in targets)
            {
                if (go.GetComponent<GOSaver>() == null)
                {
                    go.AddComponent<GOSaver>();
                }
            }
        }

    }
}

