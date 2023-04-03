using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class ToDoAppData
    {
        public List<ToDoData> todoDataList = new List<ToDoData>();

        public ToDoAppData(List<ToDoData> todoDataList)
        {
            this.todoDataList = todoDataList;
        }
    } 
}
