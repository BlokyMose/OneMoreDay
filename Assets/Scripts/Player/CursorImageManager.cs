using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Encore.Utility;
using TMPro;

namespace Encore.CharacterControllers
{
    [AddComponentMenu("Encore/Character Controllers/Cursor Image Manager")]
    public class CursorImageManager : MonoBehaviour
    {
        #region [Classes]

        [System.Serializable]
        public class CursorSprites
        {
            [PreviewField]
            public Sprite curClick;
            [PreviewField]
            public Sprite curNormal;
            [PreviewField]
            public Sprite curFar;
            [PreviewField]
            public Sprite curLook;
            [PreviewField]
            public Sprite curSpin;
            [PreviewField]
            public Sprite curExit;
            [PreviewField]
            public Sprite curGrab;
            [PreviewField]
            public Sprite curMonologue;
            [PreviewField]
            public Sprite curDialogue;
            [PreviewField]
            public Sprite curDialogueNext;
            [PreviewField]
            public Sprite curMultiMonologue;
            [PreviewField]
            public Sprite curDisabled;
            [PreviewField]
            public Sprite curChoose;            
            [PreviewField]
            public Sprite curInvisible;
        }

        public enum CursorImage { Normal, Click, Far, Look, Spin, Exit, Grab, Monologue, Dialogue, DialogueNext, MultiMonologue, Disabled, Choose, Invisible }
        enum CursorState { Normal, Speaking, Far }

        #endregion

        #region [Components]

        [SerializeField]
        Image itemSprite;

        [Title("Tooltip")]
        [SerializeField]
        Transform tooltipContainer;

        [SerializeField]
        Animator tooltipAnimator;

        [SerializeField]
        HorizontalLayoutGroup tooltipLayoutGroup;

        [SerializeField]
        TextMeshProUGUI tooltipText;

        CursorSprites cursorSprites;
        Image image;
        RectTransform rectTransform;
        MouseManager mouseManager;

        #endregion

        #region [Vars: Data Handlers]

        // Passed properties
        Vector2 offset = new Vector2(1f, -1f);
        Vector2 _itemSpriteScale = new Vector2(1, 1);
        public Vector2 ItemSpriteScale { get { return _itemSpriteScale; } set { _itemSpriteScale = value; } }

        CursorState cursorState = CursorState.Normal;

        Coroutine corFollowCursor;
        bool isFollowingMouse = false;

        int boo_show;

        float tooltipLocalPositionYNormal = -45;

        CursorImage previousCursorImage = CursorImage.Normal;

        #endregion

        private void Awake()
        {
            Cursor.visible = false;
            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            itemSprite.enabled = false;
            boo_show = Animator.StringToHash(nameof(boo_show));
        }

        public void Setup(CharacterBrain brain, MouseManager mouseManager, CursorSprites cursorSprites, Vector2 scale, Vector2 offset, Color color, Vector2 itemSpriteScale)
        {
            this.mouseManager = mouseManager;
            this.cursorSprites = cursorSprites;
            brain.OnClick += Click;
            image.sprite = cursorSprites.curNormal;
            transform.localScale = scale;
            this.offset = offset;
            image.color = color;
            ItemSpriteScale = itemSpriteScale;
            StartFollowingMouse();
        }

        public void Setdown(CharacterBrain brain)
        {
            brain.OnClick -= Click;
        }


        #region [Methods: Cursor Position]

        public void StartFollowingMouse()
        {
            corFollowCursor = this.RestartCoroutine(FollowMouse());
            IEnumerator FollowMouse()
            {
                isFollowingMouse = true;
                while (true)
                {
                    var newPos = mouseManager.MouseScreenPos + offset;
                    SetCursorPosition(newPos);
                    SetTooltipLocalTransform();

                    yield return null;
                }
            }
        }

        void SetTooltipLocalTransform()
        {
            int flipMultiplier = DoFlipYTooltip() ? -1 : 1;
            tooltipContainer.localScale = new Vector2(1, flipMultiplier * 1);
            tooltipText.transform.localScale = new Vector2(1, flipMultiplier * 1);
            tooltipContainer.localPosition = new Vector2(CalculateTooltipPositionX(), flipMultiplier * tooltipLocalPositionYNormal);

            float CalculateTooltipPositionX()
            {
                // Tooltip is beyond the screen's right bound
                if (mouseManager.MouseScreenPos.x + tooltipText.rectTransform.sizeDelta.x > Screen.width)
                {
                    return -(mouseManager.MouseScreenPos.x + tooltipText.rectTransform.sizeDelta.x - Screen.width);
                }
                // Tooltip is beyond the screen's left bound
                else if (mouseManager.MouseScreenPos.x - tooltipText.rectTransform.sizeDelta.x / 2 < 0)
                {
                    return -(mouseManager.MouseScreenPos.x - tooltipText.rectTransform.sizeDelta.x / 2);
                }
                else
                {
                    return 0;
                }

            }

            bool DoFlipYTooltip()
            {
                if (mouseManager.MouseScreenPos.y - tooltipText.rectTransform.sizeDelta.y < 100)
                    return true;
                else
                    return false;
            }
        }

        public void StopFollowingMouse()
        {
            this.StopCoroutineIfExists(corFollowCursor);
            isFollowingMouse = false;
        }

        public void SetCursorPosition(Vector2 pos)
        {
            transform.position = new Vector3(pos.x, pos.y, 0);
            itemSprite.transform.position = new Vector3(transform.position.x - rectTransform.rect.width / 2, transform.position.y + rectTransform.rect.height / 2);
        }

        #endregion

        #region [Methods: Cursor Image]

        void Click(bool isClicking)
        {
            // Don't change sprite when far
            if (cursorState == CursorState.Far || cursorState == CursorState.Speaking) return;

            if (isClicking)
                SetSprite(CursorImage.Click);
            else
                SetSprite(CursorImage.Normal);
        }

        public void SetSprite(CursorImage cursorImage)
        {
            if (previousCursorImage == cursorImage) return;
            previousCursorImage = cursorImage;

            switch (cursorImage)
            {
                case CursorImage.Normal:
                    image.sprite = cursorSprites.curNormal;
                    cursorState = CursorState.Normal;
                    break;
                                   
                case CursorImage.Click:
                    image.sprite = cursorSprites.curClick;
                    break;

                case CursorImage.Far:
                    image.sprite = cursorSprites.curFar;
                    cursorState = CursorState.Far;
                    break;

                case CursorImage.Look:
                    image.sprite = cursorSprites.curLook;
                    break;

                case CursorImage.Spin:
                    image.sprite = cursorSprites.curSpin;
                    break;

                case CursorImage.Exit:
                    image.sprite = cursorSprites.curExit;
                    break;

                case CursorImage.Grab:
                    image.sprite = cursorSprites.curGrab;
                    break;

                case CursorImage.Monologue:
                    image.sprite = cursorSprites.curMonologue;
                    cursorState = CursorState.Speaking;
                    break;

                case CursorImage.Dialogue:
                    image.sprite = cursorSprites.curDialogue;
                    cursorState = CursorState.Speaking;
                    break;

                case CursorImage.DialogueNext:
                    image.sprite = cursorSprites.curDialogueNext;
                    cursorState = CursorState.Speaking;
                    break;

                case CursorImage.MultiMonologue:
                    image.sprite = cursorSprites.curMultiMonologue;
                    cursorState = CursorState.Speaking;
                    break;

                case CursorImage.Disabled:
                    image.sprite = cursorSprites.curDisabled;
                    break;

                case CursorImage.Choose:
                    image.sprite = cursorSprites.curChoose;
                    break;

                case CursorImage.Invisible:
                    image.sprite = cursorSprites.curInvisible;
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region [Methods: Item Sprite]

        public void SetItemSprite(Sprite sprite)
        {
            itemSprite.enabled = true;
            itemSprite.sprite = sprite;
            HighlightItemSprite(false);
        }

        public void HighlightItemSprite(bool isHighlighting)
        {
            itemSprite.color = new Color(1, 1, 1, isHighlighting ? 0.75f : 0.25f);
        }

        public void RemoveItemSprite()
        {
            itemSprite.enabled = false;
        }

        #endregion

        #region [Methods: Tooltip]

        public void ShowTooltip(string text)
        {
            tooltipText.text = text;
            UpdateTooltipLayoutGroup();
            tooltipAnimator.SetBool(boo_show, true);
        }

        public void HideTooltip()
        {
            tooltipAnimator.SetBool(boo_show, false);
        }

        void UpdateTooltipLayoutGroup()
        {
            tooltipLayoutGroup.enabled = false;
            tooltipLayoutGroup.enabled = true;
            Canvas.ForceUpdateCanvases();
        }

        #endregion

    }
}