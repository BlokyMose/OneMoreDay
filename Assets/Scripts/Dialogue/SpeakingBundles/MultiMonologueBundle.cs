using Encore.Localisations;
using Encore.Utility;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Dialogues
{
    [CreateAssetMenu(menuName = "SO/Dialogue/MultiMonologue Bundle")]
    public class MultiMonologueBundle : ScriptableObject, ISpeakingBundle
    {
        [SerializeField] List<MonologuesList> dialogues;
        [SerializeField] string dSyntax;
        [SerializeField] CSVFile csv;

        public List<MonologuesList> Dialogues { get => dialogues; set => dialogues = value; }
        public string DSyntax { get => dSyntax; set => dSyntax = value; }
        public TextAsset CSV { get => csv.textAsset; set => csv.textAsset = value; }

        public List<string> ActorNames
        {
            get 
            {
                List<string> actorNames = new List<string>();
                foreach (var monologue in dialogues)
                    actorNames.AddRangeUnique(monologue.GetActorNames());
                return actorNames;
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