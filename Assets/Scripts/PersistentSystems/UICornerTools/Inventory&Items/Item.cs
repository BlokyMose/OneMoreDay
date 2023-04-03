using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Encore.Inventory
{
    [CreateAssetMenu(menuName = "SO/Item/Item")]
    [InlineEditor]
    public class Item : ScriptableObject
    {
        #region [Classes]

        [System.Serializable]
        public class TagData
        {
            [Tooltip("Targeted tag, so it can be stored in ItemStorage which allows this tag")]
            public ItemTag tag;

            [Tooltip("Substitute item.spirte depending on the storage's tag filter"), PreviewField]
            public Sprite sprite;
        }

        #endregion

        #region [Main Properties]

        [SerializeField, HorizontalGroup("Main"), PreviewField(125, ObjectFieldAlignment.Left), HideLabel]
        private Sprite sprite;

        [SerializeField, VerticalGroup("Main/Column"), LabelWidth(70)]
        private string itemName;

        [SerializeField, VerticalGroup("Main/Column"), LabelWidth(70)]
        private string saveKey;

        [SerializeField, VerticalGroup("Main/Column"), TextArea, PropertySpace(15)]
        private string description;

        #endregion

        #region [Storage]

        [Title("Storage")]
        [SerializeField]
        private int storageSize = 1;

        [SerializeField]
        List<TagData> tags = new List<TagData>();

        #endregion

        #region [Grip Feature]

        [Title("Grip Feature")]

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private bool followCursor = false;

        public enum HandGripMode { Left, Right, Both }
        [SerializeField]
        private HandGripMode gripMode = HandGripMode.Left;

        [SerializeField]
        private bool isGripOnly = false;

        [Header("Transform")]
        [SerializeField]
        private Vector2 offsetPosition = Vector2.zero;
        [SerializeField]
        private Vector3 offsetRotation = Vector3.zero;
        [SerializeField]
        private Vector3 scaleMultiplier = Vector3.one;

        #region [Getters]

        public bool FollowCursor { get { return followCursor; } }
        public HandGripMode GripMode { get { return gripMode; } }
        public bool IsGripOnly { get { return isGripOnly; } }
        public Vector2 OffsetPosition { get { return offsetPosition; } }
        public Vector3 ScaleMultiplier { get { return scaleMultiplier; } }
        public Vector3 OffsetRotation { get { return offsetRotation; } }

        #endregion

        #endregion

        #region [Item Action]

        [Title("Item Action")]
        [SerializeField, Tooltip("Inventory shows all possible ItemActions which exist in this prefab")]
        GameObject actionPrefab;

        #endregion

        #region [Getters]

        public string ItemName
        {
            get { return itemName; }
        }

        public string Description
        {
            get { return description; }
        }

        public string SaveKey
        {
            get { return saveKey; }
        }

        public Sprite Sprite
        {
            get { return sprite; }
        }

        public GameObject Prefab
        {
            get { return prefab; }
        }

        public int StorageSize
        {
            get { return storageSize; }
        }

        public List<ItemTag> Tags
        {
            get
            {
                var _tags = new List<ItemTag>(); 
                foreach (var tagData in tags) _tags.Add(tagData.tag); 
                return _tags; 
            }
        }

        public List<TagData> TagsData
        {
            get { return tags; }
        }

        public GameObject ActionPrefab
        {
            get { return actionPrefab; }
        }

        #endregion

        /// <summary> Used only when instantiating Item by code </summary>
        public void Setup(Sprite sprite, string itemName, string description, GameObject prefab)
        {
            this.sprite = sprite;
            this.itemName = itemName;
            this.description = description;
            this.prefab = prefab;
        }

        private void OnValidate()
        {
            if (!saveKey.Contains(itemName)) saveKey = itemName;
        }
    }
}