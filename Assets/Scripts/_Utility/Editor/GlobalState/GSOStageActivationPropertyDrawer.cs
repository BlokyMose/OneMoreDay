using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Encore.SceneMasters;

namespace Encore.Editor
{
    [CustomPropertyDrawer(typeof(GlobalStateObject.Activation.StageActivation))]

    public class GSOStageActivationPropertyDrawer : PropertyDrawer
    {
        int rowCount = 0;
        const float rowHeight = 20f;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return rowCount * 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Temp variables
            rowCount = 0;
            var currentX = position.x;
            var currentY = position.y;
            var availableWidth = position.width - 10;
            var defaultColor = GUI.color;

            // Get all the properties
            SerializedProperty stage = property.FindPropertyRelative(nameof(stage));
            SerializedProperty stageName = stage.FindPropertyRelative(nameof(stageName));
            SerializedProperty activationMode = property.FindPropertyRelative(nameof(activationMode));

            EditorGUI.BeginProperty(position, label, property);

            // StageName
            var stageNameRect = new Rect(currentX, currentY, availableWidth*0.5f, rowHeight);
            EditorGUI.LabelField(stageNameRect, stageName.stringValue);
            currentX += stageNameRect.width + 5;

            // ActivationMode
            var activationModeRect = new Rect(currentX, currentY, availableWidth*0.5f, rowHeight);
            var activationModeTemp = (GlobalStateObject.ActivationMode) activationMode.enumValueIndex;
            GUI.color = GetActivationColor(activationModeTemp);
            activationModeTemp = (GlobalStateObject.ActivationMode) EditorGUI.EnumPopup(activationModeRect, activationModeTemp);
            activationMode.enumValueIndex = (int)activationModeTemp;
            GUI.color = defaultColor;

            rowCount++;

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public static Color GetActivationColor(GlobalStateObject.ActivationMode mode)
        {
            Color colorAsIs = Color.gray;
            Color colorActive = Color.green;
            Color colorInactive = Utility.ColorUtility.salmon;
            Color colorDestroyed = Color.yellow;

            switch (mode)
            {
                case GlobalStateObject.ActivationMode.AsIs: return colorAsIs; 
                case GlobalStateObject.ActivationMode.Active: return colorActive; 
                case GlobalStateObject.ActivationMode.Inactive: return colorInactive; 
                case GlobalStateObject.ActivationMode.Destroyed: return colorDestroyed; 
                default: return colorAsIs;
            }
        }
    }

}
