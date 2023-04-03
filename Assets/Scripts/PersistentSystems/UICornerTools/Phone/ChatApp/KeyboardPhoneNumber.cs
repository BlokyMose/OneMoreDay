using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using System;

namespace Encore.Phone
{
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Encore/Phone/Keyboard Phone Number")]
    public class KeyboardPhoneNumber : MonoBehaviour
    {
        [FoldoutGroup("Buttons")] [SerializeField] Button but0;
        [FoldoutGroup("Buttons")] [SerializeField] Button but1;
        [FoldoutGroup("Buttons")] [SerializeField] Button but2;
        [FoldoutGroup("Buttons")] [SerializeField] Button but3;
        [FoldoutGroup("Buttons")] [SerializeField] Button but4;
        [FoldoutGroup("Buttons")] [SerializeField] Button but5;
        [FoldoutGroup("Buttons")] [SerializeField] Button but6;
        [FoldoutGroup("Buttons")] [SerializeField] Button but7;
        [FoldoutGroup("Buttons")] [SerializeField] Button but8;
        [FoldoutGroup("Buttons")] [SerializeField] Button but9;
        [FoldoutGroup("Buttons")] [SerializeField] Button butBackSpace;
        [FoldoutGroup("Buttons")] [SerializeField] Button butSend;

        [SerializeField] TextMeshProUGUI targetText;
        [SerializeField] AudioClip clipTap;
        [SerializeField] AudioClip clipSend;

        AudioSource audioSource;

        int currentDigit = 0;

        public Action<string> OnSend;

        private void Awake()
        {
            targetText.text = "";
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            but0.onClick.AddListener(() => { AddCharacter("0"); });
            but1.onClick.AddListener(() => { AddCharacter("1"); });
            but2.onClick.AddListener(() => { AddCharacter("2"); });
            but3.onClick.AddListener(() => { AddCharacter("3"); });
            but4.onClick.AddListener(() => { AddCharacter("4"); });
            but5.onClick.AddListener(() => { AddCharacter("5"); });
            but6.onClick.AddListener(() => { AddCharacter("6"); });
            but7.onClick.AddListener(() => { AddCharacter("7"); });
            but8.onClick.AddListener(() => { AddCharacter("8"); });
            but9.onClick.AddListener(() => { AddCharacter("9"); });
            butBackSpace.onClick.AddListener(() => { RemoveCharacter(); });
            butSend.onClick.AddListener(() => { Send(); });
        }

        private void OnDisable()
        {
            but0.onClick.RemoveAllListeners();
            but1.onClick.RemoveAllListeners();
            but2.onClick.RemoveAllListeners();
            but3.onClick.RemoveAllListeners();
            but4.onClick.RemoveAllListeners();
            but5.onClick.RemoveAllListeners();
            but6.onClick.RemoveAllListeners();
            but7.onClick.RemoveAllListeners();
            but8.onClick.RemoveAllListeners();
            but9.onClick.RemoveAllListeners();
            butBackSpace.onClick.RemoveAllListeners();
            butSend.onClick.RemoveAllListeners();

        }

        void AddCharacter(string character)
        {
            audioSource.clip = clipTap;
            audioSource.Play();
            if (currentDigit > 8) return;

            if (currentDigit % 3 == 0 && currentDigit < 9 && currentDigit != 0)
            {
                targetText.text += "-";
            }
            targetText.text += character;
            currentDigit++;
        }

        void RemoveCharacter()
        {
            audioSource.clip = clipTap;
            audioSource.Play();
            if (currentDigit < 1) return;

            currentDigit--;
            if (currentDigit % 3 == 0 && currentDigit > 0 && currentDigit != 9)
            {
                targetText.text = targetText.text.Remove(targetText.text.Length - 1);
            }
            targetText.text = targetText.text.Remove(targetText.text.Length - 1);
        }

        void Send()
        {
            audioSource.clip = clipSend;
            audioSource.Play();
            OnSend(targetText.text);
            Clear();
        }

        public void Clear()
        {
            targetText.text = "";
            currentDigit = 0;
        }
    }
}