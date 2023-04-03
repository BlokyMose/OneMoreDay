using Encore.Dialogues;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor.DSyntaxEditor
{
    [CustomPropertyDrawer(typeof(DSyntaxString))]
    public class DSyntaxStringPropertyDrawer : PropertyDrawer
    {
        float scrollViewRectHeight;
        int rowCount;
        Vector2 scrollPos;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return scrollViewRectHeight + GetRowCountHeight(0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty textAsset = property.FindPropertyRelative(nameof(textAsset));
            SerializedProperty dSyntax = property.FindPropertyRelative(nameof(dSyntax));

            EditorGUI.BeginProperty(position, label, property);

            #region [First Row]

            var prefixLabelRect = new Rect(position);
            prefixLabelRect.height = 20;
            var afterPrefixLabelRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var textAssetRect = new Rect(afterPrefixLabelRect);
            textAssetRect.size = new Vector2(afterPrefixLabelRect.width * 0.6f, 20);
            textAsset.objectReferenceValue = EditorGUI.ObjectField(textAssetRect, textAsset.objectReferenceValue, typeof(TextAsset), false);

            var textAssetEditButRect = new Rect(position);
            textAssetEditButRect.x = textAssetRect.x + textAssetRect.width;
            textAssetEditButRect.size = new Vector2(afterPrefixLabelRect.width * 0.2f, 20);
            var textAssetEditButStyle = new GUIStyle(GUI.skin.button);
            textAssetEditButStyle.alignment = TextAnchor.MiddleCenter;
            if(textAsset.objectReferenceValue == null)
            {
                if (GUI.Button(textAssetEditButRect, "New"))
                {
                    CreateNewTextAsset(property);
                }
            }
            else
            {
                if (GUI.Button(textAssetEditButRect, "Sync"))
                {
                    SyncDSyntaxToTextAsset(property, dSyntax.stringValue);
                }
            }

            var dSyntaxEditorButRect = new Rect(position);
            dSyntaxEditorButRect.x = textAssetEditButRect.x + textAssetEditButRect.width;
            dSyntaxEditorButRect.size = new Vector2(afterPrefixLabelRect.width * 0.2f, 20);
            var dSyntaxEditorButStyle = new GUIStyle(GUI.skin.button);
            dSyntaxEditorButStyle.alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(dSyntaxEditorButRect, "\u2197"))
            {
                    DSyntaxEditor.OpenWindow(
                        textAsset: (TextAsset) textAsset.objectReferenceValue, 
                        OnSetDSyntax: (newDSyntax) => 
                        { 
                            dSyntax.stringValue = newDSyntax; 
                            property.serializedObject.ApplyModifiedProperties();
                        },
                        GetDSyntax: () => { return dSyntax.stringValue; }
                        );
            }

            rowCount = 1;

            #endregion

            #region [TextArea Row]

            var dSyntaxRect = new Rect(position);
            dSyntaxRect.y = GetRowCountHeight(position.y);
            var dSyntaxStyle = new GUIStyle(GUI.skin.textArea);
            dSyntaxStyle.padding = new RectOffset(5, 5, 5, 5);
            dSyntaxRect.height = dSyntaxStyle.CalcHeight(new GUIContent(dSyntax.stringValue), position.width);

            scrollViewRectHeight = dSyntaxRect.height > 120 ? 120 : dSyntaxRect.height;

            var scrollViewRect = new Rect(
                position.x,
                dSyntaxRect.y,
                position.width,
                scrollViewRectHeight);


            var viewRect = new Rect(
                position.x,
                dSyntaxRect.y,
                position.width-15,
                dSyntaxRect.height);

            scrollPos = GUI.BeginScrollView(scrollViewRect, scrollPos, viewRect);
            dSyntax.stringValue = EditorGUI.TextArea(dSyntaxRect, dSyntax.stringValue, dSyntaxStyle);
            GUI.EndScrollView();

            #endregion

            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }

        float GetRowCountHeight(float positionY)
        {
            return  (rowCount) * 25 + positionY;
        }

        
        string saveFolder = "Assets/Contents/SOAssets/DSyntaxAsset";

        void CreateNewTextAsset(SerializedProperty property)
        {
            // Get path
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string folderPath = saveFolder + "/" + sceneName + "/" + "DSyntaxTextAsset";
            System.IO.Directory.CreateDirectory(folderPath);
            string filePath = folderPath + "/" +  property.serializedObject.targetObject.name + ".txt";
            SerializedProperty dSyntax = property.FindPropertyRelative(nameof(dSyntax));
            string dSyntaxString = dSyntax.stringValue;
            System.IO.File.WriteAllText(filePath, dSyntaxString);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SerializedProperty textAsset = property.FindPropertyRelative(nameof(textAsset));
            textAsset.objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);

            Debug.Log("Created:\n\n" + filePath.Replace(saveFolder+"/", ""));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void SyncDSyntaxToTextAsset(SerializedProperty property, string dSyntax)
        {
            SerializedProperty textAsset = property.FindPropertyRelative(nameof(textAsset));
            string filePath = AssetDatabase.GetAssetPath(textAsset.objectReferenceInstanceIDValue);
            System.IO.File.WriteAllText(filePath, dSyntax);
            Debug.Log("Updated:\n\n" + filePath.Replace(saveFolder+"/",""));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
