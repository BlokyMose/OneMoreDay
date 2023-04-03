using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Phone.ToDo
{
    [CreateAssetMenu(menuName = "SO/ToDo/ToDoTag", fileName = "ToDoTag_")]
    public class ToDoTag : ScriptableObject
    {
        [SerializeField]
        string tagName;

        [SerializeField, PreviewField]
        Sprite image;

        public string TagName { get { return tagName; } }
        public Sprite Image { get { return image; } }
    }
}