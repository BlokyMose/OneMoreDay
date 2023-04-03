using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System.Text;
using NodeCanvas.DialogueTrees;
using System;
using DialogueSyntax;
using static DialogueSyntax.DSyntaxUtility;
using Encore.Dialogues;
using Encore.Utility;
using Encore.Localisations;

/// <summary>
/// 
/// [About Nodes] 
/// - Each node doesn't necessarily represent a DialogueTrees's nodes (DT.Nodes)
/// - Nodes can be used to contract several complicated DT.Nodes
/// - Different nodes can refer to the same DT.Nodes, but with different properties
/// - Here are explanations of current nodes that are available:
/// 
///     - DSyntaxData.NodeSay: contains speaker name and what he says
///         [bob] Hello world 
///     
///     - DSyntaxData.NodeChoice: allows player to choose different options which will proceed the dialogue to different branch, ex: 
///         [CHOICES] {Choose a menu!}
///         Apple, please {branch_1} 
///         Orange, please {branch_2}
///     
///     - DSyntaxData.NodeUrgent: like DSyntaxData.NodeChoice, but with different UI and time limit, ex: 
///         [URGENT] Who are you? {2}
///         I'm Batman {branch_1} {speed:3} {color:red} 
///         I'm Vengeance {branch_2} {int_liar<2} {speed:1}
///     
///     - DSyntaxData.NodeGoTo: proceeds dialogue to a specified branch, ex:
///         [GOTO] {branch_1} 
///     
///     - DSyntaxData.NodeConditions: proceeds to a branch based on a condition of a variable;
///     without the target branch name, the dialogue proceeds expecting another node in this branch
///     without the condition, the dialogue automatically proceeds to the targeted branch, ex:
///         [IF] 
///         var_a > 2 {branch_1} 
///         var_a <= 2 {branch_2} 
///         var_b != Hello {} 
///         pass {branch_3} 
///     
///     - DSyntaxData.NodeSet: assigns a value to a variable, also declares the variable if needed,ex:
///         [SET] var_a = Hello,world 
///     
///     - NodeOnce: prevent player to repeat the dialogue after this node in this branch, but proceed to another branch,ex:
///         [ONCE] {branch_1} 
/// 
/// ========================================================
/// 
/// [About adding new nodes]
/// 1. Add new class in Classes: DialogueSyntax Data
/// 2. Add new if in GetNodes
/// 3. Add new if in GenerateDTNodes
/// 4. Add new if in GenerateDTConnections if the command branches abnormally
/// 5. Add new if in GetTree's extract variables part if the command may contain a variable or a condition inside
/// 
/// </summary>

public class DialogueConverterWindow : OdinEditorWindow
{
    #region Editor

    [MenuItem("Tools/Dialogue Converter")]
    public static void OpenWindow()
    {
        GetWindow<DialogueConverterWindow>("Dialogue Converter").Show();
    }

    private void Awake()
    {
        fileName = DEFAULT_FILE_NAME;

        // WARNING: Always put DialogueConverterWindowSettings.json beside this script
        // Find Settings file
        string thisFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        string thisFolderPath = thisFilePath.Substring(0, thisFilePath.Length - (GetType().Name +".cs").Length);
        string thisFolderRelativePath = thisFolderPath.Substring(thisFolderPath.IndexOf("Assets"));

        var settingsFile = AssetDatabase.LoadAssetAtPath<TextAsset>(thisFolderRelativePath + "DialogueConverterWindowSettings.json");
        if (settingsFile)
        {
            // Load data based on key which is the variable's name
            Dictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(settingsFile.text);
            settings.TryGetValue(nameof(saveFolderPath), out saveFolderPath);
            settings.TryGetValue(nameof(saveFolderPingFile), out saveFolderPingFile);
            settings.TryGetValue(nameof(playerName), out playerName);
        }

        var dataArray = AssetDatabase.FindAssets("t:" + nameof(DSyntaxSettings));
        if (dataArray != null)
        {
            var path = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            settings = AssetDatabase.LoadAssetAtPath<DSyntaxSettings>(path);
        }
    }

    private void OnDisable()
    {
        // Find Settings file
        string thisFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        string thisFolderPath = thisFilePath.Substring(0, thisFilePath.Length - (GetType().Name + ".cs").Length);
        string thisFolderRelativePath = thisFolderPath.Substring(thisFolderPath.IndexOf("Assets"));
        string settingsFilePath = thisFolderRelativePath + "DialogueConverterWindowSettings.json";

        if (File.Exists(settingsFilePath))
        {
            // Serialize settings that are needed to be saved
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {nameof(saveFolderPath), saveFolderPath },
                {nameof(saveFolderPingFile), saveFolderPingFile },
                {nameof(playerName), playerName },
            };
            string settingsJSON = JsonConvert.SerializeObject(settings, Formatting.Indented);

            // Clear the file before writing
            File.WriteAllText(settingsFilePath, "");

            var file = File.OpenWrite(settingsFilePath);
            byte[] text = new UTF8Encoding(true).GetBytes(settingsJSON);
            file.Write(text, 0, text.Length);
            file.Close();
        }

        AssetDatabase.Refresh();
    }

    [FoldoutGroup("Save Settings")]
    [HorizontalGroup("Save Settings/Ping")]
    [ButtonGroup("Save Settings/Ping/Save Folder")]
    [Button(ButtonHeight = 30)] [PropertyOrder(10)]
    void PingSaveFolder()
    {
        if (File.Exists(saveFolderPingFile))
        {
            Selection.activeObject = EditorGUIUtility.Load(saveFolderPingFile);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }

    [FoldoutGroup("Save Settings")] [Button(ButtonHeight = 30)] [PropertyOrder(10)] [ButtonGroup("Save Settings/Ping/Editor")]
    void PingEditorScript()
    {
        var scriptGUID = AssetDatabase.FindAssets(nameof(DialogueConverterWindow) + " t:Script");
        Selection.activeObject = EditorGUIUtility.Load(AssetDatabase.GUIDToAssetPath(scriptGUID[0]));
        EditorGUIUtility.PingObject(Selection.activeObject);

        #region Another way to get this filename
        //string thisFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
        //string thisFileRelativePath = thisFilePath.Substring(thisFilePath.IndexOf("Assets"));
        #endregion

    }

    #endregion

    #region UI: How to Use & Editor Settings

    [DetailedInfoBox("How To Use:", "1. Paste in input text \n" +
        "2. Click [Convert] \n" +
        "3. Check [Result], adjust accordingly \n" +
        "4. Type the [FileName] \n" +
        "5. Click [Save]", 
        InfoMessageType.None)]

    [CustomContextMenu("Generate Simple DialogueSyntax", nameof(GenerateSimpleDialogueSyntax))]
    [CustomContextMenu("Generate Choice DialogueSyntax", nameof(GenerateChoiceDialogueSyntax))]
    [CustomContextMenu("Generate Complex DialogueSyntax (for testing)", nameof(GenerateComplexDialogueSyntax))]


    #endregion

    #region Variables: Input-Output

    [TextArea(3,10),PropertyOrder(0),Space(15)]
    public string inputText = "";

    [FoldoutGroup("Converted Text", order: 4, VisibleIf = "@!string.IsNullOrEmpty(result)")][Space(10)][TextArea(1, 15)][HideLabel]
    public string result;

    DSyntaxData.Tree tree;

    #endregion

    #region Variables: Save

    [Header(" ")]
    [SuffixLabel(".asset")] [PropertyOrder(6)][Required]
    public string fileName;
    
    [FolderPath(RequireExistingPath = true)] [FoldoutGroup("Save Settings", order: 7)]
    public string saveFolderPath;
    
    [Sirenix.OdinInspector.FilePath(RequireExistingPath = true)][FoldoutGroup("Save Settings")]
    public string saveFolderPingFile;
    
    [FoldoutGroup("Save Settings")][Required][PropertyTooltip("Assign Choices node's actor with this actor if exists")]
    public string playerName = "Zach";
    
    [FoldoutGroup("Save Settings")]
    public bool makeDialogueBundle = true;      
    
    [FoldoutGroup("Save Settings")]
    public bool makeCSV = true;    
    
    [FoldoutGroup("Save Settings")]
    public bool makeDTFile = false;

    #endregion

    #region Variables: Data Handlers

    int incrementingNodeID = 0;
    int incrementingVarID = 0;
    string savedInputText;
    string savedFileName;
    string savedResult;
    private const string DEFAULT_FILE_NAME = "actorName_place_title";
    bool playerExists = false;
    //string cachedTextCSV;

    #endregion

    #region Variables: Graph Adjustment

    [FoldoutGroup("Graph Adjustment")] [PropertyOrder(2)]
    public Vector2 startPosition = new Vector2(800, 400);
    [FoldoutGroup("Graph Adjustment")] [PropertyOrder(2)]
    public Vector2 spacing = new Vector2(250, 100);

    #endregion

    #region Variables: Keys Adjustment

    [FoldoutGroup("Keys Adjustment", order: 3), InlineEditor]
    public DSyntaxSettings settings;

    #endregion

    // ====

    #region Methods: DialogueSyntax Data to DT Data

    Dictionary<string, object> GenerateDTData(DSyntaxData.Tree tree)
    {
        var actorParameters = GenerateDTActorParameter(tree);
        var nodes = GenerateDTNodes(tree);
        SetPositionOfNodes(nodes, tree.branches);
        var connections = GenerateDTConnections(tree.branches);
        var variables = GenerateDTVariables(tree.variables);

        Dictionary<string, object> dt = new Dictionary<string, object>()
        {
            {"type", "NodeCanvas.DialogueTrees.DialogueTree"},
            {"nodes", nodes},
            {"connections", connections},
            {"canvasGroups", new List<object>()},
            {"localBlackboard", new Dictionary<string, object>()
                {
                    { "_variables", variables}
                }
            },
            {"derivedData", new Dictionary<string, object>()
                {
                    {"actorParameters", actorParameters },
                    {"$type", "NodeCanvas.DialogueTrees.DialogueTree+DerivedSerializationData" },
                }
            }
        };

        return dt;
    }

    List<object> GenerateDTActorParameter(DSyntaxData.Tree tree)
    {
        List<object> actorParameters = new List<object>();

        //cachedTextCSV = string.Format("\"{0}\",\"{1}\",\"\"\n", "key", LocalisationSystem.Language.ENG.ToString());

        int index = 1;
        foreach (var actor in tree.actors)
        {
            Dictionary<string, object> actor_data = new Dictionary<string, object>()
                {
                    {"_keyName",actor.Key },
                    {"_id",actor.Value },
                    {"_actorObject", index },
                };
            index++;

            // Record
            actorParameters.Add(actor_data);
            //cachedTextCSV += string.Format("\"{0}\",\"{1}\",\"\"\n", actor.Key, actor.Key);
        }

        return actorParameters;

        //foreach (DSyntaxData.Branch branch in tree.branches)
        //{
        //    foreach (DSyntaxData.Node node in branch.nodes)
        //    {
        //        if(node is DSyntaxData.NodeSay)
        //        {
        //            if (!addedActorNames.ContainsKey(node.name))
        //            {
        //                Dictionary<string, object> actor = new Dictionary<string, object>()
        //                {
        //                    {"_keyName",node.name },
        //                    {"_id",GenerateGUID() },
        //                    {"_actorObject", addedActorNames.Count+1 },
        //                };

        //                // Find player name
        //                if (!playerExists) if (node.name == playerName) playerExists = true;

        //                // Record
        //                actorParameters.Add(actor);
        //                addedActorNames.Add(node.name, (string) actor["_id"]);
        //            }
        //        }
        //    }
        //}

    }

    List<object> GenerateDTNodes(DSyntaxData.Tree tree)
    {
        List<object> nodes = new List<object>();

        foreach (DSyntaxData.Branch branch in tree.branches)
        {
            foreach (DSyntaxData.Node node in branch.nodes)
            {
                if (node is DSyntaxData.NodeSay)
                {
                    Dictionary<string, object> nodeSay = new Dictionary<string, object>()
                    {
                        {"statement", new Dictionary<string,object>
                            {
                                {"_text",(node as DSyntaxData.NodeSay).text.text },
                                {"_expression",(node as DSyntaxData.NodeSay).expression },
                                {"_gesture",(node as DSyntaxData.NodeSay).gesture },
                                {"_textIndex", (node as DSyntaxData.NodeSay).text.id}
                            }
                        },
                        {"_actorName", node.name },
                        {"_actorParameterID", tree.actors[node.name] },
                        {"_position", new Dictionary<string, float>()
                            {
                                {"x",0 },
                                {"y",0 },
                            }
                        },
                        {"$type", "NodeCanvas.DialogueTrees.StatementNode"},
                        {"$id", node.id }
                    };
                    nodes.Add(nodeSay);

                    //cachedTextCSV += string.Format("\"{0}\",\"{1}\",\"\"\n", (node as DSyntaxData.NodeSay).text.id, (node as DSyntaxData.NodeSay).text.text);
                }

                else if (node is DSyntaxData.NodeChoices)
                {
                    var titleText = (node as DSyntaxData.NodeChoices).title.text;
                    var titleID = (node as DSyntaxData.NodeChoices).title.id;

                    // Generate choice
                    List<object> availableChoices = new List<object>();
                    foreach (DSyntaxData.NodeChoices.Choice choice in  (node as DSyntaxData.NodeChoices).choices)
                    {
                        Dictionary<string, object> choiceData = new Dictionary<string, object>();

                        // Write statement
                        choiceData.Add(
                            "statement", new Dictionary<string, object>()
                                {
                                    {"_text", choice.text.text},
                                    {"_textIndex", choice.text.id}
                                }
                            );
                        
                        // Write conditions if exists
                        var conditions = ExtractConditions(tree, choice.conditions);
                        if (conditions.Count > 0 && conditions.Find(c=>c!=null) != null) // Conditions cannot be a list of null conditions
                        {
                            choiceData.Add(
                            "condition", new Dictionary<string, object>()
                                {
                                    {"conditions", conditions },
                                    {"$type", "NodeCanvas.Framework.ConditionList"}
                                }
                            );
                        }


                        availableChoices.Add(choiceData);
                        //cachedTextCSV += string.Format("\"{0}\",\"{1}\",\"\"\n", choice.text.id, choice.text.text);
                    }

                    Dictionary<string, object> nodeChoice = null;
                    if (playerExists)
                    {
                        nodeChoice = new Dictionary<string, object>()
                        {
                            {"title",  titleText},
                            {"textIndex", titleID },
                            {"availableChoices", availableChoices },
                            {"_actorName", playerName },
                            {"_actorParameterID", tree.actors[playerName] },
                            {"_position", new Dictionary<string, float>()
                                {
                                    {"x",0 },
                                    {"y",0 },
                                }
                            },
                            {"$type", "NodeCanvas.DialogueTrees.MultipleChoiceNode"},
                            {"$id", node.id }
                        };

                    }
                    else
                    {
                        nodeChoice = new Dictionary<string, object>()
                        {
                            {"title", titleText },
                            {"textIndex", titleID },
                            {"availableChoices", availableChoices },
                            //{"_actorName", node.name },
                            //{"_actorParameterID", addedActorNames[node.name] },
                            {"_position", new Dictionary<string, float>()
                                {
                                    {"x",0 },
                                    {"y",0 },
                                }
                            },
                            {"$type", "NodeCanvas.DialogueTrees.MultipleChoiceNode"},
                            {"$id", node.id }
                        };
                    }

                    //cachedTextCSV += string.Format("\"{0}\",\"{1}\",\"\"\n", titleID, titleText);
                    nodes.Add(nodeChoice);
                }

                else if (node is DSyntaxData.NodeUrgent)
                {
                    var titleText = (node as DSyntaxData.NodeUrgent).title.text;
                    var titleID = (node as DSyntaxData.NodeUrgent).title.id;
                    var initialDelay = (node as DSyntaxData.NodeUrgent).initialDelay;

                    // Generate choice
                    List<object> availableChoices = new List<object>();
                    foreach (DSyntaxData.NodeUrgent.Choice choice in (node as DSyntaxData.NodeUrgent).choices)
                    {
                        Dictionary<string, object> choiceData = new Dictionary<string, object>();

                        // Write statement
                        choiceData.Add(
                            "statement", new Dictionary<string, object>()
                                {
                                    {"_text", choice.text.text},
                                    {"_meta", WriteParameter(settings,choice.speed)+WriteParameter(settings,choice.color)},
                                    {"_textIndex", choice.text.id}
                                }
                            );

                        // Write conditions if exists
                        var conditions = ExtractConditions(tree, choice.conditions);
                        if (conditions.Count > 0 && conditions.Find(c => c != null) != null) // Conditions cannot be a list of null conditions
                        {
                            choiceData.Add(
                            "condition", new Dictionary<string, object>()
                                {
                                    {"conditions", conditions },
                                    {"$type", "NodeCanvas.Framework.ConditionList"}
                                }
                            );
                        }


                        availableChoices.Add(choiceData);
                        //cachedTextCSV += string.Format("\"{0}\",\"{1}\",\"\"\n", choice.text.id, choice.text.text);
                    }

                    Dictionary<string, object> nodeUrgent = null;
                    if (playerExists)
                    {
                        nodeUrgent = new Dictionary<string, object>()
                        {
                            {"mode", (int)MultipleChoiceRequestInfo.Mode.Urgent },
                            {"title", titleText},
                            {"textIndex", titleID },
                            {"initialDelay", initialDelay },
                            {"availableChoices", availableChoices },
                            {"_actorName", playerName },
                            {"_actorParameterID", tree.actors[playerName] },
                            {"_position", new Dictionary<string, float>()
                                {
                                    {"x",0 },
                                    {"y",0 },
                                }
                            },
                            {"$type", "NodeCanvas.DialogueTrees.MultipleChoiceNode"},
                            {"$id", node.id }
                        };

                    }
                    else
                    {
                        nodeUrgent = new Dictionary<string, object>()
                        {
                            {"availableChoices", availableChoices },
                            {"title", titleText},
                            {"textIndex", titleID },
                            //{"_actorName", node.name },
                            //{"_actorParameterID", addedActorNames[node.name] },
                            {"_position", new Dictionary<string, float>()
                                {
                                    {"x",0 },
                                    {"y",0 },
                                }
                            },
                            {"$type", "NodeCanvas.DialogueTrees.MultipleChoiceNode"},
                            {"$id", node.id }
                        };
                    }

                    //cachedTextCSV += string.Format("\"{0}\",\"{1}\",\"\"\n", titleID, titleText);
                    nodes.Add(nodeUrgent);
                }

                else if (node is DSyntaxData.NodeConditions)
                {
                    List<object> conditions = ExtractConditions(tree, (node as DSyntaxData.NodeConditions).conditions );
                    Dictionary<string, object> nodeConditions = new Dictionary<string, object>()
                    {
                        {"conditions", conditions },
                        {"_position", new Dictionary<string, float>()
                            {
                                {"x",0 },
                                {"y",0 },
                            }
                        },
                        {"$type", "NodeCanvas.DialogueTrees.MultipleConditionNode"},
                        {"$id", node.id }
                    };

                    nodes.Add(nodeConditions);
                }

                else if (node is DSyntaxData.NodeSet)
                {

                    Dictionary<string, object> nodeSet = new Dictionary<string, object>();

                    if ((node as DSyntaxData.NodeSet).variable.varType == "int")
                    {
                        // TODO: check whether this is legacy feature or not 2022-1-28
                        //var type = "NodeCanvas.Tasks.Actions.SetVariable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + GetPublicKey(tree.variables, (node as DSyntaxData.NodeSet).variable.varName) + "]]";
                        nodeSet = new Dictionary<string, object>()
                        {
                            {"_action", new Dictionary<string,object>(){
                                { "valueA", new Dictionary<string, object>(){
                                    {"_name", (node as DSyntaxData.NodeSet).variable.varName},
                                    {"_targetVariableID", GetVariableID(tree.variables,(node as DSyntaxData.NodeSet).variable.varName)},
                                } },
                                { "Operation", (int)(node as DSyntaxData.NodeSet).operationType},
                                { "valueB", new Dictionary<string, object>(){
                                    {"_value",int.Parse((node as DSyntaxData.NodeSet).variable.varValue) }
                                } },
                                {"$type", "NodeCanvas.Tasks.Actions.SetInt"}
                            }},
                            {"_position", new Dictionary<string, float>()
                                {
                                    {"x",0 },
                                    {"y",0 },
                                }
                            },
                            {"$type", "NodeCanvas.DialogueTrees.ActionNode"},
                            {"$id", node.id }
                        };
                    }
                    else
                    {
                        var type = "NodeCanvas.Tasks.Actions.SetVariable`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + GetPublicKey(tree.variables, (node as DSyntaxData.NodeSet).variable.varName) + "]]";
                        nodeSet = new Dictionary<string, object>()
                        {
                            {"_action", new Dictionary<string,object>(){
                                { "valueA", new Dictionary<string, object>(){
                                    {"_name", (node as DSyntaxData.NodeSet).variable.varName},
                                    {"_targetVariableID", GetVariableID(tree.variables,(node as DSyntaxData.NodeSet).variable.varName)},
                                } },
                                { "Operation", (int)(node as DSyntaxData.NodeSet).operationType},
                                { "valueB", new Dictionary<string, object>(){
                                    {"_value",(node as DSyntaxData.NodeSet).variable.varValue }
                                } },
                                {"$type", type}
                            }},
                            {"_position", new Dictionary<string, float>()
                                {
                                    {"x",0 },
                                    {"y",0 },
                                }
                            },
                            {"$type", "NodeCanvas.DialogueTrees.ActionNode"},
                            {"$id", node.id }
                        };
                    }

                    nodes.Add(nodeSet);
                }
            }
        }

        return nodes;

        List<object> ExtractConditions(DSyntaxData.Tree tree, List<DSyntaxData.Condition> conditions)
        {
            List<object> conditionsObject = new List<object>();
            foreach (DSyntaxData.Condition condition in conditions)
            {
                Dictionary<string, object> conditionData = new Dictionary<string, object>();

                // With condition attached
                if (condition.variable != null)
                {
                    // Check if 
                    var left = tree.variables.Find(v => v.varName == condition.variable.varValue);
                    var right = tree.variables.Find(v => v.varName == condition.variable.varValue);

                    var valueA = new Dictionary<string, object>()
                                        {
                                            {"_name", condition.variable.varName},
                                            {"_targetVariableID", GetVariableID(tree.variables, condition.variable.varName)},
                                        };
                    var valueB = new Dictionary<string, object>();
                    if(right == null)
                    {
                        if (condition.variable.varType == "int")
                        {
                            valueB = new Dictionary<string, object>()
                            {
                                {"_value", int.Parse(condition.variable.varValue)}
                            };
                        }
                        else if (condition.variable.varType == "str")
                        {
                            valueB = new Dictionary<string, object>()
                            {
                                {"_value", condition.variable.varValue}
                            };
                        }
                    }
                    else
                    {
                        valueB = new Dictionary<string, object>()
                        {
                            {"_name", condition.variable.varValue},
                            {"_targetVariableID", GetVariableID(tree.variables, condition.variable.varValue)},
                        };
                    }



                    // Generate based on var type
                    if (condition.variable.varType == "int")
                    {
                        var type = left == null && right == null ?
                            "NodeCanvas.Tasks.Conditions.CheckInt" :
                            "NodeCanvas.Tasks.Conditions.CheckVariable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + GetPublicKey(tree.variables, condition.variable.varName) + "]";

                        var checkType = (int)condition.checkKey;
                        if (checkType < 0) checkType = 0;

                        conditionData = new Dictionary<string, object>()
                                {
                                    {"valueA", valueA},
                                    {"checkType", checkType},
                                    {"valueB", valueB},
                                    {"$type", type}
                                };
                    }
                    else
                    {
                        var type = left == null && right == null ? 
                            "NodeCanvas.Tasks.Conditions.CheckString" :
                            "NodeCanvas.Tasks.Conditions.CheckVariable`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken="+ GetPublicKey(tree.variables, condition.variable.varName) + "]";
                        conditionData = new Dictionary<string, object>()
                                {
                                    {"valueA", valueA},
                                    {"valueB", valueB},
                                    {"$type", type}
                                };
                    }

                    if (condition.checkKey == DSyntaxData.Condition.CheckKey.NotEqual)
                        conditionData.Add("_invert", true);
                }

                // No condition attached
                else
                {
                    conditionData = null;
                }

                conditionsObject.Add(conditionData);
            }

            return conditionsObject;
        }
    }

    List<object> GenerateDTConnections(List<DSyntaxData.Branch> branches)
    {
        List<object> connections = new List<object>();

        foreach (DSyntaxData.Branch branch in branches)
        {
            int i = 0;
            foreach (DSyntaxData.Node node in branch.nodes)
            {
                int index = i;

                // Don't make any connection at the first say node
                if (index !=0)
                {
                    if (node is DSyntaxData.NodeSay)
                    {
                        connections.Add(CreateConnection(branch.nodes[index - 1].id, node.id));
                    }
                    else if (node is DSyntaxData.NodeGoTo)
                    {
                        connections.Add(CreateConnection(branch.nodes[index - 1].id, GetBranchFirstNodeID(branches, (node as DSyntaxData.NodeGoTo).toBranchName)));
                        break; // No nodes should be added after DSyntaxData.NodeGoTo
                    }
                    else if (node is DSyntaxData.NodeChoices)
                    {
                        connections.Add(CreateConnection(branch.nodes[index - 1].id, node.id));

                        foreach (DSyntaxData.NodeChoices.Choice choice in (node as DSyntaxData.NodeChoices).choices)
                        {
                            var condiCon = CreateConnection(node.id, GetBranchFirstNodeID(branches, choice.toBranchName));
                            if (!connections.Contains(condiCon))
                            {
                                connections.Add(condiCon);
                            }
                        }
                        break;// No nodes should be added after DSyntaxData.NodeChoice
                    }
                    else if (node is DSyntaxData.NodeUrgent)
                    {
                        connections.Add(CreateConnection(branch.nodes[index - 1].id, node.id));

                        foreach (DSyntaxData.NodeUrgent.Choice choice in (node as DSyntaxData.NodeUrgent).choices)
                        {
                            var condiCon = CreateConnection(node.id, GetBranchFirstNodeID(branches, choice.toBranchName));
                            if (!connections.Contains(condiCon))
                            {
                                connections.Add(condiCon);
                            }
                        }
                        break;// No nodes should be added after DSyntaxData.NodeChoice
                    }
                    else if (node is DSyntaxData.NodeConditions)
                    {
                        connections.Add(CreateConnection(branch.nodes[index - 1].id, node.id));
                        bool hasEmptyToBranchName = false;
                        foreach (DSyntaxData.Condition condition in (node as DSyntaxData.NodeConditions).conditions)
                        {
                            if (!string.IsNullOrEmpty(condition.toBranchName))
                            {
                                var condiCon = CreateConnection(node.id, GetBranchFirstNodeID(branches, condition.toBranchName));
                                if (!connections.Contains(condiCon)) connections.Add(condiCon);
                            }

                            else // Expect another DSyntaxData.Node after this NodeCondition in his branch if there's an empty toBranchName
                                hasEmptyToBranchName = true;
                        }

                        if (!hasEmptyToBranchName) break;// No nodes should be added after this DSyntaxData.NodeConditions
                    }
                    else if (node is DSyntaxData.NodeSet)
                    {
                        connections.Add(CreateConnection(branch.nodes[index - 1].id, node.id));
                    }
                    else
                    {
                        connections.Add(CreateConnection(branch.nodes[index - 1].id, node.id));
                    }
                }

                // Only make connections after the first node
                else
                {
                    if (node is DSyntaxData.NodeChoices)
                    {
                        foreach (DSyntaxData.NodeChoices.Choice choice in (node as DSyntaxData.NodeChoices).choices)
                        {
                            connections.Add(CreateConnection(node.id, GetBranchFirstNodeID(branches, choice.toBranchName)));
                        }
                    }

                    if (node is DSyntaxData.NodeConditions)
                    {
                        foreach (DSyntaxData.Condition condition in (node as DSyntaxData.NodeConditions).conditions)
                        {
                            if (!string.IsNullOrEmpty(condition.toBranchName))
                                connections.Add(CreateConnection(node.id, GetBranchFirstNodeID(branches, condition.toBranchName)));
                        }
                    }
                }

                i++;
            }
        }

        return connections;

        Dictionary<string, object> CreateConnection(string sourceNode, string targetNode)
        {
            return new Dictionary<string, object>()
            {
                {"_sourceNode", new Dictionary<string,object>()
                    {
                        {"$ref", sourceNode },
                    }
                },
                {"_targetNode", new Dictionary<string,object>()
                    {
                        {"$ref", targetNode },
                    }
                },
                {"$type", "NodeCanvas.DialogueTrees.DTConnection" }
            };
        }
    }

    Dictionary<string,object> GenerateDTVariables(List<DSyntaxData.Variable> variables)
    {
        Dictionary<string,object> dtVariables = new Dictionary<string,object>();

        foreach (var variable in variables)
        {
            if (variable.varType == "int")
            {
                dtVariables.Add(variable.varName, new Dictionary<string, object>(){
                    {"_value",int.Parse(variable.varValue)},
                    {"_name",variable.varName},
                    {"_id",variable.id},
                    {"$type","NodeCanvas.Framework.Variable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + variable.publicKey + "]]"},
                });
            }
            else
            {
                dtVariables.Add(variable.varName, new Dictionary<string, object>(){
                    {"_value",variable.varValue},
                    {"_name",variable.varName},
                    {"_id",variable.id},
                    {"$type","NodeCanvas.Framework.Variable`1[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=" + variable.publicKey + "]]"},
                });
            }
        }

        return dtVariables;
    }

    class BranchPos
    {
        public DSyntaxData.Branch branch;
        public int row;
        public bool addColumn;

        public BranchPos(DSyntaxData.Branch b, int r, bool ac)
        {
            branch = b;
            row = r;
            addColumn = ac;
        }
    }

    void SetPositionOfNodes(List<object> nodes, List<DSyntaxData.Branch> branches)
    {
        List<BranchPos> branchesToProcess = new List<BranchPos>();
        List<DSyntaxData.Branch> finishedBranches = new List<DSyntaxData.Branch>();

        branchesToProcess.Add(new BranchPos(branches[0], 0, false));
        int currentColumn = 0;

        for (int i = 0; i < branchesToProcess.Count; i++)
        {
            finishedBranches.Add(branchesToProcess[i].branch);
            
            int currentRow = branchesToProcess[i].row;
            currentColumn += branchesToProcess[i].addColumn ? 1 : 0;

            foreach (DSyntaxData.Node node in branchesToProcess[i].branch.nodes)
            {
                if (node is DSyntaxData.NodeSay)
                {
                    Dictionary<string, object> n = (Dictionary<string, object>)nodes.Find(o => ((Dictionary<string, object>)o)["$id"].ToString() == node.id);
                    n["_position"] = new Dictionary<string, float>() {
                        {"x",startPosition.x+spacing.x*currentColumn },
                        {"y",startPosition.y+spacing.y*currentRow },
                    };
                    currentRow++;
                }
                else if (node is DSyntaxData.NodeChoices)
                {
                    DSyntaxData.NodeChoices nodeChoice = node as DSyntaxData.NodeChoices;

                    Dictionary<string, object> n = (Dictionary<string, object>)nodes.Find(o => ((Dictionary<string, object>)o)["$id"].ToString() == node.id);
                    n["_position"] = new Dictionary<string, float>() {
                        {"x",startPosition.x+spacing.x*currentColumn },
                        {"y",startPosition.y+spacing.y*currentRow },
                    };

                    currentRow = currentRow + 1 + nodeChoice.choices.Count / 2; // add extra space between DSyntaxData.NodeChoice and the next DSyntaxData.NodeSay

                    for (int choiceIndex = nodeChoice.choices.Count - 1; choiceIndex >= 0; choiceIndex--)
                    {
                        DSyntaxData.Branch toBranch = branches.Find(b => b.name == nodeChoice.choices[choiceIndex].toBranchName);

                        if (finishedBranches.Contains(toBranch))
                        {
                            continue;
                        }
                        else
                        {
                            bool addColumn = true;
                            if (choiceIndex == 0) addColumn = false;
                            branchesToProcess.Insert(i + 1, new BranchPos(toBranch, currentRow, addColumn));
                        }
                    }
                }
                else if (node is DSyntaxData.NodeUrgent)
                {
                    DSyntaxData.NodeUrgent nodeUrgent = node as DSyntaxData.NodeUrgent;

                    Dictionary<string, object> n = (Dictionary<string, object>)nodes.Find(o => ((Dictionary<string, object>)o)["$id"].ToString() == node.id);
                    n["_position"] = new Dictionary<string, float>() {
                        {"x",startPosition.x+spacing.x*currentColumn },
                        {"y",startPosition.y+spacing.y*currentRow },
                    };

                    currentRow = currentRow + 1 + nodeUrgent.choices.Count / 2; // add extra space between DSyntaxData.NodeChoice and the next DSyntaxData.NodeSay

                    for (int choiceIndex = nodeUrgent.choices.Count - 1; choiceIndex >= 0; choiceIndex--)
                    {
                        DSyntaxData.Branch toBranch = branches.Find(b => b.name == nodeUrgent.choices[choiceIndex].toBranchName);

                        if (finishedBranches.Contains(toBranch)) continue;
                        else
                        {
                            bool addColumn = true;
                            if (choiceIndex == 0) addColumn = false;
                            branchesToProcess.Insert(i + 1, new BranchPos(toBranch, currentRow, addColumn));
                        }
                    }
                }
                else if (node is DSyntaxData.NodeConditions)
                {
                    DSyntaxData.NodeConditions nc = node as DSyntaxData.NodeConditions;

                    Dictionary<string, object> n = (Dictionary<string, object>)nodes.Find(o => ((Dictionary<string, object>)o)["$id"].ToString() == node.id);
                    n["_position"] = new Dictionary<string, float>() {
                        {"x",startPosition.x+spacing.x*currentColumn },
                        {"y",startPosition.y+spacing.y*currentRow },
                    };

                    currentRow = currentRow + 1 + nc.conditions.Count / 2; // add extra space between DSyntaxData.NodeChoice and the next DSyntaxData.NodeSay

                    // Set condition which has no toBranchName as the first condition to be repositioned
                    var conditions = nc.conditions;
                    var conditionWithoutToBranchName = nc.conditions.Find(x => string.IsNullOrEmpty(x.toBranchName));
                    if (conditionWithoutToBranchName != null)
                    {
                        conditions = new List<DSyntaxData.Condition>();
                        foreach (var condition in nc.conditions) conditions.Add(condition);
                        conditions.Remove(conditionWithoutToBranchName);
                        conditions.Insert(0, conditionWithoutToBranchName);
                    }

                    for (int conditionIndex = conditions.Count - 1; conditionIndex >= 0; conditionIndex--)
                    {
                        if (string.IsNullOrEmpty(conditions[conditionIndex].toBranchName)) continue;

                        DSyntaxData.Branch toBranch = branches.Find(b => b.name == conditions[conditionIndex].toBranchName);
                        if (finishedBranches.Contains(toBranch)) continue;
                        else
                        {
                            bool addColumn = true;
                            if (conditionIndex == 0) addColumn = false;
                            branchesToProcess.Insert(i + 1, new BranchPos(toBranch, currentRow, addColumn));
                        }
                    }
                }
                else if (node is DSyntaxData.NodeGoTo)
                {
                    DSyntaxData.NodeGoTo ngt = node as DSyntaxData.NodeGoTo;
                    DSyntaxData.Branch toBranch = branches.Find(b => b.name == ngt.toBranchName);

                    currentRow++;
                    if (finishedBranches.Contains(toBranch)) continue;
                    else
                    {
                        branchesToProcess.Insert(i + 1, new BranchPos(toBranch, currentRow, false));
                    }
                }
                else if (node is DSyntaxData.NodeSet)
                {
                    Dictionary<string, object> n = (Dictionary<string, object>)nodes.Find(o => ((Dictionary<string, object>)o)["$id"].ToString() == node.id);
                    n["_position"] = new Dictionary<string, float>() {
                        {"x",startPosition.x+spacing.x*currentColumn },
                        {"y",startPosition.y+spacing.y*currentRow },
                    };
                    currentRow++;
                }
            }
        }
    }

    #endregion

    #region Methods: Utility

    string GetBranchFirstNodeID(List<DSyntaxData.Branch> branches, string branchName)
    {
        string id = "";
        foreach (DSyntaxData.Branch branch in branches)
        {
            if (branch.name == branchName)
            {
                id = branch.nodes[0].id;
            }
        }

        if (id == "")
        {
            Debug.LogWarning("[DialogueConverter] Cannot find branch: " + branchName);
        }

        return id;
    }

    string GetVariableID(List<DSyntaxData.Variable> variables, string varName)
    {
        var variable = variables.Find(x => x.varName == varName);
        if (variable == null) { Debug.Log("Cannot find variable: " + varName); return ""; }
        else return variable.id;
    }

    string GetPublicKey(List<DSyntaxData.Variable> variables, string varName)
    {
        var variable = variables.Find(x => x.varName == varName);
        if (variable == null) { Debug.Log("Cannot find variable: " + varName); return ""; }
        else return variable.publicKey;
    }

    #endregion
    
    // ====

    #region Methods: UI Buttons

    [EnableIf("@!string.IsNullOrEmpty(inputText)")]
    [Button(ButtonHeight = 30),PropertyOrder(1),GUIColor("@Color.green")]
    void Convert()
    {
        string _inputText = inputText;
        _inputText = StringUtility.RemoveByTokens(_inputText,settings.TOKEN_COMMENT_OPENING,settings.TOKEN_COMMENT_CLOSING);

        tree = GetTree(_inputText, settings);
        if (tree == null) return;

        playerExists = false;
        foreach (var actor in tree.actors)
            if (actor.Key == playerName) { playerExists = true; break; }

        Dictionary<string, object> dt = GenerateDTData(tree);
        result = JsonConvert.SerializeObject(dt, Formatting.Indented);

        #region [Debug Result]

        string variables = ""; int index = 0;
        foreach (var variable in tree.variables)
        {
            index++;
            variables += index + ". " + variable.varType + " " + variable.varName + " = " + variable.varValue + "\n";
        }

        string actorNames = "";
        foreach (var actor in tree.actors) actorNames += actor.Key + ", ";
        Debug.Log("Success!\n" +
            "Actors: " + actorNames + "\n-\n" +
            "Variables: \n" + variables); 
        
        #endregion
    }

    [EnableIf("@!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(fileName)"),LabelText("@ConvertFirstString()"), GUIColor("@Color.green")]
    [Button(ButtonHeight = 30)] [PropertyOrder(8)]
    void Save()
    {
        // Create folders
        string dtAssetFolderPath = saveFolderPath + "/DTAsset";
        string dtAssetPath = dtAssetFolderPath + "/" + fileName + ".asset";
        System.IO.Directory.CreateDirectory(dtAssetFolderPath);

        string csvFolderPath = saveFolderPath + "/CSV";
        string csvPath = csvFolderPath + "/" + fileName + ".csv";
        System.IO.Directory.CreateDirectory(csvFolderPath);

        string dtJSONFolderPath = saveFolderPath + "/DTJson";
        string dtJSONPath = dtJSONFolderPath + "/" + fileName + ".DT";
        System.IO.Directory.CreateDirectory(dtJSONFolderPath);

        string dialogueBundleFolderPath = saveFolderPath + "/DialogueBundle";
        string dialogueBundlePath = dialogueBundleFolderPath + "/" + fileName + ".asset";
        System.IO.Directory.CreateDirectory(dialogueBundleFolderPath);

        // Generate DialogueTree asset
        var dialogueTreeAsset = AssetDatabase.LoadMainAssetAtPath(dtAssetPath);
        DialogueTree dialogueTree = null;
        if (dialogueTreeAsset)
        {
            if (!EditorUtility.DisplayDialog("Same Asset Name", "You are to overwrite a dialogue tree with the same name. (Please backup first if necessary)\n\nAre you sure to overwrite?", "Yes", "No"))
                return;
            dialogueTree = dialogueTreeAsset as DialogueTree;
            (dialogueTreeAsset as DialogueTree).Deserialize(result, null, true);
            Debug.Log("Overwrote asset: " + dtAssetPath);
        }
        else
        {
            dialogueTree = ScriptableObject.CreateInstance<DialogueTree>();
            AssetDatabase.CreateAsset(dialogueTree, dtAssetPath);

            dialogueTree.Deserialize(result, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log("Created asset: " + dtAssetPath);
        }

        // Generate csv 
        if (makeCSV)
        {
            File.WriteAllText(csvPath, ConvertTreeToCSV(tree));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // Generate DT File
        if (makeDTFile)
        {
            if (File.Exists(dtJSONPath))
            {
                if (!EditorUtility.DisplayDialog("Same File Name", "You are to overwrite a file with the same name\n\nAre you sure?", "Yes", "No"))
                    return;

                // Clear file before rewriting
                File.WriteAllText(dtJSONPath, "");

                var file = File.OpenWrite(dtJSONPath);
                byte[] text = new UTF8Encoding(true).GetBytes(result);
                file.Write(text, 0, text.Length);
                file.Close();
                Debug.Log("[DialogueConverter] Overwrote file: " + dtJSONPath);
            }
            else
            {
                if (fileName == DEFAULT_FILE_NAME)
                    if (!EditorUtility.DisplayDialog("Boring File Name", "You are saving the file using the default file name:\n\n" + DEFAULT_FILE_NAME + "\n\nAre you sure?", "Yes", "No"))
                        return;

                var file = File.CreateText(dtJSONPath);
                file.Write(result);
                file.Close();
                Debug.Log("[DialogueConverter] Created file: " + dtJSONPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // Generate DialogueBundle asset
        if (makeDialogueBundle)
        {
            var dialogueBundleAsset = AssetDatabase.LoadMainAssetAtPath(dialogueBundlePath);
            DialogueBundle dialogueBundle = null;
            if (dialogueBundleAsset)
            {
                if (!EditorUtility.DisplayDialog("Same Bundle Name", "You are to overwrite a dialogue bundle with the same name. (Please backup first if necessary)\n\nAre you sure to overwrite?", "Yes", "No"))
                    return;
                dialogueBundle = dialogueBundleAsset as DialogueBundle;
                Debug.Log("Overwrote asset: " + dialogueBundlePath);
            }
            else
            {
                dialogueBundle = ScriptableObject.CreateInstance<DialogueBundle>();
                AssetDatabase.CreateAsset(dialogueBundle, dialogueBundlePath);
                AssetDatabase.SaveAssets();
                Debug.Log("Created asset: " + dtAssetPath);
            }

            dialogueBundle.DialogueTree = dialogueTree;
            dialogueBundle.CSV = AssetDatabase.LoadMainAssetAtPath(csvPath) as TextAsset;
            dialogueBundle.DTJSON = AssetDatabase.LoadMainAssetAtPath(dtJSONPath) as TextAsset;
            dialogueBundle.DSyntax = inputText;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // Cache saved data
        savedInputText = inputText; 
        savedFileName = fileName;
        savedResult = result;

        // Reset
        inputText = "";
        fileName = DEFAULT_FILE_NAME;
        result = "";

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [EnableIf("@!string.IsNullOrEmpty(savedFileName)")][Button(ButtonHeight = 30)][PropertyOrder(9)] [LabelText("@PingInProjectString()")]
    void ShowInProject()
    {
        string path = saveFolderPath + "/" + savedFileName + ".asset";

        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    [Button(ButtonHeight = 30)] [ShowIf(nameof(CanOpenAnalysis))] [PropertyOrder(5)]
    public void OpenAnalysis()
    {
        DialogueAnalysisWindow.OpenWindow(tree.branches);
    }

    public bool CanOpenAnalysis()
    {
        if (tree==null) return false;
        if (tree.branches==null) return false;
        if (tree.branches.Count==0) return false;
        return true;
    }

    #endregion

    #region Methods: UI Utility

    string PingInProjectString(){ return "Ping [" + savedFileName + "] in Project";}

    string ConvertFirstString() { return string.IsNullOrEmpty(result) ? "Save: No converted text" : "Save"; }

    void GenerateSimpleDialogueSyntax()
    {
        inputText = DSyntaxUtility.GenerateDialogueSimple(settings);
    }

    void GenerateChoiceDialogueSyntax()
    {
        inputText = GenerateDialogueChoices(settings);
    }

    void GenerateComplexDialogueSyntax()
    {
        inputText = GenerateComplex(settings);
    }

    #region DT File Template

    Dictionary<string, object> dtTemplate = new Dictionary<string, object>()
    {
        {"type", "NodeCanvas.DialogueTrees.DialogueTree"},
        {"nodes", new List<object>()},
        {"connections", new List<object>()},
        {"canvasGroups", new List<object>()},
        {"localBlackboard", new Dictionary<string, object>()
            {
                { "_variables", new List<object>() }
            }
        },
        {"derivedData", new Dictionary<string, object>()
            {
                { "actorParameters", new Dictionary<string,object>()}
            }
        },

    };

    Dictionary<string, object> node_say_template = new Dictionary<string, object>()
    {
        {"statement", new Dictionary<string, object>()},
        {"_actorName", ""},
        {"_actorParameterID", ""},
        {"_position", new Dictionary<string, int>()},
        {"$type", ""},
        {"$id", ""},
    };

    Dictionary<string, object> node_choice_template = new Dictionary<string, object>()
    {
        {"availableChoices", new List<object>()},
        //{"_actorName", ""},
        //{"_actorParameterID", ""},
        {"_position", new Dictionary<string, int>()},
        {"$type", ""},
        {"$id", ""},
    };

    Dictionary<string, object> connection_template = new Dictionary<string, object>()
    {
        {"_sourceNode", new Dictionary<string, string>()
            {
                {"$ref", "0" }
            }
        },
        {"_targetNode", new Dictionary<string, string>()
            {
                {"$ref", "0" }
            }
        },
        {"$type", ""},
    };

    Dictionary<string, object> actorParameter_template = new Dictionary<string, object>()
    {
        {"_keyName", ""},
        {"_id", ""},
        {"_actorObject", 0},
    };

    #endregion

    #endregion

}
