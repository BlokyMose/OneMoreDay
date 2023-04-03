using UnityEngine;
using Sirenix.OdinInspector;
using Encore.Locations;

namespace Encore.Phone.ToDo
{
    [CreateAssetMenu(menuName = "SO/ToDo/ToDoData", fileName = "ToDo_")]
    [InlineEditor]
    public class ToDoData : ScriptableObject
    {
        public enum ToDoStatus { ToDo, Done, Aborted }

        [SerializeField, InlineButton(nameof(_GetRandomID), "Random")] string id;
        [SerializeField] ToDoStatus status = ToDoStatus.ToDo;
        [SerializeField] string text;
        [SerializeField] ToDoTag tag;
        [SerializeField] Location location;

        [InlineButton(nameof(_OnCSVClick), "@"+nameof(csv)+"?\"Update\":\"Add\"")]
        [SerializeField] TextAsset csv;

        public string ID { get { return id; } }
        public string Text { get { return text; } set { text = value; } }
        public ToDoStatus Status { get { return status; } set { status = value; } }
        public ToDoTag Tag { get { return tag; } set { tag = value; } }
        public Location Location { get { return location; } set { location = value; } }
        public TextAsset CSV { get { return csv; } }

        public void _GetRandomID()
        {
            id = System.Guid.NewGuid().ToString();
        }

        void _OnCSVClick()
        {
#if UNITY_EDITOR

            var parentFolderPath = "Assets/Contents/SOAssets/ToDoData/Localisation";
            System.IO.Directory.CreateDirectory(parentFolderPath);
            var path = "";
            var csvText = string.Format("\"{0}\",\"{1}\",\"\"\n", "key", Encore.Localisations.LocalisationSystem.Language.ENG.ToString());
            csvText += string.Format("\"{0}\",\"{1}\",\"\"\n", nameof(text), text);
            if (csv != null)
            {
                path = UnityEditor.AssetDatabase.GetAssetPath(csv);
            }
            else
            {
                path = parentFolderPath + "/" + name + "_Localisation.csv";
            }

            System.IO.File.WriteAllText(path, csvText);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            csv = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
#endif
        }
    }
}