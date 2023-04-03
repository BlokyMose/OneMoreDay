using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DialogueSyntax;
using Encore.Dialogues;
using Encore.Utility;
using System;

/// <summary>
/// [GENERAL IDEA]<br></br>
/// Edit DSyntax inside a TextAsset or a component's <see cref="DSyntaxString"/><br></br><br></br>
/// 
/// [FLOW]<br></br>
/// To add a new command:<br></br>
/// 1. Open CommandUIDrawersSettings.txt, and add a new [command] with its parameters (Parameter, Label, Break, List)
/// 2. For parameter, make sure to edit its properties for according to the desired design
/// 3. Open StringGroupSettings.txt, and add a new [group] if needed
/// 
/// </summary>
namespace Encore.Editor.DSyntaxEditor
{
    public class DSyntaxEditor : EditorWindow
    {
        #region [Editor]

        [MenuItem("Tools/DSyntax Editor")]
        public static void OpenWindow()
        {
            GetWindow<DSyntaxEditor>("DSyntax").Show();
        }

        public static void OpenWindow(TextAsset textAsset, Action<string> OnSetDSyntax, Func<string> GetDSyntax)
        {
            var window = GetWindow<DSyntaxEditor>("DSyntax");
            window.textAsset = textAsset;
            window.OnSetDSyntax = OnSetDSyntax;
            window.GetDSyntax = GetDSyntax;
            window.Show();
            window.OnEnable();
        }

        #endregion

        #region [Class: UIDrawer]

        public class CommandUIDrawer
        {
            public string commandName;
            public string commandType;
            public SubUIListDrawer ui = new SubUIListDrawer();
            public List<SubUIListDrawer> listUIs = new List<SubUIListDrawer>();
            public bool hasBreak = false;

            public const string COMMAND = "COMMAND";
            public const string SPEECH = "SPEECH";
            public const string DEFAULT = "DEFAULT";
            public SubUIListDrawer listSubUITemplate = new SubUIListDrawer();

            public CommandUIDrawer(string commandName, string commandType, SubUIListDrawer ui, bool hasBreak)
            {
                this.commandName = commandName;
                this.commandType = commandType;
                this.ui = ui;
                this.hasBreak = hasBreak;
            }

            public CommandUIDrawer(CommandUIDrawer copy)
            {
                commandName = copy.commandName;
                commandType = copy.commandType;
                hasBreak = copy.hasBreak;

                ui = new SubUIListDrawer();
                foreach (var subUI in copy.ui.subUIs)
                    ui.subUIs.Add(subUI.DeepCopy());

                listSubUITemplate = new SubUIListDrawer();
                foreach (var subUI in copy.listSubUITemplate.subUIs)
                    listSubUITemplate.subUIs.Add(subUI.DeepCopy());

                listUIs = new List<SubUIListDrawer>();
                foreach (var list in copy.listUIs)
                {
                    listUIs.Add(new SubUIListDrawer());
                    foreach (var subUI in list.subUIs)
                        listUIs[listUIs.Count - 1].subUIs.Add(subUI.DeepCopy());
                }
            }

            public SubUIListDrawer GetListUITemplateCopy()
            {
                var newList = new List<SubUIDrawer>();
                foreach (var subUI in listSubUITemplate.subUIs)
                {
                    newList.Add(subUI.DeepCopy());
                }
                return new SubUIListDrawer(newList);
            }
        }

        public class SubUIListDrawer
        {
            public List<SubUIDrawer> subUIs = new List<SubUIDrawer>();
            public SubUIListDrawer()
            {
                subUIs = new List<SubUIDrawer>();
            }

            public SubUIListDrawer(List<SubUIDrawer> subUIs)
            {
                this.subUIs = subUIs;
            }

            public ParameterUIDrawer GetParameter(string parameterName)
            {
                foreach (var subUI in subUIs)
                {
                    if (subUI is ParameterUIDrawer)
                    {
                        var parameterUI = subUI as ParameterUIDrawer;
                        if (parameterUI.parameterName.ToLower() == parameterName.ToLower())
                            return parameterUI;
                    }
                }

                return null;
            }
        }

        public class SubUIDrawer
        {
            public float width;
            public float height;

            public SubUIDrawer(float width, float height)
            {
                this.width = width;
                this.height = height;
            }

            public virtual SubUIDrawer DeepCopy() { return new SubUIDrawer(width, height); }
        }

        public class ParameterUIDrawer : SubUIDrawer
        {
            public const string PARAMETER = "PARAMETER";
            public string parameterName;
            public string parameterType;
            public string parameterValue;
            public string alignment;
            public string stringGroupName;
            public string stringGroupType;
            public string exportName;
            public bool isTextArea = false;

            public Color parameterColor;

            // Parameter Types
            public const string STRING = "STRING";
            public const string FLOAT = "FLOAT";

            // String Group Types
            public const string ADD = "ADD";
            public const string CHECK = "CHECK";

            // Alignment
            public const string LEFT = "LEFT";
            public const string UPPER_LEFT = "UPPER_LEFT";
            public const string CENTER = "CENTER";
            public const string RIGHT = "RIGHT";

            public ParameterUIDrawer(float width, float height, string parameterName, string parameterType, string parameterValue, string stringGroupName, string stringGroupType, string alignment, string exportName, bool isTextArea) : base(width, height)
            {
                this.parameterName = parameterName;
                this.parameterType = parameterType;
                this.parameterValue = parameterValue;
                this.stringGroupName = stringGroupName;
                this.stringGroupType = stringGroupType;
                this.alignment = alignment;
                this.exportName = !string.IsNullOrEmpty(exportName) ? exportName : parameterName;
                this.isTextArea = isTextArea;
            }

            public UnityEngine.TextAnchor GetAlignment()
            {
                return
                    alignment.ToUpper() == LEFT ? TextAnchor.MiddleLeft :
                    alignment.ToUpper() == CENTER ? TextAnchor.MiddleCenter :
                    alignment.ToUpper() == RIGHT ? TextAnchor.MiddleRight :
                    alignment.ToUpper() == UPPER_LEFT ? TextAnchor.UpperLeft :
                    TextAnchor.MiddleLeft;
            }

            public override SubUIDrawer DeepCopy()
            {
                var newSubUIDrawer = new ParameterUIDrawer(
                    width, 
                    height, 
                    parameterName, 
                    parameterType, 
                    parameterValue,
                    stringGroupName, 
                    stringGroupType, 
                    alignment,
                    exportName,
                    isTextArea);
                return newSubUIDrawer;
            }


        }

        public class LabelUIDrawer : SubUIDrawer
        {
            public const string LABEL = "LABEL";
            public string labelName;
            public string alignment;

            public const string LEFT = "LEFT";
            public const string CENTER = "CENTER";
            public const string RIGHT = "RIGHT";

            public LabelUIDrawer(float width, float height, string labelName, string alignment) : base(width, height)
            {
                this.labelName = labelName;
                this.alignment = alignment;
            }

            public UnityEngine.TextAnchor GetAlignment()
            {
                return
                    alignment.ToUpper() == LEFT ? TextAnchor.MiddleLeft :
                    alignment.ToUpper() == CENTER ? TextAnchor.MiddleCenter :
                    alignment.ToUpper() == RIGHT ? TextAnchor.MiddleRight :
                    TextAnchor.MiddleLeft;
            }

            public override SubUIDrawer DeepCopy()
            {
                var newSubUIDrawer = new LabelUIDrawer(width, height, labelName, alignment);
                return newSubUIDrawer;
            }
        }

        public class BreakUIDrawer : SubUIDrawer
        {
            public const string BREAK = "BREAK";

            public BreakUIDrawer(float width, float height) : base(width, height)
            {
            }

            public override SubUIDrawer DeepCopy()
            {
                var newSubUIDrawer = new BreakUIDrawer(width, height);
                return newSubUIDrawer;
            }
        }

        public class ListUIDrawer : SubUIDrawer
        {
            public const string LIST = "LIST";

            public ListUIDrawer(float width, float height) : base(width, height)
            {
            }

            public override SubUIDrawer DeepCopy()
            {
                var newSubUIDrawer = new ListUIDrawer(width, height);
                return newSubUIDrawer;
            }
        }

        #endregion

        #region [Class: StringGroup]

        public class StringGroup
        {
            public const string ADD = "ADD";
            public const string PLURAL = "PLURAL";
            public const string CHECK = "CHECK";

            public string sgName;
            public List<SGWord> sgWords = new List<SGWord>();

            public StringGroup(string sgName, List<SGWord> sgWords)
            {
                this.sgName = sgName;
                this.sgWords = sgWords;
            }
        }

        public class SGWord
        {
            public string word;

            public SGWord(string word)
            {
                this.word = word;
            }

            public virtual bool Compare(string word)
            {
                return this.word == word;
            }

            public virtual string GetDisplayWord()
            {
                return this.word;
            }
        }

        public class SGWordSynonym : SGWord
        {
            public List<string> synonyms = new List<string>();
            public int displayIndex = 0;

            public SGWordSynonym(string word, List<string> synonyms, int displayIndex) : base(word)
            {
                this.synonyms = synonyms;
                this.displayIndex = displayIndex;
                if (!synonyms.Contains(word)) synonyms.Insert(0, word);
            }

            public override bool Compare(string word)
            {
                foreach (var w in synonyms)
                    if (w == word) return true;
                return false;
            }

            public override string GetDisplayWord()
            {
                return synonyms[displayIndex];
            }
        }

        #endregion

        #region [Vars: Properties]

        const float subUIDefaultWidth = 100;
        const float subUIDefaultHeight = 25;
        Padding padding = new Padding(10, 5, 10, 10);
        const float commandNameLabelWidth = 50f;

        const string NONE = "_none_";
        const string SAY = "SAY";
        const int footerHeight = 25;

        #endregion

        #region [Vars: Settings]

        public TextAsset commandUIDrawersSettings;
        public TextAsset stringGroupSettings;
        public Dictionary<string,CommandUIDrawer> commandUIDrawers = new Dictionary<string,CommandUIDrawer>();
        public Dictionary<string,StringGroup> stringGroups = new Dictionary<string,StringGroup>();
        public DSyntaxSettings dSyntaxSettings;

        #endregion

        #region [Vars: File Components]

        public TextAsset textAsset;
        public Action<string> OnSetDSyntax;
        public Func<string> GetDSyntax;

        #endregion

        #region [Vars: Data Handlers]

        public DSyntaxData.Tree tree;
        public List<CommandUIDrawer> commandsWithData = new List<CommandUIDrawer>();
        public Color defaultGUIColor;
        public Color defaultGUIContentColor;
        Vector2 scrollViewPos;
        string toAddCommand = SAY;

        #endregion

        #region [Methods: Main]

        private void OnEnable()
        {
            // CommandUIDrawersSettings.txt and StringGroupSettingsPath.txt have to exist in the same folder with this file

            var dSyntaxEditorScripts = AssetDatabase.FindAssets("t:script " + nameof(DSyntaxEditor));
            var thisFilePath = AssetDatabase.GUIDToAssetPath(dSyntaxEditorScripts[0]);
            string thisFolderPath = thisFilePath.Substring(0, thisFilePath.Length - (GetType().Name + ".cs").Length);

            var commandUIDrawersSettingsPath = thisFolderPath + "/" + "CommandUIDrawersSettings.txt";
            commandUIDrawersSettings = AssetDatabase.LoadAssetAtPath<TextAsset>(commandUIDrawersSettingsPath);            
            
            var stringGroupSettingsPath = thisFolderPath + "/" + "StringGroupSettings.txt";
            stringGroupSettings = AssetDatabase.LoadAssetAtPath<TextAsset>(stringGroupSettingsPath);

            var allDSyntaxSettings = AssetDatabase.FindAssets("t:" + nameof(DSyntaxSettings));
            var dSYntaxSettingPath = AssetDatabase.GUIDToAssetPath(allDSyntaxSettings[0]);
            dSyntaxSettings = AssetDatabase.LoadAssetAtPath<DSyntaxSettings>(dSYntaxSettingPath);

            commandUIDrawers = GetCommandUIDrawers(dSyntaxSettings, commandUIDrawersSettings.text);

            ResetStringGroupsList();

            commandsWithData = new List<CommandUIDrawer>();
            if (GetDSyntax != null)
                tree = DSyntaxUtility.GetTree(GetDSyntax(), dSyntaxSettings);
            else if (textAsset != null)
                tree = DSyntaxUtility.GetTree(textAsset.text, dSyntaxSettings);

            defaultGUIColor = GUI.color;
            defaultGUIContentColor = GUI.contentColor;
        }

        private void OnGUI()
        {
            var currentY = padding.top;
            var currentX = padding.left;
            var availableWidth = position.width - padding.left - padding.right;
            ProcessCommands();

            MakeDSyntaxStringControllers();
            MakeTextAssetControllers();
            currentX = padding.left;
            currentY += subUIDefaultHeight + 5;

            MakeActorsList();
            currentY += subUIDefaultHeight + 5;
            currentX = padding.left;

            MakeVarsList();
            currentY += subUIDefaultHeight + 5;
            currentX = padding.left;

            MakeCommandsList();
            currentX = 0;
            currentY = position.height - 25;

            MakeFooter();

            ResetStringGroupsList();

            Undo.RecordObject(this, "DSyntaxEditor");


            void ProcessCommands()
            {
                #region [Fill commandUIDrawer with data if hasn't]

                if (commandsWithData.Count == 0)
                {
                    foreach (var branch in tree.branches)
                    {
                        var branchUI = new CommandUIDrawer(commandUIDrawers[dSyntaxSettings.COMMAND_BRANCH.ToUpper()]);
                        branchUI.ui.GetParameter("branchName").parameterValue = branch.name;
                        commandsWithData.Add(branchUI);

                        foreach (var node in branch.nodes)
                        {
                            if (node is DSyntaxData.NodeGoTo)
                            {
                                var nodeGoTo = node as DSyntaxData.NodeGoTo;
                                var gotoUI = new CommandUIDrawer(commandUIDrawers[dSyntaxSettings.COMMAND_GOTO.ToUpper()]);
                                gotoUI.ui.GetParameter("toBranchName").parameterValue = nodeGoTo.toBranchName;
                                commandsWithData.Add(gotoUI);
                            }
                            else if (node is DSyntaxData.NodeChoices)
                            {
                                var nodeChoice = node as DSyntaxData.NodeChoices;
                                var choiceUI = new CommandUIDrawer(commandUIDrawers[dSyntaxSettings.COMMAND_CHOICES.ToUpper()]);
                                choiceUI.ui.GetParameter("title").parameterValue = nodeChoice.title.text;
                                foreach (var choice in nodeChoice.choices)
                                {
                                    var choiceListUI = choiceUI.GetListUITemplateCopy();
                                    choiceListUI.GetParameter("text").parameterValue = choice.text.text;
                                    if (choice.conditions[0].variable != null)
                                    {
                                        choiceListUI.GetParameter("varName").parameterValue = choice.conditions[0].variable.varName;
                                        choiceListUI.GetParameter("checkKey").parameterValue = choice.conditions[0].checkKey.ToString();
                                        choiceListUI.GetParameter("value").parameterValue = choice.conditions[0].variable.varValue;
                                    }
                                    choiceListUI.GetParameter("toBranchName").parameterValue = choice.conditions[0].toBranchName;
                                    choiceUI.listUIs.Add(choiceListUI);
                                }
                                commandsWithData.Add(choiceUI);

                            }
                            else if (node is DSyntaxData.NodeUrgent)
                            {
                                var nodeUrgent = node as DSyntaxData.NodeUrgent;
                                var urgentUI = new CommandUIDrawer(commandUIDrawers[dSyntaxSettings.COMMAND_URGENT.ToUpper()]);
                                urgentUI.ui.GetParameter("title").parameterValue = nodeUrgent.title.text;
                                urgentUI.ui.GetParameter("initialDelay").parameterValue = nodeUrgent.initialDelay;
                                foreach (var choice in nodeUrgent.choices)
                                {
                                    var choiceListUI = urgentUI.GetListUITemplateCopy();
                                    choiceListUI.GetParameter("text").parameterValue = choice.text.text;
                                    if (choice.conditions[0].variable != null)
                                    {
                                        choiceListUI.GetParameter("varName").parameterValue = choice.conditions[0].variable.varName;
                                        choiceListUI.GetParameter("checkKey").parameterValue = choice.conditions[0].checkKey.ToString();
                                        choiceListUI.GetParameter("value").parameterValue = choice.conditions[0].variable.varValue;
                                    }
                                    choiceListUI.GetParameter("toBranchName").parameterValue = choice.conditions[0].toBranchName;
                                    urgentUI.listUIs.Add(choiceListUI);
                                }
                                commandsWithData.Add(urgentUI);
                            }
                            else if (node is DSyntaxData.NodeConditions)
                            {
                                var nodeConditions = node as DSyntaxData.NodeConditions;
                                var conditionsUI = new CommandUIDrawer(commandUIDrawers[dSyntaxSettings.COMMAND_IF.ToUpper()]);
                                foreach (var condition in nodeConditions.conditions)
                                {
                                    var conditionListUI = conditionsUI.GetListUITemplateCopy();
                                    conditionListUI.GetParameter("varName").parameterValue = condition.variable.varName;
                                    conditionListUI.GetParameter("checkKey").parameterValue = condition.checkKey.ToString();
                                    conditionListUI.GetParameter("value").parameterValue = condition.variable.varValue;
                                    conditionListUI.GetParameter("toBranchName").parameterValue = condition.toBranchName;
                                    conditionsUI.listUIs.Add(conditionListUI);
                                }
                                commandsWithData.Add(conditionsUI);
                            }
                            else if (node is DSyntaxData.NodeSet)
                            {
                                var nodeSet = node as DSyntaxData.NodeSet;
                                var setUI = new CommandUIDrawer(commandUIDrawers[dSyntaxSettings.COMMAND_SET.ToUpper()]);
                                setUI.ui.GetParameter("varName").parameterValue = nodeSet.variable.varName;
                                setUI.ui.GetParameter("setKey").parameterValue = nodeSet.operationType.ToString();
                                setUI.ui.GetParameter("value").parameterValue = nodeSet.variable.varValue;
                                commandsWithData.Add(setUI);
                            }
                            else if (node is DSyntaxData.NodeOnce)
                            {
                                // NodeOnce is a virtual node which instantly converted to other notes via GetTree()
                            }
                            else // NodeSay
                            {
                                var nodeSay = node as DSyntaxData.NodeSay;
                                var sayUI = new CommandUIDrawer(commandUIDrawers[SAY]);
                                sayUI.ui.GetParameter("actor").parameterValue = nodeSay.name;
                                sayUI.ui.GetParameter("statement").parameterValue = nodeSay.text.text;
                                sayUI.ui.GetParameter("expression").parameterValue = nodeSay.expression.ToString();
                                sayUI.ui.GetParameter("gesture").parameterValue = nodeSay.gesture.ToString();
                                sayUI.ui.GetParameter("duration").parameterValue = nodeSay.duration.ToString();
                                commandsWithData.Add(sayUI);
                            }
                        }
                    }
                }

                #endregion

                #region [Validate the string groups]

                var parameterUIs = new List<ParameterUIDrawer>();
                foreach (var command in commandsWithData)
                {
                    foreach (var subUI in command.ui.subUIs)
                        if (subUI is ParameterUIDrawer)
                            parameterUIs.Add(subUI as ParameterUIDrawer);

                    foreach (var list in command.listUIs)
                        foreach (var subUI in list.subUIs)
                            if (subUI is ParameterUIDrawer)
                                parameterUIs.Add(subUI as ParameterUIDrawer);
                }

                foreach (var parameter in parameterUIs)
                    AddWordToStringGroupBasedOnType(stringGroups, parameter);

                foreach (var parameter in parameterUIs)
                    parameter.parameterColor = GetColorByStringGroup(stringGroups, parameter);

                RemoveDuplicateWords(stringGroups);

                #endregion

            }

            float GetRelativeScreenWidth()
            {
                return position.width - padding.left - padding.right - commandNameLabelWidth;
            }

            void MakeDSyntaxStringControllers()
            {
                if (OnSetDSyntax != null)
                {
                    var dSyntaxCompRect = new Rect(currentX, currentY, availableWidth * 0.4f, subUIDefaultHeight);
                    EditorGUI.LabelField(dSyntaxCompRect, "DSyntaxString detected");
                    currentX += dSyntaxCompRect.width;

                    var saveDSyntaxStringComponentButRect = new Rect(currentX, currentY, availableWidth * 0.08f, subUIDefaultHeight); ;
                    GUI.color = Color.green;
                    var saveButDSyntaxStringComponentLabel = "Save";
                    if (GUI.Button(saveDSyntaxStringComponentButRect, saveButDSyntaxStringComponentLabel))
                    {
                        OnSetDSyntax(ConvertUIDrawersToDSyntax(dSyntaxSettings, commandsWithData));
                    }
                    currentX += saveDSyntaxStringComponentButRect.width + 5;
                }

                GUI.color = defaultGUIColor;
            }

            void MakeTextAssetControllers()
            {
                var textAssetRect = new Rect(currentX, currentY, availableWidth * 0.4f, subUIDefaultHeight);
                textAsset = (TextAsset)EditorGUI.ObjectField(textAssetRect, textAsset, typeof(TextAsset), false);
                currentX += textAssetRect.width;

                var saveTextAssetButRect = new Rect(currentX, currentY, availableWidth * 0.08f, subUIDefaultHeight); ;
                GUI.color = Color.green;
                var saveButTextAssetLabel = textAsset == null ? "New" : "Save";
                if (GUI.Button(saveTextAssetButRect, saveButTextAssetLabel))
                {
                    var filePath = "";
                    if (textAsset != null)
                    {
                        filePath = AssetDatabase.GetAssetPath(textAsset);
                    }
                    else
                    {
                        filePath = EditorUtility.SaveFilePanelInProject("Save to TextAsset", "dSyntax", "txt", "Save");
                        if (string.IsNullOrEmpty(filePath)) return;
                    }

                    System.IO.File.WriteAllText(filePath, ConvertUIDrawersToDSyntax(dSyntaxSettings, commandsWithData));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log("Updated: "+filePath);
                    textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
                    Repaint();
                }
                GUI.color = defaultGUIColor;
                currentX += saveTextAssetButRect.width;
            }

            void MakeActorsList()
            {
                var actorsString = "";
                foreach (var actor in stringGroups["actor"].sgWords)
                    actorsString += "[" + actor.GetDisplayWord() + "]  ";


                var labelActorsRect = new Rect(currentX, currentY, position.width - padding.left - padding.right, subUIDefaultHeight);
                EditorGUI.LabelField(labelActorsRect, "Actors: " + actorsString);
            }

            void MakeVarsList()
            {
                var varsString = "";
                foreach (var var in stringGroups["var"].sgWords)
                    varsString += "[" + var.GetDisplayWord() + "]  ";

                var labelVarsRect = new Rect(currentX, currentY, position.width - padding.left - padding.right, subUIDefaultHeight);
                EditorGUI.LabelField(labelVarsRect, "Vars or Values: " + varsString);
            }

            void MakeCommandsList()
            {
                if (tree == null) return;

                #region [BeginScrollView]

                var scrollViewRect = new Rect(
                    0,
                    currentY,
                    position.width,
                    position.height - currentY - footerHeight);
                var viewWidth = GetRelativeScreenWidth();
                var viewHeight = 0f;
                foreach (var command in commandsWithData)
                {
                    foreach (var subUI in command.ui.subUIs)
                        if (subUI is BreakUIDrawer)
                            viewHeight += (subUI as BreakUIDrawer).height + 5;
                    foreach (var list in command.listUIs)
                        foreach (var subUI in list.subUIs)
                            if (subUI is BreakUIDrawer)
                                viewHeight += (subUI as BreakUIDrawer).height + 5;
                    viewHeight += subUIDefaultHeight + 5;
                }
                var viewRect = new Rect(0, 0, viewWidth - 20, viewHeight + 60);

                scrollViewPos = GUI.BeginScrollView(scrollViewRect, scrollViewPos, viewRect);

                #endregion

                currentX = padding.left;
                currentY = 0;
                int index = 0;
                foreach (var command in commandsWithData)
                {
                    currentX = padding.left;

                    #region [Label Number]

                    int _index = index;
                    GUI.contentColor = new Color(1, 1, 1, 0.5f);
                    var indexLabelStyle = new GUIStyle(GUI.skin.label);
                    indexLabelStyle.alignment = TextAnchor.MiddleLeft;
                    var indexLabelRect = new Rect(currentX,currentY, 25, subUIDefaultHeight);
                    EditorGUI.LabelField(indexLabelRect, _index.ToString(), indexLabelStyle);
                    GUI.contentColor = defaultGUIContentColor;

                    if(indexLabelRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown)
                            PopupWindow.Show(indexLabelRect, new CommandPopup(this, _index));
                    }

                    #endregion

                    currentX += indexLabelRect.width-5;

                    float rowHeight = subUIDefaultHeight;
                    foreach (var subUI in command.ui.subUIs)
                    {
                        var newRowHeight = MakeSubUI(subUI, rowHeight);
                        rowHeight = newRowHeight > rowHeight ? newRowHeight : rowHeight;
                    }

                    rowHeight = subUIDefaultHeight;
                    if (command.listSubUITemplate.subUIs.Count > 0)
                    {
                        int listItemIndex = 0;
                        foreach (var list in command.listUIs)
                        {
                            #region [Label Number]

                            GUI.contentColor = new Color(1, 1, 1, 0.5f);
                            var indexListItemlStyle = new GUIStyle(GUI.skin.label);
                            indexListItemlStyle.alignment = TextAnchor.MiddleLeft;
                            var indexListItemRect = new Rect(currentX, currentY, 25, subUIDefaultHeight);
                            EditorGUI.LabelField(indexListItemRect, listItemIndex.ToString(), indexListItemlStyle);
                            GUI.contentColor = defaultGUIContentColor;
                            currentX += indexListItemRect.width - 5;

                            #endregion

                            foreach (var subUI in list.subUIs)
                            {
                                var newRowHeight = MakeSubUI(subUI, rowHeight);
                                rowHeight = newRowHeight > rowHeight ? newRowHeight : rowHeight;
                            }

                            listItemIndex++;
                        }

                        GUI.color = new Color(0.75f, 0.75f, 0.75f, 1);
                        var listAddRect = new Rect(currentX, currentY, availableWidth - currentX, 20);
                        if (GUI.Button(listAddRect, "Add"))
                            command.listUIs.Add(command.GetListUITemplateCopy());
                        GUI.color = defaultGUIColor;
                    }


                    currentY += subUIDefaultHeight + 5;
                    index++;
                }
                GUI.EndScrollView();

            }

            void MakeFooter()
            {
                #region [AddCommand Label]

                var toAddLabelRect = new Rect(currentX, currentY, 35, footerHeight);
                var toAddCommandLabelStyle = new GUIStyle(GUI.skin.label);
                toAddCommandLabelStyle.alignment = TextAnchor.MiddleCenter;
                EditorGUI.LabelField(toAddLabelRect, "Add", toAddCommandLabelStyle);
                currentX += toAddLabelRect.width;

                #endregion

                #region [AddCommand TextField]

                var toAddCommandRect = new Rect(currentX, currentY, position.width*0.15f, footerHeight);
                var toAddCommandStyle = new GUIStyle(GUI.skin.textField);
                toAddCommandStyle.alignment = TextAnchor.MiddleLeft;
                toAddCommand = EditorGUI.TextField(toAddCommandRect, toAddCommand, toAddCommandStyle);
                currentX += toAddCommandRect.width;

                #endregion

                #region [StringGroup Button]

                var sgRect = new Rect(currentX, currentY, 15, footerHeight);
                GUI.color = Color.gray;
                if (GUI.Button(sgRect, "\u25bc"))
                {
                    PopupWindow.Show(sgRect, new StringGroupPopup(stringGroups, this, "commands", (word) => { toAddCommand = word; }));
                }
                GUI.color = defaultGUIColor;
                currentX += sgRect.width;

                var foundSGWord = stringGroups["commands"].sgWords.Find(w => w.Compare(toAddCommand));
                if (foundSGWord != null)
                    toAddCommand = foundSGWord.GetDisplayWord();

                #endregion

                #region [+Add Button]

                GUI.color = Color.green;
                var addCommandRect = new Rect(currentX, currentY, 25, footerHeight);
                if (GUI.Button(addCommandRect, "+"))
                {
                    AddCommand(commandsWithData.Count);
                }
                GUI.color = defaultGUIColor;
                currentX += addCommandRect.width + 25;
            
                #endregion

                #region [Convert]

                var convertRect = new Rect(currentX, currentY, 75, footerHeight);
                if (GUI.Button(convertRect, "Copy DS"))
                {
                    var result = ConvertUIDrawersToDSyntax(dSyntaxSettings, commandsWithData);
                    GUIUtility.systemCopyBuffer = result;
                    Debug.Log(result);
                }
                currentX += convertRect.width;

                #endregion

                #region [Refresh]

                var refreshRect = new Rect(currentX, currentY, 75, footerHeight);
                if (GUI.Button(refreshRect, "Refresh"))
                {
                    OnEnable();
                }
                currentX += refreshRect.width;

                    #endregion

            }

            float MakeSubUI(SubUIDrawer subUI, float rowHeight)
            {
                if (subUI is ParameterUIDrawer)
                {
                    var parameterUIDrawer = subUI as ParameterUIDrawer;

                    var width = parameterUIDrawer.width < 1f ? GetRelativeScreenWidth() * parameterUIDrawer.width : parameterUIDrawer.width;
                    var rect = new Rect(currentX, currentY, width, parameterUIDrawer.height);
                    var style = new GUIStyle(GUI.skin.textField);
                    style.alignment = parameterUIDrawer.GetAlignment();
                    if (string.IsNullOrWhiteSpace(parameterUIDrawer.parameterValue) || parameterUIDrawer.parameterValue == parameterUIDrawer.parameterName)
                    {
                        style.fontStyle = FontStyle.Italic;
                        GUI.contentColor = new Color(1, 1, 1, 0.66f);
                        parameterUIDrawer.parameterValue = EditorGUI.TextField(rect, parameterUIDrawer.parameterName, style);
                        GUI.contentColor = defaultGUIContentColor;
                    }
                    else
                    {
                        GUI.color = parameterUIDrawer.parameterColor;
                        if (parameterUIDrawer.isTextArea)
                            parameterUIDrawer.parameterValue = EditorGUI.TextArea(rect, parameterUIDrawer.parameterValue, style);
                        else
                            parameterUIDrawer.parameterValue = EditorGUI.TextField(rect, parameterUIDrawer.parameterValue, style);
                        GUI.color = defaultGUIColor;
                    }
                    
                    currentX += rect.width;
                    if (!string.IsNullOrEmpty(parameterUIDrawer.stringGroupName))
                    {
                        var sgRect = new Rect(currentX, currentY, 15, rect.height);
                        GUI.color = Color.gray; 
                        if (GUI.Button(sgRect, "\u25bc"))
                        {
                            PopupWindow.Show(sgRect, new StringGroupPopup(stringGroups, this, parameterUIDrawer.stringGroupName, (word) => { parameterUIDrawer.parameterValue = word; }));
                        }
                        GUI.color = defaultGUIColor;
                        currentX += sgRect.width;

                        var foundSGWord = stringGroups[parameterUIDrawer.stringGroupName].sgWords.Find(w => w.Compare(parameterUIDrawer.parameterValue));
                        if (foundSGWord!=null)
                            parameterUIDrawer.parameterValue = foundSGWord.GetDisplayWord();
                    }
                    currentX += 5;
                    return rect.height;
                }
                else if (subUI is BreakUIDrawer)
                {
                    currentY += rowHeight + 5;
                    currentX = padding.left;
                }
                else if (subUI is LabelUIDrawer)
                {
                    var labelUIDrawer = subUI as LabelUIDrawer;
                    var width = labelUIDrawer.width < 1f ? GetRelativeScreenWidth() * labelUIDrawer.width : labelUIDrawer.width;
                    var rect = new Rect(currentX, currentY, width, labelUIDrawer.height);
                    var style = new GUIStyle(GUI.skin.label);
                    style.alignment = labelUIDrawer.GetAlignment();
                    EditorGUI.LabelField(rect, labelUIDrawer.labelName, style);
                    currentX += rect.width + 5;
                    return rect.height;
                }

                return rowHeight;
            }

            void MakeAllEmptyCommandDrawers()
            {
                foreach (var commandUIDrawer in commandUIDrawers)
                {
                    currentX = padding.left;

                    float rowHeight = subUIDefaultHeight;
                    foreach (var subUI in commandUIDrawer.Value.ui.subUIs)
                    {
                        if (subUI is SubUIListDrawer) break;
                        var newRowHeight = MakeSubUI(subUI, rowHeight);
                        rowHeight = newRowHeight > rowHeight ? newRowHeight : rowHeight;
                    }

                    foreach (var list in commandUIDrawer.Value.listUIs)
                    {
                        foreach (var subUI in list.subUIs)
                        {
                            var newRowHeight = MakeSubUI(subUI, rowHeight);
                            rowHeight = newRowHeight > rowHeight ? newRowHeight : rowHeight;
                        }
                    }

                    currentY += subUIDefaultHeight + 5;
                }

            }
        }

        string ConvertUIDrawersToDSyntax(DSyntaxSettings dSyntaxSettings, List<CommandUIDrawer> ui)
        {
            var dSyntax = "";
            foreach (var command in ui)
            {
                if (command.hasBreak)
                    dSyntax += " \n";

                var parentParameters = new List<DSyntaxUtility.Parameter>();

                foreach (var subUI in command.ui.subUIs)
                    AddParameterToExportList(parentParameters, subUI);

                // Write Command
                if (command.listUIs.Count == 0)
                {
                    if (command.commandName == SAY)
                    {
                        var actor = parentParameters[0].value;
                        parentParameters.RemoveAt(0);
                        RemoveParameterNameAt(parentParameters,0);
                        RemoveParameterNone(parentParameters);
                        dSyntax += DSyntaxUtility.WriteCommand(dSyntaxSettings, actor, parentParameters);
                    }
                    else
                    {
                        RemoveParameterNameAt(parentParameters,0);
                        RemoveParameterNone(parentParameters);
                        dSyntax += DSyntaxUtility.WriteCommand(dSyntaxSettings, command.commandName, parentParameters);
                    }
                }

                // Write CommandList
                else
                {
                    var childParametersList = new List<List<DSyntaxUtility.Parameter>>();
                    foreach (var list in command.listUIs)
                    {
                        var listParameters = new List<DSyntaxUtility.Parameter>();

                        foreach (var subUI in list.subUIs)
                            AddParameterToExportList(listParameters, subUI);

                        RemoveParameterNameAt(listParameters,1);
                        RemoveParameterNone(listParameters);
                        if (listParameters.Count > 0)
                            childParametersList.Add(listParameters);
                    }

                    RemoveParameterNameAt(parentParameters,0);
                    RemoveParameterNone(parentParameters);
                    dSyntax += DSyntaxUtility.WriteListCommand(dSyntaxSettings, command.commandName, parentParameters, childParametersList);
                }

            }

            return dSyntax;

            void AddParameterToExportList(List<DSyntaxUtility.Parameter> parameters, SubUIDrawer subUI)
            {
                if (subUI is ParameterUIDrawer)
                {
                    var parameterUI = subUI as ParameterUIDrawer;
                    if (parameterUI.parameterValue == parameterUI.parameterName) return;


                    var foundParameter = parameters.Find(p => p.name == parameterUI.exportName);
                    if (foundParameter != null)
                    {
                        foundParameter.value += parameterUI.parameterValue;
                    }
                    else
                    {
                        parameters.Add(new DSyntaxUtility.Parameter(parameterUI.exportName, parameterUI.parameterValue));
                    }
                }
            }

            void RemoveParameterNameAt(List<DSyntaxUtility.Parameter> parameters, int index)
            {
                if (parameters.Count > index)
                    parameters[index].name = "";
            }

            void RemoveParameterNone(List<DSyntaxUtility.Parameter> parameters)
            {
                for (int i = parameters.Count - 1; i >= 0; i--)
                    if(parameters[i].value == NONE) parameters.RemoveAt(i);
            }
        }
        
        void ResetStringGroupsList()
        {
            stringGroups = GetStringGroups(dSyntaxSettings, stringGroupSettings.text);
            var commandsStringGroup = new StringGroup("commands", new List<SGWord>());
            foreach (var command in commandUIDrawers)
                commandsStringGroup.sgWords.Add(new SGWord(command.Key));
            stringGroups.Add(commandsStringGroup.sgName, commandsStringGroup);
        }

        #endregion


        #region [Methods: Modification]

        public void SwapCommand(int indexFrom, int indexTo)
        {
            if (indexFrom == indexTo) return;
            if (indexFrom < 0 || indexFrom > commandsWithData.Count - 1) return;
            if (indexTo < 0 || indexTo > commandsWithData.Count - 1) return;

            var _temp = new CommandUIDrawer(commandsWithData[indexFrom]);
            commandsWithData[indexFrom] = new CommandUIDrawer(commandsWithData[indexTo]);
            commandsWithData[indexTo] = _temp;
            Repaint();
        }

        public void MoveCommand(int indexFrom, int indexTo)
        {
            if (indexFrom == indexTo) return;
            if (indexFrom < 0 || indexFrom > commandsWithData.Count - 1) return;
            if (indexTo < 0 || indexTo > commandsWithData.Count - 1) return;

            var _temp = new CommandUIDrawer(commandsWithData[indexFrom]);
            commandsWithData.RemoveAt(indexFrom);
            commandsWithData.Insert(indexTo, _temp);
            Repaint();
        }

        public void AddCommand(int index)
        {
            if (index < 0) index = 0;
            else if (index > commandsWithData.Count) index = commandsWithData.Count;
            commandsWithData.Insert(index, new CommandUIDrawer(commandUIDrawers[toAddCommand]));
            Repaint();
        }

        public void DeleteCommand(int index)
        {
            commandsWithData.RemoveAt(index);
            Repaint();
        }

        #endregion


        #region [Methods: UI Drawers]

        Dictionary<string,CommandUIDrawer> GetCommandUIDrawers(DSyntaxSettings dSyntaxSettings, string text)
        {
            var commandUIDrawers = new Dictionary<string, CommandUIDrawer>();
            text = text.Replace(dSyntaxSettings.TOKEN_COMMAND_OPENING + CommandUIDrawer.COMMAND.ToLower() + dSyntaxSettings.TOKEN_COMMAND_CLOSING, dSyntaxSettings.TOKEN_COMMAND_OPENING + CommandUIDrawer.COMMAND + dSyntaxSettings.TOKEN_COMMAND_CLOSING);
            var commands = DSyntaxUtility.ReadCommandsByGroups(dSyntaxSettings, text, CommandUIDrawer.COMMAND);
            foreach (var command in commands)
            {
                #region [Get Command's metadata]

                var commandName = command[0].GetParameter(dSyntaxSettings, "name", 0);
                var commandType = command[0].GetParameter(dSyntaxSettings, "type", 1);
                var commandExportHasBreak = command[0].GetParameter(dSyntaxSettings, "exportHasBreak", 2, "false");
                var hasBreak = commandExportHasBreak.Equals("true", StringComparison.CurrentCultureIgnoreCase) ? true : false;
                var commandUIDrawer = new CommandUIDrawer(commandName.ToUpper(), commandType, new SubUIListDrawer(), hasBreak);

                #endregion

                command.RemoveAt(0);
                var subUIs = command;
                bool isList = false;
                foreach (var subUI in subUIs)
                {
                    if (subUI.name.ToUpper() == ParameterUIDrawer.PARAMETER)
                    {
                        var name = subUI.GetParameter(dSyntaxSettings, "name", 0);
                        var type = subUI.GetParameter(dSyntaxSettings, "type", 1, ParameterUIDrawer.STRING);
                        var sgName = subUI.GetParameter(dSyntaxSettings, "sgName", 2, "");
                        var sgType = subUI.GetParameter(dSyntaxSettings, "sgType", 3, "");
                        var width = subUI.GetParameter(dSyntaxSettings, "width", 4, subUIDefaultWidth.ToString());
                        var height = subUI.GetParameter(dSyntaxSettings, "height", 5, subUIDefaultHeight.ToString());
                        var alignment = subUI.GetParameter(dSyntaxSettings, "alignment", 6, ParameterUIDrawer.LEFT);
                        var exportName = subUI.GetParameter(dSyntaxSettings, "exportName", 7, "");
                        var isTextAreaString = subUI.GetParameter(dSyntaxSettings, "isTextArea", 8, "false");
                        var isTextArea = isTextAreaString.Equals("true", StringComparison.CurrentCultureIgnoreCase) ? true : false;

                        var parameterUIDrawer = new ParameterUIDrawer(float.Parse(width), float.Parse(height), name, type,"", sgName, sgType, alignment, exportName, isTextArea);
                        if (!isList)
                            commandUIDrawer.ui.subUIs.Add(parameterUIDrawer);
                        else
                            commandUIDrawer.listSubUITemplate.subUIs.Add(parameterUIDrawer);
                    }

                    else if (subUI.name.ToUpper() == LabelUIDrawer.LABEL)
                    {
                        var name = subUI.GetParameter(dSyntaxSettings, "name", 0);
                        var alignment = subUI.GetParameter(dSyntaxSettings, "alignment", 1, LabelUIDrawer.LEFT);
                        var width = subUI.GetParameter(dSyntaxSettings, "width", 2, subUIDefaultWidth.ToString());
                        var height = subUI.GetParameter(dSyntaxSettings, "height", 3, subUIDefaultHeight.ToString());
                        var labelUIDrawer = new LabelUIDrawer(float.Parse(width), float.Parse(height), name, alignment);
                        if (!isList)
                            commandUIDrawer.ui.subUIs.Add(labelUIDrawer);
                        else
                            commandUIDrawer.listSubUITemplate.subUIs.Add(labelUIDrawer);
                    }

                    else if (subUI.name.ToUpper() == BreakUIDrawer.BREAK)
                    {
                        var height = subUI.GetParameter(dSyntaxSettings, "height", 0, subUIDefaultHeight.ToString());
                        var width = subUI.GetParameter(dSyntaxSettings, "width", 1, subUIDefaultWidth.ToString());
                        var breakUIDrawer = new BreakUIDrawer(float.Parse(width), float.Parse(height));
                        if (!isList)
                            commandUIDrawer.ui.subUIs.Add(breakUIDrawer);
                        else
                            commandUIDrawer.listSubUITemplate.subUIs.Add(breakUIDrawer);

                    }

                    else if (subUI.name.ToUpper() == ListUIDrawer.LIST)
                    {
                        isList = true;
                    }
                }

                commandUIDrawers.Add(commandUIDrawer.commandName, commandUIDrawer);
            }

            return commandUIDrawers;
        }

        Dictionary<string,StringGroup> GetStringGroups(DSyntaxSettings dSyntaxSettings, string text)
        {
            Dictionary<string,StringGroup> sgs = new Dictionary<string,StringGroup>();
            const string GROUP = "GROUP";
            text = text.Replace(dSyntaxSettings.TOKEN_COMMAND_OPENING + GROUP.ToLower() + dSyntaxSettings.TOKEN_COMMAND_CLOSING, dSyntaxSettings.TOKEN_COMMAND_OPENING + GROUP + dSyntaxSettings.TOKEN_COMMAND_CLOSING);
            var groups = DSyntaxUtility.ReadCommandsByGroups(dSyntaxSettings, text, GROUP);
            foreach (var group in groups)
            {
                var groupName = group[0].GetParameter(dSyntaxSettings, "name", 0, "groupName");
                var displayIndex = group[0].GetParameter(dSyntaxSettings, "displayIndex", 1, "0");
                var sg = new StringGroup(groupName, new List<SGWord>());

                group.RemoveAt(0);
                foreach (var word in group)
                {
                    var synonyms = word.parameters;
                    if (synonyms.Count > 0)
                        sg.sgWords.Add(new SGWordSynonym(word.name, synonyms, int.Parse(displayIndex)));
                    else
                        sg.sgWords.Add(new SGWord(word.name));
                }

                sgs.Add(sg.sgName, sg);
            }

            return sgs;
        }


        #endregion

        #region [Methods: StringGroup]

        public void AddWordToStringGroupBasedOnType(Dictionary<string, StringGroup> stringGroups, ParameterUIDrawer parameterUIDrawer)
        {
            if (parameterUIDrawer.parameterName == parameterUIDrawer.parameterValue) return;
            if (parameterUIDrawer.stringGroupType.ToUpper() == StringGroup.ADD)
            {
                stringGroups[parameterUIDrawer.stringGroupName].sgWords.Add(new SGWord(parameterUIDrawer.parameterValue));
            }

            else if (parameterUIDrawer.stringGroupType.ToUpper() == StringGroup.PLURAL)
            {
                stringGroups[parameterUIDrawer.stringGroupName].sgWords.Add(new SGWord(parameterUIDrawer.parameterValue));
            }
        }

        public Color GetColorByStringGroup(Dictionary<string, StringGroup> stringGroups, ParameterUIDrawer parameterUIDrawer)
        {
            if (parameterUIDrawer.stringGroupType.ToUpper() == StringGroup.CHECK)
            {
                var foundWord = stringGroups[parameterUIDrawer.stringGroupName].sgWords.Find(w => w.Compare(parameterUIDrawer.parameterValue));
                if (foundWord != null)
                {
                    return defaultGUIColor; 
                }
                else
                    return Color.red;
            }
            else if (parameterUIDrawer.stringGroupType.ToUpper() == StringGroup.PLURAL)
            {
                int wordCount = 0;
                foreach (var word in stringGroups[parameterUIDrawer.stringGroupName].sgWords)
                {
                    if (word.Compare(parameterUIDrawer.parameterValue)) 
                    { 
                        wordCount++;
                        if (wordCount > 1)
                            return defaultGUIColor; 
                    }
                }
                return Color.yellow;
            }

            return defaultGUIColor;
        }

        public void RemoveDuplicateWords(Dictionary<string, StringGroup> stringGroups)
        {
            foreach (var group in stringGroups)
            {
                for (int i = group.Value.sgWords.Count - 1; i >= 0; i--)
                {
                    for (int f = 0; f < group.Value.sgWords.Count-1; f++)
                    {
                        if (i == f) break;
                        if(group.Value.sgWords[i].word == group.Value.sgWords[f].word)
                        {
                            group.Value.sgWords.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        #endregion

    }
}

