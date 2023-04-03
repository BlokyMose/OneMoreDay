using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEditor;
using Encore.Inventory;

namespace Encore.Interactables
{
    /// <summary>
    /// 
    /// [GENERAL IDEA] <br></br>
    /// - Store items without specifying the item of each storageElement <br></br>
    /// - SpriteRenderer's sprite always be nulled after being pickedup <br></br>
    /// - Item's prefab is always prioritized over item's sprite when storing the item <br></br>
    /// 
    /// [FEATURES]<br></br>
    /// - Filter tags: prevent some items to be stored by checking its tags <br></br>
    /// 
    /// </summary>

[AddComponentMenu("Encore/Interactables/Item Storage Any")]
    public class ItemStorageAny : ItemStorage
    {
        #region [Vars: Properties]

        public enum StorageTagFilter { None, Include, Exclude, Both }
        public enum StorageTagFilterInclusion { HaveAll, HaveOne }

        #region [Tag Filter]

        [VerticalGroup("Properties"), SerializeField, Tooltip(
            "None: accept all item\n\n" +
            "Include: only accepts item which has certain tags\n\n" +
            "Exclude: accepts any item except item which has certain tags\n\n" +
            "Both: filter by Inlcude and Exclude tags")]
        StorageTagFilter tagFilter = StorageTagFilter.None; 
        public StorageTagFilter TagFilter { get => tagFilter; }


        #endregion

        #region [Tags Include]

        [HideIf("@"+nameof(tagFilter)+ "==StorageTagFilter.None||"+nameof(tagFilter)+ "==StorageTagFilter.Exclude")]
        [FoldoutGroup("Properties/Filter"), SerializeField, Tooltip(
            "HaveAll: item must have all tags in the tags list\n\n" +
            "HaveOne: item must have at least one of the tags in the list\n\n")]
        StorageTagFilterInclusion tagsIncludeMode = StorageTagFilterInclusion.HaveOne;

        [HideIf("@" + nameof(tagFilter) + "==StorageTagFilter.None||" + nameof(tagFilter) + "==StorageTagFilter.Exclude")]
        [FoldoutGroup("Properties/Filter"), SerializeField]
        List<ItemTag> tagsInclude = new List<ItemTag>();
        public List<ItemTag> TagsInclude { get => tagsInclude; }

        #endregion

        #region [Tags Exlcude]

        [HideIf("@" + nameof(tagFilter) + "==StorageTagFilter.None||" + nameof(tagFilter) + "==StorageTagFilter.Include")]
        [FoldoutGroup("Properties/Filter"), SerializeField, Tooltip(
            "HaveAll: item must have all tags in the tags list\n\n" +
            "HaveOne: item must have at least one of the tags in the list\n\n")]
        StorageTagFilterInclusion tagsExcludeMode = StorageTagFilterInclusion.HaveOne;

        [HideIf("@" + nameof(tagFilter) + "==StorageTagFilter.None||" + nameof(tagFilter) + "==StorageTagFilter.Include")]
        [FoldoutGroup("Properties/Filter"), SerializeField]
        List<ItemTag> tagsExclude = new List<ItemTag>();

        #endregion

        [VerticalGroup("Properties"), SerializeField, ListDrawerSettings(HideRemoveButton = true, HideAddButton = true)]
        List<StorageElementAny> storageElements = new List<StorageElementAny>();

        #endregion

        #region [Methods: Utilities]

        public override IList<StorageElement> GetElements()
        {
            var elements = new List<StorageElement>();
            foreach (var storageElement in storageElements)
                elements.Add(storageElement);
            return elements;
        }

        public override void AddElement(StorageElement element)
        {
            storageElements.Add(new StorageElementAny(element, this));
        }

        protected override StorageElement GetAvailableElement(Item clickedItem)
        {
            if (clickedItem == null) return null;
            if (!FilterUsingTag(clickedItem)) return null;

            foreach (var element in storageElements)
            {
                // Only checks an element that's not active
                if (!element.pickup.IsActive)
                    return element;
            }

            return null;
        }

        /// <summary>Checks whether item has or doesn't have certain tags</summary>
        /// <returns>Passed the tag filters</returns>
        protected bool FilterUsingTag(Item item)
        {
            switch (tagFilter)
            {
                case StorageTagFilter.None: return true;
                case StorageTagFilter.Include: return CheckTags(item, tagsInclude, tagsIncludeMode);
                case StorageTagFilter.Exclude: return !CheckTags(item, tagsExclude, tagsExcludeMode);
                case StorageTagFilter.Both: return (CheckTags(item, tagsInclude, tagsIncludeMode) && !CheckTags(item, tagsExclude, tagsExcludeMode));
            }
            return true;

            bool CheckTags(Item item, List<ItemTag> filterTags, StorageTagFilterInclusion mode)
            {
                switch (mode)
                {
                    case StorageTagFilterInclusion.HaveAll:
                        foreach (var mustHaveTag in filterTags)
                            if (!item.Tags.Contains(mustHaveTag)) return false;
                        return true;
                    case StorageTagFilterInclusion.HaveOne:
                        foreach (var tag in filterTags)
                            if (item.Tags.Contains(tag)) return true;
                        return false;
                    default:
                        return true;
                }
            }
        }

        #endregion


    }
}