using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Encore.Utility
{
    public static class CSVUtility
    {
        public static Dictionary<string, string> GetColumn(TextAsset csvFile, string columnHeaderName)
        {
            return GetColumn(csvFile.text, columnHeaderName);
        }

        /// <summary>
        /// Returns a dictionary of column header's name and the text in the intersection between the row and the column<br></br>
        /// </summary>
        public static Dictionary<string, string> GetColumn(string csvText, string columnHeaderName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            char lineSeparator = '\n';
            char surround = '"';
            char fieldSeparator = ',';

            string[] lines = csvText.Split(lineSeparator);

            int columnIndex = -1;

            var columnHeaders = lines[0].SplitEsc(fieldSeparator.ToString(), surround.ToString(), surround.ToString());
            if (columnHeaders == null) return dictionary;

            for (int i = 0; i < columnHeaders.Count; i++)
            {
                if (columnHeaders[i].Contains(columnHeaderName))
                {
                    columnIndex = i;
                    break;
                }
            }

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                string[] fields = CSVParser.Split(line);

                for (int f = 0; f < fields.Length; f++)
                {
                    fields[f] = fields[f].TrimStart(' ', surround);
                    fields[f] = fields[f].TrimEnd(surround);
                }

                if (fields.Length > columnIndex)
                {
                    var key = fields[0];

                    if (dictionary.ContainsKey(key)) { continue; }

                    var value = fields[columnIndex];

                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }

        public static Dictionary<string, string> GetRow(TextAsset csvFile, string rowHeaderName)
        {
            return GetRow(csvFile.text, rowHeaderName);
        }

        public static Dictionary<string, string> GetRow(string csvText, string rowHeaderName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            char lineSeparator = '\n';
            char surround = '"';
            char fieldSeparator = ',';

            string[] lines = csvText.Split(lineSeparator);

            // Get column header names
            var columnHeaders = lines[0].SplitEsc(fieldSeparator.ToString(), surround.ToString(), surround.ToString());
            if (columnHeaders == null) return dictionary;

            // Get targeted all texts in the targeted row
            var rowTexts = new List<string>();
            foreach (var line in lines)
            {
                var rowHeader = line.SplitHalf(fieldSeparator.ToString()).Item1;
                rowHeader = rowHeader.ReplaceTokens(surround.ToString(), surround.ToString(), "", "");

                if (rowHeader.Equals(rowHeaderName, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    rowTexts = line.SplitEsc(fieldSeparator.ToString(), surround.ToString(), surround.ToString());
                    break;
                }
            }
            if (rowTexts == null || rowTexts.Count == 0) return dictionary;

            // Pack each row text by its column
            for (int i = 0; i < rowTexts.Count; i++)
            {
                var columnHeader = columnHeaders[i].ReplaceTokens(surround.ToString(), surround.ToString(), "", "");
                var text = rowTexts[i].ReplaceTokens(surround.ToString(), surround.ToString(), "", "");
                dictionary.Add(columnHeader, text);
            }

            return dictionary;
        }

        public static List<List<string>> GetAllByRow(TextAsset csvFile)
        {
            return GetAllByRow(csvFile.text);
        }

        public static List<List<string>> GetAllByRow(string csvText)
        {
            var rows = new List<List<string>>();
            char lineSeparator = '\n';
            char surround = '"';
            char fieldSeparator = ',';

            string[] lines = csvText.Split(lineSeparator);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length == 0) continue;
                var cells = lines[i].SplitEsc(fieldSeparator.ToString(), surround.ToString(), surround.ToString());
                var cellsWithoutSurround = new List<string>();
                foreach (var cell in cells)
                    cellsWithoutSurround.Add(cell.ReplaceTokens(surround.ToString(), surround.ToString(), "", ""));
                rows.Add(cellsWithoutSurround);
            }

            return rows;
        }

        public static string WriteRow(int columnsCount, List<string> columns, string defaultColumnValue = "")
        {
            var columnsString = "";
            int index = 0;
            for (int i = 0; i < columnsCount; i++)
            {
                columnsString += "\"" + columns.GetAt(index, defaultColumnValue) + "\"";
                columnsString += index < columnsCount - 1 ? "," : "\n";
                index++;
            }

            return columnsString;
        }

    }
}