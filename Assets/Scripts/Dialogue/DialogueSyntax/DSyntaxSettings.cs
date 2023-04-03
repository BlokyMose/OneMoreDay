using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSyntax 
{
    [CreateAssetMenu(menuName = "SO/Dialogue/Dialogue Syntax Settings", fileName = "DialogueSyntaxSettings")]
    public class DSyntaxSettings : ScriptableObject
    {
        #region Classes

        [System.Serializable]
        public class StylingFromTo
        {
            public string oldOpenToken;
            public string oldCloseToken;
            public string newOpenToken;
            public string newCloseToken;

            public StylingFromTo(string oldOpenToken, string oldCloseToken, string newOpenToken, string newCloseToken)
            {
                this.oldOpenToken = oldOpenToken;
                this.oldCloseToken = oldCloseToken;
                this.newOpenToken = newOpenToken;
                this.newCloseToken = newCloseToken;
            }
        }


        #endregion

        [FoldoutGroup("Tokens Adjustment", order: 3)] public string TOKEN_BRANCH_OPENING = "<";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_BRANCH_CLOSING = ">";

        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_COMMAND_OPENING = "[";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_COMMAND_CLOSING = "]";

        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_COMMENT_OPENING = "/*";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_COMMENT_CLOSING = "*/";

        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_PARAMETER_OPENING = "{";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_PARAMETER_CLOSING = "}";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_PARAMETER_NAME = ":";

        [FoldoutGroup("Tokens Adjustment"), SerializeField]
        List<string> invalidTokensInBranchName = new List<string>() { "}", "=", "/" };

        [Header("Logic Tokens")]
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_IS_EQUAL = "==";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_IS_NOT_EQUAL = "!=";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_IS_GREATER_THAN = ">";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_IS_LESS_THAN = "<";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_IS_GREATER_OR_EQUAL = ">=";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_IS_LESS_OR_EQUAL = "<=";

        [Header("Operation Tokens")]
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_EQUAL = "=";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_INCREMENT = "+=";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_DECREMENT = "-=";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_MULTIPLICATION = "*=";
        [FoldoutGroup("Tokens Adjustment")] public string TOKEN_DIVISION = "/=";

        // When adding new command names, add it to GetCommandNames() as well
        [FoldoutGroup("Commands Adjustment")] public string COMMAND_BRANCH = "BRANCH";
        [FoldoutGroup("Commands Adjustment")] public string COMMAND_CHOICES = "CHOICES";
        [FoldoutGroup("Commands Adjustment")] public string COMMAND_URGENT = "URGENT";
        [FoldoutGroup("Commands Adjustment")] public string COMMAND_GOTO = "GOTO";
        [FoldoutGroup("Commands Adjustment")] public string COMMAND_IF = "IF";
        [FoldoutGroup("Commands Adjustment")] public string COMMAND_SET = "SET";
        [FoldoutGroup("Commands Adjustment")] public string COMMAND_ONCE = "ONCE";
        [FoldoutGroup("Commands Adjustment")] public string START = "START";
        [FoldoutGroup("Commands Adjustment")] public string PASS = "PASS";

        [FoldoutGroup("Styling Adjustment")] public StylingFromTo italic = new StylingFromTo("*", "*", "<i>", "</i>");
        [FoldoutGroup("Styling Adjustment")] public StylingFromTo bold = new StylingFromTo("**", "**", "<b>", "</b>");
        [FoldoutGroup("Styling Adjustment")] public StylingFromTo underline = new StylingFromTo("__", "__", "<u>", "</u>");
        [FoldoutGroup("Styling Adjustment")] public StylingFromTo strikethrough = new StylingFromTo("--", "--", "<s>", "</s>");

        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_LISTENING = "F0-9F-A4-94"; // thinking face
        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_SAD = "F0-9F-99-81"; // slightly frowning face
        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_HAPPY_BIT = "F0-9F-99-82"; // slightly smiling face
        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_HAPPY = "F0-9F-98-81"; // beaming face with smiling eyes

        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_CONFUSED = "F0-9F-98-95"; // confused face
        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_SURPRISED = "F0-9F-98-AE"; // face with open mouth
        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_ANGRY = "F0-9F-98-A0"; // angry face
        [FoldoutGroup("Expression Adjustment")] public string EXPRESSION_UNTRUST = "F0-9F-A4-A8"; // face with raised eyebrow

        [FoldoutGroup("Gesture Adjustment")] public string GESTURE_SPEAKING = "speaking";
        [FoldoutGroup("Gesture Adjustment")] public string GESTURE_NOD = "nod";
        [FoldoutGroup("Gesture Adjustment")] public string GESTURE_THINKING = "thinking";
        [FoldoutGroup("Gesture Adjustment")] public string GESTURE_PONDERING = "pondering";

        [FoldoutGroup("Gesture Adjustment")] public string GESTURE_THIS = "this";
        [FoldoutGroup("Gesture Adjustment")] public string GESTURE_NOIDEA = "noIdea";
        [FoldoutGroup("Gesture Adjustment")] public string GESTURE_LEANBACK = "leanBack";

        public static string GetExpressionHex(Expression expression, DSyntaxSettings settings)
        {
            switch (expression)
            {
                case Expression.None: return "";
                case Expression.Listening: return settings.EXPRESSION_LISTENING;
                case Expression.Sad: return settings.EXPRESSION_SAD;
                case Expression.HappyBit: return settings.EXPRESSION_HAPPY_BIT;
                case Expression.Happy: return settings.EXPRESSION_HAPPY;
                case Expression.Confused: return settings.EXPRESSION_CONFUSED;
                case Expression.Surprised: return settings.EXPRESSION_SURPRISED;
                case Expression.Angry: return settings.EXPRESSION_ANGRY;
                case Expression.Untrust: return settings.EXPRESSION_UNTRUST;
                default: return "";
            }
        }

        public List<string> GetCommandNames()
        {
            return new List<string>()
            {
                COMMAND_BRANCH,
                COMMAND_CHOICES,
                COMMAND_URGENT,
                COMMAND_GOTO,
                COMMAND_IF,
                COMMAND_SET,
                COMMAND_ONCE,
                START,
                PASS,
            };
        }
    }
}
