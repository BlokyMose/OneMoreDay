using Encore.Localisations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor
{
    [CustomPropertyDrawer(typeof(CSVFile))]
    public class CSVFilePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            SerializedProperty textAsset = property.FindPropertyRelative(nameof(textAsset));
            var csvTextAssetRect = new Rect(position);
            csvTextAssetRect.width = position.width * 0.75f;
            textAsset.objectReferenceValue = EditorGUI.ObjectField(csvTextAssetRect, textAsset.objectReferenceValue, typeof(TextAsset), false);

            var csvEditorButRect = new Rect(position);
            csvEditorButRect.x = csvTextAssetRect.x + csvTextAssetRect.width;
            csvEditorButRect.width = position.width * 0.25f;
            if(GUI.Button(csvEditorButRect, "\u2197"))
            {
                CSVEditor.CSVEditor.OpenWindow((TextAsset) textAsset.objectReferenceValue);
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
    }
}
