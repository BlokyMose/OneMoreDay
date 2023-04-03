using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/RB2D Modifier")]
    public class RB2DModifier : Interactable
    {
        #region [Vars: Components]

        [Title("RB2D Modifier")]
        [SerializeField] Rigidbody2D rb;
        [SerializeField] bool simulated;

        #endregion

        #region [Methods: Inspector UI]

        [HorizontalGroup, ShowIf("@!"+nameof(rb)), Button("Get Existing"), GUIColor(0.3f, 0.8f, 0.3f)]
        public void GetExistingRB()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        [HorizontalGroup, ShowIf("@!"+nameof(rb)), Button("Add Here"), GUIColor(0.3f,0.8f,0.3f)]
        public void AddRB()
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        #endregion

        protected override void InteractModule(GameObject interactor)
        {
            rb.simulated = simulated;
        }
    }
}

