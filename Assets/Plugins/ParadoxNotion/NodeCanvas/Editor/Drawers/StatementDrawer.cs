#if UNITY_EDITOR

using ParadoxNotion.Design;
using UnityEngine;
using NodeCanvas.DialogueTrees;

namespace NodeCanvas.Editor
{

    ///<summary>A drawer for dialogue tree statements</summary>
    public class StatementDrawer : ObjectDrawer<Statement>
    {
        public override Statement OnGUI(GUIContent content, Statement instance) {
            if ( instance == null ) { instance = new Statement("..."); }
            instance.text = UnityEditor.EditorGUILayout.TextArea(instance.text, Styles.wrapTextArea, GUILayout.Height(100));
            instance.expression = (Expression)UnityEditor.EditorGUILayout.EnumPopup("Expression", instance.expression); //SAVE: Custom property
            instance.gesture = (Gesture)UnityEditor.EditorGUILayout.EnumPopup("Gesture", instance.gesture); // Custom property
            instance.textIndex = UnityEditor.EditorGUILayout.TextField("Text Index", instance.textIndex); // Custom property
            instance.duration = UnityEditor.EditorGUILayout.Slider("Duration", instance.duration, 1.0f, 10.0f);

            // Commented out because not needed in Encore Project
            //instance.audio = UnityEditor.EditorGUILayout.ObjectField("Audio File", instance.audio, typeof(AudioClip), false) as AudioClip;
            //instance.meta = UnityEditor.EditorGUILayout.TextField("Metadata", instance.meta);

            return instance;
        }
    }
}

#endif