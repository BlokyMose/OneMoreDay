using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Encore.Utility
{
    public static class StringUtility
    {
        #region [Modifying Methods]

        /// <summary>
        /// Replace only the first occurence of the searched string<br></br><br></br>
        /// Search: "world", Replace: "you"<br></br>
        /// Input: "Hello, world! world"<br></br>
        /// Ouput: "Hello, you! world"
        /// </summary>
        /// <param name="text">whole text</param>
        /// <param name="search">to be replaced text</param>
        /// <param name="replace">new text to replace search</param>
        /// <returns>text that has been replaced</returns>
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// Replace only the first occurence of the each item from a list of strings<br></br><br></br>
        /// Search: "Hello", "wold", Replace: "you"<br></br>
        /// Input: "Hello, world!"<br></br>
        /// Ouput: "you, you!"
        /// </summary>
        /// <param name="text">whole text</param>
        /// <param name="search">to be replaced list of texts</param>
        /// <param name="replace">new text to replace each item of search</param>
        /// <returns>text that has been replaced</returns>
        public static string ReplaceFirst(this string text, List<string> search, string replace)
        {
            foreach (var _search in search)
                text = text.ReplaceFirst(_search, replace);

            return text;
        }

        /// <summary>
        /// Remove parts of text that are enclosed by an open and a close token<br></br><br></br>
        /// OpenToken: "{", CloseToken: "}"<br></br>
        /// Input: "Hello{, world}!"<br></br>
        /// Ouput: "Hello!"
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="openToken">The string that indentify the start of the string</param>
        /// <param name="closeToken">The string that identify the end of the string</param>
        /// <param name="count">The number of removal; To remove all occurence : -1</param>
        /// <returns>The remaining text which has no enclosed texts and the tokens</returns>
        public static string RemoveByTokens(this string text, string openToken, string closeToken, int count = -1)
        {
            if (string.IsNullOrEmpty(text)) return "";

            while (text.IndexOf(openToken) != -1)
            {
                if (count == 0) break;
                count--;

                int start = text.IndexOf(openToken);
                int end = text.IndexOf(closeToken);
                #region [Checking error]

                if (end == -1)
                {
                    Debug.Log("[StringUtility] RemoveByTokens: expecting another '" + closeToken + "' in this text:\n" + text);
                    break;
                }

                #endregion

                text = text.ReplaceFirst(text.Substring(start, end + closeToken.Length - start), "").Trim();
            }

            return text;
        }

        #endregion

        #region [Split Methods]

        /// <summary>
        /// Split  text into texts using a token
        /// Separation starts after the token; Example:<br></br><br></br>
        /// Token: "["<br></br>
        /// Input: "separtion zero [separation one [separation two"<br></br>
        /// Output: "[separation one", "[separation two"
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="splitToken">The string that identify the separation from one text to another</param>
        /// <returns>A list of separated texts</returns>
        public static List<string> SplitAfterToken(this string text, string splitToken, bool suppressWarning = false, bool caseSensitive = true)
        {
            List<string> separatedTexts = new List<string>();
            string _text = text;
            while (_text.Length > 0)
            {
                // Find the range of a segregation
                int start = caseSensitive ? _text.IndexOf(splitToken) : _text.ToLower().IndexOf(splitToken.ToLower());
                int end = caseSensitive ? _text.IndexOf(splitToken, start + splitToken.Length) : _text.ToLower().IndexOf(splitToken.ToLower(), start + splitToken.Length);
                if (end == -1) end = _text.Length;
                #region [Checking Error]
                if (start == -1)
                {
                    if (!suppressWarning)
                        Debug.Log("[StringUtility] SeparateToList: expecting another '" + splitToken + "'\n in this text: " + _text + "\n\nLocated inside this text:\n" + text);
                    break;
                }
                #endregion

                // Record a separated text into a list
                separatedTexts.Add(_text.Substring(start, end - start).Trim());

                // Remove the separated text from the whole text
                _text = _text.ReplaceFirst(_text.Substring(start, end - start), "").Trim();
            }

            return separatedTexts;
        }

        /// <summary>
        /// Split text into texts using a splitToken and validatorToken<br></br>
        /// Separation is not valid if an escapeToken exists after the separator<br></br>
        /// Example:<br></br><br></br>
        /// SeparatorToken: "["<br></br>
        /// ValidatorToken: "]"<br></br>
        /// EscaperToken: "\"<br></br>
        /// Input: "separtion zero [separation one [separation\ two] [separation three"<br></br>
        /// Output: "[separation one [separation\ two]", "[separation three"
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="splitToken">The string that identify the separation from one text to another</param>
        /// <returns>A list of separated texts</returns>
        public static List<string> SplitAfterToken(this string text, string splitToken, string validatorToken, List<string> escaperTokens, bool suppressWarning = false)
        {
            List<string> separatedTexts = new List<string>();
            string _text = text;

            int escaperTokenIndex = -1;
            string foundEscaperToken = "";

            while (_text.Length > 0)
            {
                // Find the range of a segregation
                int start = _text.IndexOf(splitToken);
                int possibleEnd = escaperTokenIndex == -1
                    ? _text.IndexOf(splitToken, start + splitToken.Length)
                    : _text.IndexOf(splitToken, escaperTokenIndex + foundEscaperToken.Length);

                // No separator detected anymore
                if (possibleEnd == -1) possibleEnd = _text.Length;
                else
                {
                    // Find validatorTokenIndex
                    int validatorTokenIndex = _text.IndexOf(validatorToken, possibleEnd + splitToken.Length);
                    if (validatorTokenIndex != -1)
                    {
                        // Find escaperToken inside separator and validator
                        var checkEscapeTokenString = _text.Substring(possibleEnd + splitToken.Length, validatorTokenIndex - (possibleEnd + splitToken.Length));
                        bool hasFoundEscaper = false;
                        foreach (var escaper in escaperTokens)
                        {
                            var escaperTokenIndexInsideCheck = checkEscapeTokenString.IndexOf(escaper);
                            // Continue loop to find the true separator which has no escapeToken after separator and before validator
                            if (escaperTokenIndexInsideCheck != -1)
                            {
                                hasFoundEscaper = true;
                                foundEscaperToken = escaper;
                                escaperTokenIndex = possibleEnd + splitToken.Length + escaperTokenIndexInsideCheck;
                                break;
                            }
                        }

                        if (hasFoundEscaper) continue;
                    }

                    // Cannot find validatorTokenIndex
                    else
                    {
                        possibleEnd = _text.Length;
                    }
                }

                foundEscaperToken = "";
                escaperTokenIndex = -1;


                #region [Checking Error]
                if (start == -1)
                {
                    if (!suppressWarning)
                        Debug.Log("[StringUtility] SeparateToList: expecting another '" + splitToken + "'\n in this text: " + _text + "\n\nLocated inside this text:\n" + text);
                    break;
                }
                #endregion

                // Record a separated text into a list
                separatedTexts.Add(_text.Substring(start, possibleEnd - start).Trim());

                // Remove the separated text from the whole text
                _text = _text.ReplaceFirst(_text.Substring(start, possibleEnd - start), "").Trim();
            }
            return separatedTexts;
        }

        /// <summary>
        /// Split text into texts using a token
        /// Separation starts before the token; Example:<br></br><br></br>
        /// Token: "["<br></br>
        /// Input: "separtion zero [separation one [separation 2"<br></br>
        /// Output: "separation zero [", "separation one ["
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="splitToken">The string that identify the separation from one text to another</param>
        /// <returns>A list of separated texts</returns>
        public static List<string> SplitBeforeToken(this string text, string splitToken, bool suppressWarning = false, bool caseSensitive = true)
        {
            List<string> separatedTexts = new List<string>();
            string _text = text;
            while (_text.Length > 0)
            {
                // Find the range of a segregation
                int start = 0;
                int end = caseSensitive ? _text.IndexOf(splitToken) : _text.ToLower().IndexOf(splitToken.ToLower());
                if (end == -1) break;

                // Record a separated text into a list
                separatedTexts.Add(_text.Substring(start, end + splitToken.Length).Trim());

                // Remove the separated text from the whole text
                _text = _text.ReplaceFirst(_text.Substring(start, end + splitToken.Length), "").Trim();
            }

            #region [Checking Error]
            if (!suppressWarning)
                if (_text.Length > 0)
                {
                    Debug.Log("[StringUtility] SeparateToList: expecting another '" + splitToken + "' in this text:\n" + _text + "\n\nLocated inside this text:\n" + text);
                }
            #endregion

            return separatedTexts;
        }

        public enum SeparateByTokenMode { ExcludeToken, IncludeTokenInItem1, IncludeTokenInItem2 }

        /// <summary>
        /// Split text into a tuple of two strings; If splitToken cannot be found, each tuple's item contains empty string
        /// </summary>
        public static (string, string) SplitHalf(this string text, string splitToken, int startIndex = 0, SeparateByTokenMode mode = SeparateByTokenMode.ExcludeToken)
        {
            (string, string) tuple = ("", "");

            // Find the range of a segregation
            int end = text.IndexOf(splitToken, startIndex);

            if (end == -1)
            {
                tuple = ("", "");
            }
            else
            {
                switch (mode)
                {
                    case SeparateByTokenMode.ExcludeToken:
                        tuple.Item1 = text.Substring(startIndex, end);
                        tuple.Item2 = text.Substring(end + splitToken.Length, text.Length - (end + splitToken.Length));
                        break;
                    case SeparateByTokenMode.IncludeTokenInItem1:
                        tuple.Item1 = text.Substring(startIndex, end + splitToken.Length);
                        tuple.Item2 = text.Substring(end + splitToken.Length, text.Length - (end + splitToken.Length));
                        break;
                    case SeparateByTokenMode.IncludeTokenInItem2:
                        tuple.Item1 = text.Substring(startIndex, end);
                        tuple.Item2 = text.Substring(end, text.Length - end);
                        break;
                }
            }


            return tuple;
        }

        /// <summary>
        /// Split text into texts using a token and escaper tokens<br></br>
        /// Wrapping escaper tokens inside another escaper tokens may result undesired outcome; This method only supports basic nesting<br></br>
        /// 
        /// Example:<br></br><br></br>
        /// SplitToken: , <br></br>
        /// EscaperOpeningToken: " <br></br>
        /// EscaperClosingToken: " <br></br>
        /// Input: one, "Hello,World", three  <br></br>
        /// Output in List: [one] ["Hello,World"] [three]  <br></br>
        /// </summary>
        public static List<string> SplitEsc(this string text, string splitToken, string escaperOpeningToken, string escaperClosingToken)
        {
            // Validate
            if (text == null || text.Length == 0) return null;

            List<string> separatedTexts = new List<string>();
            var _text = string.Copy(text);

            while (_text.Length > 0)
            {
                // Find the range of a segregation
                int start = 0;
                int end = _text.Length;
                end = GetEnd(_text.IndexOf(splitToken));

                // Record a separated text into a list
                separatedTexts.Add(_text.Substring(start, end));

                // Remove the separated text from the whole text
                _text = _text.ReplaceFirst(_text.Substring(start, end), "").Trim();

                // Remove leftover, if exists
                _text = _text.ReplaceFirst(splitToken, "");

                int GetEnd(int possibleEnd)
                {
                    // Reaching the end of text
                    if (possibleEnd == -1)
                        return _text.Length;

                    int finalEnd = possibleEnd;

                    var separatedText = _text.Substring(0, possibleEnd);

                    // Count escaper tokens
                    int escOpeningTokenCount = 0;
                    var currentIndex = 0;
                    while (true)
                    {
                        int _escOpeningIndex = separatedText.IndexOf(escaperOpeningToken, currentIndex);
                        if (_escOpeningIndex != -1)
                        {
                            currentIndex = _escOpeningIndex + 1;
                            escOpeningTokenCount++;
                        }
                        else break;
                    }

                    int escClosingTokenCount = 0;
                    currentIndex = 0;
                    while (true)
                    {
                        int _escClosingIndex = separatedText.IndexOf(escaperClosingToken, currentIndex);
                        if (_escClosingIndex != -1)
                        {
                            currentIndex = _escClosingIndex + 1;
                            escClosingTokenCount++;
                        }
                        else break;
                    }

                    // Opening token differs with Closing token
                    if (escaperOpeningToken != escaperClosingToken)
                    {
                        // If the tokens are not even, move to the next splitToken
                        if (escOpeningTokenCount != escClosingTokenCount)
                            finalEnd = GetEnd(_text.IndexOf(splitToken, possibleEnd + 1));
                    }
                    else
                    {
                        // If the tokens are not even, move to the next splitToken
                        if (escOpeningTokenCount % 2 == 1)
                            finalEnd = GetEnd(_text.IndexOf(splitToken, possibleEnd + 1));
                    }


                    return finalEnd;
                }
            }

            return separatedTexts;
        }

        #endregion

        #region [Extractor Methods]

        /// <summary>
        /// Extract a string that's enclosed by an open token and a close token<br></br><br></br>
        /// OpenToken: "{", CloseToken: "}"<br></br>
        /// Input: "Hello, {world}!"<br></br>
        /// Output:"world"
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="openToken">The string that indentifies the start of the string</param>
        /// <param name="closeToken">The string that identifies the end of the string</param>
        /// <param name="defaultValue">If a string is enclosed by tokens but is empty, force string's value to defaultValue</param>
        /// <returns>Text that's enclosed</returns>
        public static string Extract(this string text, string openToken, string closeToken, string defaultValue = "", bool suppressWarning = false)
        {
            int startIndex_Name = text.IndexOf(openToken);
            int endIndex_Name = text.IndexOf(closeToken, startIndex_Name + 1);
            int length = endIndex_Name - (startIndex_Name + openToken.Length);

            #region [Checking error]

            if (startIndex_Name == -1)
            {
                if (!suppressWarning) Debug.Log("[StringUtility] ExtractByTokens: cannot find '" + openToken + "' in this text:\n" + text);
                return "";
            }

            if (endIndex_Name == -1)
            {
                if (!suppressWarning) Debug.Log("[StringUtility] ExtractByTokens: cannot find '" + closeToken + "' in this text:\n" + text);
                return "";
            }

            if (length <= -1)
            {
                if (!suppressWarning) Debug.Log("[StringUtility] ExtractByTokens: Length is less than zero in this text:\n" + text);
                return "";
            }



            #endregion

            var extractedText = text.Substring(startIndex_Name + openToken.Length, length).Trim();
            if (defaultValue != "" && string.IsNullOrEmpty(extractedText)) extractedText = defaultValue;

            return extractedText;
        }

        /// <summary>
        /// Extract a string that's enclosed by the last found open token and a close token<br></br><br></br>
        /// OpenToken: "{", CloseToken: "}"<br></br>
        /// Input: "He{llo, {world}!"<br></br>
        /// Output:"world"
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="openToken">The string that indentifies the start of the string</param>
        /// <param name="closeToken">The string that identifies the end of the string</param>
        /// <param name="defaultValue">If a string is enclosed by tokens but is empty, force string's value to defaultValue</param>
        /// <returns>Text that's enclosed</returns>
        public static string ExtractLast(this string text, string openToken, string closeToken, string defaultValue = "", bool suppressWarning = false)
        {
            var startIndexes = text.IndexesOf(openToken);
            int startIndex_Name = startIndexes.Count > 0 ? startIndexes[startIndexes.Count-1] : -1;
            int endIndex_Name = text.IndexOf(closeToken, startIndex_Name + 1);
            int length = endIndex_Name - (startIndex_Name + openToken.Length);

            #region [Checking error]

            if (startIndex_Name == -1)
            {
                if (!suppressWarning) Debug.Log("[StringUtility] ExtractByTokens: cannot find '" + openToken + "' in this text:\n" + text);
                return "";
            }

            if (endIndex_Name == -1)
            {
                if (!suppressWarning) Debug.Log("[StringUtility] ExtractByTokens: cannot find '" + closeToken + "' in this text:\n" + text);
                return "";
            }

            if (length <= -1)
            {
                if (!suppressWarning) Debug.Log("[StringUtility] ExtractByTokens: Length is less than zero in this text:\n" + text);
                return "";
            }



            #endregion

            var extractedText = text.Substring(startIndex_Name + openToken.Length, length).Trim();
            if (defaultValue != "" && string.IsNullOrEmpty(extractedText)) extractedText = defaultValue;

            return extractedText;
        }

        /// <summary>
        /// Extract all texts that are enclosed by an open token and a close token<br></br><br></br>
        /// OpenToken: "{", CloseToken: "}"<br></br>
        /// Input: "{Hello}, {world}!"<br></br>
        /// Output:"Hello","world"
        /// </summary>
        /// <param name="text">input string</param>
        /// <param name="openToken">The string that indentify the start of the string</param>
        /// <param name="closeToken">The string that identify the end of the string</param>
        /// <param name="defaultValue">If a string is enclosed by tokens but is empty, force string's value to defaultValue</param>
        /// <returns>All texts that are enclosed</returns>
        public static List<string> ExtractAll(this string text, string openToken, string closeToken, string defaultValue = "", bool suppressWarning = false)
        {
            List<string> extractedTexts = new List<string>();
            while (true)
            {
                string extractedText = text.Extract(openToken, closeToken, defaultValue, suppressWarning);
                if (!string.IsNullOrEmpty(extractedText))
                {
                    extractedTexts.Add(extractedText);
                    text = text.RemoveByTokens(openToken, closeToken, 1);
                    if (string.IsNullOrEmpty(text)) break;
                }
                else
                    break;
            }

            return extractedTexts;
        }

        #endregion

        #region [IndexOf Methods]

        /// <summary>
        /// Finds all indexes of targeted string inside this text
        /// </summary>
        public static List<int> IndexesOf(this string text, string target)
        {
            List<int> indexes = new List<int>();

            int antiInfi = 0;
            while (text.Length > 0)
            {
                if (antiInfi > 250) break;
                antiInfi++;

                var targetIndex = text.IndexOf(target);
                if (targetIndex != -1)
                {
                    if (indexes.Count > 0) indexes.Add(targetIndex + indexes[indexes.Count-1] + 1);
                    else indexes.Add(targetIndex);
                    text = text.Remove(0, targetIndex + 1);
                }
                else break;
            }

            return indexes;
        }

        #endregion

        #region [Misc Methods]

        public static string ReplaceTokens(this string text, string oldOpenToken, string oldCloseToken, string newOpenToken, string newCloseToken)
        {
            string result = text;

            if (oldOpenToken != oldCloseToken)
            {
                string lookingForToken = oldOpenToken;
                string replaceWithToken = newOpenToken;
                while (true)
                {
                    int pos = result.IndexOf(lookingForToken);
                    if (pos < 0)
                    {
                        if (lookingForToken == oldCloseToken) Debug.Log("Expecting another: " + oldCloseToken + " in this text: " + text);
                        break;
                    }
                    result = result.Substring(0, pos) + replaceWithToken + result.Substring(pos + lookingForToken.Length);

                    lookingForToken = lookingForToken == oldOpenToken ? oldCloseToken : oldOpenToken;
                    replaceWithToken = replaceWithToken == newOpenToken ? newCloseToken : newOpenToken;
                }
            }

            // If oldOpenToken and oldCloseToken are the same
            else
            {
                bool isReplacingOpen = true;
                string replaceWithToken = newOpenToken;

                while (true)
                {
                    int pos = result.IndexOf(oldOpenToken);
                    if (pos < 0)
                    {
                        if (!isReplacingOpen) Debug.Log("Expecting another: " + oldOpenToken + " in this text: " + text);
                        break;
                    }
                    result = result.Substring(0, pos) + replaceWithToken + result.Substring(pos + oldOpenToken.Length);

                    isReplacingOpen = !isReplacingOpen;
                    replaceWithToken = replaceWithToken == newOpenToken ? newCloseToken : newOpenToken;
                }
            }

            return result;
        }

        #endregion

        #region [Encoder]

        /// <summary>
        /// Convert a text into bytes, then convert to hexadecimal code<br></br><br></br>
        /// Input: "\U0001f381" <br></br>
        /// Output: "F0-9F-8E-81"
        /// </summary>
        public static string GetHex(this string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            string hex = BitConverter.ToString(bytes);
            return hex;
        }

        /// <summary>
        /// Convert a hex into bytes, then convert to string<br></br><br></br>
        /// Input: "F0-9F-8E-81"
        /// Output: "\U0001f381" <br></br>
        /// </summary>
        public static string GetString(this string hex)
        {
            hex = hex.Replace("-", "");
            byte[] rawByte = new byte[hex.Length / 2];
            for (int i = 0; i < rawByte.Length; i++)
            {
                rawByte[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            var result = Encoding.UTF8.GetString(rawByte);
            return result;
        }

        #endregion
    }
}