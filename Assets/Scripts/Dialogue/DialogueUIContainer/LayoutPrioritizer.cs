using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Encore.Dialogues
{
    [RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Encore/Dialogues/Layout Prioritizer")]
    public class LayoutPrioritizer : UIBehaviour, ILayoutElement
    {
        public float minWidth
        {
            get
            {
                return GetLayoutGroup().minWidth;
            }
        }

        public float preferredWidth
        {
            get
            {
                return GetLayoutGroup().preferredWidth;
            }
        }

        public float flexibleWidth
        {
            get
            {
                return GetLayoutGroup().flexibleWidth;
            }
        }

        public float minHeight
        {
            get
            {
                return GetLayoutGroup().minHeight;
            }
        }

        public float preferredHeight
        {
            get
            {
                return GetLayoutGroup().preferredHeight;
            }
        }

        public float flexibleHeight
        {
            get
            {
                return GetLayoutGroup().preferredHeight;
            }
        }

        public int layoutPriority
        {
            get
            {
                return 100;
            }
        }

        ILayoutElement layoutGroup;

        public void CalculateLayoutInputHorizontal()
        {
        }

        public void CalculateLayoutInputVertical()
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
        }

        protected override void OnDisable()
        {
            LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
            base.OnDisable();
        }

        ILayoutElement GetLayoutGroup()
        {
            if (layoutGroup == null)
            {
                layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            }
            return layoutGroup;
        }
    }
}