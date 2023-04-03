using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Encore.SceneMasters
{
    [AddComponentMenu("Encore/GlobalState/Global State Object Container - GSO")]
    public class GlobalStateObjectContainer : MonoBehaviour
    {
        [InlineButton(nameof(CreateGlobalStateObject), "New", ShowIf = "@!"+nameof(gso))]
        public GlobalStateObject gso;

        public void Evaluate(Dictionary<string,Serializables.GlobalStateData> statesData)
        {
            foreach (var stateData in statesData)
                Evaluate(stateData.Value.stateName, stateData.Value.currentStage);
        }

        public void Evaluate(string stateName, string currentStage)
        {
            var foundActivation = gso.activations.Find(act => act.gs.stateName == stateName);
            if (foundActivation != null)
            {
                var foundStage = foundActivation.stageActivations.Find(stageAct => stageAct.stage.stageName == currentStage);
                if (foundStage != null)
                {
                    switch (foundStage.activationMode)
                    {
                        case GlobalStateObject.ActivationMode.AsIs:
                            break;
                        case GlobalStateObject.ActivationMode.Active:
                            gameObject.SetActive(true);
                            break;
                        case GlobalStateObject.ActivationMode.Inactive:
                            gameObject.SetActive(false);
                            break;
                        case GlobalStateObject.ActivationMode.Destroyed:
                            DestroyImmediate(gameObject);
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning("Unknown stageName: ["+currentStage+"]\nGSO: ["+gso.name+"]");
                }
            }
        }

        void CreateGlobalStateObject()
        {
#if UNITY_EDITOR

            var newGSO = ScriptableObject.CreateInstance<GlobalStateObject>();
            string saveFolderParentPath = "Assets/Contents/SOAssets/GlobalStateObjects";

            string saveFolderPath = saveFolderParentPath + "/" + SceneManager.GetActiveScene().name;
            System.IO.Directory.CreateDirectory(saveFolderPath);
            UnityEditor.AssetDatabase.CreateAsset(newGSO, saveFolderPath + "/GSO_" + gameObject.name + ".asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.EditorUtility.FocusProjectWindow();
            UnityEditor.Selection.activeObject = newGSO;

            gso = newGSO;
#endif
        }

    }
}
