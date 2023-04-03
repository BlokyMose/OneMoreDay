using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using NodeCanvas.DialogueTrees;
using DialogueSyntax;
using Encore.Utility;
using Encore.Localisations;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/MultiMonologue Hook")]
    public partial class MultiMonologueHook : SpeakingHook
    {
        [SerializeField]
        List<MonologuesList> dialogues = new List<MonologuesList>();

        [FoldoutGroup("Dialogue Bundle", order: 10), SerializeField]
#if UNITY_EDITOR
        [InlineButton(nameof(UpdateMultiMonologueBundle), "Update")]
#endif
        MultiMonologueBundle bundle;

        protected override IList GetList() { return dialogues; }

        protected override bool TriggerSpeaking(int index)
        {
            base.TriggerSpeaking(index);
            if (bundle == null)
            {
                return GameManager.Instance.DialogueManager.BeginMultiMonologue(dialogues[index]);
            }
            else
            {
                return GameManager.Instance.DialogueManager.BeginMultiMonologue(bundle,index);
            }
        }

        #region [Inspector]

#if UNITY_EDITOR
        [FoldoutGroup("Dialogue Bundle"), SerializeField, InlineButton(nameof(UpdateCSV), "Update"), LabelWidth(50)]
        TextAsset localisationCSV;

        [FoldoutGroup("Dialogue Bundle"), SerializeField, FolderPath, LabelWidth(50), LabelText("Folder")]
        string saveFolder = "Assets/Contents/SOAssets/MultiMonologueAssets";

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

        [HorizontalGroup("Dialogue Bundle/Buttons"), Button("DSyntax to Dialogues", ButtonSizes.Large)]
        void ConvertFromDSyntax()
        {
            if (dSyntaxSettings == null) FindAndAssignDefaultDSyantaxSettings();

            // Load actors
            var actorsInProject = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(Actor));

            int textIndex = 0;

            dialogues = new List<MonologuesList>();

            // Separating branches 
            var _dialogues = DSyntaxUtility.ReadCommandsByGroups(dSyntaxSettings, dSyntax, dSyntaxSettings.COMMAND_BRANCH);
            foreach (var _dialogue in _dialogues)
            {
                // Separating each monologue
                List<Monologue> monologues = new List<Monologue>();

                // Extract branch's name
                var branchName = _dialogue[0].name;
                _dialogue.RemoveAt(0);

                foreach (var command in _dialogue)
                {
                    // Assign actor
                    Actor actor = null;
                    if (actorsInProject != null)
                    {
                        foreach (var guid in actorsInProject)
                        {
                            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                            var _actor = UnityEditor.AssetDatabase.LoadAssetAtPath<Actor>(path);

                            var actorName = command.name;

                            if (_actor.ActorName == actorName)
                            {
                                actor = _actor;
                                break;
                            }
                        }
                    }
                    if (actor == null)
                    {
                        Debug.Log("Cannot find actor: [" + command.name + "]");
                    }

                    // Assign statement
                    Statement statement = DSyntaxUtility.ConvertDSyntaxToStatement(dSyntaxSettings, command);
                    statement.textIndex = textIndex.ToString();

                    monologues.Add(new Monologue(actor, statement, new Monologue.MonologueSettings(-1)));

                    textIndex++;
                }

                dialogues.Add(new MonologuesList(branchName, monologues));
            }
        }

        [HorizontalGroup("Dialogue Bundle/Buttons"), Button("Dialogues to DSyntax", ButtonSizes.Large)]
        void ConvertToDSyntax()
        {
            if (dSyntaxSettings == null) FindAndAssignDefaultDSyantaxSettings();

            string result = "";

            foreach (var dialogue in dialogues)
            {
                // Write branch command
                result += DSyntaxUtility.WriteCommand(dSyntaxSettings, dSyntaxSettings.COMMAND_BRANCH, dialogue.title, true);

                // Write all commands
                foreach (var monologue in dialogue.monologues)
                {
                    var actor = monologue.Actor;
                    var statement = monologue.Statement;

                    result += DSyntaxUtility.WriteCommand(dSyntaxSettings, monologue.Actor.ActorName, statement.text);

                    result += DSyntaxUtility.WriteParameters(dSyntaxSettings, new List<string>() {
                statement.expression != Expression.None
                    ? DSyntaxSettings.GetExpressionHex(statement.expression, dSyntaxSettings).GetString()
                    : "",
                statement.gesture != Gesture.None
                    ? statement.gesture.ToString()
                    : ""
                });
                }
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
                if (!System.IO.File.Exists(folderPath))
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
            List<Actor> actors = new List<Actor>();
            foreach (var dialogue in dialogues)
            {
                foreach (var monologue in dialogue.monologues)
                {
                    if (!actors.Contains(monologue.Actor)) actors.Add(monologue.Actor);
                }
            }
            foreach (var actor in actors)
            {
                csv += string.Format("\"{0}\",\"{1}\"\n", actor.ActorName, actor.ActorName);
            }

            // Write dialogues
            int textIndex = 0;
            foreach (var dialogue in dialogues)
            {
                foreach (var monologue in dialogue.monologues)
                {
                    var statement = monologue.Statement;
                    statement.textIndex = textIndex.ToString();
                    csv += string.Format("\"{0}\",\"{1}\"\n", statement.textIndex, statement.text);
                    textIndex++;
                }
            }

            System.IO.File.WriteAllText(filePath, csv);
            localisationCSV = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            Debug.Log("Updated:\n\n" + csv);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        void UpdateMultiMonologueBundle()
        {
            // Get path
            string filePath = "";
            if (bundle == null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                string folderPath = saveFolder + "/" + sceneName + "/" + "MultiMonologueBundle";
                if (!System.IO.File.Exists(folderPath)) System.IO.Directory.CreateDirectory(folderPath);
                filePath = folderPath + "/" + gameObject.name + ".asset";
                bundle = ScriptableObject.CreateInstance<MultiMonologueBundle>();
                UnityEditor.AssetDatabase.CreateAsset(bundle, filePath);
            }
            else
            {
                filePath = UnityEditor.AssetDatabase.GetAssetPath(bundle.GetInstanceID());
            }

            bundle = UnityEditor.AssetDatabase.LoadAssetAtPath<MultiMonologueBundle>(filePath);
            bundle.Dialogues = dialogues;
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

        void OnValidate()
        {
            int index = 0;
            foreach (var dialogue in dialogues)
            {
                int _index = index;
                if (string.IsNullOrEmpty(dialogue.title)) dialogue.title = _index.ToString();
                index++;
            }
        }

#endif

        #endregion

    }
}