using Encore.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor.CSVEditor
{
    public class CSVEditor : EditorWindow
    {
        #region [Editor]

        [MenuItem("Tools/CSV Editor")]
        public static void OpenWindow()
        {
            GetWindow<CSVEditor>("CSV").Show();
        }

        public static void OpenWindow(TextAsset textAsset)
        {
            var window = GetWindow<CSVEditor>("CSV");
            window.csvFile = textAsset;
            window.Show();
        }

        #endregion

        #region [Classes]

        public enum ViewMode { Fit, Manual, Uniform }

        public enum ColRow { Col, Row}

        public class HeaderSlider
        {
            public int index;
            /// <summary>Height or Width</summary>
            public float length;
            public float position;

            public HeaderSlider(int index, float length, float position)
            {
                this.index = index;
                this.length = length;
                this.position = position;
            }

            public HeaderSlider(HeaderSlider deepCopyHeaderSlide)
            {
                index = deepCopyHeaderSlide.index;
                length = deepCopyHeaderSlide.length;
                position = deepCopyHeaderSlide.position;
            }
        }

        #endregion

        #region [Vars: Properties]

        const int columnHeaderHeight = 30;
        public const int minimalColLength = 100;
        public const int minimalRowLength = 30;


        #endregion

        #region [Vars: Data Handlers]

        #region [CSV File]

        public TextAsset csvFile;
        TextAsset currentCSVFile;

        #endregion

        #region [Edit ColRow]

        ColRow editColRow = ColRow.Col;
        int rowAt = -1;
        int colAt = -1;

        #endregion

        #region [Cells]

        public List<string> currentCells = new List<string>();
        int currentRowsCount = 0;
        int currentColumnsCount { get { return currentCells.Count/currentRowsCount; } }
        Vector2 scrollViewPos;

        #endregion

        #region [Header Sliders]

        List<HeaderSlider> colHeaderSliders = new List<HeaderSlider>();
        public List<HeaderSlider> GetColHeaderSliders() { return colHeaderSliders; }
        List<HeaderSlider> rowHeaderSliders = new List<HeaderSlider>();
        public List<HeaderSlider> GetRowHeadersSliders() { return rowHeaderSliders; }

        #endregion

        #region [View Mode]

        ViewMode viewMode = ViewMode.Fit;
        public ViewMode GetViewMode() { return viewMode; }

        ColRow manualColRow = ColRow.Col;
        int manualColRowIndex = 0;

        int uniformColLength = 200;
        int uniformRowLength = 30;

        #endregion

        #endregion

        private void OnGUI()
        {
            var horizontalGroupsCount = 0;

            MakeCSVFileControllers();
            if (csvFile != null)
            {
                MakeColRowEditAndViewMode();
                MakeCSVTable();
            }

            Undo.RecordObject(this, "csv");

            void MakeCSVFileControllers()
            {
                GUILayout.BeginHorizontal();
                csvFile = (TextAsset)EditorGUILayout.ObjectField(csvFile, typeof(TextAsset), true);
                GUILayout.Space(20);
                MakeRegenerateBut();
                MakeSaveBut();
                horizontalGroupsCount++;
                GUILayout.EndHorizontal();


                void MakeRegenerateBut()
                {
                    if (GUILayout.Button("Reset", GUILayout.Width(100)))
                    {
                        currentCells.Clear();
                        colHeaderSliders = new List<HeaderSlider>();
                        rowHeaderSliders = new List<HeaderSlider>();
                    }
                }

                void MakeSaveBut()
                {
                    var defaultColor = GUI.backgroundColor;
                    var guiStyle = new GUIStyle(GUI.skin.button);
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Save", GUILayout.Width(100)))
                    {
                        var newCSV = "";
                        var columnsCount = currentCells.Count / currentRowsCount;
                        for (int r = 0; r < currentRowsCount; r++)
                        {
                            var columns = new List<string>();
                            for (int c = 0; c < columnsCount; c++)
                            {
                                columns.Add(currentCells[r * columnsCount + c]);
                            }
                            newCSV += CSVUtility.WriteRow(columnsCount, columns);
                        }

                        var filePath = AssetDatabase.GetAssetPath(csvFile.GetInstanceID());
                        System.IO.File.WriteAllText(filePath, newCSV);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log("Updated:\n\n" + newCSV);

                    }

                    GUI.backgroundColor = defaultColor;
                }

            }

            void MakeColRowEditAndViewMode()
            {
                GUILayout.BeginHorizontal();
                var defaultBackgroundColor = GUI.backgroundColor;

                #region [ColRow Edit]

                editColRow = (ColRow)EditorGUILayout.EnumPopup(editColRow, GUILayout.Width(50));
                if (editColRow == ColRow.Row)
                {
                    rowAt = EditorGUILayout.IntField(rowAt, GUILayout.Width(25));

                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Add", GUILayout.Width(50)))
                    {
                        AddRowAt(rowAt);
                    }
                    GUI.backgroundColor=defaultBackgroundColor;

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Del", GUILayout.Width(50)))
                    {
                        DelRowAt(rowAt);
                    }
                    GUI.backgroundColor = defaultBackgroundColor;

                }
                else if (editColRow == ColRow.Col)
                {
                    colAt = EditorGUILayout.IntField(colAt, GUILayout.Width(25));

                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Add", GUILayout.Width(50)))
                    {
                        AddColumnAt(colAt);
                    }
                    GUI.backgroundColor = defaultBackgroundColor;

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("Del", GUILayout.Width(50)))
                    {
                        DelColumnAt(colAt);
                    }
                    GUI.backgroundColor = defaultBackgroundColor;
                }

                #endregion

                GUILayout.Space(25);

                #region [View Mode]

                EditorGUILayout.LabelField("View", GUILayout.Width(30));
                viewMode = (ViewMode)EditorGUILayout.EnumPopup(viewMode, GUILayout.Width(65));
                if (viewMode == ViewMode.Uniform)
                {
                    EditorGUILayout.LabelField("Col", GUILayout.Width(30));
                    uniformColLength = EditorGUILayout.IntField(uniformColLength, GUILayout.Width(30));
                    EditorGUILayout.LabelField("Row", GUILayout.Width(30));
                    uniformRowLength = EditorGUILayout.IntField(uniformRowLength, GUILayout.Width(30));
                }
                else if (viewMode == ViewMode.Manual)
                {
                    if (currentColumnsCount > 0)
                    {
                        manualColRow = (ColRow)EditorGUILayout.EnumPopup(manualColRow, GUILayout.Width(50));
                        manualColRowIndex = EditorGUILayout.IntField(manualColRowIndex, GUILayout.Width(25));
                        EditorGUILayout.LabelField("Length", GUILayout.Width(45));
                        if (manualColRow == ColRow.Col)
                        {
                            if(colHeaderSliders.GetAt(manualColRowIndex) != null)
                                colHeaderSliders[manualColRowIndex].length = EditorGUILayout.FloatField(colHeaderSliders[manualColRowIndex].length, GUILayout.Width(50));
                        }
                        else if (manualColRow == ColRow.Row)
                        {
                            if(rowHeaderSliders.GetAt(manualColRowIndex)!=null)
                                rowHeaderSliders[manualColRowIndex].length = EditorGUILayout.FloatField(rowHeaderSliders[manualColRowIndex].length, GUILayout.Width(50));
                        }
                    }
                }

                #endregion

                horizontalGroupsCount++;
                GUILayout.EndHorizontal();
            }

            void MakeCSVTable()
            {
                if (csvFile != currentCSVFile)
                {
                    currentCSVFile = csvFile;
                    currentCells.Clear();
                }

                #region [Process Rows Data]

                var rows = new List<List<string>>();
                var colHeadersCount = 0;
                float labelNumberWidth = 30;

                
                if (currentCells.Count == 0)
                {
                    rows = CSVUtility.GetAllByRow(csvFile);
                    colHeadersCount = rows[0].Count;
                }
                else
                {
                    rows.Clear();
                    colHeadersCount = currentColumnsCount;
                    for (int i = 0; i < currentRowsCount; i++)
                        rows.Add(currentCells.GetRange(i * colHeadersCount, colHeadersCount));
                }

                currentRowsCount = rows.Count;

                #endregion

                #region [Set Row & Col Header Sliders' Properties]

                float currentColHeadersSumLength = 0;
                for (int i = 0; i < colHeadersCount; i++)
                {
                    var defaultLength = position.width / colHeadersCount;
                    if (defaultLength < minimalColLength) defaultLength = minimalColLength;

                    switch (viewMode)
                    {
                        case ViewMode.Fit:
                            if (colHeaderSliders.Count <= i)
                                colHeaderSliders.Add(new HeaderSlider(i, defaultLength, currentColHeadersSumLength));
                            else
                                colHeaderSliders[i] = new HeaderSlider(i, defaultLength, currentColHeadersSumLength);
                            break;
                        case ViewMode.Manual:
                            if (colHeaderSliders.Count <= i)
                                colHeaderSliders.Add(new HeaderSlider(i, defaultLength, currentColHeadersSumLength));
                            else
                                colHeaderSliders[i].position = currentColHeadersSumLength;
                            break;
                        case ViewMode.Uniform:
                            if (colHeaderSliders.Count <= i)
                                colHeaderSliders.Add(new HeaderSlider(i, uniformColLength, currentColHeadersSumLength));
                            else
                                colHeaderSliders[i] = new HeaderSlider(i, uniformColLength, currentColHeadersSumLength);
                            break;
                    }

                    currentColHeadersSumLength += colHeaderSliders[i].length;
                }

                float currentRowHeadersSumLength = 0;
                for (int i = 0; i < currentRowsCount; i++)
                {
                    var defaultLength = (position.height - GetHeaderControllersHeight() - minimalRowLength - 10) / currentRowsCount;
                    if (defaultLength < minimalRowLength) defaultLength = minimalRowLength;

                    switch (viewMode)
                    {
                        case ViewMode.Fit:
                            if (rowHeaderSliders.Count <= i)
                                rowHeaderSliders.Add(new HeaderSlider(i, defaultLength, currentRowHeadersSumLength));
                            else
                                rowHeaderSliders[i] = new HeaderSlider(i, defaultLength, currentRowHeadersSumLength);
                            break;
                        case ViewMode.Manual:
                            if (rowHeaderSliders.Count <= i)
                                rowHeaderSliders.Add(new HeaderSlider(i, defaultLength, currentRowHeadersSumLength));
                            else
                                rowHeaderSliders[i].position = currentRowHeadersSumLength;
                            break;
                        case ViewMode.Uniform:
                            if (rowHeaderSliders.Count <= i)
                                rowHeaderSliders.Add(new HeaderSlider(i, uniformRowLength, currentRowHeadersSumLength));
                            else
                                rowHeaderSliders[i] = new HeaderSlider(i, uniformRowLength, currentRowHeadersSumLength);
                            break;
                    }

                    currentRowHeadersSumLength += rowHeaderSliders[i].length;
                }

                #endregion

                #region [Begin Scroll View]

                var scrollViewRect = new Rect(
                    0, 
                    GetHeaderControllersHeight(), 
                    position.width, 
                    position.height - GetHeaderControllersHeight());

                float viewWidth = 0;
                float viewHeight = 0;
                foreach (var colHeader in colHeaderSliders) 
                    viewWidth += colHeader.length;
                foreach (var rowHeader in rowHeaderSliders)
                    viewHeight += rowHeader.length;

                var viewRect = new Rect(0,0, viewWidth - 20, viewHeight + 30);

                scrollViewPos = GUI.BeginScrollView(scrollViewRect, scrollViewPos, viewRect);

                #endregion

                #region [Make Column Number Labels]

                for (int i = 0; i < colHeadersCount; i++)
                {
                    var rect = new Rect(
                        colHeaderSliders[i].position + labelNumberWidth, 
                        0, 
                        colHeaderSliders[i].length,
                        columnHeaderHeight);

                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if(Event.current.type == EventType.MouseDown)
                        {
                            int index = i;
                            PopupWindow.Show(rect, new CSVEditorColHeaderSliderPopup(index, this));
                        }
                    }

                    var labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    EditorGUI.LabelField(rect, i.ToString(), labelStyle);
                }

                for (int i = 0; i < colHeadersCount; i++)
                {
                    var rect = new Rect(
                        colHeaderSliders[i].position + colHeaderSliders[i].length + labelNumberWidth-5, 
                        0, 
                        10, 
                        columnHeaderHeight);
                    currentColHeadersSumLength += colHeaderSliders[i].length;
                    var labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    EditorGUI.LabelField(rect, "|", labelStyle);
                }


                horizontalGroupsCount++;

                #endregion

                int cellIndex = 0;
                int rowIndex = 0;
                foreach (var row in rows)
                {
                    int _rowIndex = rowIndex;

                    #region [Make Row Number Label]

                    var labelNumberRect = new Rect(
                        0, 
                        rowHeaderSliders[_rowIndex].position + columnHeaderHeight, 
                        labelNumberWidth,
                        rowHeaderSliders[_rowIndex].length);

                    if (labelNumberRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown)
                        {
                            int index = _rowIndex;
                            PopupWindow.Show(labelNumberRect, new CSVEditorRowHeaderSliderPopup(index, this));
                        }
                    }
                    var labelStyle = new GUIStyle(GUI.skin.label);
                    labelStyle.alignment = TextAnchor.MiddleRight;
                    labelStyle.onHover = new GUIStyleState { textColor = Color.gray };
                    EditorGUI.LabelField(labelNumberRect, _rowIndex.ToString()+" ", labelStyle);

                    #endregion

                    int columnIndex = 0;
                    foreach (var cell in row)
                    {
                        #region [Make Cell TextField]

                        int _columnIndex = columnIndex;
                        var rect = new Rect(
                            colHeaderSliders[_columnIndex].position + labelNumberWidth, 
                            rowHeaderSliders[_rowIndex].position + columnHeaderHeight, 
                            colHeaderSliders[_columnIndex].length,
                            rowHeaderSliders[_rowIndex].length);

                        // If cell hasn't been made, add new cell to the list
                        if (currentCells.Count <= cellIndex)
                            currentCells.Add(cell);

                        var textFieldStyle = new GUIStyle(GUI.skin.textArea);
                        textFieldStyle.alignment = TextAnchor.MiddleLeft;
                        currentCells[cellIndex] = EditorGUI.TextField(rect, currentCells[cellIndex], textFieldStyle);

                        #endregion

                        cellIndex++;
                        columnIndex++;
                    }
                    rowIndex++;
                }

                GUI.EndScrollView();
            }

            float GetHeaderControllersHeight()
            {
                return (horizontalGroupsCount + 1) * 20;
            }
        }

        #region [Methods: Row]

        public void AddRowAt(int index)
        {
            int _currentColumnsCount = currentColumnsCount;
            var startAddCellsAt = index > -1
                ? index * _currentColumnsCount
                : currentRowsCount * _currentColumnsCount;

            for (int i = 0; i < _currentColumnsCount; i++)
            {
                currentCells.Insert(startAddCellsAt + i, "");
            }

            currentRowsCount++;
            Repaint();
        }

        public void DelRowAt(int index)
        {
            if (index >= currentRowsCount) return;

            int _currentColumnsCount = currentColumnsCount;
            var startRemoveCellsAt = index > -1
                ? index * _currentColumnsCount
                : (currentRowsCount - 1) * _currentColumnsCount;

            for (int i = _currentColumnsCount - 1; i >= 0; i--)
                currentCells.RemoveAt(startRemoveCellsAt + i);

            var rowIndex = index > -1 && index < currentRowsCount
                ? index
                : currentRowsCount - 1;

            rowHeaderSliders.RemoveAt(rowIndex);

            currentRowsCount--;
            Repaint();
        }

        public void SwapRow(int indexFrom, int indexTo)
        {
            if (indexFrom < 0) indexFrom = currentRowsCount - 1;
            if (indexTo < 0) indexTo = currentRowsCount - 1;

            if (rowHeaderSliders.GetAt(indexFrom) == null || rowHeaderSliders.GetAt(indexTo) == null) return;

            var tempRow = new List<string>();
            var emptyCellsFrom = indexFrom;
            var swapWith = indexTo;
            for (int i = 0; i < currentColumnsCount; i++)
            {
                tempRow.Add(currentCells[emptyCellsFrom * currentColumnsCount + i]);
                currentCells[emptyCellsFrom * currentColumnsCount + i] = currentCells[swapWith * currentColumnsCount + i];
            }

            for (int i = 0; i < currentColumnsCount; i++)
            {
                currentCells[swapWith * currentColumnsCount + i] = tempRow[i];
            }

            var tempHeaderSlide = new HeaderSlider(rowHeaderSliders[indexFrom]);
            rowHeaderSliders[indexFrom] = new HeaderSlider(rowHeaderSliders[indexTo]);
            rowHeaderSliders[indexTo] = tempHeaderSlide;

            Repaint();
        }

        public void ChangeRowHeight(int index, float height)
        {
            var foundRow = rowHeaderSliders.GetAt(index);
            if (foundRow != null)
                foundRow.length = height;
            Repaint();
        }

        public float GetRowHeight(int index)
        {
            var foundRow = rowHeaderSliders.GetAt(index);
            if (foundRow != null)
                return foundRow.length;
            else
                return 0;
        }


        #endregion

        #region [Methods: Col]

        public void AddColumnAt(int index)
        {
            var addCellsEvery = index > -1
                ? index
                : currentColumnsCount;

            for (int i = currentRowsCount - 1; i >= 0; i--)
                currentCells.Insert(currentColumnsCount * i + addCellsEvery, "");
            Repaint();
        }

        public void DelColumnAt(int index)
        {
            if (index >= currentColumnsCount) return;

            var _currentColumnsCount = currentColumnsCount;
            var removeCellsEvery = index > -1
                ? index
                : _currentColumnsCount - 1;

            for (int i = currentRowsCount - 1; i >= 0; i--)
                currentCells.RemoveAt(_currentColumnsCount * i + removeCellsEvery);

            var colIndex = index > -1 && index < _currentColumnsCount
                ? index
                : _currentColumnsCount - 1;

            colHeaderSliders.RemoveAt(colIndex);
            Repaint();
        }

        public void SwapColumn(int indexFrom, int indexTo)
        {
            if(indexFrom < 0) indexFrom = currentColumnsCount - 1;
            if(indexTo < 0) indexTo = currentColumnsCount - 1;

            if (colHeaderSliders.GetAt(indexFrom) == null || colHeaderSliders.GetAt(indexTo) == null) return;

            var tempCol = new List<string>();
            var emptyCellsEvery = indexFrom;
            var swapWithEvery = indexTo;
            for (int i = 0; i < currentRowsCount; i++)
            {
                tempCol.Add(currentCells[currentColumnsCount * i + emptyCellsEvery]);
                currentCells[currentColumnsCount * i + emptyCellsEvery] = currentCells[currentColumnsCount * i + swapWithEvery];
            }

            for (int i = 0; i < currentRowsCount; i++)
            {
                currentCells[currentColumnsCount * i + swapWithEvery] = tempCol[i];
            }

            var tempHeaderSlide = new HeaderSlider(colHeaderSliders[indexFrom]);
            colHeaderSliders[indexFrom] = new HeaderSlider(colHeaderSliders[indexTo]);
            colHeaderSliders[indexTo] = tempHeaderSlide;

            Repaint();
        }

        public void ChangeColumnWidth(int index, float width)
        {
            var foundCol = colHeaderSliders.GetAt(index);
            if (foundCol != null)
               foundCol.length = width;
            Repaint();
        }

        public float GetColumnWidth(int index)
        {
            var foundCol = colHeaderSliders.GetAt(index);
            if (foundCol != null)  
                return foundCol.length;
            else
                return 0;
        }

        #endregion
    }
}
