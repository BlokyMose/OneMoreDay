using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;
using DialogueSyntax;
using Encore.Utility;
using Encore.Localisations;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/Monologue Hook")]
    public partial class MonologueHook : SpeakingHook
    {
        [SerializeField, ListDrawerSettings(HideAddButton = true)] 
        List<Monologue> monologues = new List<Monologue>();

        [FoldoutGroup("Dialogue Bundle", order: 10), SerializeField]
        #if UNITY_EDITOR 
        [InlineButton(nameof(UpdateMonologueBundle), "Update")] 
        #endif
        MonologueBundle bundle;

        protected override IList GetList() { return monologues; }

        protected override bool TriggerSpeaking(int index)
        {
            base.TriggerSpeaking(index);
            if (bundle == null)
            {
                return GameManager.Instance.DialogueManager.BeginMonologue(monologues[index]);
            }
            else
            {
                return GameManager.Instance.DialogueManager.BeginMonologue(bundle, index);
            }
        }


        #region [Inspector]



#if UNITY_EDITOR


        [FoldoutGroup("Dialogue Bundle"), SerializeField, InlineButton(nameof(UpdateCSV), "Update"), LabelWidth(50)]
        TextAsset localisationCSV;

        [FoldoutGroup("Dialogue Bundle"), SerializeField, FolderPath,LabelWidth(50), LabelText("Folder")]
        string saveFolder = "Assets/Contents/SOAssets/MonologueAssets";

        [HorizontalGroup("Add", width: 0.8f)]
        [FoldoutGroup("Add/Add Monologue")]
        [SerializeField] Actor presetActor;
        [FoldoutGroup("Add/Add Monologue")]
        [SerializeField] Monologue.MonologueSettings presetSettings = new Monologue.MonologueSettings(-1);

        [HorizontalGroup("Add", width: 0.2f)]
        [Button(" + "), GUIColor("@Color.green")]
        public void AddMonologue()
        {
            monologues.Add(new Monologue(presetActor, new Statement(), presetSettings));
        }

        [CustomContextMenu("Generate simple DialogueSyntax", nameof(GenerateSimpleDSyntax))]
        [FoldoutGroup("Dialogue Bundle"), SerializeField, TextArea(1, 5)]
        string dSyntax;

        [FoldoutGroup("Dialogue Bundle"), SerializeField, InlineButton(nameof(FindAndAssignDefaultDSyantaxSettings), "Default", ShowIf = "@!" + nameof(dSyntaxSettings))]
        DSyntaxSettings dSyntaxSettings;

        void FindAndAssignDefaultDSyantaxSettings()
        {
            var dataArray = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(DSyntaxSettings));
            if (dataArray != null)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(dataArray[0]);
                dSyntaxSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<DSyntaxSettings>(path);
            }
        }

        [HorizontalGroup("Dialogue Bundle/Buttons"), Button("DSyntax to Statements", ButtonSizes.Large)]
        void ConvertFromDSyntax()
        {
            if (dSyntaxSettings == null) FindAndAssignDefaultDSyantaxSettings();

            List<Monologue> result = new List<Monologue>();
            var commands = DSyntaxUtility.ReadCommands(dSyntaxSettings, dSyntax);

            int textIndex = 0;
            foreach (var command in commands)
            {
                // Find actor in project
                Actor actor = null;
                var dataArray = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(Actor));
                if (dataArray != null)
                {
                    foreach (var guid in dataArray)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        var actorInProject = UnityEditor.AssetDatabase.LoadAssetAtPath<Actor>(path);

                        // Take the first actor in the text as the sole actor in this monologue
                        var actorName = command.name;
                        if (actorInProject.ActorName == actorName)
                        {
                            actor = actorInProject;
                            break;
                        }
                    }
                }

                // Assign statement
                Statement statement = DSyntaxUtility.ConvertDSyntaxToStatement(dSyntaxSettings, command);
                statement.textIndex = textIndex.ToString();

                result.Add(new Monologue(actor, statement, presetSettings));

                textIndex++;
            }

            monologues = result;
        }

        [HorizontalGroup("Dialogue Bundle/Buttons"), Button("Statements to DSyntax", ButtonSizes.Large)]
        void ConvertToDSyntax()
        {
            if (dSyntaxSettings == null) FindAndAssignDefaultDSyantaxSettings();

            string result = "";
            foreach (var monologue in monologues)
            {
                result += DSyntaxUtility.WriteCommand(dSyntaxSettings, monologue.Actor.ActorName, monologue.Statement.text);
                result += DSyntaxUtility.WriteParameters(dSyntaxSettings, new List<string>() {
                monologue.Statement.expression != Expression.None
                    ? DSyntaxSettings.GetExpressionHex(monologue.Statement.expression, dSyntaxSettings).GetString()
                    : "",
                monologue.Statement.gesture != Gesture.None
                    ? monologue.Statement.gesture.ToString()
                    : ""
            });

                result += "\n";
            }

            dSyntax = result;
        }

        void UpdateCSV()
        {
            // Get path
            string filePath = "";
            if (localisationCSV == null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                string folderPath = saveFolder + "/" + sceneName + "/" + "CSV";
                System.IO.Directory.CreateDirectory(folderPath);
                filePath = folderPath + "/" + gameObject.name + ".csv";
            }
            else
            {
                filePath = UnityEditor.AssetDatabase.GetAssetPath(localisationCSV.GetInstanceID());
            }

            // Generating csv
            string csv = "";

            // Write column header 
            string COLUMN_KEY = "key";
            string COLUMN_LANGUAGE = LocalisationSystem.Language.ENG.ToString();
            csv += string.Format("\"{0}\",\"{1}\"\n", COLUMN_KEY, COLUMN_LANGUAGE);

            // Write actors
            var writtenActors = new List<Actor>();
            foreach (var monologue in monologues)
            {
                if (!writtenActors.Contains(monologue.Actor))
                {
                    csv += string.Format("\"{0}\",\"{1}\"\n", monologue.Actor.ActorName, monologue.Actor.ActorName);
                    writtenActors.Add(monologue.Actor);
                }
            }

            // Write monologues
            int textIndex = 0;
            foreach (var monologue in monologues)
            {
                monologue.Statement.textIndex = textIndex.ToString();
                csv += string.Format("\"{0}\",\"{1}\"\n", monologue.Statement.textIndex, monologue.Statement.text);
                textIndex++;
            }

            System.IO.File.WriteAllText(filePath, csv);
            localisationCSV = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            Debug.Log("Updated:\n\n" + csv);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        void UpdateMonologueBundle()
        {
            // Get path
            string filePath = "";
            if (bundle == null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                string folderPath = saveFolder + "/" + sceneName + "/" + "MonologueBundle";
                if (!System.IO.File.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);
                filePath = folderPath + "/" + gameObject.name + ".asset";
                bundle = ScriptableObject.CreateInstance<MonologueBundle>();
                UnityEditor.AssetDatabase.CreateAsset(bundle, filePath);
            }
            else
            {
                filePath = UnityEditor.AssetDatabase.GetAssetPath(bundle.GetInstanceID());
            }

            bundle = UnityEditor.AssetDatabase.LoadAssetAtPath<MonologueBundle>(filePath);
            bundle.Monologues = monologues;
            bundle.CSV = localisationCSV;
            bundle.DSyntax = dSyntax;

            Debug.Log("Updated:\n\n" + bundle);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        void GenerateSimpleDSyntax()
        {
            dSyntax = DSyntaxUtility.GenerateDialogueMultiBranches(dSyntaxSettings);
        }

#endif

        #endregion

    }
}