using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Interactables;
using Encore.Saves;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/GO Activator")]
    public class GOActivator : Interactable
    {
        #region [Classes]

        [System.Serializable]
        public class GOActivatedItem
        {
            public enum GOActivationMode { AsIs, Active, Inactive }

            [LabelWidth(1)]
            public GameObject go;
            [LabelText("Highlight SR")]
            public SpriteRenderer highlightSpriteRenderer;
            [ShowIf("@go!=null"), HorizontalGroup, LabelText("Activation Before"), OnValueChanged(nameof(_OnActivationBeforeChanged)), GUIColor("@" + nameof(_ActivationColor) + "(" + nameof(activationBeforeInteraction) + ")")]
            public GOActivationMode activationBeforeInteraction = GOActivationMode.AsIs;
            [ShowIf("@go!=null"), HorizontalGroup, LabelText("After"), LabelWidth(40), GUIColor("@"+nameof(_ActivationColor)+"("+nameof(activationAfterInteraction)+")")]
            public GOActivationMode activationAfterInteraction = GOActivationMode.AsIs;

            public GOActivatedItem(GameObject go, SpriteRenderer highlightSpriteRenderer, GOActivationMode activationBeforeInteraction, GOActivationMode activationAfterInteraction)
            {
                this.go = go;
                this.highlightSpriteRenderer = highlightSpriteRenderer;
                this.activationBeforeInteraction = activationBeforeInteraction;
                this.activationAfterInteraction = activationAfterInteraction;
            }

            #region [Methods: Inspector]

            void _OnActivationBeforeChanged()
            {
                switch (activationBeforeInteraction)
                {
                    case GOActivationMode.AsIs:
                        break;
                    case GOActivationMode.Active:go.SetActive(true);
                        break;
                    case GOActivationMode.Inactive:go.SetActive(false);
                        break;
                }
            }

            Color _ActivationColor(GOActivationMode mode)
            {
                switch (mode)
                {
                    case GOActivationMode.AsIs: return new Color(0.75f, 0.75f, 0.75f);
                    case GOActivationMode.Active: return new Color(0.5f, 1f, 0.5f);
                    case GOActivationMode.Inactive: return new Color(1f, 0.5f, 0.5f);
                    default: return new Color(0.75f, 0.75f, 0.75f);
                }
            }

            #endregion
        }
        public enum ActivationMode { BeforeAfter, Toggle }

        #endregion

        #region [Vars: Properties]

        [Title("GO Activator", "All are optional")]

        [PropertyTooltip(   "BeforeAfter: interaction changes activation using Before to After Interaction bool\n" +
                            "Toggle: interaction changes activation by go's current activation state")]
        [SerializeField]
        ActivationMode activationMode = ActivationMode.BeforeAfter;

        [ListDrawerSettings(DraggableItems = false)]
        public List<GOActivatedItem> goActivatedItems = new List<GOActivatedItem>();

        #endregion

        #region [Vars: Data Handlers]

        bool isInteracted = false;

        #endregion

        #region [Methods: Inspector]

        [Button("Add GOSaver To Targets"), GUIColor("@Color.green")]
        void _AddGOSaverToTargets()
        {
            foreach (var go in goActivatedItems)
            {
                if (go.go.GetComponent<GOSaver>() == null)
                {
                    go.go.AddComponent<GOSaver>();
                }
            }
        }

        #endregion

        protected override void InteractModule(GameObject interactor)
        {
            if(activationMode == ActivationMode.BeforeAfter)
            {
                isInteracted = true;
                foreach (var item in goActivatedItems)
                {
                    if (item.go == null) continue;
                    if (item.activationAfterInteraction == GOActivatedItem.GOActivationMode.AsIs) continue;
                    item.go.SetActive(item.activationAfterInteraction == GOActivatedItem.GOActivationMode.Active ? true : false);
                }
            }
            else if(activationMode == ActivationMode.Toggle)
            {
                isInteracted = !isInteracted;
                foreach (var item in goActivatedItems)
                {
                    if (item.go == null) continue;
                    item.go.SetActive(!item.go.activeSelf);
                }
            }
        }
    }
}
