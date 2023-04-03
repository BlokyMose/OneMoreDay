using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class InventoryData
    {
        public List<string> items = new List<string>();
        public int inventoryMaxSize;
        public string clickedItem;

        public InventoryData(List<string> items, int inventoryMaxSize, string clickedItem)
        {
            this.items = items;
            this.inventoryMaxSize = inventoryMaxSize;
            this.clickedItem = clickedItem;
        }
    }
}
