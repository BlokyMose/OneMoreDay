using Encore.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static DialogueSyntax.DSyntaxData;

namespace DialogueSyntax
{
    public static class DSyntaxUtility
    {
        #region [Processing DSyntaxData]

        public static DSyntaxData.Tree GetTree(string text, DSyntaxSettings settings)
        {
            int _nodeID = -1;
            string IncrementingNodeID() { _nodeID++; return _nodeID.ToString(); }

            int _varID = -1;
            string IncrementingVarID() { _varID++; return _varID.ToString(); }

            int _textID = -1;
            string IncrementingTextID() { _textID++; return _textID.ToString(); }

            // Add [BRANCH] at the beginning if it doesn't exist 
            text = text.Replace(settings.TOKEN_COMMAND_OPENING + settings.COMMAND_BRANCH.ToLower() + settings.TOKEN_COMMAND_CLOSING, settings.TOKEN_COMMAND_OPENING + settings.COMMAND_BRANCH + settings.TOKEN_COMMAND_CLOSING);
            text = text.Replace(settings.TOKEN_PARAMETER_OPENING + settings.START.ToLower() + settings.TOKEN_PARAMETER_CLOSING, settings.TOKEN_PARAMETER_OPENING + settings.START + settings.TOKEN_PARAMETER_CLOSING);
            if (!text.Contains(settings.TOKEN_COMMAND_OPENING + settings.COMMAND_BRANCH + settings.TOKEN_COMMAND_CLOSING)) 
            { text = settings.TOKEN_COMMAND_OPENING + settings.COMMAND_BRANCH + settings.TOKEN_COMMAND_CLOSING + settings.TOKEN_PARAMETER_OPENING + settings.START + settings.TOKEN_PARAMETER_CLOSING + "\n" + text; }

            // Segregate one branch from another
            var branches = ReadCommandsByGroups(settings, text, settings.COMMAND_BRANCH);

            // Group each branch with its commands
            List<Tuple<string, List<Command>>> branchNamesAndCommands = new List<Tuple<string, List<Command>>>();
            foreach (var branch in branches)
            {
                var branchName = branch[0].GetParameter(settings, Branch.BRANCH_NAME, 0);
                branch.RemoveAt(0);
                branchNamesAndCommands.Add(new Tuple<string, List<Command>>(branchName, branch));
            }

            // Set the branch with the settings.START as its name to be the first branch
            var startBranch = branchNamesAndCommands.Find(branch => branch.Item1.ToUpper() == settings.START.ToUpper());
            if (startBranch != null)
            {
                branchNamesAndCommands.Remove(startBranch);
                branchNamesAndCommands.Insert(0, startBranch);
            }

            // Convert into usable branch data
            List<Branch> branchesData = new List<Branch>();
            foreach (var branchNameAndNode in branchNamesAndCommands)
            {
                // Extract a branch
                var branchName = branchNameAndNode.Item1;
                var commands = branchNameAndNode.Item2;

                // Extract nodes
                List<Node> nodes = GetNodes(commands);

                // Check identical branch name
                foreach (Branch branch in branchesData)
                {
                    if (branch.name == branchName)
                    {
                        Debug.Log("[DialogueConverter] Same branch name detected: " + branchName);
                    }
                }

                branchesData.Add(new Branch(branchName, nodes));
            }

            // Extract variables from NodeConditions or NodeSet
            List<Variable> variables = new List<Variable>();
            foreach (Branch branch in branchesData)
            {
                List<Node> nodesToRemove = new List<Node>();

                foreach (Node node in branch.nodes)
                {
                    if (node is NodeConditions)
                    {
                        var nodeConditions = node as NodeConditions;
                        foreach (var condition in nodeConditions.conditions)
                        {
                            if (condition.checkKey == Condition.CheckKey.Unassigned) continue;
                            var publicKey = Generate16Digit();
                            if (variables.Find(x => x.varName == condition.variable.varName) == null)
                                variables.Add(new Variable(
                                    varName: condition.variable.varName,
                                    varValue: condition.variable.varValue,
                                    varType: condition.variable.varType,
                                    id: GenerateGUID(),
                                    type: condition.variable.varType,
                                    publicKey: publicKey
                                    ));
                        }
                    }
                    else if (node is NodeChoices)
                    {
                        var nodeChoice = node as NodeChoices;
                        foreach (var choice in nodeChoice.choices)
                        {
                            if (choice.conditions != null && choice.conditions.Count > 0)
                                foreach (var condition in choice.conditions)
                                {
                                    if (condition == null || condition.checkKey == Condition.CheckKey.Unassigned) continue;
                                    var publicKey = Generate16Digit();
                                    if (variables.Find(x => x.varName == condition.variable.varName) == null)
                                        variables.Add(new Variable(
                                            varName: condition.variable.varName,
                                            varValue: condition.variable.varValue,
                                            varType: condition.variable.varType,
                                            id: GenerateGUID(),
                                            type: condition.variable.varType,
                                            publicKey: publicKey
                                            ));
                                }
                        }

                    }
                    else if (node is NodeUrgent)
                    {
                        var nodeUrgent = node as NodeUrgent;
                        foreach (var choice in nodeUrgent.choices)
                        {
                            if (choice.conditions != null)
                                foreach (var condition in choice.conditions)
                                {
                                    if (condition == null || condition.checkKey == Condition.CheckKey.Unassigned) continue;
                                    var publicKey = Generate16Digit();
                                    if (variables.Find(x => x.varName == condition.variable.varName) == null)
                                        variables.Add(new Variable(
                                            varName: condition.variable.varName,
                                            varValue: condition.variable.varValue,
                                            varType: condition.variable.varType,
                                            id: GenerateGUID(),
                                            type: condition.variable.varType,
                                            publicKey: publicKey
                                            ));
                                }
                        }

                    }
                    else if (node is NodeSet)
                    {
                        if (string.IsNullOrEmpty((node as NodeSet).variable.varName)) continue;
                        var nodeSet = node as NodeSet;
                        var publicKey = Generate16Digit();

                        if (variables.Find(x => x.varName == nodeSet.variable.varName) == null)
                            variables.Add(new Variable(
                                varName: nodeSet.variable.varName,
                                varValue: nodeSet.variable.varValue,
                                varType: nodeSet.variable.varType,
                                id: GenerateGUID(),
                                type: nodeSet.variable.varType == "int"
                                ? "NodeCanvas.Framework.Variable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + publicKey + "]]"
                                : "NodeCanvas.Framework.Variable`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + publicKey + "]]"
                                ,
                                publicKey: publicKey
                                ));
                    }
                    else if (node is NodeOnce)
                    {
                        var nodeOnce = node as NodeOnce;
                        var publicKey = Generate16Digit();
                        if (variables.Find(x => x.varName == nodeOnce.variable.varName) == null)
                            variables.Add(new Variable(
                                varName: nodeOnce.variable.varName,
                                varValue: nodeOnce.variable.varValue,
                                varType: nodeOnce.variable.varType,
                                id: GenerateGUID(),
                                type: nodeOnce.variable.varType == "int"
                                ? "NodeCanvas.Framework.Variable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + publicKey + "]]"
                                : "NodeCanvas.Framework.Variable`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + publicKey + "]]"
                                ,
                                publicKey: publicKey
                                ));
                        nodesToRemove.Add(node);
                    }
                }

                foreach (Node node in nodesToRemove) branch.nodes.Remove(node);
            }

            // Bracket variable names inside every text
            foreach (var branch in branchesData)
            {
                foreach (Node node in branch.nodes)
                {
                    if (node is NodeSay)
                    {
                        var nodeSay = node as NodeSay;
                        nodeSay.text = new Text(BracketVariableNames(nodeSay.text.text), nodeSay.text.id);
                    }
                    else if (node is NodeChoices)
                    {
                        var nodeChoice = node as NodeChoices;
                        foreach (var choice in nodeChoice.choices)
                            choice.text = new Text(BracketVariableNames(choice.text.text), choice.text.id);
                    }
                    else if (node is NodeUrgent)
                    {
                        var nodeUrgent = node as NodeUrgent;
                        foreach (var choice in nodeUrgent.choices)
                            choice.text = new Text(BracketVariableNames(choice.text.text), choice.text.id);
                    }
                }
            }

            string BracketVariableNames(string statement)
            {
                string bracketedStatement = statement;
                foreach (var variable in variables)
                    bracketedStatement = bracketedStatement.Replace(variable.varName, "[" + variable.varName + "]");
                return bracketedStatement;
            }

            // Extract actors
            Dictionary<string, string> actors = new Dictionary<string, string>();
            foreach (var branch in branchesData)
            {
                foreach (var node in branch.nodes)
                {
                    if (node is NodeSay && !actors.ContainsKey(node.name))
                    {
                        actors.Add(node.name, GenerateGUID());
                    }
                }
            }

            // Check errors
            foreach (var branch in branchesData)
            {
                foreach (var node in branch.nodes)
                {
                    if (node is NodeGoTo)
                    {
                        var nodeGoTo = node as NodeGoTo;
                        if (branchesData.Find(br => br.name == nodeGoTo.toBranchName) == null)
                        {
                            Debug.Log("Branch: {" + nodeGoTo.toBranchName + "} doesn't exist \n\n" + node.parameter);
                            return null;
                        }
                    }
                    else if (node is NodeChoices)
                    {
                        var nodeChoice = node as NodeChoices;
                        foreach (var choice in nodeChoice.choices)
                        {
                            if (!string.IsNullOrEmpty(choice.toBranchName) && branchesData.Find(br => br.name == choice.toBranchName) == null)
                            {
                                Debug.Log("Branch: {" + choice.toBranchName + "} doesn't exist \n\n" + node.parameter);
                                return null;
                            }
                        }
                    }

                    else if (node is NodeConditions)
                    {
                        var nodeConditions = node as NodeConditions;

                        foreach (var condition in nodeConditions.conditions)
                        {
                            if (!string.IsNullOrEmpty(condition.toBranchName) && branchesData.Find(br => br.name == condition.toBranchName) == null)
                            {
                                Debug.Log("Branch: {" + condition.toBranchName + "} doesn't exist \n\n" + node.parameter);
                                return null;
                            }
                        }
                    }
                }
            }

            return new DSyntaxData.Tree(branchesData, variables, actors);

            List<Node> GetNodes(List<Command> commands)
            {
                var nodes = new List<Node>();
                foreach (var command in commands)
                {
                    if (command.name.Equals(settings.COMMAND_GOTO, StringComparison.CurrentCultureIgnoreCase))
                    {
                        nodes.Add(new NodeGoTo(command.name, command.rawText, "-1", command.GetParameter(settings, NodeGoTo.BRANCH_NAME, 0)));
                    }

                    else if (command.name.Equals(settings.COMMAND_CHOICES, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var listCommand = command.ConvertToListCommand(settings, true);

                        var title = listCommand.GetParameter(settings, "title", 0);

                        // Convert choices
                        List<NodeChoices.Choice> choices = new List<NodeChoices.Choice>();
                        for (int i = 0; i < listCommand.childParameters.Count; i++)
                        {
                            var choiceText = listCommand.GetChildParameter(settings, i, "text", 0);
                            var toBranchName = listCommand.GetChildParameter(settings, i, Condition.BRANCH_NAME, 1);
                            var conditionRaw = listCommand.GetChildParameter(settings, i, "condition", 2);
                            var condition = ConvertToCondition(conditionRaw, toBranchName);
                            var conditions = new List<Condition>() { condition };

                            choices.Add(new NodeChoices.Choice(
                                text: new Text(choiceText, IncrementingTextID()), 
                                conditions: conditions));
                        }

                        nodes.Add(new NodeChoices(command.name, command.rawText, IncrementingNodeID(), new Text(title, IncrementingTextID()), choices));
                    }

                    else if (command.name.Equals(settings.COMMAND_URGENT, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var listCommand = command.ConvertToListCommand(settings, true);

                        var title = listCommand.GetParameter(settings, "title", 0);
                        var initialDelay = listCommand.GetParameter(settings, "initialDelay", 1);

                        // Convert choices
                        List<NodeUrgent.Choice> choices = new List<NodeUrgent.Choice>();
                        for (int i = 0; i < listCommand.childParameters.Count; i++)
                        {
                            var choiceText = listCommand.GetChildParameter(settings, i, "text", 0);
                            var toBranchName = listCommand.GetChildParameter(settings, i, Condition.BRANCH_NAME, 1);
                            var conditionRaw = listCommand.GetChildParameter(settings, i, "condition", 2);
                            var condition = ConvertToCondition(conditionRaw, toBranchName);
                            var conditions = new List<Condition>() { condition };
                            var speed = listCommand.GetChildParameter(settings, i, "speed", 3);
                            var color = listCommand.GetChildParameter(settings, i, "color", 4);
                            choices.Add(new NodeUrgent.Choice(new Text(choiceText, IncrementingTextID()), conditions, speed, color));
                        }

                        nodes.Add(new NodeUrgent(command.name, command.rawText, IncrementingNodeID(), choices, new Text(title, IncrementingTextID()), initialDelay));
                    }

                    else if (command.name.Equals(settings.COMMAND_IF, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var listCommand = command.ConvertToListCommand(settings, false);

                        // Convert conditions
                        List<Condition> conditions = new List<Condition>();
                        for (int i = 0; i < listCommand.childParameters.Count; i++)
                        {
                            var conditionRaw = listCommand.GetChildParameter(settings, i, NodeConditions.CONDITION, 0);
                            var toBranchName = listCommand.GetChildParameter(settings, i, Condition.BRANCH_NAME, 1);
                            var condition = ConvertToCondition(conditionRaw, toBranchName);
                            if (condition!=null)
                                conditions.Add(condition);
                        }

                        // Reposition condition without toBranchName as the last element
                        var conditionWithoutToBranchName = conditions.Find(condition => string.IsNullOrEmpty(condition.toBranchName));
                        if (conditionWithoutToBranchName != null)
                        {
                            conditions.Remove(conditionWithoutToBranchName);
                            conditions.Add(conditionWithoutToBranchName);
                        }

                        nodes.Add(new NodeConditions(command.name, command.rawText, IncrementingNodeID(), conditions));
                    }

                    else if (command.name.Equals(settings.COMMAND_SET, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string setOperatorKey = null;
                        NodeSet.OperationType operationType = NodeSet.OperationType.Invalid;

                        #region [Set operator key]
                        var condition = command.GetParameter(settings, NodeConditions.CONDITION, 0);
                        if (condition.Contains(settings.TOKEN_INCREMENT))
                        {
                            setOperatorKey = settings.TOKEN_INCREMENT;
                            operationType = NodeSet.OperationType.Increment;
                        }
                        else if (condition.Contains(settings.TOKEN_DECREMENT))
                        {
                            setOperatorKey = settings.TOKEN_DECREMENT;
                            operationType = NodeSet.OperationType.Decrement;
                        }
                        else if (condition.Contains(settings.TOKEN_MULTIPLICATION))
                        {
                            setOperatorKey = settings.TOKEN_MULTIPLICATION;
                            operationType = NodeSet.OperationType.Multiplication;
                        }
                        else if (condition.Contains(settings.TOKEN_DIVISION))
                        {
                            setOperatorKey = settings.TOKEN_DIVISION;
                            operationType = NodeSet.OperationType.Division;
                        }
                        else if (condition.Contains(settings.TOKEN_EQUAL))
                        {
                            setOperatorKey = settings.TOKEN_EQUAL;
                            operationType = NodeSet.OperationType.Equal;
                        }

                        if (setOperatorKey == null) Debug.Log("Cannot find valid set operator key in: " + command.rawText);

                        #endregion

                        var varNameValue = condition.SplitHalf(setOperatorKey);
                        var varName = varNameValue.Item1.Trim();
                        var value = varNameValue.Item2.Trim();

                        if (int.TryParse(value, out int intValue))
                            nodes.Add(new NodeSet(command.name, command.rawText, IncrementingNodeID(), new Node.Variable(varName, "int", value), operationType));
                        else
                            nodes.Add(new NodeSet(command.name, command.rawText, IncrementingNodeID(), new Node.Variable(varName, "str", value), operationType));

                    }

                    else if (command.name.Equals(settings.COMMAND_ONCE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var varName = "local_has_once_" + IncrementingVarID();
                        Node.Variable variable = new Node.Variable(varName, "str", "false");
                        var toAlternateBranchName = command.GetParameter(settings, Condition.BRANCH_NAME, 0);

                        nodes.Add(new NodeOnce("once_" + command.name, command.rawText, "", variable));

                        nodes.Add(new NodeConditions("once_if_" + command.name, command.rawText, IncrementingNodeID(), new List<Condition>()
                        {
                            new Condition(new Node.Variable(varName,"str","true"),settings.TOKEN_IS_EQUAL,toAlternateBranchName,settings),
                            new Condition(variable,settings.TOKEN_IS_EQUAL,"", settings) // Empty toBranchName always comes last, ordered like DTConnection
                        }));

                        nodes.Add(new NodeSet("once_set_" + command.name, command.rawText, IncrementingNodeID(), new Node.Variable(varName, "str", "true"), NodeSet.OperationType.Equal));
                    }

                    else
                    {
                        var nodeCanvasStatement = ConvertDSyntaxToStatement(settings, command);
                        nodes.Add(new NodeSay(
                            name: command.name, 
                            parameter: command.rawText, 
                            id: IncrementingNodeID(), 
                            text: new Text(nodeCanvasStatement.text,IncrementingTextID()), 
                            expression: (int)nodeCanvasStatement.expression, 
                            gesture: (int)nodeCanvasStatement.gesture));
                    }
                }

                return nodes;
            }

            Condition ConvertToCondition(string text, string toBranchName)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.Equals(settings.PASS, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return new Condition(null, "", toBranchName, settings);
                    }
                    else
                    {
                        string checkKey =
                            text.Contains(settings.TOKEN_IS_EQUAL) ? settings.TOKEN_IS_EQUAL :
                            text.Contains(settings.TOKEN_IS_NOT_EQUAL) ? settings.TOKEN_IS_NOT_EQUAL :
                            text.Contains(settings.TOKEN_IS_GREATER_OR_EQUAL) ? settings.TOKEN_IS_GREATER_OR_EQUAL :
                            text.Contains(settings.TOKEN_IS_LESS_OR_EQUAL) ? settings.TOKEN_IS_LESS_OR_EQUAL :
                            text.Contains(settings.TOKEN_IS_GREATER_THAN) ? settings.TOKEN_IS_GREATER_THAN :
                            text.Contains(settings.TOKEN_IS_LESS_THAN) ? settings.TOKEN_IS_LESS_THAN :
                            null;

                        if (checkKey == null)
                        {
                            Debug.Log("Cannot find valid condition check key in: " + text);
                            return new Condition(new Node.Variable(), "", toBranchName, settings);
                        }

                        var varNameValue = text.SplitHalf(checkKey);
                        var varName = varNameValue.Item1.Trim();
                        var value = varNameValue.Item2.Trim();

                        if (int.TryParse(value, out int intValue))
                            return new Condition(new Node.Variable(varName, "int", value), checkKey, toBranchName, settings);
                        else
                            return new Condition(new Node.Variable(varName, "str", value), checkKey, toBranchName, settings);
                    }
                }
                else return new Condition(null, Condition.CheckKey.Unassigned, toBranchName);
            }

        }

        public static List<string> GetActors(string text, DSyntaxSettings settings)
        {
            var allCommandNames = text.ExtractAll(settings.TOKEN_COMMAND_OPENING, settings.TOKEN_COMMAND_CLOSING, suppressWarning: true);

            // Extract actors
            var actors = new List<string>();
            foreach (var commandName in allCommandNames)
            {
                if(!settings.GetCommandNames().Contains(commandName.ToUpper()) && !actors.Contains(commandName))
                    actors.Add(commandName);
            }

            return actors;
        }

        /// <summary>
        /// Return a CSV file containing all texts in some nodes that should be localised
        /// </summary>
        public static string ConvertTreeToCSV(DSyntaxData.Tree tree)
        {
            var headerNames = new List<string>() { "key" };
            var languageCodes = Enum.GetNames(typeof(Encore.Localisations.LocalisationSystem.Language));
            headerNames.AddRange(languageCodes);


            // Header
            string cachedTextCSV = CSVUtility.WriteRow(headerNames.Count, headerNames);

            // Actors
            foreach (var actor in tree.actors)
                cachedTextCSV += CSVUtility.WriteRow(headerNames.Count, new List<string>() { actor.Key, actor.Key });

            // Nodes
            foreach (Branch branch in tree.branches)
                foreach (Node node in branch.nodes)
                {
                    if (node is NodeSay)
                    {
                        cachedTextCSV += CSVUtility.WriteRow(headerNames.Count, new List<string>(){ (node as NodeSay).text.id, (node as NodeSay).text.text });
                    }

                    else if (node is NodeChoices)
                    {
                        var titleText = (node as NodeChoices).title.text;
                        var titleID = (node as NodeChoices).title.id;

                        // Title
                        cachedTextCSV += CSVUtility.WriteRow(headerNames.Count, new List<string>(){ titleID, titleText });

                        // Choices
                        foreach (NodeChoices.Choice choice in (node as NodeChoices).choices)
                            cachedTextCSV += CSVUtility.WriteRow(headerNames.Count, new List<string>(){ choice.text.id, choice.text.text });

                    }
                    else if (node is NodeUrgent)
                    {
                        var titleText = (node as NodeUrgent).title.text;
                        var titleID = (node as NodeUrgent).title.id;

                        // Title
                        cachedTextCSV += CSVUtility.WriteRow(headerNames.Count, new List<string>() { titleID, titleText });

                        // Choices
                        foreach (NodeUrgent.Choice choice in (node as NodeUrgent).choices)
                            cachedTextCSV += CSVUtility.WriteRow(headerNames.Count, new List<string>() { choice.text.id, choice.text.text });
                    }
                }

            return cachedTextCSV;
        }

        static string GenerateGUID()
        {
            return System.Guid.NewGuid().ToString();
        }

        static string Generate16Digit()
        {
            System.Random RNG = new System.Random();
            var builder = new StringBuilder();
            while (builder.Length < 16)
            {
                builder.Append(RNG.Next(10).ToString());
            }
            return builder.ToString();
        }

        #endregion

        #region [Writer/Reader]

        #region [Classes]

        public class Command
        {
            public string name;
            public List<string> parameters;
            public string rawText;

            public Command(string name, List<string> parameters, string rawText)
            {
                this.name = name;
                this.parameters = parameters;
                this.rawText = rawText;
            }

            /// <summary>
            /// Returns a parameter using its parameterName; if failed, get it by index; if failed again, returns empty string
            /// </summary>
            /// <param name="parameterName">The name of the parameter which is located behind the separator token</param>
            /// <param name="index">Parameter's default location (index) inside the parameters list</param>
            public string GetParameter(DSyntaxSettings settings, string parameterName, int index, string defaultValue = "")
            {
                return GetParameter(settings.TOKEN_PARAMETER_NAME, parameters, parameterName, index, defaultValue);
            }

            /// <summary>
            /// Returns a parameter using its index; if failed, returns empty string
            /// </summary>
            /// <param name="index">Parameter's location (index) inside the parameters list</param>
            public string GetParameter(DSyntaxSettings settings, int index, string defaultValue = "")
            {
                return GetParameter(settings.TOKEN_PARAMETER_NAME, parameters, "", index, defaultValue);
            }

            public static string GetParameter(string parameterNameToken, List<string> parameters, string parameterName, int index, string defaultValue = "")
            {
                // Find parameter with parameterName
                if (!string.IsNullOrEmpty(parameterName))
                    foreach (var parameter in parameters)
                    {
                        var nameAndValue = parameter.SplitHalf(parameterNameToken);
                        var _parameterName = nameAndValue.Item1.Trim();
                        var _parameterValue = nameAndValue.Item2.Trim();
                        if (_parameterName.Equals(parameterName, System.StringComparison.CurrentCultureIgnoreCase))
                            return _parameterValue;
                    }

                // Find parameter with index
                var parameterByIndex = parameters.GetAt(index, defaultValue).Trim();

                // This parameter has a name which differs with the targeted parameterName
                if (!string.IsNullOrEmpty(parameterByIndex) && !string.IsNullOrEmpty(parameterByIndex.SplitHalf(parameterNameToken).Item1))
                    return defaultValue;

                // This parameter has no name, thus it's safe to assume this is the desired parameter
                else
                    return parameterByIndex;
            }
        }

        /// <summary>
        /// A class to group parameters based on unwrapped parameter
        /// </summary>
        public class ListCommand : Command
        {
            public List<List<string>> childParameters;

            public ListCommand(Command command, List<List<string>> childParameters) : base(command.name, command.parameters, command.rawText)
            {
                this.childParameters = childParameters;
            }

            /// <summary>Try to get a parameter of a childParameter if exists, else return defaultValue</summary>
            public string GetChildParameter(int indexList, int index, string defaultValue = "")
            {
                if (childParameters.Count > indexList)
                    return childParameters[indexList].GetAt(index, defaultValue);
                else
                    return defaultValue;
            }

            /// <summary>Try to get a parameter of a childParameter if exists, else return defaultValue</summary>
            public string GetChildParameter(DSyntaxSettings settings, int indexList, string parameterName, int index, string defaultValue = "")
            {
                if (childParameters.Count > indexList)
                {
                    var result = GetParameter(settings.TOKEN_PARAMETER_NAME, childParameters[indexList], parameterName, index, defaultValue);
                    return result;
                }
                else
                    return defaultValue;
            }


        }

        public class Parameter
        {
            public string name;
            public string value;

            public Parameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }

        #endregion

        #region [Methods: Write]

        /// <summary>Wrap command name using command opening and closing token, and wrap parameter text using parameter opening and closing token</summary>
        public static string WriteCommand(DSyntaxSettings settings, string commandName, string parameter, bool wrapParameter = false)
        {
            if (wrapParameter)
                return settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " " + WriteParameter(settings, parameter) + "\n";
            else
                return settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " " + parameter + "\n";
        }

        /// <summary>Wrap command name using command opening and closing token, and wrap parameter text using parameter opening and closing token</summary>
        public static string WriteCommand(DSyntaxSettings settings, string commandName, Parameter parameter, bool wrapParameter = false)
        {
            if (wrapParameter)
                return settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " " + WriteParameter(settings, parameter) + "\n";
            else
                return settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " " + parameter.value + "\n";
        }

        /// <summary>Wrap command name using command opening and closing token, and wrap parameter text using parameter opening and closing token</summary>
        public static string WriteCommand(DSyntaxSettings settings, string commandName, List<string> parameters, bool wrapFirstParameter = false)
        {
            string result = settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " ";
            if (!wrapFirstParameter)
            {
                result += parameters[0] + "\n";
                parameters.RemoveAt(0);
            }

            foreach (var parameter in parameters) result += WriteParameter(settings, parameter) + "\n";

            return result;
        }

        /// <summary>Wrap command name using command opening and closing token, and wrap parameter text using parameter opening and closing token</summary>
        public static string WriteCommand(DSyntaxSettings settings, string commandName, List<Parameter> parameters)
        {
            string result = settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " ";

            foreach (var parameter in parameters) result += WriteParameter(settings, parameter) + "\n";

            return result;
        }

        /// <summary>Wrap command name using command opening and closing token, and wrap parameter text using parameter opening and closing token</summary>
        /// <param name="wrapParentFirstParameter">First parent's parameter can have no wrapping tokens if there are more than one parent's parameter</param>
        public static string WriteListCommand(DSyntaxSettings settings, string commandName, List<string> parentParameters, List<List<string>> childParameters, bool wrapParentFirstParameter = false)
        {
            string result = settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " ";

            if (parentParameters != null)
            {
                if (parentParameters.Count > 1 && !wrapParentFirstParameter)
                {
                    result += parentParameters[0];
                    parentParameters.RemoveAt(0);
                }

                foreach (var parameter in parentParameters) result += WriteParameter(settings, parameter);
            }

            result += "\n";

            foreach (var parametersList in childParameters)
            {
                result += parametersList[0];
                parametersList.RemoveAt(0);
                foreach (var parameter in parametersList) result += WriteParameter(settings, parameter);
                result += "\n";
            }

            return result;
        }

        /// <summary>Wrap command name using command opening and closing token, and wrap parameter text using parameter opening and closing token</summary>
        /// <param name="wrapParentFirstParameter">First parent's parameter can have no wrapping tokens if there are more than one parent's parameter</param>
        public static string WriteListCommand(DSyntaxSettings settings, string commandName, List<Parameter> parentParameters, List<List<Parameter>> childParameters)
        {
            string result = settings.TOKEN_COMMAND_OPENING + commandName + settings.TOKEN_COMMAND_CLOSING + " ";

            if (parentParameters != null)
            {
                foreach (var parameter in parentParameters) result += WriteParameter(settings, parameter);
            }

            result += "\n";

            foreach (var parametersList in childParameters)
            {
                result += parametersList[0].value;
                parametersList.RemoveAt(0);
                foreach (var parameter in parametersList) result += WriteParameter(settings, parameter);
                result += "\n";
            }

            return result;
        }

        /// <summary>Wrap parameter text using Parameter opening and closing token</summary>
        public static string WriteParameter(DSyntaxSettings settings, string parameter)
        {
            return settings.TOKEN_PARAMETER_OPENING + parameter + settings.TOKEN_PARAMETER_CLOSING;
        }        
        
        /// <summary>Wrap parameter's name and value using Parameter opening and closing token</summary>
        public static string WriteParameter(DSyntaxSettings settings, Parameter parameter)
        {
            if (!string.IsNullOrEmpty(parameter.name))
                return settings.TOKEN_PARAMETER_OPENING + parameter.name + " : " + parameter.value + settings.TOKEN_PARAMETER_CLOSING;
            else
                return WriteParameter(settings, parameter.value);
        }

        /// <summary>Wrap each parameter texts using Parameter opening and closing token, but doesn't wrap empty parameters</summary>
        public static string WriteParameters(DSyntaxSettings settings, List<string> parameters)
        {
            string result = "";

            for (int i = parameters.Count - 1; i >= 0; i--)
                if (string.IsNullOrEmpty(parameters[i]))
                    result += WriteParameter(settings, parameters[i]);

            return result;
        }

        /// <summary>Wrap each parameter name and value using Parameter opening and closing token, but doesn't wrap empty parameters</summary>
        public static string WriteParameters(DSyntaxSettings settings, List<Parameter> parameters)
        {
            string result = "";

            for (int i = parameters.Count - 1; i >= 0; i--)
                if (parameters[i]!=null)
                    result += WriteParameter(settings, parameters[i]);

            return result;
        }

        #endregion

        #region [Methods: Read]

        /// <summary>Extract a list of parameters by using parameter tokens</summary>
        public static List<string> ReadParameters(DSyntaxSettings settings, string text)
        {
            return StringUtility.ExtractAll(text, settings.TOKEN_PARAMETER_OPENING, settings.TOKEN_PARAMETER_CLOSING, suppressWarning: true);
        }

        /// <summary>Extract parameter by using parameter tokens</summary>
        public static string ReadParameter(DSyntaxSettings settings, string text)
        {
            return StringUtility.Extract(text, settings.TOKEN_PARAMETER_OPENING, settings.TOKEN_PARAMETER_CLOSING, suppressWarning: true);
        }

        /// <summary>Extract command's name and its parameter from a dialogue syntax text</summary>
        public static Command ReadCommand(DSyntaxSettings settings, string text)
        {
            var _text = text;

            // Extract command name
            var commandName = _text.Extract(settings.TOKEN_COMMAND_OPENING, settings.TOKEN_COMMAND_CLOSING);
            _text = _text.ReplaceFirst(commandName, "");

            // Extract parameters that are wrapped by tokens
            var parameters = _text.ExtractAll(settings.TOKEN_PARAMETER_OPENING, settings.TOKEN_PARAMETER_CLOSING, suppressWarning: true);
            _text = _text.ReplaceFirst(parameters, "");

            // Extract one parameter that is not wrapped by tokens
            _text = RemoveWrapperTokens(settings, _text);
            if (!string.IsNullOrEmpty(_text))
                parameters.Insert(0, _text);

            return new Command(commandName, parameters, text);
        }

        /// <summary>Extract multiple command's name and its parameter into a list from a dialogue syntax text</summary>
        public static List<Command> ReadCommands(DSyntaxSettings settings, string text)
        {
            List<Command> result = new List<Command>();
            var commands = StringUtility.SplitAfterToken(text, settings.TOKEN_COMMAND_OPENING);
            foreach (var command in commands) result.Add(ReadCommand(settings, command));

            return result;
        }

        /// <summary> Extract multiple lists of commands by separating each group by a command name </summary>
        /// <param name="separatorCommandName">Command name which starts a group</param>
        /// <param name="removeSeparatorCommand">Whether to remove the command which separates groups</param>
        public static List<List<Command>> ReadCommandsByGroups(DSyntaxSettings settings, string text, string separatorCommandName)
        {
            List<List<Command>> result = new List<List<Command>>();

            var groups = StringUtility.SplitAfterToken(text, settings.TOKEN_COMMAND_OPENING + separatorCommandName + settings.TOKEN_COMMAND_CLOSING);
            foreach (var group in groups)
            {
                var commands = ReadCommands(settings, group);
                result.Add(commands);
            }

            return result;
        }

        #endregion

        public static ListCommand ConvertToListCommand (this Command command, DSyntaxSettings settings, bool parentHasParameters)
        {
            ListCommand listCommand = new ListCommand(command, childParameters: new List<List<string>>());
            listCommand.parameters = new List<string>();

            // Use temporal variable to store text
            var text = command.rawText.Trim();

            // Extract parent's parameter
            if (parentHasParameters)
            {
                // Extract parent's unwrapped parameters
                var parentUnwrappedParameter = text.Extract(settings.TOKEN_COMMAND_CLOSING, settings.TOKEN_PARAMETER_OPENING);
                if (!string.IsNullOrEmpty(parentUnwrappedParameter))
                {
                    listCommand.parameters.Add(parentUnwrappedParameter);
                    text = text.Remove(0, text.IndexOf(settings.TOKEN_PARAMETER_OPENING));
                }

                // Extract parent's wrapped parameters
                while (text.Length > 0)
                {
                    var param = text.Extract(settings.TOKEN_PARAMETER_OPENING, settings.TOKEN_PARAMETER_CLOSING);
                    listCommand.parameters.Add(param);
                    text = text.Remove(0, text.IndexOf(settings.TOKEN_PARAMETER_CLOSING));

                    if (!string.IsNullOrEmpty(text.Extract(settings.TOKEN_PARAMETER_CLOSING, settings.TOKEN_PARAMETER_OPENING)))
                        break;

                    text = text.Remove(0, text.IndexOf(settings.TOKEN_PARAMETER_CLOSING) + settings.TOKEN_PARAMETER_CLOSING.Length);
                }
            }
            else
            {
                text = text.RemoveByTokens(settings.TOKEN_COMMAND_OPENING, settings.TOKEN_COMMAND_CLOSING).Trim();
                text = text.Insert(0, settings.TOKEN_PARAMETER_CLOSING); // Add extra token at start, so it can be extracted by child
            }

            // Extract child's parameters
            while (text.Length > 0) 
            {
                // Prevent adding parameters if there are no more wrapping tokens
                if (text.IndexOf(settings.TOKEN_PARAMETER_OPENING) == -1 || text.IndexOf(settings.TOKEN_PARAMETER_CLOSING) == -1)
                    break;

                // Extract child's main parameters, and create new list
                var mainParameter = text.Extract(settings.TOKEN_PARAMETER_CLOSING, settings.TOKEN_PARAMETER_OPENING, suppressWarning: true);
                if (!string.IsNullOrEmpty(mainParameter))
                    listCommand.childParameters.Add(new List<string>() { mainParameter });
                text = text.Remove(0, text.IndexOf(settings.TOKEN_PARAMETER_OPENING));

                // Extract child's wrapped parameters
                var param = text.Extract(settings.TOKEN_PARAMETER_OPENING, settings.TOKEN_PARAMETER_CLOSING);
                listCommand.childParameters[listCommand.childParameters.Count-1].Add(param);
                text = text.Remove(0, text.IndexOf(settings.TOKEN_PARAMETER_CLOSING));
            }

            return listCommand;
        }

        public static string RemoveWrapperTokens(DSyntaxSettings settings, string text)
        {
            var result = text;
            result = result.Replace(settings.TOKEN_COMMAND_OPENING, "");
            result = result.Replace(settings.TOKEN_COMMAND_CLOSING, "");
            result = result.Replace(settings.TOKEN_PARAMETER_OPENING, "");
            result = result.Replace(settings.TOKEN_PARAMETER_CLOSING, "");
            result = result.Trim();
            return result;
        }

        #endregion

        #region [Custom Converter]

        public static NodeCanvas.DialogueTrees.Statement ConvertDSyntaxToStatement(DSyntaxSettings settings, Command command)
        {
            string statement = command.GetParameter(settings, "statement", 0);
            string expressionString = command.GetParameter(settings, "expression", 1);
            string gestureString = command.GetParameter(settings, "gesture", 2);
            string durationString = command.GetParameter(settings, "duration", 3);

            // Switch styling
            statement.ReplaceTokens(settings.bold.oldOpenToken, settings.bold.oldCloseToken, settings.bold.newOpenToken, settings.bold.newCloseToken);
            statement.ReplaceTokens(settings.italic.oldOpenToken, settings.italic.oldCloseToken, settings.italic.newOpenToken, settings.italic.newCloseToken);
            statement.ReplaceTokens(settings.underline.oldOpenToken, settings.underline.oldCloseToken, settings.underline.newOpenToken, settings.underline.newCloseToken);
            statement.ReplaceTokens(settings.strikethrough.oldOpenToken, settings.strikethrough.oldCloseToken, settings.strikethrough.newOpenToken, settings.strikethrough.newCloseToken);

            // Retrieve expression data
            int expression = -1;
            if (!string.IsNullOrEmpty(expressionString))
            {
                var hex = StringUtility.GetHex(expressionString);
                expression =
                    hex == settings.EXPRESSION_LISTENING ? (int)NodeCanvas.DialogueTrees.Expression.Listening :
                    hex == settings.EXPRESSION_SAD ? (int)NodeCanvas.DialogueTrees.Expression.Sad :
                    hex == settings.EXPRESSION_HAPPY_BIT ? (int)NodeCanvas.DialogueTrees.Expression.HappyBit :
                    hex == settings.EXPRESSION_HAPPY ? (int)NodeCanvas.DialogueTrees.Expression.Happy :

                    hex == settings.EXPRESSION_CONFUSED ? (int)NodeCanvas.DialogueTrees.Expression.Confused :
                    hex == settings.EXPRESSION_SURPRISED ? (int)NodeCanvas.DialogueTrees.Expression.Surprised :
                    hex == settings.EXPRESSION_ANGRY ? (int)NodeCanvas.DialogueTrees.Expression.Angry :
                    hex == settings.EXPRESSION_UNTRUST ? (int)NodeCanvas.DialogueTrees.Expression.Untrust : expression;
            }

            // Retrieve gesture data
            int gesture = -1;
            if (!string.IsNullOrEmpty(gestureString))
            {
                gesture =
                    gestureString.Equals(settings.GESTURE_SPEAKING, System.StringComparison.CurrentCultureIgnoreCase) ? (int)NodeCanvas.DialogueTrees.Gesture.Speaking :
                    gestureString.Equals(settings.GESTURE_NOD, System.StringComparison.CurrentCultureIgnoreCase) ? (int)NodeCanvas.DialogueTrees.Gesture.Nod :
                    gestureString.Equals(settings.GESTURE_THINKING, System.StringComparison.CurrentCultureIgnoreCase) ? (int)NodeCanvas.DialogueTrees.Gesture.Thinking :
                    gestureString.Equals(settings.GESTURE_PONDERING, System.StringComparison.CurrentCultureIgnoreCase) ? (int)NodeCanvas.DialogueTrees.Gesture.Pondering :

                    gestureString.Equals(settings.GESTURE_THIS, System.StringComparison.CurrentCultureIgnoreCase) ? (int)NodeCanvas.DialogueTrees.Gesture.This :
                    gestureString.Equals(settings.GESTURE_NOIDEA, System.StringComparison.CurrentCultureIgnoreCase) ? (int)NodeCanvas.DialogueTrees.Gesture.NoIdea :
                    gestureString.Equals(settings.GESTURE_LEANBACK, System.StringComparison.CurrentCultureIgnoreCase) ? (int)NodeCanvas.DialogueTrees.Gesture.LeanBack : gesture;
            }

            // Retrieve duration data
            float duration = 2.2f;
            float.TryParse(durationString, out duration);

            return new NodeCanvas.DialogueTrees.Statement()
            {
                text = statement,
                expression = (NodeCanvas.DialogueTrees.Expression)expression,
                gesture = (NodeCanvas.DialogueTrees.Gesture)gesture,
                duration = duration,
            };
        }

        #endregion

        #region [Sample Generators]

        public static string GenerateDialogueSimple(DSyntaxSettings settings, List<string> actorNames = null)
        {
            if (actorNames == null) actorNames = new List<string>() { "Zach", "Gabriel" };
            string GetRandomName() { return actorNames[UnityEngine.Random.Range(0, actorNames.Count)]; }

            var result = WriteCommand(settings, settings.COMMAND_BRANCH, settings.START, true);
            result += WriteCommand(settings, GetRandomName(), "Hello, World!");

            return result;
        }

        public static string GenerateDialogueMultiBranches(DSyntaxSettings settings, List<string> actorNames = null)
        {
            if (actorNames == null) actorNames = new List<string>() { "Zach", "Gabriel" };
            string GetRandomName() { return actorNames[UnityEngine.Random.Range(0, actorNames.Count)]; }

            var result = WriteCommand(settings, settings.COMMAND_BRANCH, settings.START, true);
            foreach (var actorName in actorNames) result += WriteCommand(settings, actorName, "Hello, World!");

            result += WriteCommand(settings, settings.COMMAND_BRANCH, "branch_one", true);
            foreach (var actorName in actorNames) result += WriteCommand(settings, actorName, "We're in branch_one");

            return result;
        }

        public static string GenerateDialogueChoices(DSyntaxSettings settings, List<string> actorNames = null)
        {
            if (actorNames == null) actorNames = new List<string>() { "Zach", "Gabriel" };
            string GetRandomName() { return actorNames[UnityEngine.Random.Range(0, actorNames.Count)]; }

            var result = WriteCommand(settings, settings.COMMAND_BRANCH, settings.START, true);
            result += WriteCommand(settings, GetRandomName(), "Let's choose!");

            result += WriteListCommand(settings, settings.COMMAND_CHOICES, 
                new List<string>() { "Title here" }, new List<List<string>>() 
                {
                    new List<string>(){ "Go to branch_one ", "branch_one" },
                    new List<string>(){ "Go to branch_two ", "branch_two" }
                },
                wrapParentFirstParameter: true);
  

            result += "\n";

            result += WriteCommand(settings, settings.COMMAND_BRANCH, "branch_one", true);
            result += WriteCommand(settings, GetRandomName(), "We are in branch_one");

            result += "\n";

            result += WriteCommand(settings, settings.COMMAND_BRANCH, "branch_two", true);
            result += WriteCommand(settings, GetRandomName(), "We are in branch_two");

            return result;
        }


        public static string GenerateComplex(DSyntaxSettings settings, List<string> actorNames = null)
        {
            if (actorNames == null) actorNames = new List<string>() { "Zach", "Gabriel" };
            string GetRandomName() { return actorNames[UnityEngine.Random.Range(0,actorNames.Count)]; }

            var result = WriteCommand(settings, settings.COMMAND_BRANCH, settings.START, true);
            result += WriteCommand(settings, GetRandomName(), "Let's choose!");

            result += WriteListCommand(settings, settings.COMMAND_CHOICES, 
                new List<string>() { "Title here" }, new List<List<string>>()
                {
                    new List<string>(){ "Go to branch_one ", "branch_one" },
                    new List<string>(){ "Go to branch_two ", "branch_two" }
                },
                wrapParentFirstParameter: true);


            result += "\n";

            result += WriteCommand(settings, settings.COMMAND_BRANCH, "branch_one", true);
            result += WriteCommand(settings, settings.COMMAND_ONCE, "branch_two");
            result += WriteCommand(settings, GetRandomName(), "We are in branch_one, but only for one time");
            result += WriteCommand(settings, settings.COMMAND_SET, "int_liar += 1");
            result += WriteCommand(settings, settings.COMMAND_GOTO, settings.START);

            result += "\n";

            result += WriteCommand(settings, settings.COMMAND_BRANCH, "branch_two", true);
            result += WriteCommand(settings, GetRandomName(), "We are in branch_two");
            result += WriteListCommand(settings, settings.COMMAND_URGENT, 
                new List<string> () { "Title here", "2" }, new List<List<string>>() 
                {
                    new List<string>(){"Let's go to branch_one", "branch_one", "speed:3", "color:red"},
                    new List<string>(){"Let's visit branch_three", "branch_three", "int_liar < 2", "speed:1" },
                    new List<string>(){"DONE", "branch_four" }
                });

            result += "\n";

            result += WriteCommand(settings, settings.COMMAND_BRANCH, "branch_three", true);
            result += WriteCommand(settings, GetRandomName(), "We are in branch_three, let go to branch_two");
            result += WriteCommand(settings, settings.COMMAND_GOTO, "branch_two");

            result += "\n";

            result += WriteCommand(settings, settings.COMMAND_BRANCH, "branch_four", true);
            result += WriteCommand(settings, GetRandomName(), "Finish!");

            return result;
        }

        #endregion
    }
}

