using Encore.Serializables;
using Encore.Utility;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.SceneMasters
{
    [AddComponentMenu("Encore/GlobalState/Global State Setter - GS")]
    public class GlobalStateSetter : MonoBehaviour
    {
        [System.Serializable]
        public class GlobalStateModified
        {
            public GlobalState gs;
            public string targetStage;

            public GlobalStateModified(GlobalState gs, string targetStage)
            {
                this.gs = gs;
                this.targetStage = targetStage;
            }
        }

        [SerializeField, Tooltip("States will be reapplied immediately when trigered")]
        bool refreshThisGlobalStates = true;

        [SerializeField]
        public List<GlobalStateModified> modifiedStates = new List<GlobalStateModified>();

        public void Trigger()
        {
            foreach (var stateModified in modifiedStates)
            {
                GameManager.Instance.ModifyGlobalStateData(stateModified);
                GameManager.Instance.SetAsLastGlobalState(stateModified.gs);
            }

            if(refreshThisGlobalStates)
                SceneMaster.current.ApplyAllStatesCurrentStage();
        }

        [Button("Create New GS"), GUIColor("@Color.green")]
        void CreateGlobalState()
        {
#if UNITY_EDITOR

            var newGS = ScriptableObject.CreateInstance<GlobalState>();
            string saveFolderParentPath = "Assets/Contents/SOAssets/GlobalStates";

            System.IO.Directory.CreateDirectory(saveFolderParentPath);
            UnityEditor.AssetDatabase.CreateAsset(newGS, saveFolderParentPath + "/GS_" + gameObject.name + ".asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = newGS;

            modifiedStates.Add(new GlobalStateModified(newGS, "init"));
#endif
        }
    }
}
