using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Relocate")]
    public class Relocate : Interactable
    {
        [Title("Relocate")]
        [SerializeField] Transform objectToRelocate;
        [SerializeField, InlineButton(nameof(AddTargetPos), "Add", ShowIf = "@!"+nameof(targetPos))] Transform targetPos;

        protected override void InteractModule(GameObject interactor)
        {
            objectToRelocate.position = targetPos.position;
        }

        public void AddTargetPos()
        {
            GameObject go = new GameObject("TargetPos");
            go.transform.parent = transform;
            go.transform.position = Vector3.zero;
            targetPos = go.transform;
        }
    }
}

