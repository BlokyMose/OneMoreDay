using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace Encore.Desktop
{
    [AddComponentMenu("Encore/Desktop/Lockscreen Button")]
    public class LockscreenButton : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Transform passwordBox;

        private void Start()
        {
            inputField.onEndEdit.AddListener(EnterKeyPressed);
            this.GetComponent<Button>().onClick.AddListener(Submit);
        }

        public void EnterKeyPressed(string s)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                Submit();
            }
        }

        public void Submit()
        {
            bool result = this.GetComponentInParent<DesktopManager>().UnlockScreen(inputField.text);

            if (!result)
            {
                inputField.text = "";
                passwordBox.DOShakePosition(0.5f, 8f, 20, 0);
            }
        }
    }
}