using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace Encore.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("Encore/Inventory/UI Elements/Item Holder")]
    public class ItemHolder : MonoBehaviour
    {
        #region [Classes]

        [System.Serializable]
        public class ActionButton
        {
            public ActionExecutioner.ActionExecution actionReference;
            public Button button;
            public GameObject actionPrefab;
            public GameObject instantiatedActionGO;
            public ActionExecutioner instantiatedActionExecutioner;

            public ActionButton(ActionExecutioner.ActionExecution actionReference, Button button, GameObject actionPrefab)
            {
                this.actionReference = actionReference;
                this.button = button;
                this.actionPrefab = actionPrefab;

                button.onClick.AddListener(() =>
                {
                    // For initial click, or when player moves to different scene, causing the GO to be destroyed
                    if (instantiatedActionGO == null)
                    {
                        instantiatedActionGO = Instantiate(actionPrefab);
                        instantiatedActionExecutioner = instantiatedActionGO.GetComponent<ActionExecutioner>();
                        instantiatedActionExecutioner.Execute(actionReference.actionName);
                    }

                    // InstantiatedGO should deactivate itself after being used if destroying itself is not possible
                    else
                    {
                        instantiatedActionGO.SetActive(true);
                        instantiatedActionExecutioner.Execute(actionReference.actionName);
                    }
                });
            }
        }

        #endregion

        #region [Vars: Components]

        Animator animator;
        TextMeshProUGUI text;
        Transform actionsParent;

        #endregion

        #region [Vars: Properties]

        [SerializeField]
        GameObject itemHolderActionButPrefab;

        #endregion

        #region [Vars: Data Handlers]

        Item item;
        public Item Item { get { return item; } set { item = value; } }

        List<ActionButton> actionButtons = new List<ActionButton>();
        public List<ActionButton> ActionButtons { get { return actionButtons; } }

        int boo_showText;

        #endregion

        private void Awake()
        {
            animator = GetComponent<Animator>();
            boo_showText = Animator.StringToHash(nameof(boo_showText));
            text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            actionsParent = transform.Find("Actions").GetComponent<Transform>();
        }

        private void Start()
        {
            StartCoroutine(RefreshLayout());
        }

        IEnumerator RefreshLayout()
        {
            yield return new WaitForFixedUpdate();
            PreviewImage();
            ScaleWidthToHeight();
        }


        //[OnInspectorInit]
        private void PreviewImage()
        {
            GetComponent<Image>().sprite = item.Sprite;
        }

        //[OnInspectorInit]
        private void ScaleWidthToHeight()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            float height = GetComponent<RectTransform>().rect.height;
            GetComponent<RectTransform>().sizeDelta = new Vector2(height, height);
        }

        public void ShowText(bool isShowing)
        {
            animator.SetBool(boo_showText, isShowing);
        }

        public void SetText(string text)
        {
            this.text.text = text;
        }

        public void AddAction(ActionExecutioner.ActionExecution actionReference, GameObject actionPrefab)
        {
            var actionButGO = Instantiate(itemHolderActionButPrefab, actionsParent);
            var actionBut = actionButGO.GetComponent<Button>();
            actionBut.image.sprite = actionReference.icon;

            actionButtons.Add(new ActionButton(actionReference, actionBut, actionPrefab));
        }

        public void DestroyActionButtons()
        {
            foreach (var actionBut in actionButtons)
            {
                if (actionBut.instantiatedActionGO != null)
                    Destroy(actionBut.instantiatedActionGO);
                Destroy(actionBut.button.gameObject);
            }
        }

    }
}