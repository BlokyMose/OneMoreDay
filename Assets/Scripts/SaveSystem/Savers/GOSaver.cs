using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encore.Interactables;
using Encore.Serializables;

namespace Encore.Saves
{
    [AddComponentMenu("Encore/Saves/GO Saver")]
    public class GOSaver : ObjectSaver
    {
        #region [Classes]

        [Serializable]
        public class ComponentActivation
        {
            [HorizontalGroup("H"), HideLabel]
            public Component component;
            [HorizontalGroup("H"), HideLabel]
            public string saveKey;
            public enum ActivationStatus { Enabled = 1, Disabled = 0, Destroyed = -1 }
            [HideInInspector]
            public ActivationStatus activationStatus;

            public ComponentActivation(Component component, string saveKey, ActivationStatus activationStatus)
            {
                this.component = component;
                this.saveKey = saveKey;
                this.activationStatus = activationStatus;
            }

            public ComponentActivation(Component component, string saveKey)
            {
                this.component = component;
                this.saveKey = saveKey;
                UpdateActivationStatus(component);
            }

            public void UpdateActivationStatus(Component component)
            {
                activationStatus =
                        component is Interactable
                            ? (component as Interactable).IsActive ? ActivationStatus.Enabled : ActivationStatus.Disabled :
                        component is Behaviour
                            ? (component as Behaviour).enabled ? ActivationStatus.Enabled : ActivationStatus.Disabled :
                        component is Renderer
                            ? (component as Renderer).enabled ? ActivationStatus.Enabled : ActivationStatus.Disabled :

                        ActivationStatus.Enabled;
            }

            public void ApplyActivationStatusToComponent()
            {
                if (activationStatus == ActivationStatus.Destroyed)
                    DestroyImmediate(component);
                else if (component is Interactable)
                    (component as Interactable).Activate(activationStatus == ActivationStatus.Enabled ? true : false);
                else if (component is Behaviour)
                    (component as Behaviour).enabled = activationStatus == ActivationStatus.Enabled ? true : false;
                else if (component is Renderer)
                    (component as Renderer).enabled = activationStatus == ActivationStatus.Enabled ? true : false;
            }
        }

        public enum TransformSaveMode { SaveWorld, SaveLocal, DontSave }

        #endregion

        #region [Vars: Properties]

        [Title("GO & Components")]
        [SerializeField]
        bool autoDetectComponents = true;

        [SerializeField, ListDrawerSettings(DraggableItems = false, Expanded = true), LabelText(" "), GUIColor(0.5f, 0.85f, 0.85f)]
        List<ComponentActivation> componentActivations = new List<ComponentActivation>();

        [FoldoutGroup("Transform Data"), SerializeField]
        TransformSaveMode savePosition = TransformSaveMode.DontSave;

        [FoldoutGroup("Transform Data"), SerializeField]
        TransformSaveMode saveRotation = TransformSaveMode.DontSave;

        [FoldoutGroup("Transform Data"), SerializeField]
        TransformSaveMode saveScale = TransformSaveMode.DontSave;

        #endregion

        #region [Vars: Data Handlers]

        Dictionary<string, object> savedProperties = new Dictionary<string, object>();

        #endregion

        #region [Methods: Inspector]

        protected override void OnValidate()
        {
            base.OnValidate();

            if (string.IsNullOrEmpty(saveKey) || !saveKey.Contains(name)) saveKey = name;

            if (autoDetectComponents)
            {
                var allComponents = GetComponents<Component>();
                foreach (var component in allComponents)
                {
                    if (componentActivations.Find(ca => ca.component == component) == null)
                        componentActivations.Add(new ComponentActivation(component, component.GetType().ToString()));
                }

                // Delete entry of removed component
                for (int i = componentActivations.Count - 1; i >= 0; i--)
                {
                    if (componentActivations[i].component == null) componentActivations.RemoveAt(i);
                }
            }

            // Check duplicate components
            for (int i = componentActivations.Count - 1; i >= 0; i--)
            {
                int count = 0;
                foreach (var comp in componentActivations)
                {
                    if (componentActivations[i].component == comp.component) count++;
                }
                if (count >= 2) Debug.LogError("Cannot save more than one [" + componentActivations[i].component.GetType().ToString() + "] in [" + name + "]");
            }
        }

        #endregion

        #region [Methods: ISaver]

        public override void SaveModule(GameManager.GameAssets gameAssets)
        {
            #region [Update componentActivations]

            // Update activationStatus of cached componentActivations
            List<Component> currentComps = new List<Component>(GetComponents<Component>());
            foreach (var savedComp in componentActivations)
            {
                // Try find and assign new same component for nulled components
                if (savedComp.component == null)
                {
                    // Destroyed component's key can be reused for storing the same type of component
                    var newSameComponent = currentComps.Find(_currentComp => savedComp.saveKey == _currentComp.GetType().ToString());
                    if (newSameComponent != null) savedComp.component = newSameComponent;
                    else savedComp.activationStatus = ComponentActivation.ActivationStatus.Destroyed;
                }

                // Update activationStatus for non-destroyed components
                if (savedComp.component != null)
                {
                    var currentComp = currentComps.Find(_currentComp => savedComp.component == _currentComp);
                    savedComp.UpdateActivationStatus(currentComp);
                    currentComps.Remove(currentComp);
                }
            }

            // Add new components
            foreach (var currentComp in currentComps)
            {
                componentActivations.Add(new ComponentActivation(currentComp, currentComp.GetType().ToString()));
            }

            #endregion

            #region [Save Transform]

            Vector3 pos = Vector3.zero;
            switch (savePosition)
            {
                case TransformSaveMode.SaveWorld:
                    pos = transform.position == Vector3.zero ? new Vector3(0.01f, 0.01f) : transform.position;
                    break;
                case TransformSaveMode.SaveLocal:
                    pos = transform.localPosition == Vector3.zero ? new Vector3(0.01f, 0.01f) : transform.localPosition;
                    break;
            }

            Vector3 rot = Vector3.zero;
            switch (saveRotation)
            {
                case TransformSaveMode.SaveWorld:
                    rot = transform.eulerAngles == Vector3.zero ? new Vector3(0.01f, 0.01f) : transform.eulerAngles;
                    break;
                case TransformSaveMode.SaveLocal:
                    rot = transform.localEulerAngles == Vector3.zero ? new Vector3(0.01f, 0.01f) : transform.localEulerAngles;
                    break;
            }

            Vector3 scale = Vector3.zero;
            switch (savePosition)
            {
                case TransformSaveMode.SaveWorld:
                case TransformSaveMode.SaveLocal:
                    scale = transform.localScale == Vector3.zero ? new Vector3(0.01f, 0.01f) : transform.localScale;
                    break;
            }

            #endregion

            GOSaverData data = new GOSaverData(
                isDestroyed: isDestroyed,
                isActive: gameObject.activeSelf,
                componentActivations: componentActivations,
                position: pos,
                rotation: rot,
                scale: scale,
                savedProperties: savedProperties
                );

            gameAssets.currentSceneData.SaveObjectToObjectData(saveKey, data);
        }

        public override void LoadModule(GameManager.GameAssets gameAssets)
        {
            if (string.IsNullOrEmpty(saveKey)) return;
            var data = (GOSaverData)gameAssets.currentSceneData.LoadObjectFromObjectData(saveKey);
            if (data == null) return;

            #region [Apply saved data]

            // Destroy
            if (data.isDestroyed)
            {
                isDestroyed = true;
                DestroyImmediate(gameObject);
                return;
            }

            // Activation
            gameObject.SetActive(data.isActive);

            // Components
            componentActivations.Clear();
            var allComponents = new List<Component>(GetComponents<Component>());
            foreach (var comp in data.componentActivations)
            {
                var foundComponent = allComponents.Find(c => c.GetType().ToString() == comp.Key);
                if (foundComponent == null) continue;

                componentActivations.Add(new ComponentActivation(
                    component: foundComponent,
                    saveKey: comp.Key,
                    activationStatus: (ComponentActivation.ActivationStatus)comp.Value
                    ));

                var componentActivation = componentActivations[componentActivations.Count - 1];
                componentActivation.ApplyActivationStatusToComponent();
            }

            // Transform
            if (data.position != Vector3.zero)
                switch (savePosition)
                {
                    case TransformSaveMode.SaveWorld:
                        transform.position = data.position;
                        break;
                    case TransformSaveMode.SaveLocal:
                        transform.localPosition = data.position;
                        break;
                }
            if (data.rotation != Vector3.zero)
                switch (saveRotation)
                {
                    case TransformSaveMode.SaveWorld:
                        transform.eulerAngles = data.rotation;
                        break;
                    case TransformSaveMode.SaveLocal:
                        transform.localEulerAngles = data.rotation;
                        break;
                }
            if (data.scale != Vector3.zero)
                switch (saveScale)
                {
                    case TransformSaveMode.SaveWorld:
                    case TransformSaveMode.SaveLocal:
                        transform.localScale = data.scale;
                        break;
                }

            // Saved properties
            if (data.savedProperties != null)
            {
                savedProperties = data.savedProperties;
            }

            #endregion
        }

        /// <summary>Save custom data <see cref="savedProperties"/></summary>
        public void AddProperty(string key, object value)
        {
            if (savedProperties.ContainsKey(key))
                savedProperties[key] = value;
            else
                savedProperties.Add(key, value);
        }

        /// <summary>Load custom data from <see cref="savedProperties"/></summary>
        public object GetProperty(string key)
        {
            savedProperties.TryGetValue(key, out object value);
            return value;
        }

        #endregion
    }
}