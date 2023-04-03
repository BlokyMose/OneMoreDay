using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/DSyntax Hook")]
    public class DSyntaxHook : SpeakingHook
    {
        [SerializeField]
        List<DSyntaxBundle> dSyntaxBundles  = new List<DSyntaxBundle>();

        protected override IList GetList()
        {
            return dSyntaxBundles;
        }

        protected override bool TriggerSpeaking(int index)
        {
            base.TriggerSpeaking(index);
            var bundle = dSyntaxBundles[index];
            switch (bundle.Mode)
            {
                case DSyntaxBundle.SpeakingMode.Dialogue:
                    return GameManager.Instance.DialogueManager.BeginDSyntaxDialogue(bundle);
                case DSyntaxBundle.SpeakingMode.MultiMonologue:
                    return GameManager.Instance.DialogueManager.BeginDSyntaxDialogue(bundle);
                case DSyntaxBundle.SpeakingMode.Chat:
                    return GameManager.Instance.PhoneManager.ChatApp.BeginDialogue(bundle);

                default:
                    return false;
            }

            
        }

        #region [Inspector]

#if UNITY_EDITOR

        [HorizontalGroup("_Add")]
        [FoldoutGroup("_Add/Add"), SerializeField, FolderPath]
        string saveFolder = "Assets/Contents/SOAssets/DSyntaxAsset";

        [HorizontalGroup("_Add", width: 0.2f)]
        [Button(" + "), GUIColor("@Encore.Utility.ColorUtility.paleGreen")]
        public void AddDSyntaxBundle()
        {
            // Get path
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string folderPath = saveFolder + "/" + sceneName + "/" + "DSyntaxBundle";
            System.IO.Directory.CreateDirectory(folderPath);
            string filePath = folderPath + "/" + gameObject.name + ".asset";
            var bundle = ScriptableObject.CreateInstance<DSyntaxBundle>();
            UnityEditor.AssetDatabase.CreateAsset(bundle, filePath);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            dSyntaxBundles.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<DSyntaxBundle>(filePath));

            Debug.Log("Updated:\n\n" + bundle);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

#endif

        #endregion

    }
}
