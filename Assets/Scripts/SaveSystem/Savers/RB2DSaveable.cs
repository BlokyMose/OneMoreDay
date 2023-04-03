using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Saves
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(GOSaver))]
    [AddComponentMenu("Encore/Saves/RB2D Saveable")]
    public class RB2DSaveable : MonoBehaviour, ISaveable
    {
        Rigidbody2D rb;
        GOSaver saver;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            saver = GetComponent<GOSaver>();
        }

        public void Load(GameManager.GameAssets gameAssets)
        {
            var data = saver.GetProperty(GetType().ToString()) as Dictionary<string, object>;
            if (data == null) return;

            // Apply properties
            rb.simulated = (bool)data[nameof(rb.simulated)];
        }

        public void Save()
        {
            saver.AddProperty(GetType().ToString(), new Dictionary<string, object>()
        {
            {nameof(rb.simulated), rb.simulated }
        });
        }
    }
}