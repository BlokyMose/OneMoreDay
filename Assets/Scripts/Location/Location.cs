using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Encore.Locations
{
    [CreateAssetMenu(menuName = "SO/Location/Location", fileName ="Loc_")]
    public class Location : ScriptableObject
    {
        [SerializeField, OnValueChanged(nameof(OnValueChangedScene))]
        Object scene;

        [SerializeField, ReadOnly]
        string sceneName;

        [SerializeField]
        string locationDisplayName;

        [SerializeField]
        District district;

        [SerializeField]
        Vector2 coordinate;

        [SerializeField, TextArea(5, 10)]
        string desc;

        [SerializeField]
        List<Sprite> images;

        [SerializeField]
        bool initialIsUnlocked;

        public Object Scene { get { return scene; } }
        public string SceneName { get { return sceneName; } }
        public string LocationDisplayName { get { return locationDisplayName; } }
        public District District { get { return district; } }
        public Vector2 Coordinate{ get { return coordinate; } set { coordinate = value; } }
        public string Desc { get { return desc; } }
        public List<Sprite> Images { get { return images; } }
        public bool InitialIsUnlocked { get { return initialIsUnlocked; } }

        #region [Methods: Inspector]

        [Button]
        void OnValueChangedScene()
        {
            if(scene!=null) sceneName = scene.name;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            ValidateSceneObject();
        }

        void ValidateSceneObject()
        {
            if (UnityEditor.AssetDatabase.GetAssetPath(scene.GetInstanceID()).IndexOf(".unity") == -1)
                scene = null;
        }

#endif

        #endregion
    }
}