using Encore.Localisations;
using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Dialogues
{
    [CreateAssetMenu(menuName = "SO/Dialogue/Monologue Bundle")]
    public class MonologueBundle : ScriptableObject, ISpeakingBundle
    {
        [SerializeField] List<Monologue> monologues;
        [SerializeField] string dSyntax;
        [SerializeField] CSVFile csv;

        public List<Monologue> Monologues { get => monologues; set => monologues = value; }
        public string DSyntax { get => dSyntax; set => dSyntax = value; }
        public TextAsset CSV { get => csv.textAsset; set => csv.textAsset = value; }

        public List<string> ActorNames
        {
            get
            {
                return new List<string>() { monologues[0].Actor.ActorName };
            }
        }

#if UNITY_EDITOR

        [Button, GUIColor("@Encore.Utility.ColorUtility.salmon"), PropertySpace(15)]
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