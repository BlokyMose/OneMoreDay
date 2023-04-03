using Encore.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Encore.Localisations
{
    [AddComponentMenu("Encore/Localisations/Texts Localiser")]

    public class TextsLocaliser : MonoBehaviour
    {
        #region [Classes]

        [Serializable]
        public struct Text
        {
            public string index;
            public string text;

            public Text(string index, string text)
            {
                this.index = index;
                this.text = text;
            }
        }

        public abstract class TextData
        {
            public string id;
            public TextMeshProUGUI textComp;
            public TextsLocaliser localiser;

            [GUIColor("@"+nameof(isLocalised)+"?Encore.Utility.ColorUtility.paleGreen:Encore.Utility.ColorUtility.salmon")]
            public bool isLocalised = true;

            public TextData(TextsLocaliser localiser, TextMeshProUGUI textComp)
            {
                this.localiser = localiser;
                this.id = textComp.gameObject.name;
                this.textComp = textComp;
            }
            public abstract void LocaliseText(Dictionary<string, string> localisedTexts);


        }

        [Serializable]
        public class TextDataMono : TextData
        {
            public Text text;

            public TextDataMono(TextsLocaliser localiser, TextMeshProUGUI textComp, Text text) : base(localiser,textComp)
            {
                this.text = text;
            }

            public override void LocaliseText(Dictionary<string,string> localisedTexts)
            {
                if (!isLocalised) return;

                var localisedText = localisedTexts.Get(text.index, text.text);
                if (string.IsNullOrEmpty(localisedText)) localisedText = text.text;

                textComp.text = localisedText;
            }

            [Button("Convert to Format"), GUIColor("@Encore.Utility.ColorUtility.orange")]
            void ConvertToTextDataFormat()
            {
                isLocalised = false;
                localiser.AddTextDataFormat(new TextDataFormat(localiser, textComp));
            }
        }

        [Serializable]
        public class TextDataFormat : TextData
        {
            [Serializable]
            public struct TextFormatInfo
            {
                public enum TextType { Localise, Data }

                public string index;
                public string text;
                [GUIColor("@textType == TextType.Localise ? Encore.Utility.ColorUtility.paleGreen:Encore.Utility.ColorUtility.goldenRod")]
                public TextType textType;

                public TextFormatInfo(string index, string text, TextType textType)
                {
                    this.index = index;
                    this.text = text;
                    this.textType = textType;
                }
            }

            [TextArea(5,15)]
            public string format;

            [ListDrawerSettings(DraggableItems = false, HideRemoveButton = true, HideAddButton = true)]
            public List<TextFormatInfo> texts = new List<TextFormatInfo>();

            public Dictionary<string, string> DataTexts { get; set; }


            public TextDataFormat(TextsLocaliser localiser, TextMeshProUGUI textComp) : base(localiser, textComp)
            {
                TextsToFormat();
                DataTexts = new Dictionary<string, string>();
            }

            public TextDataFormat(TextsLocaliser localiser, TextMeshProUGUI textComp, Dictionary<string,string> dataTexts) : base(localiser, textComp)
            {
                TextsToFormat();
                DataTexts = dataTexts;
            }

            public override void LocaliseText(Dictionary<string, string> localisedWords)
            {
                if (!isLocalised) return;

                var localisedText = format;
                foreach (var localisedWord in localisedWords)
                {
                    if (!string.IsNullOrEmpty(localisedWord.Value))
                        localisedText = localisedText.Replace("[" + localisedWord.Key + "]", localisedWord.Value);
                    else
                        localisedText = localisedText.Replace("[" + localisedWord.Key + "]", localisedWord.Key);
                }

                foreach (var data in DataTexts)
                {
                    localisedText = localisedText.Replace("[" + data.Key + "]", data.Value);
                }

                textComp.text = localisedText;
            }

            #region [Inspector]

            [HorizontalGroup("Buts"),Button("Reset Format")]
            void TextsToFormat()
            {
                format = "";

                var targetedText = textComp.text;

                var splitTexts = new List<string>(targetedText.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                // string: "<alpha>AAA</alpha>"

                foreach (var splitTextFirst in splitTexts)
                {
                    var splitTextSecond = splitTextFirst.ReplaceTokens("<", ">", "|[", "]|");
                    // string: "|[alpha]|AAA|[/alpha]|"

                    var splitTextThird = splitTextSecond.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    // array: [alpha], AAA, [/alpha],

                    for (int i = 0; i < splitTextThird.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(splitTextThird[i])) continue;

                        Debug.Log(splitTextThird[i]);
                        splitTextThird[i] = splitTextThird[i].Contains("[")
                            ? splitTextThird[i].ReplaceTokens("[", "]", "<", ">")
                            : "[" + splitTextThird[i] + "]";
                    }
                    // array: <alpha>, [AAA], </alpha>

                    foreach (var finalText in splitTextThird)
                        format += finalText;
                }
            }


#if UNITY_EDITOR

            [HorizontalGroup("Buts"),Button("Create Texts")]
            void FormatToTexts()
            {
                texts = new List<TextFormatInfo>();
                var indexedTexts = format.ExtractAll("[", "]", suppressWarning: true);
                foreach (var text in indexedTexts)
                {
                    texts.Add( new TextFormatInfo(
                        text, 
                        text,
                        TextFormatInfo.TextType.Localise
                        ));
                }
            }
#endif
            #endregion

        }

        #endregion

        [SerializeField, ListDrawerSettings(DraggableItems = false, HideRemoveButton = true, HideAddButton = true)]
        List<TextDataMono> monos = new List<TextDataMono>();

        [SerializeField, ListDrawerSettings(DraggableItems = false)]
        [ShowIf("@"+nameof(formats)+ ".Count>0")]
        List<TextDataFormat> formats = new List<TextDataFormat>();
        public List<TextDataFormat> Formats { get { return formats; } }

        [SerializeField]
        CSVFile csv;


        #region [Data Handlers]

        List<TextData> textDatas = new List<TextData>();

        #endregion


        void Awake()
        {
            if (GameManager.Instance != null)
            {
                LocaliseTexts();
            }

            textDatas.AddRange(monos);
            textDatas.AddRange(formats);
        }

        public void LocaliseTexts()
        {
            if (csv.textAsset == null) return;

            var localisedTexts = CSVUtility.GetColumn(csv.textAsset, GameManager.Instance.LanguageCode);
            foreach (var textData in textDatas)
            {
                textData.LocaliseText(localisedTexts);
            }
        }

        public void AddTextDataFormat(TextDataFormat format)
        {
            if(formats.Find(f=>f.textComp == format.textComp)==null)
                formats.Add(format);
        }

        public void SetFormatDataText(string formatID, Dictionary<string,string> dataTexts, bool isRelocalising = true)
        {
            var foundFormat = formats.Find(f=>f.id == formatID);
            if (foundFormat != null)
            {
                foundFormat.DataTexts = dataTexts;
                var localisedTexts = CSVUtility.GetColumn(csv.textAsset, GameManager.Instance.LanguageCode);
                if (isRelocalising) foundFormat.LocaliseText(localisedTexts);
            }
        }

        #region [Inspector]

#if UNITY_EDITOR


        [SerializeField, FolderPath, LabelWidth(50), LabelText("Folder")]
        string saveFolder = "Assets/Contents/Localisations/TextsLocaliser";

        [Title("Controls")]
        [Button("Setup", ButtonSizes.Large), GUIColor("@Encore.Utility.ColorUtility.paleGreen")]
        [ShowIf("@"+nameof(monos)+ ".Count==0")]
        void SetupAll()
        {
            SetupTextDatas();
            SetupCSV();
        }

        void SetupTextDatas()
        {
            monos = new List<TextDataMono>();

            var texts = GetComponentsInChildren<TextMeshProUGUI>();
            int textIndex = 0;

            foreach (var textComp in texts)
            {
                var textGOPath = GetGOPath(textComp.transform);
                monos.Add(new TextDataMono(
                    this,
                    textComp,
                    new Text(textGOPath, textComp.text)
                    ));
                textIndex++;

                string GetGOPath(Transform targetTransform, string separatorToken = "/")
                {
                    var path = GetAncestorPath("", targetTransform) + targetTransform.gameObject.name;

                    string GetAncestorPath(string pathSoFar, Transform currentTransform)
                    {
                        if (currentTransform.parent == this.transform)
                            return "";

                        if (currentTransform.parent != null)
                        {
                            pathSoFar = pathSoFar.ReplaceFirst(separatorToken, "");

                            return GetAncestorPath(pathSoFar, currentTransform.parent) + currentTransform.parent.gameObject.name + separatorToken + pathSoFar;
                        }
                        else
                            return "";
                    }

                    return path;
                }
            }
        }

        void SetupCSV()
        {
            var csvString = ConvertTextDatasToCSV();

            // Save file
            System.IO.Directory.CreateDirectory(saveFolder);
            var filePath = saveFolder + "/" + gameObject.name + ".csv";
            System.IO.File.WriteAllText(filePath, csvString);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            csv = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);

            Debug.Log("Updated:\n\n" + csv);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        [Button("Update", ButtonSizes.Large), GUIColor("@Encore.Utility.ColorUtility.paleGreen")]
        [ShowIf("@" + nameof(monos) + ".Count>0")]
        public void UpdateTexts()
        {
            var texts = GetComponentsInChildren<TextMeshProUGUI>();
            int index = 0;
            var monoToDelete = new List<int>();
            foreach (var textComp in texts)
            {
                var foundMono = monos.Find(mono => mono.textComp == textComp);
                if (foundMono != null)
                {
                    foundMono.text.text = textComp.text;
                }
                else
                {
                    monoToDelete.Add(index);
                }
                index++;
            }

            foreach (var deleteIndex in monoToDelete)
                monos.RemoveAt(deleteIndex);

            UpdateCSV();
        }

        [Button("Update CSV"), GUIColor("@Encore.Utility.ColorUtility.paleGreen")]
        public void UpdateCSV()
        {
            if (csv.textAsset == null) return;

            var newCSVString = ConvertTextDatasToCSV();

            char lineSeparator = '\n';
            char surround = '"';
            char fieldSeparator = ',';
            string[] lines = newCSVString.Split(lineSeparator);

            var updatedCSVString = "";
            foreach (var line in lines)
            {
                var targetedRowHeader = line.SplitHalf(fieldSeparator.ToString()).Item1;
                targetedRowHeader = targetedRowHeader.ReplaceTokens(surround.ToString(), surround.ToString(), "", "");
                var newCSVRowTexts = CSVUtility.GetRow(newCSVString, targetedRowHeader);
                var oldCSVRowTexts = CSVUtility.GetRow(csv.textAsset, targetedRowHeader);

                var updatedColumns = new List<string>();
                foreach (var column in newCSVRowTexts)
                {
                    var columnHeader = column.Key;
                    var newText = column.Value;

                    updatedColumns.Add(string.IsNullOrEmpty(newText)
                        ? oldCSVRowTexts.Get(columnHeader, "")
                        : newText);

                }
                updatedCSVString += CSVUtility.WriteRow(newCSVRowTexts.Count, updatedColumns);
            }

            var filePath = UnityEditor.AssetDatabase.GetAssetPath(csv.textAsset.GetInstanceID());
            System.IO.File.WriteAllText(filePath, updatedCSVString);

            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log("Updated:\n\n" + csv);
        }

        string ConvertTextDatasToCSV()
        {
            // Write headers
            var headerNames = new List<string>() { "key" };
            var languageCodes = Enum.GetNames(typeof(LocalisationSystem.Language));
            headerNames.AddRange(languageCodes);
            var csvString = CSVUtility.WriteRow(headerNames.Count, headerNames);

            // Write rows for each mono
            foreach (var mono in monos)
            {
                if (!mono.isLocalised) continue;
                csvString += CSVUtility.WriteRow(headerNames.Count, new List<string>() { mono.text.index, mono.text.text });
            }

            foreach (var format in formats)
            {
                if (!format.isLocalised) continue;
                foreach (var text in format.texts)
                {
                    if (text.textType == TextDataFormat.TextFormatInfo.TextType.Localise)
                        csvString += CSVUtility.WriteRow(headerNames.Count, new List<string>() { text.index, text.text });
                }
            }

            return csvString;
        }

#endif
        #endregion

    }
}
