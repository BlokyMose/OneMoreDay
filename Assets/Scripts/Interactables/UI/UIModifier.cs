using Encore.CharacterControllers;
using Encore.Dialogues;
using Encore.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Interactables
{
    [AddComponentMenu("Encore/Interactables/UI Modifier")]
    public class UIModifier : Interactable
    {
        public enum UIStatus { CanShow, CannotShow, LetBe }
        [SerializeField] UIStatus inventory = UIStatus.LetBe;
        [SerializeField] UIStatus doomclock = UIStatus.LetBe;
        [SerializeField] UIStatus phone = UIStatus.LetBe;
        [SerializeField] bool destroyAfterInteract = true;

        [Header("Optional:")]
        [SerializeField] MonologueHook monologueHook;

        protected override void Awake()
        {
            base.Awake();
            if (!monologueHook) monologueHook = GetComponent<MonologueHook>();
        }

        protected override CursorImageManager.CursorImage GetCursorImageModule()
        {
            return CursorImageManager.CursorImage.Grab;
        }

        protected override void InteractModule(GameObject interactor)
        {
            GameManager.Instance.InventoryManager.SetCanShow(inventory == UIStatus.CanShow ? true : inventory == UIStatus.CannotShow ? false : GameManager.Instance.InventoryManager.CanShow, true);
            GameManager.Instance.DoomclockManager.SetCanShow(doomclock == UIStatus.CanShow ? true : doomclock == UIStatus.CannotShow ? false : GameManager.Instance.DoomclockManager.CanShow, true);
            GameManager.Instance.PhoneManager.SetCanShow(phone == UIStatus.CanShow ? true : phone == UIStatus.CannotShow ? false : GameManager.Instance.PhoneManager.CanShow, true);

            if (destroyAfterInteract) StartCoroutine(DelayToDestroy(0.1f));
            IEnumerator DelayToDestroy(float delay)
            {
                yield return new WaitForSeconds(delay);
                Destroy(gameObject);
            }

            if (monologueHook) monologueHook.Speak(0);
        }
    }
}
