using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using DialogueSyntax;
using System.Collections.Generic;
using Encore.Localisations;

namespace Encore.Dialogues
{
    [CreateAssetMenu(menuName = "SO/Dialogue/DSyntax Bundle")]
    [InlineEditor]
    public class DSyntaxBundle : ScriptableObject, ISpeakingBundle
    {
        public enum SpeakingMode { Dialogue, MultiMonologue, Chat}

        [SerializeField, Required]
        string title;

        [SerializeField]
        SpeakingMode mode;

        [SerializeField] 
        DSyntaxString dSyntax;

        [SerializeField] 
        DialogueSettings settings;

        [SerializeField, LabelWidth(50)]
        CSVFile csv;

        [SerializeField, FolderPath, LabelWidth(50), LabelText("Folder")]
        string saveFolder = "Assets/Contents/SOAssets/DSyntaxAsset";

        public string Title { get => title; set => title = value; }
        public SpeakingMode Mode { get => mode; set => mode = value; }
        public DSyntaxString DSyntax { get => dSyntax; set => dSyntax = value; }
        public DialogueSettings Settings { get => settings; }
        public TextAsset CSV { get => csv.textAsset; set => csv.textAsset = value; }

        public List<string> ActorNames
        {
            get
            {
                return DSyntaxUtility.GetActors(dSyntax.dSyntax, GameManager.Instance.DialogueManager.GetDSyntaxSettings);
            }
        }
#if UNITY_EDITOR

        [Button]
        void UpdateCSV()
        {
            if (string.IsNullOrEmpty(title)) title = "untitled";

            // Get path
            string filePath = "";
            if (csv == null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                string folderPath = saveFolder + "/" + sceneName + "/" + "CSV";
                System.IO.Directory.CreateDirectory(folderPath);
                filePath = folderPath + "/" + title + ".csv";
            }
            else
            {
                filePath = UnityEditor.AssetDatabase.GetAssetPath(csv.textAsset.GetInstanceID());
            }

            // Generating csv
            var dSyntaxSettingsGUIDs = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(DSyntaxSettings));
            var dSyntaxSettingsPath = UnityEditor.AssetDatabase.GUIDToAssetPath(dSyntaxSettingsGUIDs[0]);
            var dSyntaxSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<DSyntaxSettings>(dSyntaxSettingsPath);
            var _dSyntax = dSyntax;
            var tree = DSyntaxUtility.GetTree(_dSyntax.dSyntax, dSyntaxSettings);
            string csvString = DSyntaxUtility.ConvertTreeToCSV(tree);

            System.IO.File.WriteAllText(filePath, csvString);
            
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            csv = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            Debug.Log("Updated:\n\n" + csvString);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        [Button,GUIColor("@Encore.Utility.ColorUtility.salmon"), PropertySpace(15)]
        void DeleteAll()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("Delete", "Are you sure?", "Yes", "No"))
                return;

            if (csv.textAsset != null)
            {
                var csvPath = UnityEditor.AssetDatabase.GetAssetPath(csv.textAsset.GetInstanceID());
                UnityEditor.AssetDatabase.DeleteAsset(csvPath);
            }

            var thisPath = UnityEditor.AssetDatabase.GetAssetPath(GetInstanceID());
            UnityEditor.AssetDatabase.DeleteAsset(thisPath);
        }

#endif
    }
}