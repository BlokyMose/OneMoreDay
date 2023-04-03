using Encore.Saves;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Serializables
{
    [System.Serializable]
    public class GOSaverData
    {
        public bool isDestroyed;
        public bool isActive;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public Dictionary<string, int> componentActivations = new Dictionary<string, int>();
        public Dictionary<string, object> savedProperties = new Dictionary<string, object>();

        public GOSaverData(bool isDestroyed, bool isActive, List<GOSaver.ComponentActivation> componentActivations = null, Vector3 position = default, Vector3 rotation = default, Vector3 scale = default, Dictionary<string, object> savedProperties = null)
        {
            this.isDestroyed = isDestroyed;
            this.isActive = isActive;
            foreach (var comp in componentActivations)
            {
                this.componentActivations.Add(comp.saveKey, (int)comp.activationStatus);
            }

            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.savedProperties = savedProperties;
        }

        public GOSaverData()
        {
        }
    }
}
