using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSyntax
{
    public static class DSyntaxData
    {
        public class Tree
        {
            public Tree(List<Branch> branches, List<Variable> variables, Dictionary<string, string> actors)
            {
                this.branches = branches;
                this.variables = variables;
                this.actors = actors;
            }

            public List<Branch> branches { get; set; }

            public List<Variable> variables { get; set; }
            /// <summary>
            /// Key= actorName; Value= ID
            /// </summary>
            public Dictionary<string, string> actors { get; set; }
        }

        public class Branch
        {
            public Branch(string name, List<Node> nodes)
            {
                this.name = name;
                this.nodes = nodes;
            }
            public string name { get; set; }
            public List<Node> nodes { get; set; }
            public const string BRANCH_NAME = "branchName";

        }

        public struct Text
        {
            public string text;
            public string id;

            public Text(string text, string id)
            {
                this.text = text;
                this.id = id;
            }
        }

        public interface Node
        {
            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }

            public class Variable
            {
                public string varName { get; set; }
                public string varType { get; set; }
                public string varValue { get; set; }

                public Variable(string varName, string varType, string varValue)
                {
                    this.varName = varName;
                    this.varType = varType;
                    this.varValue = varValue;
                }

                public Variable()
                {
                    this.varName = "";
                    this.varType = "str";
                    this.varValue = "";
                }
            }
        }

        public class NodeSay : Node
        {
            public NodeSay(string name, string parameter, string id, Text text, int expression = -1, int gesture = -1, float duration = 2.2f)
            {
                this.name = name;
                this.parameter = parameter;
                this.text = text;
                this.id = id;

                this.expression = expression;
                this.gesture = gesture;
                this.duration = duration;
            }
            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }
            public Text text { get; set; }
            public int expression { get; set; }
            public int gesture { get; set; }
            public float duration { get; set; }

            public const string STATEMENT = "statement";
            public const string EXPRESSION = "expression";
            public const string GESTURE = "gesture";
            public const string DURATION = "duration";
        }

        public class NodeChoices : Node
        {
            public NodeChoices(string name, string parameter, string id, Text title, List<Choice> choices)
            {
                this.name = name;
                this.parameter = parameter;
                this.id = id;
                this.title = title;
                this.choices = choices;
            }
            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }

            public Text title { get; set; }
            public List<Choice> choices { get; set; }

            public class Choice
            {
                public Choice(Text text, List<Condition> conditions)
                {
                    this.text = text;
                    this.conditions = conditions;
                }
                public Text text { get; set; }
                public List<Condition> conditions { get; set; }
                public string toBranchName { get { return conditions != null ? conditions.Count > 0 ? conditions[0].toBranchName : null : null; } }

                public const string CONDITION = "condition";
            }
        }

        public class NodeUrgent : Node
        {
            public NodeUrgent(string name, string parameter, string id, List<Choice> choices, Text title, string initialDelay)
            {
                this.name = name;
                this.parameter = parameter;
                this.id = id;
                this.choices = choices;
                this.title = title;
                this.initialDelay = initialDelay;
            }
            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }

            public List<Choice> choices { get; set; }

            public Text title { get; set; }
            public string initialDelay { get; set; }

            public class Choice
            {
                public Choice(Text text, List<Condition> conditions, string speed, string color)
                {
                    this.text = text;
                    this.conditions = conditions;
                    this.speed = speed;
                    this.color = color;
                }
                public Text text { get; set; }
                public string speed { get; set; }
                public string color { get; set; }
                public List<Condition> conditions { get; set; }
                public string toBranchName { get { return conditions != null ? conditions.Count > 0 ? conditions[0].toBranchName : null : null; } }

                public const string CONDITION = "condition";
            }
        }

        public class NodeGoTo : Node
        {
            public NodeGoTo(string name, string parameter, string id, string toBranchName)
            {
                this.name = name;
                this.parameter = parameter;
                this.id = id;
                this.toBranchName = toBranchName;
            }
            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }
            public string toBranchName { get; set; }

            public const string BRANCH_NAME = "branchName";

        }

        public class NodeConditions : Node
        {
            public NodeConditions(string name, string parameter, string id, List<Condition> conditions)
            {
                this.name = name;
                this.parameter = parameter;
                this.id = id;
                this.conditions = conditions;
            }

            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }
            public List<Condition> conditions { get; set; }

            public const string CONDITION = "condition";

        }

        public class NodeSet : Node
        {
            public enum OperationType { Invalid = -1, Equal, Increment, Decrement, Multiplication, Division }
            public NodeSet(string name, string parameter, string id, Node.Variable variable, OperationType operationType)
            {
                this.name = name;
                this.parameter = parameter;
                this.id = id;
                this.variable = variable;
                this.operationType = operationType;
            }

            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }
            public Node.Variable variable { get; set; }
            public OperationType operationType { get; set; }
        }

        public class NodeOnce : Node
        {
            public Node.Variable variable;

            public NodeOnce(string name, string parameter, string id, Node.Variable variable)
            {
                this.variable = variable;
                this.name = name;
                this.parameter = parameter;
                this.id = id;
            }

            public string name { get; set; }
            public string parameter { get; set; }
            public string id { get; set; }
        }

        public class Variable
        {
            public Variable(string varValue, string varName, string varType, string id, string type, string publicKey)
            {
                this.varValue = varValue;
                this.varName = varName;
                this.varType = varType;
                this.id = id;
                this.type = type;
                this.publicKey = publicKey;
            }

            public string varValue { get; set; }
            public string varName { get; set; }
            public string varType { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string publicKey { get; set; }
        }

        public class Condition
        {
            // CheckKey's int value is aligned with NodeCanvas checkType, except Unassigned and NotEqual which should be translated as 0 in checkType
            public enum CheckKey { Unassigned = -2,  NotEqual = -1, Equal, GreaterThan, LessThan, GreaterOrEqual, LessOrEqual }

            public Node.Variable variable { get; set; }
            public CheckKey checkKey { get; set; }
            public string toBranchName { get; set; }

            public const string BRANCH_NAME = "branchName";

            public Condition(Node.Variable variable, CheckKey checkKey, string toBranchName)
            {
                this.variable = variable;
                this.checkKey = checkKey;
                this.toBranchName = toBranchName;
            }

            public Condition(Node.Variable variable, string checkKey, string toBranchName, DSyntaxSettings settings)
            {
                this.variable = variable;
                this.toBranchName = toBranchName;

                if (checkKey == settings.TOKEN_IS_NOT_EQUAL)
                    this.checkKey = CheckKey.NotEqual;
                else if (checkKey == settings.TOKEN_IS_EQUAL)
                    this.checkKey = CheckKey.Equal;
                else if (checkKey == settings.TOKEN_IS_GREATER_THAN)
                    this.checkKey = CheckKey.GreaterThan;
                else if (checkKey == settings.TOKEN_IS_LESS_THAN)
                    this.checkKey = CheckKey.LessThan;
                else if (checkKey == settings.TOKEN_IS_GREATER_OR_EQUAL)
                    this.checkKey = CheckKey.GreaterOrEqual;
                else if (checkKey == settings.TOKEN_IS_LESS_OR_EQUAL)
                    this.checkKey = CheckKey.LessOrEqual;
                else
                    this.checkKey = CheckKey.Unassigned;
            }
        }

    }
}