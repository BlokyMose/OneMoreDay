using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor.CSVEditor
{
    public abstract class CSVEditorHeaderSliperPopup : PopupWindowContent
    {
        #region [Classes]

        public enum EditMode { Add, Del, Swap }

        public CSVEditorHeaderSliperPopup(int headerIndex, CSVEditor csvEditor)
        {
            this.csvEditor = csvEditor;
            this.headerIndex = headerIndex;
        }

        #endregion

        #region [Properties]

        public Color colorAdd = Color.green;
        public Color colorDel = new Color(0.9f, 0.43f, 0.33f, 1f);
        public Color colorSwap = Color.yellow;
        public Color colorDisabled = Color.gray;

        #endregion

        #region [Data Handlers]

        public CSVEditor csvEditor;
        public int headerIndex;
        public int swapTo = 0;
        public int addTo = 0;
        public int delTo = 0;
        public EditMode editMode = EditMode.Add;
        public Color defaultGUIColor;

        #endregion

        public override void OnGUI(Rect rect)
        {
            defaultGUIColor = GUI.color;

            MakeEditMode();

            switch (editMode)
            {
                case EditMode.Add:
                    MakeAdd();
                    break;

                case EditMode.Del:
                    MakeDel();
                    break;
                case EditMode.Swap:
                    MakeSwap();
                    break;
            }

            MakeLength();

            void MakeEditMode()
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label("Mode", GUILayout.Width(50));
                GUI.color = editMode == EditMode.Add ? colorAdd : colorDisabled;
                if (GUILayout.Button("Add"))
                {
                    SetEditMode(EditMode.Add);
                }
                GUI.color = defaultGUIColor;

                GUI.color = editMode == EditMode.Del ? colorDel : colorDisabled;
                if (GUILayout.Button("Del"))
                {
                    SetEditMode(EditMode.Del);
                }
                GUI.color = defaultGUIColor;

                GUI.color = editMode == EditMode.Swap ? colorSwap : colorDisabled;
                if (GUILayout.Button("Swap"))
                {
                    SetEditMode(EditMode.Swap);
                }
                GUI.color = defaultGUIColor;

                GUILayout.EndHorizontal();
            }

            void MakeAdd()
            {
                GUI.color = colorAdd;

                #region [Add]

                GUILayout.BeginHorizontal();
                GUILayout.Label("Add", GUILayout.Width(50));
                if (GUILayout.Button("|\u25C0", GUILayout.Width(25)))
                {
                    AddHeaderToFirst();
                }
                if (GUILayout.Button("\u25C0", GUILayout.Width(25)))
                {
                    AddHeaderToPrev();
                }
                if (GUILayout.Button("\u25B6", GUILayout.Width(25)))
                {
                    AddHeaderToNext();
                }
                if (GUILayout.Button("\u25B6|", GUILayout.Width(25)))
                {
                    AddHeaderToLast();
                }
                GUILayout.EndHorizontal();

                #endregion

                #region [Add to]

                GUILayout.BeginHorizontal();

                GUILayout.Label("Add to", GUILayout.Width(50));
                addTo = EditorGUILayout.IntField(addTo, GUILayout.Width(80));
                if (GUILayout.Button("Go", GUILayout.Width(25)))
                {
                    AddHeaderToIndex(addTo);
                }

                GUILayout.EndHorizontal();
                GUI.color = defaultGUIColor;

                #endregion

                GUILayout.Space(15);
            }

            void MakeDel()
            {
                GUI.color = colorDel;

                #region [Del]

                GUILayout.BeginHorizontal();
                GUILayout.Label("Del", GUILayout.Width(50));
                if (GUILayout.Button("|\u25C0", GUILayout.Width(25)))
                {
                    DelHeaderToFirst();
                }
                if (GUILayout.Button("This", GUILayout.Width(50)))
                {
                    DelHeaderThis();
                }
                if (GUILayout.Button("\u25B6|", GUILayout.Width(25)))
                {
                    DelHeaderToLast();
                }
                GUILayout.EndHorizontal();

                #endregion

                #region [Del to]

                GUILayout.BeginHorizontal();

                GUILayout.Label("Del to", GUILayout.Width(50));
                delTo = EditorGUILayout.IntField(delTo, GUILayout.Width(80));
                if (GUILayout.Button("Go", GUILayout.Width(25)))
                {
                    DelHeaderToIndex(delTo);
                }

                GUILayout.EndHorizontal();

                #endregion

                GUI.color = defaultGUIColor;

                GUILayout.Space(15);
            }

            void MakeSwap()
            {
                GUI.color = colorSwap;

                #region [Swap]

                GUILayout.BeginHorizontal();
                GUILayout.Label("Swap", GUILayout.Width(50));
                if (GUILayout.Button("|\u25C0", GUILayout.Width(25)))
                {
                    SwapHeaderToFirst();
                }
                if (GUILayout.Button("\u25C0", GUILayout.Width(25)))
                {
                    SwapHeaderToPrev();
                }
                if (GUILayout.Button("\u25B6", GUILayout.Width(25)))
                {
                    SwapHeaderToNext();
                }
                if (GUILayout.Button("\u25B6|", GUILayout.Width(25)))
                {
                    SwapHeaderToLast();
                }
                GUILayout.EndHorizontal();

                #endregion

                #region [Swap to]

                GUILayout.BeginHorizontal();

                GUILayout.Label("Swap to", GUILayout.Width(50));
                swapTo = EditorGUILayout.IntField(swapTo, GUILayout.Width(80));
                if (GUILayout.Button("Go", GUILayout.Width(25)))
                {
                    SwapHeaderToIndex(swapTo);
                }

                GUILayout.EndHorizontal();

                #endregion

                GUI.color = defaultGUIColor;

                GUILayout.Space(15);
            }

            void MakeLength()
            {
                GUILayout.BeginHorizontal();

                if (csvEditor.GetViewMode() == CSVEditor.ViewMode.Manual)
                {
                    GUILayout.Label("Length", GUILayout.Width(50));
                    SetLength(headerIndex, EditorGUILayout.FloatField(GetLength(headerIndex), GUILayout.Width(55)));
                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        AddLength();
                    }
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        ReduceLength();
                    }

                    GUILayout.EndHorizontal();
                    SetLength(headerIndex, GUILayout.HorizontalSlider(GetLength(headerIndex), GetMinimalLength(), 500));
                }
                else
                {
                    GUILayout.Label("Width is controlled by View");
                }

            }
        }

        #region [Utilities]

        protected virtual void SetEditMode(EditMode editMode)
        {
            this.editMode = editMode;
        }

        protected abstract float GetMinimalLength();

        #endregion

        #region [Add]

        protected abstract void AddHeaderToFirst();
        
        protected abstract void AddHeaderToPrev();
        
        protected abstract void AddHeaderToNext();

        protected abstract void AddHeaderToLast();
        
        protected abstract void AddHeaderToIndex(int index);
        

        #endregion

        #region [Delete]

        protected abstract void DelHeaderToFirst();
        

        protected abstract void DelHeaderThis();
        

        protected abstract void DelHeaderToLast();
        
        

        protected abstract void DelHeaderToIndex(int index);
        
        
        #endregion

        #region [Swap]

        protected abstract void SwapHeaderToFirst();
        
        protected abstract void SwapHeaderToPrev();
        
        protected abstract void SwapHeaderToNext();
        

        protected abstract void SwapHeaderToLast();
        

        protected abstract void SwapHeaderToIndex(int index);


        #endregion

        #region [Length]

        protected abstract void AddLength(float addBy = 50);
        protected abstract void ReduceLength(float reduceBy = 50);
        protected abstract void SetLength(int index, float length);
        protected abstract float GetLength(int index);


        #endregion
    }
}
