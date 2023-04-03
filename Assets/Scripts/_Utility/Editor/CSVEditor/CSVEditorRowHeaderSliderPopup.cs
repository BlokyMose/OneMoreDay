using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor.CSVEditor
{
    public class CSVEditorRowHeaderSliderPopup : CSVEditorHeaderSliperPopup
    {
        public CSVEditorRowHeaderSliderPopup(int headerIndex, CSVEditor csvEditor) : base(headerIndex, csvEditor)
        {
        }

        protected override float GetMinimalLength()
        {
            return CSVEditor.minimalRowLength;
        }

        #region [Add]

        protected override void AddHeaderToFirst()
        {
            csvEditor.AddRowAt(0);
            headerIndex++;
        }

        protected override void AddHeaderToPrev()
        {
            csvEditor.AddRowAt(headerIndex);
            headerIndex++;
        }

        protected override void AddHeaderToNext()
        {
            csvEditor.AddRowAt(headerIndex + 1);
        }

        protected override void AddHeaderToLast()
        {
            csvEditor.AddRowAt(-1);
        }

        protected override void AddHeaderToIndex(int index)
        {
            csvEditor.AddRowAt(index);
            if (index <= headerIndex) headerIndex++;
        }

        #endregion

        #region [Delete]

        protected override void DelHeaderToFirst()
        {
            csvEditor.DelRowAt(0);
            headerIndex--;
        }

        protected override void DelHeaderThis()
        {
            csvEditor.DelRowAt(headerIndex);
            editorWindow.Close();
        }

        protected override void DelHeaderToLast()
        {
            csvEditor.DelRowAt(-1);
        }

        protected override void DelHeaderToIndex(int index)
        {
            if (index >= 0)
            {
                csvEditor.DelRowAt(index);
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
            csvEditor.SwapRow(0, headerIndex);
            headerIndex = 0;
        }

        protected override void SwapHeaderToPrev()
        {
            csvEditor.SwapRow(headerIndex - 1, headerIndex);
            headerIndex--;
        }

        protected override void SwapHeaderToNext()
        {
            csvEditor.SwapRow(headerIndex, headerIndex + 1);
            headerIndex++;
        }

        protected override void SwapHeaderToLast()
        {
            csvEditor.SwapRow(headerIndex, csvEditor.GetRowHeadersSliders().Count - 1);
            headerIndex = csvEditor.GetRowHeadersSliders().Count - 1;
        }

        protected override void SwapHeaderToIndex(int index)
        {
            if (index < 0)
                index = csvEditor.GetRowHeadersSliders().Count - 1;

            csvEditor.SwapRow(headerIndex, index);

            if (index >= 0 && index < csvEditor.GetRowHeadersSliders().Count)
                headerIndex = index;
        }

        #endregion

        #region [Length]

        protected override void AddLength(float addBy = 50)
        {
            csvEditor.ChangeRowHeight(headerIndex, csvEditor.GetRowHeight(headerIndex) + addBy);
        }

        protected override void ReduceLength(float reduceBy = 50)
        {
            csvEditor.ChangeRowHeight(headerIndex, csvEditor.GetRowHeight(headerIndex) - reduceBy);
        }

        protected override void SetLength(int index, float length)
        {
            csvEditor.ChangeRowHeight(index, length);
        }

        protected override float GetLength(int index)
        {
            return csvEditor.GetRowHeight(headerIndex);
        }

        #endregion
    }
}
