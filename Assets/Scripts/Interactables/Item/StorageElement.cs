using Encore.Inventory;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Encore.Interactables.ItemStorageAny;

namespace Encore.Interactables
{
    [Serializable]
    public class StorageElement 
    {
        public StorageElement(GameObject go, ItemUseHook useHook, ItemPickupHook pickupHook, ItemPickup pickup, SpriteRenderer sr, bool isOccupied)
        {
            this.go = go;
            this.useHook = useHook;
            this.pickupHook = pickupHook;
            this.pickup = pickup;
            this.sr = sr;
            this.isOccupied = isOccupied;
        }

        #region [Vars: Components]

        [FoldoutGroup("Components")]
        public GameObject go;

        [FoldoutGroup("Components")]
        public ItemUseHook useHook;

        [FoldoutGroup("Components")]
        public ItemPickupHook pickupHook;

        [FoldoutGroup("Components")]
        public ItemPickup pickup;

        [FoldoutGroup("Components")]
        public SpriteRenderer sr;

        [FoldoutGroup("Components"), SerializeField, Tooltip("GO repositioned based on storage current size; optional")]
        List<Transform> posSize = new List<Transform>();

        public List<Transform> PosSize { get { return posSize; } }

        #endregion

        #region [Vars: Data Handlers]

        public const string PREFAB = "PREFAB";
        public const string EMPTY = "EMPTY";

        [FoldoutGroup("Properties"), SerializeField, OnValueChanged(nameof(_ModifyInteractable)), GUIColor("@" + nameof(isOccupied) + "?Color.green:new Color(1f,0.66f,0.66f)")]
        public bool isOccupied;

        #endregion

        #region [Methods: Inspector]

        [HorizontalGroup("But"), Button("Add Pos")]
        [PropertyTooltip("Remember to add initial pos as the first pos before adding alternate pos\n" +
            "Default: if list is empty, position won't be changed")]
        void _AddPosSize()
        {
            // Find parent
            GameObject posParent = null;
            for (int i = 0; i < go.transform.parent.childCount; i++)
            {
                if (go.transform.parent.GetChild(i).name == "StoragePositions")
                {
                    posParent = go.transform.parent.GetChild(i).gameObject;
                    break;
                }
            }

            // Else, create parent
            if (posParent == null)
            {
                posParent = new GameObject();
                posParent.name = "StoragePositions";
                posParent.transform.parent = go.transform.parent;
                posParent.transform.localPosition = Vector3.zero;
            }

            // Create pos
            GameObject posGO = new GameObject();
            posGO.name = useHook.Item.ItemName + "_" + (posSize.Count + 1).ToString();
            posGO.transform.parent = posParent.transform;
            posGO.transform.position = go.transform.position;

            AddPosSize(posGO.transform);
        }

        [HorizontalGroup("But"), Button("Delete Capacity")]
        void _Delete()
        {
            for (int i = posSize.Count - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(posSize[i].gameObject);
            }
            GameObject.DestroyImmediate(go);
        }

        public void _ModifyInteractable()
        {
            sr.enabled = isOccupied;
            pickup.Activate(isOccupied);
        }

        #endregion

        #region [Methods]

        public void Move(int currentSize)
        {
            if (PosSize.Count > 0 && PosSize.Count >= currentSize)
            {
                go.transform.position = PosSize[currentSize - 1].position;
            }
        }

        public void AddPosSize(Transform transform)
        {
            posSize.Add(transform);
        }

        public virtual void SetupItem(Item item)
        {
            pickupHook.Item = item;
            useHook.Item = item;
            sr.sprite = item.Sprite;
        }

        public virtual bool Occupy(Item item)
        {
            if (useHook.Use())
            {
                isOccupied = true;
                pickup.Activate(true);
                return true;
            }
            return false;
        }

        public virtual void Emptied()
        {
            isOccupied = false;

            // Reset highlighted spriteRenderers
            pickup.HighlightedSRs.Clear();
            pickup.HighlightedSRs.Add(sr);

            // Destroy item's prefab
            for (int i = 0; i < go.transform.childCount; i++)
            {
                if (go.transform.GetChild(i).name == PREFAB)
                {
                    GameObject.Destroy(go.transform.GetChild(i).gameObject);
                    break;
                }
            }
        }

        public virtual void ChangeSprite(Item item)
        {
            sr.sprite = item.Sprite;
        }

        public virtual void InstantiateItemPrefab()
        {
            sr.enabled = false;
            GameObject itemPrefabGO = GameObject.Instantiate(useHook.Item.Prefab);
            itemPrefabGO.name = StorageElement.PREFAB;
            itemPrefabGO.transform.parent = this.go.transform;
            itemPrefabGO.transform.localPosition = Vector3.zero;

            var itemPrefabSR = itemPrefabGO.GetComponent<SpriteRenderer>();
            if (itemPrefabSR != null) 
                pickup.HighlightedSRs.Add(itemPrefabSR);

            var srInChildren = new List<SpriteRenderer>(itemPrefabGO.GetComponentsInChildren<SpriteRenderer>());
            if (srInChildren.Count > 0) 
                pickup.HighlightedSRs.AddRange(srInChildren);
        }

        #endregion
    }

    [Serializable]
    public class StorageElementAny : StorageElement
    {
        [SerializeField]
        ItemStorageAny storage;

        public StorageElementAny(
            GameObject go, 
            ItemUseHook useHook, 
            ItemPickupHook pickupHook, 
            ItemPickup pickup, 
            SpriteRenderer sr, 
            bool isOccupied) : base(go, useHook, pickupHook, pickup, sr, isOccupied)
        {
        }

        public StorageElementAny(StorageElement element, ItemStorageAny storage) : base(element.go, element.useHook, element.pickupHook, element.pickup, element.sr, element.isOccupied)
        {
            this.storage = storage;
        }

        public override void Emptied()
        {
            base.Emptied();
            sr.sprite = null;
            var oldCol = go.GetComponent<Collider2D>();
            if (oldCol != null) GameObject.DestroyImmediate(oldCol);
        }

        public override void SetupItem(Item item)
        {
            base.SetupItem(item);
            var col = go.GetComponent<Collider2D>();
            if (col == null) pickup.AddPolygonCol();
        }

        public override void ChangeSprite(Item item)
        {
            if (item == null)
            {
                sr.sprite = null;
                return;
            }

            if (storage == null)
            {
                storage = go.transform.GetComponentInParent<ItemStorageAny>();
                if (storage == null)
                {
                    Debug.Log("Cannot find item storage any for element: " + go.name);
                    return;

                }
            }

            if (storage.TagFilter == StorageTagFilter.Include || storage.TagFilter == StorageTagFilter.Both)
            {
                bool breakLoop = false;
                foreach (var tag in storage.TagsInclude)
                {
                    foreach (var tagData in item.TagsData)
                    {
                        if (tagData.tag == tag)
                        {
                            if (tagData.sprite == null)
                                sr.sprite = item.Sprite;
                            else
                                sr.sprite = tagData.sprite;

                            breakLoop = true;
                            break;
                        }
                    }
                    if (breakLoop) break;
                }
            }
            else
            {
                sr.sprite = item.Sprite;
            }

        }

        public override bool Occupy(Item item)
        {
            if (base.Occupy(item))
            {
                // Assign item
                pickupHook.Item = item;
                useHook.Item = item;

                // Instantiate item's prefab or Set sprite
                if (useHook.Item.Prefab != null)
                    InstantiateItemPrefab();
                else
                    ChangeSprite(item);

                // Extra
                pickup.AddPolygonCol();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [Serializable]
    public class StorageElementSpecific : StorageElement
    {
        public StorageElementSpecific(
            GameObject go,
            ItemUseHook useHook, 
            ItemPickupHook pickupHook, 
            ItemPickup pickup, 
            SpriteRenderer sr,
            bool isOccupied,
            bool makeItemPrefab) : base(go, useHook, pickupHook, pickup, sr,isOccupied)
        {
            this.makeItemPrefab = makeItemPrefab;
        }

        public StorageElementSpecific(StorageElement element) : base(element.go, element.useHook, element.pickupHook,element.pickup,element.sr, element.isOccupied)
        {
        }

        #region [Properties]

        [FoldoutGroup("Properties"), SerializeField, Tooltip("If item's prefab exists, instantiate it, and turn off spriteRenderer")]
        bool makeItemPrefab = true;
        public bool MakeItemPrefab { get { return makeItemPrefab; } }


        #endregion


        public override void ChangeSprite(Item item)
        {

        }

        public override bool Occupy(Item item)
        {
            if (base.Occupy(item))
            {
                // Instantiate item's prefab
                if (MakeItemPrefab && useHook.Item.Prefab != null)
                    InstantiateItemPrefab();

                return true;
            }
            else
            {
                return false;
            }
        }

    }
}