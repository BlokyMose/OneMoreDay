using Encore.Utility;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.SceneMasters
{
    /// <summary>
    /// [GENERAL IDEA] <br></br>
    /// 
    /// </summary>
    [CreateAssetMenu(menuName = "SO/GlobalState/Global State", fileName = "GS_Name_Init")]

    public class GlobalState : ScriptableObject
    {
        [System.Serializable]
        public class Stage
        {
            public string stageName;

            public Stage(string stageName)
            {
                this.stageName = stageName;
            }
        }

        [InlineButton(nameof(SyncWithSOName), "Sync Name")]
        public string stateName;

        public List<Stage> stages = new List<Stage>() { new Stage("init") };

        [ListDrawerSettings(HideAddButton = true, DraggableItems = false, HideRemoveButton = true)]
        public List<GlobalStateObject> objects = new List<GlobalStateObject>();


        #region [Methods: Inspector]

        void SyncWithSOName()
        {
#if UNITY_EDITOR
            stateName = name;
            stateName = stateName.Replace("GS_", "");
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        [Button("Update Objects")]  
        void UpdateObjectsStages()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] == null) objects.RemoveAt(i);

            foreach (var gso  in objects)
                gso.UpdateStagesOfAllActivations();
        }

        [Button("Delete"), GUIColor("@Encore.Utility.ColorUtility.salmon")]
        void Delete()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog("Deleting this GSO", "Are you sure to delete this GS?\n\nDeleting this will also remove Activations from all GSO's activations list.", "Yes"))
            {
                foreach (var gso in objects)
                    gso.RemoveActivationsWithGS(this);
                var thisPath = UnityEditor.AssetDatabase.GetAssetPath(GetInstanceID());
                UnityEditor.AssetDatabase.DeleteAsset(thisPath);
            }
#endif
        }

        #endregion


    }
}
