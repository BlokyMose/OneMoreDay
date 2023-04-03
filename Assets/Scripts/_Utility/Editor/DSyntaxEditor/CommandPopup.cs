using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor.DSyntaxEditor
{
    public class CommandPopup : PopupWindowContent
    {

        #region [Editor]

        public CommandPopup(DSyntaxEditor dSyntaxEditor, int commandIndex)
        {
            this.dSyntaxEditor = dSyntaxEditor;
            this.commandIndex = commandIndex;
        }

        #endregion


        #region [Components]

        DSyntaxEditor dSyntaxEditor;
        int commandIndex;

        #endregion


        #region [Properties]

        Color colorAdd = Color.green;
        Color colorDel = new Color(0.9f, 0.43f, 0.33f, 1f);
        Color colorMove = Color.yellow;
        Color colorDisabled = Color.gray;

        #endregion


        #region [Data handlers]

        Color defaultColor;

        #endregion



        public override void OnGUI(Rect rect)
        {
            defaultColor = GUI.color;
            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();
            MakeAddButUp();
            EditorGUILayout.LabelField("\u2191", labelStyle, GUILayout.Width(rect.width * 0.2f));
            MakeMoveButUp();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            MakeDelBut();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            MakeAddButDown();
            EditorGUILayout.LabelField("\u2193", labelStyle, GUILayout.Width(rect.width * 0.2f));
            MakeMoveButDown();
            EditorGUILayout.EndHorizontal();

            void MakeAddButUp()
            {
                GUI.color = colorAdd;
                if (GUILayout.Button("Add", GUILayout.Width(rect.width * 0.4f)))
                {
                    dSyntaxEditor.AddCommand(commandIndex);
                    commandIndex++;
                }
                GUI.color = defaultColor;
            }

            void MakeMoveButUp()
            {
                GUI.color = colorMove;
                if (GUILayout.Button("Move", GUILayout.Width(rect.width * 0.4f)))
                {
                    dSyntaxEditor.MoveCommand(commandIndex, commandIndex - 1);
                    commandIndex--;
                }
                GUI.color = defaultColor;
            }

            void MakeDelBut()
            {
                GUI.color = colorDel;
                if(GUILayout.Button("Delete", GUILayout.Width(rect.width)))
                {
                    dSyntaxEditor.DeleteCommand(commandIndex);
                    editorWindow.Close();
                }
                GUI.color = defaultColor;
            }

            void MakeAddButDown()
            {
                GUI.color = Color.green;
                if (GUILayout.Button("Add", GUILayout.Width(rect.width * 0.4f)))
                {
                    dSyntaxEditor.AddCommand(commandIndex + 1);
                }
                GUI.color = defaultColor;
            }

            void MakeMoveButDown()
            {
                GUI.color = colorMove;
                if (GUILayout.Button("Move", GUILayout.Width(rect.width * 0.4f)))
                {
                    dSyntaxEditor.MoveCommand(commandIndex, commandIndex + 1);
                    commandIndex++;
                }
                GUI.color = defaultColor;
            }
        }
    }
}
