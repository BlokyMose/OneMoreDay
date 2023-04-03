using Encore.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Table")]
    public class Table : Interactable
    {
        protected override void InteractModule(GameObject interactor)
        {
             
            Debug.Log("Hi, I'm a table!");
        }
    }
}
