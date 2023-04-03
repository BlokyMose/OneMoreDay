using Encore.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/Reset Clicked Item")]
    public class ResetClickedItem : Interactable
    {
        protected override void InteractModule(GameObject interactor)
        {
            GameManager.Instance.InventoryManager.ResetClickedItem(); 
        }
    } 
}
