using UnityEditor;
using UnityEngine;

namespace MStudio
{
    [InitializeOnLoad]
    public class StyleHierarchy
    {
        static string[] dataArray;//Find ColorPalette GUID
        static string path;//Get ColorPalette(ScriptableObject) path
        static ColorPalette colorPalette;

        static StyleHierarchy()
        {
            dataArray = AssetDatabase.FindAssets("t:ColorPalette"); 

            if (dataArray != null)
            {    //We have only one color palette, so we use dataArray[0] to get the path of the file
                path = AssetDatabase.GUIDToAssetPath(dataArray[0]);

                colorPalette = AssetDatabase.LoadAssetAtPath<ColorPalette>(path);

                //Draw UI on top of each item in hierarchy
                EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindow;
            } 
        }

        private static void OnHierarchyWindow(int instanceID, Rect selectionRect)
        {
            UnityEngine.Object instance = EditorUtility.InstanceIDToObject(instanceID);

            if (instance != null)
            {
                for (int i = 0; i < colorPalette.colorDesigns.Count; i++)
                {
                    var design = colorPalette.colorDesigns[i];

                    //Check if the name of each gameObject is begin with keyChar in colorDesigns list.
                    if (instance.name == design.token)
                    {
                        //Draw a rectangle as a background, and set the color.
                        Rect overlayRect = new Rect(
                            selectionRect.x + design.backgroundRectOffset.x, 
                            selectionRect.y + design.backgroundRectOffset.y, 
                            selectionRect.width + 20, 
                            selectionRect.height);
                        EditorGUI.DrawRect(overlayRect, design.backgroundColor);

                        //Create a new GUIStyle based on the colorDesign
                        GUIStyle newStyle = new GUIStyle
                        {
                            alignment = design.textAlignment,
                            fontStyle = design.fontStyle,
                            normal = new GUIStyleState()
                            {
                                textColor = design.textColor,
                            },
                            padding = new RectOffset(design.textOffset.x,design.textOffset.y,0,0)
                        };

                        //Draw a label to show the name in newStyle.
                        string newName = string.IsNullOrWhiteSpace(design.newName) ? instance.name : design.newName;
                        EditorGUI.LabelField(selectionRect, newName, newStyle);
                    }
                }
            }
        }
    }
}
