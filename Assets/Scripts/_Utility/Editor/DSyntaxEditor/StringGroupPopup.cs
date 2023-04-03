using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Encore.Editor.DSyntaxEditor.DSyntaxEditor;

namespace Encore.Editor.DSyntaxEditor
{
    public class StringGroupPopup : PopupWindowContent
    {
        #region [Editor]

        public StringGroupPopup(Dictionary<string,StringGroup> stringGroups, DSyntaxEditor dSyntaxEditor, string chosenGroup, Action<string> onSelected)
        {
            this.stringGroups = stringGroups;
            this.dSyntaxEditor = dSyntaxEditor;
            this.chosenGroup = chosenGroup;
            this.OnSelected = onSelected;
        }

        #endregion


        #region [Data Handlers]

        public DSyntaxEditor dSyntaxEditor;
        public Dictionary<string,StringGroup> stringGroups = new Dictionary<string,StringGroup>();
        public Vector2 scrollViewPosGroups;
        public Vector2 scrollViewPosWords;
        public string chosenGroup;
        public Color defaultColor;
        public Color defaultContentColor;
        public string search;
        public Action<string> OnSelected;

        #endregion

        public override void OnGUI(Rect rect)
        {
            defaultColor = GUI.color;
            defaultContentColor = GUI.contentColor;
            MakeStringGroupsList();
            MakeWords();
            MakeSearchLabel();

            void MakeStringGroupsList()
            {
                scrollViewPosGroups = GUILayout.BeginScrollView(scrollViewPosGroups, GUILayout.Height(40));
                GUILayout.BeginHorizontal();

                GUILayout.Label("Groups:");

                // Put the chosen group's button first
                GUI.color = Color.green;
                if (GUILayout.Button(chosenGroup))
                    SelectStringGroup(chosenGroup);
                GUI.color = defaultColor;

                foreach (var sg in stringGroups)
                    if (sg.Key != chosenGroup)
                        if (GUILayout.Button(sg.Key))
                            SelectStringGroup(sg.Key);

                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }

            void MakeWords()
            {
                GUILayout.BeginHorizontal();
                search = GUILayout.TextField(search);
                GUILayout.EndHorizontal();

                scrollViewPosWords = GUILayout.BeginScrollView(scrollViewPosWords);

                if (!string.IsNullOrEmpty(chosenGroup))
                {
                    foreach (var word in stringGroups[chosenGroup].sgWords)
                    {
                        var buttonLabel = word.word;
                        if(word is SGWordSynonym)
                        {
                            buttonLabel = "";
                            foreach (var synonym in (word as SGWordSynonym).synonyms)
                                buttonLabel += synonym + " : ";
                            buttonLabel = buttonLabel[0..^2];
                        }

                        if (string.IsNullOrEmpty(search) || buttonLabel.Contains(search, System.StringComparison.CurrentCultureIgnoreCase))
                        {
                            var style = new GUIStyle(GUI.skin.button);
                            style.alignment = TextAnchor.MiddleLeft;
                            style.padding.left = 10;
                            if (GUILayout.Button(buttonLabel, style))
                                SelectWord(word.GetDisplayWord());
                        }
                    }
                }

                GUILayout.EndScrollView();

            }

            void MakeSearchLabel()
            {
                if (string.IsNullOrEmpty(search))
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.fontStyle = FontStyle.Italic;
                    var placeholderRect = new Rect(5, 40, 100, 30);
                    GUI.contentColor = Color.gray;
                    GUI.Label(placeholderRect, "search", style);
                    GUI.contentColor = defaultContentColor;
                }

            }
        }

        void SelectStringGroup(string stringGroupName)
        {
            chosenGroup = stringGroupName;
        }

        void SelectWord(string word)
        {
            OnSelected(word);
            editorWindow.Close();
        }
    }
}
