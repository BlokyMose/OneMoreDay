using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Encore.Editor
{
    public class ShowPopupExample : EditorWindow
    {
        //[MenuItem("Example/ShowPopup Example")]
        static void Init()
        {
            ShowPopupExample window = ScriptableObject.CreateInstance<ShowPopupExample>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("This is an example of EditorWindow.ShowPopup", EditorStyles.wordWrappedLabel);
            GUILayout.Space(70);
            if (GUILayout.Button("Agree!")) this.Close();
        }
    }
}
