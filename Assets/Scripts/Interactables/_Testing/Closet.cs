using Encore.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Closet")]
    public class Closet : Interactable
    {
        protected override void InteractModule(GameObject interactor)
        {
            Debug.Log("Halo, saya closet!");
        }
    }
}
