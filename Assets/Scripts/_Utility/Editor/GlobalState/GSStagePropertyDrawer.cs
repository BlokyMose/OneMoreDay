using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Encore.SceneMasters;
using Encore.Utility;

namespace Encore.Editor
{
    /// <summary>
    /// [GENERAL IDEA]<br></br>
    /// Helps control stage's property and GSO's stage actvations that is connected to this stage
    /// </summary>
    [CustomPropertyDrawer(typeof(GlobalState.Stage))]
    public class GSStagePropertyDrawer : PropertyDrawer
    {
        int rowCount = 0;
        const float rowHeight = 20f;
        bool openingGSOs = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return rowCount * 25;
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
            SerializedProperty stageName = property.FindPropertyRelative(nameof(stageName));

            EditorGUI.BeginProperty(position, label, property);

            #region [First Row]

            // Dropdown arrow button
            var gsoDropdownStyle = new GUIStyle(GUI.skin.label);
            var gsoDropdownRect = new Rect(currentX, currentY, 20, rowHeight);
            GUI.color = Color.gray;
            if (GUI.Button(gsoDropdownRect, openingGSOs ? "\u25BC" : "\u25BA", gsoDropdownStyle)) 
                openingGSOs = !openingGSOs;
            GUI.color = defaultColor;
            currentX += gsoDropdownRect.width;

            // StageName text field
            var stageNameRect = new Rect(currentX, currentY, availableWidth*0.9f, rowHeight);
            stageName.stringValue = EditorGUI.TextField(stageNameRect, stageName.stringValue);

            // StageName placeholder label
            if (string.IsNullOrEmpty(stageName.stringValue))
            {
                GUI.color = Color.gray;
                var stageNameLabelStyle = new GUIStyle(GUI.skin.label);
                stageNameLabelStyle.fontStyle = FontStyle.Italic;
                EditorGUI.LabelField(stageNameRect, "stageName", stageNameLabelStyle);
                GUI.color = defaultColor;
            }

            #endregion

            currentY += stageNameRect.height + 5;
            currentX = position.x;
            rowCount++;

            #region [Second Row]

            // GSO's activations list
            if (openingGSOs)
            {
                var gs = property.serializedObject.targetObject as GlobalState;

                if (gs.objects.Count > 0)
                {
                    foreach (var gso in gs.objects)
                    {
                        var foundAct = gso.activations.Find(act => act.gs == gs);
                        if (foundAct != null)
                        {
                            var foundStageAct = foundAct.stageActivations.Find(act => act.stage.stageName == stageName.stringValue);
                            if (foundStageAct!=null)
                            {
                                // GSO name button
                                var gsoRect = new Rect(currentX, currentY, availableWidth * 0.5f, rowHeight);
                                var gsoStyle = new GUIStyle(GUI.skin.label);
                                var gsoName = gso.name.ReplaceFirst("GSO_", "");
                                if (GUI.Button(gsoRect, gsoName, gsoStyle))
                                {
                                    EditorGUIUtility.PingObject(gso);
                                }
                                currentX += gsoRect.width;

                                // Activation mode enum popup
                                GUI.color = GSOStageActivationPropertyDrawer.GetActivationColor(foundStageAct.activationMode);
                                var stageActivationRect = new Rect(currentX, currentY, availableWidth*0.5f, rowHeight);
                                foundStageAct.activationMode = (GlobalStateObject.ActivationMode) EditorGUI.EnumPopup(stageActivationRect, foundStageAct.activationMode);
                                GUI.color = defaultColor;

                                currentX = position.x;
                                currentY += gsoRect.height + 5;
                                rowCount++;
                            }
                        }
                    }
                }
                else
                {
                    // No Objects Label
                    var noObjectsRect = new Rect(currentX, currentY, availableWidth, rowHeight);
                    EditorGUI.LabelField(noObjectsRect, "No objects to show");
                    rowCount++;
                }
            }

            #endregion

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
    }
}
