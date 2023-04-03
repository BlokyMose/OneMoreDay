using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Animations.Rigging;
using System;
using Encore.Inventory;

namespace Encore.CharacterControllers
{
    [AddComponentMenu("Encore/Character Controllers/Rig Controller Player")]
    public class RigControllerPlayer : RigController
    {
        #region [Vars: Properties]

        [Title("Phone")]
        [SerializeField] GameObject phonePrefab;
        [SerializeField] Vector3 localPosition = new Vector3(-0.15f, 0.11f, 0);
        [SerializeField] Vector3 localRotation = new Vector3(0, 0, -40f);

        #endregion

        #region [Vars: Data Handlers]

        const float FOLLOW_SPEED = 10f;
        const float CURSOR_POSITION_INFLUENCE = 1f / 50f;

        Coroutine corArmLeftFollowCursor;
        Coroutine corArmRightFollowCursor;

        #endregion

        #region [Methods: Phone]

        public void ActivatePhone(bool isActive)
        {
            if (isActive)
            {
                RemoveItemArmLeft(false);
                InstantiateGOInArmLeft(phonePrefab);
                itemGOArmLeft.transform.localPosition = localPosition;
                itemGOArmLeft.transform.localEulerAngles = localRotation;
            }
            else
            {
                RemoveItemArmLeft(true);
            }
        }

        public void ActivatePhoneFlashlight(bool isActive)
        {
            if (itemGOArmLeft != null)
                itemGOArmLeft.transform.Find("light_rear")?.gameObject.SetActive(isActive);
        }

        #endregion

        #region [Methods: Overrides Grip/Ungrip]

        public override void GripItem(Item item)
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
                    // Force ArmLeft to follow cursor despite having no item
                    if (item.FollowCursor) corArmLeftFollowCursor = StartCoroutine(ArmFollowCursor(true));
                    ChangeItemArmRight(item);
                    break;
                default:
                    break;
            }
        }

        #region [Methods: Change/Remove Item]

        public override void ChangeItemArmLeft(Item item)
        {
            base.ChangeItemArmLeft(item);
            if (corArmLeftFollowCursor != null) StopCoroutine(corArmLeftFollowCursor);
            if (item.FollowCursor) corArmLeftFollowCursor = StartCoroutine(ArmFollowCursor(true));
        }

        public override void ChangeItemArmRight(Item item)
        {
            base.ChangeItemArmRight(item);
            if (corArmRightFollowCursor != null) StopCoroutine(corArmRightFollowCursor);
            if (item.FollowCursor) corArmRightFollowCursor = StartCoroutine(ArmFollowCursor(false));
        }

        public override void RemoveItemArmLeft(bool waitForAnimation)
        {
            base.RemoveItemArmLeft(waitForAnimation);
            if (corArmLeftFollowCursor != null) StopCoroutine(corArmLeftFollowCursor);
            targetArmLeft.transform.localPosition = targetArmLeftInitPos;
        }

        public override void RemoveItemArmRight(bool waitForAnimation)
        {
            base.RemoveItemArmRight(waitForAnimation);
            if (corArmRightFollowCursor != null) StopCoroutine(corArmRightFollowCursor);
            targetArmRight.transform.localPosition = targetArmRightInitPos;
        }

        #endregion


        #endregion

        #region [Methods: Animate Arm]

        /// <param name="whichArm">true: Left; false;: Right</param>
        IEnumerator ArmFollowCursor(bool whichArm)
        {
            TwoBoneIKConstraint arm;
            Transform target;
            Action<bool> Flip;

            if (whichArm)
            {
                arm = armLeft;
                target = targetArmLeft;
                Flip = FlipArmLeftSprites;
            }
            else
            {
                arm = armRight;
                target = targetArmRight;
                Flip = FlipArmRightSprites;
            }

            while (true)
            {
                Vector2 shoulderPos = arm.data.root.transform.position;
                const float shoulderFollowLimitX = 1.5f;

                // Adjust cursor position according to shoulder position
                Vector2 cursorScreenPos = GameManager.Instance.Player.MouseManager.CursorImageManager.transform.localPosition;
                Vector2 shoulderScreenPos = Camera.main.WorldToScreenPoint(shoulderPos);
                Vector2 cursorOffset = shoulderScreenPos - new Vector2(Camera.main.scaledPixelWidth / 2, Camera.main.scaledPixelHeight / 2);
                cursorScreenPos -= cursorOffset;

                // Position target based on cursor position
                Vector2 newPos = new Vector2(
                    shoulderPos.x + cursorScreenPos.x * CURSOR_POSITION_INFLUENCE,
                    shoulderPos.y + cursorScreenPos.y * CURSOR_POSITION_INFLUENCE);
                target.position = Vector2.Lerp(target.position, newPos, Time.deltaTime * FOLLOW_SPEED);

                // Prevent following if cursor is behind the shoulder
                if (target.position.x < shoulderPos.x + shoulderFollowLimitX && transform.localEulerAngles.y == 0)
                    target.position = new Vector2(shoulderPos.x + shoulderFollowLimitX, target.position.y);
                else if (target.position.x > shoulderPos.x - shoulderFollowLimitX && transform.localEulerAngles.y == 180)
                    target.position = new Vector2(shoulderPos.x - shoulderFollowLimitX, target.position.y);

                // Rotate target based on cursor position
                float newRotationZ = Mathf.Atan2(cursorScreenPos.y, cursorScreenPos.x) * Mathf.Rad2Deg;

                if (transform.localEulerAngles.y == 0)
                {
                    newRotationZ = newRotationZ < -45 ? -45 : newRotationZ > 90 ? 90 : newRotationZ;
                    Flip(false);
                }
                else
                {
                    newRotationZ = newRotationZ > 0 && newRotationZ < 90 ? 90
                        : newRotationZ < 0 && newRotationZ > -125 ? -125
                        : newRotationZ;
                    Flip(true);
                }
                target.eulerAngles = new Vector3(0, 0, newRotationZ);

                yield return null;
            }
        }

        #endregion
    }
}