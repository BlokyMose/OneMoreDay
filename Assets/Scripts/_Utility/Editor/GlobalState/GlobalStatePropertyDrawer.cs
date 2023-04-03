//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using Encore.SceneMasters;

//namespace Encore.Editor
//{
//    [CustomPropertyDrawer(typeof(SceneMaster.GlobalState))]
//    public class GlobalStatePropertyDrawer : PropertyDrawer
//    {
//        int rowCount = 0;
//        const float rowHeight = 20f;

//        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//        {
//            return rowCount * 25;
//        }

//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            // Temp variables
//            rowCount = 0;
//            var currentX = position.x;
//            var currentY = position.y;
//            var availableWidth = position.width - 10;
//            var defaultColor = GUI.color;

//            // Get all the properties
//            SerializedProperty gsid = property.FindPropertyRelative(nameof(gsid));
//            SerializedProperty currentStage = property.FindPropertyRelative(nameof(currentStage));
//            SerializedProperty sceneMaster = property.FindPropertyRelative(nameof(sceneMaster));
//            var sceneMasterObj = sceneMaster.objectReferenceValue as SceneMaster;


//            EditorGUI.BeginProperty(position, label, property);


//            #region [Row 1]

//            // GSID
//            var gsidRect = new Rect(currentX, currentY, availableWidth*0.75f, rowHeight);
//            gsid.objectReferenceValue = EditorGUI.ObjectField(gsidRect, gsid.objectReferenceValue, typeof(GlobalStateID), false);
//            currentX += gsidRect.width + 5;

//            // Inline button
//            var gsidButRect = new Rect(currentX, currentY, availableWidth * 0.25f, rowHeight);
//            if (gsid.objectReferenceValue != null)
//            {
//                if(GUI.Button(gsidButRect, "Apply"))
//                {
//                    sceneMasterObj.ApplyState(gsid.objectReferenceValue as GlobalStateID, currentStage.stringValue);
//                }
//            }
//            else
//            {
//                if (GUI.Button(gsidButRect, "New"))
//                {
//                    gsid.objectReferenceValue = SceneMaster.GlobalState.CreateGlobalStateID(sceneMasterObj);
//                }
//            }

//            #endregion

//            currentX = position.x;
//            currentY += gsidRect.height + 5;
//            rowCount++;


//            #region [Row 2]

//            // Current Stage
//            if (gsid.objectReferenceValue != null)
//            {
//                var targetStageRect = new Rect(currentX, currentY, availableWidth, rowHeight);
//                var gsidStages = (gsid.objectReferenceValue as GlobalStateID).stages;

//                if (gsidStages.Count > 0)
//                {
//                    var stages = new List<string>();
//                    var targetStageIndex = 0;
//                    int index = 0;
//                    foreach (var stage in gsidStages)
//                    {
//                        stages.Add(stage.stageName);
//                        if (stage.stageName == currentStage.stringValue)
//                            targetStageIndex = index;
//                        index++;
//                    }

//                    targetStageIndex = EditorGUI.Popup(targetStageRect, "Current Stage", targetStageIndex, stages.ToArray());

//                    currentStage.stringValue = stages[targetStageIndex];
//                }
//                else
//                {
//                    EditorGUI.LabelField(targetStageRect, "No stage detected");
//                }

//                rowCount++;
//            }

//            #endregion

//            property.serializedObject.ApplyModifiedProperties();
//            EditorGUI.EndProperty();
//        }
//    }
//}
