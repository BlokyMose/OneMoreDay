using Encore.Localisations;
using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Dialogues
{
    [CreateAssetMenu(menuName = "SO/Dialogue/Dialogue Bundle")]
    public class DialogueBundle : ScriptableObject, ISpeakingBundle
    {
        [SerializeField] DialogueTree dialogueTree;
        [SerializeField] DialogueSettings settings;
        [SerializeField] CSVFile csv;
        [SerializeField] TextAsset dtJSON;

        [TextArea(5, 10)]
        [SerializeField] string dSyntax;

        public DialogueTree DialogueTree { get => dialogueTree; set => dialogueTree = value; }
        public DialogueSettings Settings { get => settings; set => settings = value; }
        public TextAsset CSV { get => csv.textAsset; set => csv.textAsset = value; }
        public TextAsset DTJSON { get => dtJSON; set => dtJSON = value; }
        public string DSyntax { get => dSyntax; set => dSyntax = value; }

        public List<string> ActorNames
        {
            get
            {
                var actorNames = new List<string>();
                foreach (var actor in dialogueTree.actorParameters)
                    actorNames.Add(actor.name);
                return actorNames;
            }
        }
#if UNITY_EDITOR
        [Button, GUIColor("@Encore.Utility.ColorUtility.salmon"), PropertySpace(15)]
        void DeleteAll()
        {
            if (!UnityEditor.EditorUtility.DisplayDialog("Delete", "Are you sure?", "Yes", "No"))
                return;

            if (dialogueTree != null)
            {
                var dialogueTreePath = UnityEditor.AssetDatabase.GetAssetPath(dialogueTree.GetInstanceID());
                UnityEditor.AssetDatabase.DeleteAsset(dialogueTreePath);
            }

            if (csv.textAsset != null)
            {
                var csvPath = UnityEditor.AssetDatabase.GetAssetPath(csv.textAsset.GetInstanceID());
                UnityEditor.AssetDatabase.DeleteAsset(csvPath);
            }

            if (dtJSON != null)
            {
                var dtJSONPath = UnityEditor.AssetDatabase.GetAssetPath(dtJSON.GetInstanceID());
                UnityEditor.AssetDatabase.DeleteAsset(dtJSONPath);
            }


            var thisPath = UnityEditor.AssetDatabase.GetAssetPath(GetInstanceID());
            UnityEditor.AssetDatabase.DeleteAsset(thisPath);
        }

#endif
    }
}