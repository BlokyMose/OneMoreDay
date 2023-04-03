using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor
{
    public class TestingWindow : OdinEditorWindow
    {
        #region [Editor]

        [MenuItem("Tools/Test/Testing Window")]
        public static void OpenWindow()
        {
            GetWindow<TestingWindow>("Test").Show();
        }

        #endregion

        string testString = "";

        GUIStyle style = new GUIStyle();
        string objNames = "";

        protected override void OnGUI()
        {
            base.OnGUI();

            //var guiStyle = new GUIStyle(GUI.skin.textArea);

            //testString = EditorGUI.TextArea(new Rect(0, 100, position.width, position.height), testString, style == null ? guiStyle : style);



            for (int i = 0; i < 4; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 3; j++)
                {
                    EditorGUILayout.TextField("a");
                }
                GUILayout.EndHorizontal();
            }
        }

    }
}
