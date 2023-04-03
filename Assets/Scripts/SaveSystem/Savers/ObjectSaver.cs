using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Saves
{
    [AddComponentMenu("Encore/Saves/Object Saver")]
    public abstract class ObjectSaver : MonoBehaviour
    {
        #region [Classes]

        [Serializable]
        public class ExistingSaveable
        {
            [HorizontalGroup("H"), OnValueChanged(nameof(_OnComponentValueChanged)), HideLabel]
            public Component component;
            [HorizontalGroup("H"), HideLabel]
            public bool canSaveLoad;

            public ISaveable saveable { get { return component as ISaveable; } }

            public ExistingSaveable(ISaveable iSaveable, bool canSaveLoad)
            {
                component = iSaveable as Component;
                this.canSaveLoad = canSaveLoad;
            }

            void _OnComponentValueChanged()
            {
                if (!(component is ISaveable))
                {
                    component = null;
                }
            }
        }

        #endregion

        #region [Vars: Properties]

        [SerializeField]
        protected string saveKey;
        public string SaveKey { get { return saveKey; } }

        [Title("Saveables")]
        [SerializeField]
        bool autoDetectSaveables = true;

        [SerializeField, ListDrawerSettings(DraggableItems = false, Expanded = true), LabelText(" "), GUIColor(0.5f, 0.85f, 0.85f)]
        protected List<ExistingSaveable> saveables = new List<ExistingSaveable>();

        #endregion

        #region [Vars: Data Handlers]

        protected bool isDestroyed = false;

        public Action OnLoaded { get; set; }
        public Action OnBeforeSaving { get; set; }

        #endregion

        #region [Methods: Inspector]

        protected virtual void OnValidate()
        {
            if (autoDetectSaveables)
            {
                var iSaveables = GetComponents<ISaveable>();
                foreach (var iSaveable in iSaveables)
                    if (saveables.Find(saveable => saveable.saveable == iSaveable) == null)
                        saveables.Add(new ExistingSaveable(iSaveable, true));
            }

            // Check duplicate saveables
            for (int i = saveables.Count - 1; i >= 0; i--)
            {
                int count = 0;
                foreach (var saveable in saveables)
                {
                    if (saveables[i].component == saveable.component) count++;
                }
                if (count >= 2) Debug.LogError("Cannot save more than one [" + saveables[i].component.GetType().ToString() + "] in [" + name + "]");
            }
        }

        #endregion

        public virtual void Save(GameManager.GameAssets gameAssets)
        {
            OnBeforeSaving?.Invoke();
            SaveModule(gameAssets);
        }

        public abstract void SaveModule(GameManager.GameAssets gameAssets);

        public virtual void Load(GameManager.GameAssets gameAssets)
        {
            LoadModule(gameAssets);
            if (isDestroyed) return;

            foreach (var saveable in saveables)
            {
                if (!saveable.canSaveLoad) continue;
                if (saveable.saveable == null) continue;
                OnLoaded += () => { saveable.saveable.Load(gameAssets); };
                OnBeforeSaving += saveable.saveable.Save;
            }

            OnLoaded?.Invoke();
        }

        public abstract void LoadModule(GameManager.GameAssets gameAssets);
    }
}