using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

namespace Encore.MiniGames.DIrtyKitchen
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]

    [AddComponentMenu("Encore/Interactables/MiniGame/Dirty Kitchen/Dirt")]
    public class Dirt : MonoBehaviour
    {
        [SerializeField] int maxHealth = 1000;
        [SerializeField] float cleanIfAlphaIs = 0f;

        int currentHealth;
        Tweener tweenShake;

        public SpriteRenderer sr { get; private set; }
        public List<Collider2D> cols { get; private set; }

        public Action OnClean;

        public void Setup(string cleanerGOName, bool enableColNow)
        {
            EnableCols(enableColNow);
        }

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            cols = new List<Collider2D>(GetComponents<Collider2D>());
            foreach (var col in cols)
            {
                col.isTrigger = true;
            }
            currentHealth = maxHealth;
            OnClean += () =>
            {
                foreach (var col in cols)
                {
                    col.enabled = false;
                }
            };
        }

        private void OnDestroy()
        {
            if (tweenShake != null) transform.DOKill();
        }

        public void Scrubbed(int cleanerPower)
        {
            if (sr.color.a > cleanIfAlphaIs)
            {
                currentHealth -= cleanerPower;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, (float)currentHealth / maxHealth);
                if (sr.color.a <= cleanIfAlphaIs) OnClean();
                if (tweenShake == null || !tweenShake.IsActive())
                {
                    float duration = cleanerPower * 0.0075f;
                    float strength = cleanerPower * 0.0075f;
                    tweenShake = transform.DOShakePosition(duration, strength);
                }
            }
        }

        public void EnableCols(bool enableCol)
        {
            foreach (var col in cols)
            {
                col.enabled = enableCol;
            }
        }

    }
}