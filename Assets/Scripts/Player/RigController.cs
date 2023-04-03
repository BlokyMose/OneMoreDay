using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Animations.Rigging;
using System;
using Encore.Inventory;

namespace Encore.CharacterControllers
{
    [AddComponentMenu("Encore/Character Controllers/Rig Controller")]
    public class RigController : MonoBehaviour
    {
        #region [Vars: Components]

        [Title("Rigs")]
        [SerializeField] protected TwoBoneIKConstraint armLeft;
        [SerializeField] protected TwoBoneIKConstraint armRight;

        [Title("Targets")]
        public Transform TargetArmleft { get { return targetArmLeft; } }
        [SerializeField] protected Transform targetArmLeft;
        public Transform TargetArmRight { get { return targetArmRight; } }
        [SerializeField] protected Transform targetArmRight;

        [Title("Sprites")]
        [SerializeField] protected SpriteRenderer handLeft;
        [SerializeField] protected SpriteRenderer handRight;
        [SerializeField] protected SpriteRenderer itemHandLeft;
        [SerializeField] protected SpriteRenderer itemHandRight;

        #endregion

        #region [Vars: Properties]

        [Title("Properties")]
        [SerializeField] protected float animationSpeed = 3.5f;

        #endregion

        #region [Vars: Data Handlers]

        public bool IsArmLeftRaised { get; private set; }
        public bool IsArmRightRaised { get; private set; }

        protected GameObject itemGOArmLeft;
        protected GameObject itemGOArmRight;
        protected Coroutine corChangeWeightArmLeft;
        protected Coroutine corChangeWeightArmRight;
        protected Item grippedItem;

        protected Vector2 targetArmLeftInitPos;
        protected Vector2 targetArmRightInitPos;
        protected Vector3 targetArmLeftInitRot;
        protected Vector3 targetArmRightInitRot;

        protected Vector2 itemHandLeftInitPos;
        protected Vector2 itemHandRightInitPos;
        protected Vector3 itemHandLeftInitRot;
        protected Vector3 itemHandRightInitRot;

        #endregion

        #region [Methods: Unity]

        protected virtual void Awake()
        {
            targetArmLeftInitPos = targetArmLeft.transform.localPosition;
            targetArmRightInitPos = targetArmRight.transform.localPosition;

            targetArmLeft.eulerAngles = Vector3.zero;
            targetArmLeftInitRot = targetArmLeft.localEulerAngles;

            targetArmRight.eulerAngles = Vector3.zero;
            targetArmRightInitRot = targetArmRight.localEulerAngles;

            itemHandLeftInitPos = itemHandLeft.transform.localPosition;
            itemHandLeftInitRot = itemHandLeft.transform.localEulerAngles;
            itemHandRightInitPos = itemHandRight.transform.localPosition;
            itemHandRightInitRot = itemHandRight.transform.localEulerAngles;
        }

        protected virtual void Start()
        {
            armLeft.weight = 0;
            armRight.weight = 0;
        }

        #endregion

        #region [Methods: Grip/Ungrip Item]

        /// <summary> Change the gripped item on either arms depending on the item's grip mode </summary>
        public virtual void GripItem(Item item)
        {
            grippedItem = item;
            UngripAllItems(false);
            switch (item.GripMode)
            {
                case Item.HandGripMode.Left:
                    ChangeItemArmLeft(item);
                    break;
                case Item.HandGripMode.Right:
                    ChangeItemArmRight(item);
                    break;
                case Item.HandGripMode.Both:
                    ChangeItemSpriteArmLeft(null);
                    ChangeItemArmRight(item);
                    break;
                default:
                    break;
            }
        }

        /// <summary>  Remove all item on both arms </summary>
        /// <param name="waitForAnimation">Wait for the arm to be lowered before removing the item</param>
        public virtual void UngripAllItems(bool waitForAnimation)
        {
            grippedItem = null;
            RemoveItemArmLeft(waitForAnimation);
            RemoveItemArmRight(waitForAnimation);
        }

        #region [Methods: Change/Remove Item]

        /// <summary> Instantiate item's prefab if exists; else change gripped item according to item's sprite </summary>
        public virtual void ChangeItemArmLeft(Item item)
        {
            if (item.Prefab != null) InstantiateGOInArmLeft(item.Prefab);
            else ChangeItemSpriteArmLeft(item.Sprite);
            itemHandLeft.transform.localPosition = itemHandLeftInitPos + item.OffsetPosition;
            itemHandLeft.transform.localEulerAngles = itemHandLeftInitRot + item.OffsetRotation;
            itemHandLeft.transform.localScale = item.ScaleMultiplier;
            IsArmLeftRaised = true;
        }

        /// <summary> Instantiate item's prefab if exists; else change gripped item according to item's sprite </summary>
        public virtual void ChangeItemArmRight(Item item)
        {
            if (item.Prefab != null) InstantiateGOInArmRight(item.Prefab);
            else ChangeItemSpriteArmRight(item.Sprite);
            itemHandRight.transform.localPosition = itemHandRightInitPos + item.OffsetPosition;
            itemHandRight.transform.localEulerAngles = itemHandRightInitRot + item.OffsetRotation;
            itemHandRight.transform.localScale = item.ScaleMultiplier;
            IsArmRightRaised = true;
        }

        /// <summary> Destroy instantiated prefab if exists; else nullify gripped item's sprite </summary>
        /// <param name="waitForAnimation"> Wait for hand to be lowered before removing item </param>
        public virtual void RemoveItemArmLeft(bool waitForAnimation)
        {
            if (corChangeWeightArmLeft != null) StopCoroutine(corChangeWeightArmLeft);
            corChangeWeightArmLeft = StartCoroutine(ResetArmLeft(waitForAnimation));
            IsArmLeftRaised = false;
        }

        /// <summary> Destroy instantiated prefab if exists; else nullify gripped item's sprite </summary>
        /// <param name="waitForAnimation"> Wait for hand to be lowered before removing item </param>
        public virtual void RemoveItemArmRight(bool waitForAnimation)
        {
            if (corChangeWeightArmRight != null) StopCoroutine(corChangeWeightArmRight);
            corChangeWeightArmRight = StartCoroutine(ResetArmRight(waitForAnimation));
            IsArmRightRaised = false;
        }

        #endregion

        #region [Methods: Change item's Sprite || Instantiate item's GO]

        protected void ChangeItemSpriteArmLeft(Sprite sprite)
        {
            itemHandLeft.sprite = sprite;
            if (corChangeWeightArmLeft != null) StopCoroutine(corChangeWeightArmLeft);
            corChangeWeightArmLeft = StartCoroutine(RaiseIKWeight(armLeft, 1));
        }

        protected void ChangeItemSpriteArmRight(Sprite sprite)
        {
            itemHandRight.sprite = sprite;
            if (corChangeWeightArmRight != null) StopCoroutine(corChangeWeightArmRight);
            corChangeWeightArmRight = StartCoroutine(RaiseIKWeight(armRight, 1));
        }

        protected void InstantiateGOInArmLeft(GameObject go)
        {
            itemGOArmLeft = Instantiate(go);
            itemGOArmLeft.transform.parent = itemHandLeft.transform;
            itemGOArmLeft.transform.localPosition = Vector2.zero;
            Rigidbody2D rb = itemGOArmLeft.GetComponent<Rigidbody2D>();
            if (rb != null) Destroy(rb);
            if (corChangeWeightArmLeft != null) StopCoroutine(corChangeWeightArmLeft);
            corChangeWeightArmLeft = StartCoroutine(RaiseIKWeight(armLeft, 1));
        }

        protected void InstantiateGOInArmRight(GameObject go)
        {
            itemGOArmRight = Instantiate(go);
            itemGOArmRight.transform.parent = itemHandRight.transform;
            itemGOArmRight.transform.localPosition = Vector2.zero;
            Rigidbody2D rb = itemGOArmRight.GetComponent<Rigidbody2D>();
            if (rb != null) Destroy(rb);
            if (corChangeWeightArmRight != null) StopCoroutine(corChangeWeightArmRight);
            corChangeWeightArmRight = StartCoroutine(RaiseIKWeight(armRight, 1));
        }

        #endregion

        #endregion

        #region [Methods: Animate Arm]

        protected IEnumerator RaiseIKWeight(TwoBoneIKConstraint ik, float weight)
        {
            while (ik.weight < weight)
            {
                ik.weight += Time.deltaTime * animationSpeed;
                yield return null;
            }
        }

        protected IEnumerator ResetArmLeft(bool waitForAnimation, bool destroyItemGO = true)
        {
            if (waitForAnimation)
                while (armLeft.weight > 0)
                {
                    armLeft.weight -= Time.deltaTime * animationSpeed;
                    yield return null;
                }
            else
            {
                armLeft.weight = 0;
            }
            if (itemGOArmLeft != null) { if (destroyItemGO) Destroy(itemGOArmLeft); else itemGOArmLeft.SetActive(false); }
            itemGOArmLeft = null;
            itemHandLeft.sprite = null;
            FlipArmLeftSprites(false);
            targetArmLeft.localPosition = targetArmLeftInitPos;
            targetArmLeft.localEulerAngles = targetArmLeftInitRot;
            targetArmLeft.localScale = Vector3.one;
            itemHandLeft.transform.localPosition = itemHandLeftInitPos;
            itemHandLeft.transform.localEulerAngles = itemHandLeftInitRot;
            itemHandLeft.transform.localScale = Vector3.one;

        }

        protected IEnumerator ResetArmRight(bool waitForAnimation, bool destroyItemGO = true)
        {
            if (waitForAnimation)
                while (armRight.weight > 0)
                {
                    armRight.weight -= Time.deltaTime * animationSpeed;
                    yield return null;
                }
            else
            {
                armRight.weight = 0;
            }
            if (itemGOArmRight != null) { if (destroyItemGO) Destroy(itemGOArmRight); else itemGOArmRight.SetActive(false); }
            itemGOArmRight = null;
            itemHandRight.sprite = null;
            FlipArmRightSprites(false);
            targetArmRight.localPosition = targetArmRightInitPos;
            targetArmRight.localEulerAngles = targetArmRightInitRot;
            targetArmRight.localScale = Vector3.one;
            itemHandRight.transform.localPosition = itemHandRightInitPos;
            itemHandRight.transform.localEulerAngles = itemHandRightInitRot;
            itemHandRight.transform.localScale = Vector3.one;
        }

        protected void FlipArmLeftSprites(bool isFlipped)
        {
            // Prevent reassigning same value and flip localPosition.y accordingly
            if (isFlipped == handLeft.flipX) return;

            handLeft.flipX = isFlipped;
            handLeft.flipY = isFlipped;
            itemHandLeft.flipY = isFlipped;
            itemHandLeft.transform.localPosition = new Vector2(
                itemHandLeft.transform.localPosition.x,
                itemHandLeft.transform.localPosition.y * -1);
        }

        protected void FlipArmRightSprites(bool isFlipped)
        {
            // Prevent reassigning same value and flip localPosition.y accordingly
            if (isFlipped == handRight.flipX) return;

            handRight.flipX = isFlipped;
            handRight.flipY = isFlipped;
            itemHandRight.flipY = isFlipped;
            itemHandRight.transform.localPosition = new Vector2(
                itemHandRight.transform.localPosition.x,
                itemHandRight.transform.localPosition.y * -1);
        }

        #endregion
    }
}