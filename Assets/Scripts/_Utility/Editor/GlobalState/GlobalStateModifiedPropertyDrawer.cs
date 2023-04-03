using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Encore.SceneMasters;

namespace Encore.Editor
{
    [CustomPropertyDrawer(typeof(GlobalStateSetter.GlobalStateModified))]

    public class GlobalStateModifiedPropertyDrawer : PropertyDrawer
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
            SerializedProperty gs = property.FindPropertyRelative(nameof(gs));
            SerializedProperty targetStage = property.FindPropertyRelative(nameof(targetStage));

            EditorGUI.BeginProperty(position, label, property);

            // GS object field
            var gsRect = new Rect(currentX, currentY, availableWidth, rowHeight);
            gs.objectReferenceValue = (GlobalState)EditorGUI.ObjectField(gsRect, gs.objectReferenceValue, typeof(GlobalState), false);
            
            currentY += gsRect.height;
            currentX = position.x;
            rowCount++;


            // TargetStage popup
            if (gs.objectReferenceValue != null)
            {
                var targetStageRect = new Rect(currentX, currentY, availableWidth, rowHeight);
                var stages = (gs.objectReferenceValue as GlobalState).stages;

                if (stages.Count > 0)
                {
                    var stagesNames = new List<string>();
                    var targetStageIndex = 0;
                    var index = 0; 
                    foreach (var stage in stages)
                    {
                        stagesNames.Add(stage.stageName);
                        if (stage.stageName == targetStage.stringValue)
                            targetStageIndex = index;
                        index++;
                    }

                    targetStageIndex = EditorGUI.Popup(targetStageRect, "Target Stage", targetStageIndex, stagesNames.ToArray());

                    targetStage.stringValue = stagesNames[targetStageIndex];
                }
                else
                {
                    EditorGUI.LabelField(targetStageRect, "No stage detected");
                }

                rowCount++;
            }


            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
    }
}
