using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor.CSVEditor
{
    public class CSVEditorColHeaderSliderPopup : CSVEditorHeaderSliperPopup
    {
        public CSVEditorColHeaderSliderPopup(int headerIndex, CSVEditor csvEditor) : base(headerIndex, csvEditor)
        {
        }

        protected override float GetMinimalLength()
        {
            return CSVEditor.minimalColLength;
        }


        #region [Add]

        protected override void AddHeaderToFirst()
        {
            csvEditor.AddColumnAt(0);
            headerIndex++;
        }

        protected override void AddHeaderToPrev()
        {
            csvEditor.AddColumnAt(headerIndex);
            headerIndex++;
        }

        protected override void AddHeaderToNext()
        {
            csvEditor.AddColumnAt(headerIndex + 1);
        }

        protected override void AddHeaderToLast()
        {
            csvEditor.AddColumnAt(-1);
        }

        protected override void AddHeaderToIndex(int index)
        {
            csvEditor.AddColumnAt(index);
            if (index <= headerIndex) headerIndex++;
        }

        #endregion

        #region [Delete]

        protected override void DelHeaderToFirst()
        {
            csvEditor.DelColumnAt(0);
            headerIndex--;
        }

        protected override void DelHeaderThis()
        {
            csvEditor.DelColumnAt(headerIndex);
            editorWindow.Close();
        }

        protected override void DelHeaderToLast()
        {
            csvEditor.DelColumnAt(-1);
        }

        protected override void DelHeaderToIndex(int index)
        {
            if (index >= 0)
            {
                csvEditor.DelColumnAt(index);
                if (index == headerIndex)
                    editorWindow.Close();
                else if (index < headerIndex)
                    headerIndex--;
            }
        }

        #endregion

        #region [Swap]

        protected override void SwapHeaderToFirst()
        {
            csvEditor.SwapColumn(0, headerIndex);
            headerIndex = 0;
        }

        protected override void SwapHeaderToPrev()
        {
            csvEditor.SwapColumn(headerIndex - 1, headerIndex);
            headerIndex--;
        }

        protected override void SwapHeaderToNext()
        {
            csvEditor.SwapColumn(headerIndex, headerIndex + 1);
            headerIndex++;
        }

        protected override void SwapHeaderToLast()
        {
            csvEditor.SwapColumn(headerIndex, csvEditor.GetColHeaderSliders().Count - 1);
            headerIndex = csvEditor.GetColHeaderSliders().Count - 1;
        }

        protected override void SwapHeaderToIndex(int index)
        {
            if (index < 0)
                index = csvEditor.GetColHeaderSliders().Count - 1;

            csvEditor.SwapColumn(headerIndex, index);

            if (index >= 0 && index < csvEditor.GetColHeaderSliders().Count)
                headerIndex = index;
        }

        #endregion

        #region [Length]

        protected override void AddLength(float addBy = 50)
        {
            csvEditor.ChangeColumnWidth(headerIndex, csvEditor.GetColumnWidth(headerIndex) + addBy);
        }

        protected override void ReduceLength(float reduceBy = 50)
        {
            csvEditor.ChangeColumnWidth(headerIndex, csvEditor.GetColumnWidth(headerIndex) - reduceBy);
        }

        protected override void SetLength(int index, float length)
        {
            csvEditor.ChangeColumnWidth(index, length);
        }

        protected override float GetLength(int index)
        {
            return csvEditor.GetColumnWidth(headerIndex);
        }


        #endregion

    }
}
