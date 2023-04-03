using DialogueSyntax;
using Encore.CharacterControllers;
using Encore.Localisations;
using Encore.MiniGames.UrgentChoice;
using Encore.Utility;
using NodeCanvas.DialogueTrees;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DialogueSyntax.DSyntaxData;

namespace Encore.Dialogues
{
    [RequireComponent(typeof(DialogueTreeController))]
    [AddComponentMenu("Encore/Dialogues")]
    public partial class DialogueManagerNC : MonoBehaviour, IPersistentSystem
    {
        #region [Classes]

        public class SpeakingActor
        {
            public ActorContainer actorContainer;
            public AnimationController animationController;
            public DialogueUIContainer uiContainer;

            public SpeakingActor(ActorContainer actorContainer, AnimationController animationController, DialogueUIContainer uiContainer)
            {
                this.actorContainer = actorContainer;
                this.animationController = animationController;
                this.uiContainer = uiContainer;
            }
        }

        #endregion

        #region [Vars: Properties]

        [Title("Properties")]
        [SerializeField] int maxCharacters = 25;
        [SerializeField] float showBubbleDuration = 1.2f;
        [SerializeField, Required] GameObject dialogueUIContainer_NPC_Prefab;
        [SerializeField] DSyntaxSettings dSyntaxSettings;
        public DSyntaxSettings GetDSyntaxSettings { get => dSyntaxSettings; }

        #endregion

        #region [Vars: Data Handlers]

        public DialogueTreeController TreeController { get { return treeController; } }
        DialogueTreeController treeController;
        public DialogueUIContainer_Player DialogueUIContainer_Player
        {
            get { return dialogueUIContainerPlayer; }
            set
            {
                dialogueUIContainerPlayer = value;
            }
        }
        DialogueUIContainer_Player dialogueUIContainerPlayer;

        public bool IsInSpeaking { get; private set; }
        /// <summary> Act as a trigger to continue the running dialogue; Always set to False</summary>
        bool triggerNextBubble = false;
        /// <summary> Act as a trigger to choose the multiple choice; Always set to -1</summary>
        int triggerChoice = -1;
        /// <summary> To prevent extra click when clicking when in multiple choice</summary>
        bool isInMultipleChoice = false;

        [ShowInInspector, ReadOnly]
        List<SpeakingActor> monologuingActors = new List<SpeakingActor>();
        bool canTriggerNext = true;

        /// <summary>Added via <see cref="ActorContainer.OnEnable"/> </summary>
        [ShowInInspector, ReadOnly]
        List<ActorContainer> allActorsInScene = new List<ActorContainer>();
        public List<ActorContainer> AllActorsInScene
        {
            get
            {
                return allActorsInScene;
            }
        }

        List<SpeakingActor> dialoguingActors = new List<SpeakingActor>();

        /// <summary>Dictionary containing the localised dialogues according to Language settings</summary>
        Dictionary<string, string> localisedDialogues = new Dictionary<string, string>();

        DialogueSettings currentDialogueSettings;

        Func<Dictionary<string, string>> GetCurrentDialogueVariables;

        Coroutine corRunDSyntaxDialogue;

        #endregion

        #region [Delegates]

        /// <summary> 
        /// - Activated when a dialogue/monologue is playing; Affects <see cref="AnimationController"/><br></br>
        /// - Activate/Deactivate <see cref="MouseManager.SetCanInteract(bool, CursorImageManager.CursorImage)"/> via <see cref="CharacterBrain.SetCanInteract(string, bool)"/><br></br>
        /// </summary>
        public Action<string, bool> OnActorSpeaking;
        public Action<string, Expression> OnExpressing;
        /// <summary> Affects <see cref="PlayerController.SetCanMoveInput(bool)"/></summary>
        public Action<bool> OnSpeaking;

        #endregion

        #region [Methods: Unity]

        void Awake()
        {
            DontDestroyOnLoad(this);
            treeController = GetComponent<DialogueTreeController>();
            OnBeforeSceneLoad();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            allActorsInScene.Clear();
        }

        #endregion

        #region [Methods: Persistent System]

        public void OnBeforeSceneLoad()
        {
            StopAllCoroutines();
            allActorsInScene.Clear();
            dialoguingActors.Clear();
            monologuingActors.Clear();
            OnSpeaking = (isInSpeaking) => { this.IsInSpeaking = isInSpeaking; }; // TODO: watch this!!!; it's equal not +=
        }

        public void OnAfterSceneLoad()
        {
            allActorsInScene = new List<ActorContainer>(FindObjectsOfType<ActorContainer>());

            string actorNames = "";
            foreach (var item in allActorsInScene) actorNames += item.name + ", ";
            Debug.Log("Actors detected: " + actorNames);
        }

        #endregion

        #region [Methods: Node Canvas Delegates]

        private void OnDialogueStarted(DialogueTree tree)
        {
            PrepareDialoguingActors(tree);
            SyncDialogueVariables(tree);
        }

        private void OnDialogueFinished(DialogueTree tree)
        {
            if (this == null || GameManager.Instance == null) return;

            GameManager.Instance.Player.SetCanInteract(true);

            foreach (var speakingActor in dialoguingActors)
            {
                speakingActor.animationController?.SetCanIdleSpecial(false);
                speakingActor.animationController?.PlayExpression(Expression.None);
                speakingActor.animationController?.PlayGesture(Gesture.None);
                speakingActor.actorContainer.StopFacing();
                OnActorSpeaking(speakingActor.actorContainer.name, false);
            }


            if (GameManager.Instance != null)
            {
                GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Normal);
                (GameManager.Instance.Player.Controller as PlayerController).CameraZoomIn(false);

                // Set global variables again in case a variable's value is changed in mid dialogue
                foreach (var dialogueVariable in tree.blackboard.variables)
                {
                    GameManager.Instance.ChangeGlobalVariable(dialogueVariable.Key, dialogueVariable.Value.value.ToString());
                }
            }

            ResetDialoguingActors();

            #region [DialogueTree delegates]

            DialogueTree.OnDialogueStarted -= OnDialogueStarted;
            DialogueTree.OnDialogueFinished -= OnDialogueFinished;
            DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
            DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;

            #endregion

            OnSpeaking(false);
        }

        private void OnSubtitlesRequest(SubtitlesRequestInfo info)
        {
            ShowSpeech(info);
        }

        private void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info)
        {
            if (info.mode == MultipleChoiceRequestInfo.Mode.Normal)
                ShowMultipleChoice(info);
            else if (info.mode == MultipleChoiceRequestInfo.Mode.Urgent)
                ShowUrgentChoice(info);
            else if (info.mode == MultipleChoiceRequestInfo.Mode.Chat)
                ShowMultipleChoice(info);
        }

        #endregion

        #region [Methods: Dialogue]

        public bool BeginDialogue(DialogueBundle bundle)
        {
            if (!CanBeginDialogue(bundle, bundle.Settings)) return false;
            #region [DialogueTree delegates]

            DialogueTree.OnDialogueStarted += OnDialogueStarted;
            DialogueTree.OnDialogueFinished += OnDialogueFinished;
            DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
            DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;

            #endregion
            treeController.StartDialogue(bundle.DialogueTree, GameManager.Instance.Player.ActorContainer, null);
            return true;
        }


        #endregion

        #region [Methods: DSyntax Dialogue]

        public bool BeginDSyntaxDialogue(DSyntaxBundle bundle)
        {
            var tree = DSyntaxUtility.GetTree(bundle.DSyntax.dSyntax, dSyntaxSettings);
            if (!CanBeginDialogue(bundle, bundle.Settings)) return false;
            PrepareDialoguingActors(tree);
            SyncDialogueVariables(tree);
            corRunDSyntaxDialogue = this.RestartCoroutine(RunDSyntaxDialogue(tree, OnDialogueFinished, OnNodeSay, OnNodeChoice, OnNodeUrgent));
            
            return true;

            #region [DSyntax Delegates]

            void OnDialogueFinished()
            {
                #region [On Dialogue Finished]

                foreach (var speakingActor in dialoguingActors)
                {
                    speakingActor.animationController?.SetCanIdleSpecial(false);
                    speakingActor.animationController?.PlayExpression(Expression.None);
                    speakingActor.animationController?.PlayGesture(Gesture.None);
                    speakingActor.actorContainer.StopFacing();
                    OnActorSpeaking(speakingActor.actorContainer.name, false);
                }

                if (GameManager.Instance != null)
                {
                    //GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Normal);
                    (GameManager.Instance.Player.Controller as PlayerController).CameraZoomIn(false);

                    // Set global variables again in case a variable's value is changed in mid dialogue
                    foreach (var dialogueVariable in tree.variables)
                    {
                        GameManager.Instance.ChangeGlobalVariable(dialogueVariable.varName, dialogueVariable.varValue);
                    }
                }

                ResetDialoguingActors();

                OnSpeaking(false);

                GameManager.Instance.Player.SetCanInteract(true);

                #endregion
            }

            void OnNodeSay(IStatement statement, string actorName, Action OnContinue)
            {
                ShowSpeech(
                    statement: statement,
                    actorName: actorName,
                    OnContinue: OnContinue);
            }

            void OnNodeChoice(string title, Dictionary<string, int> options, Action<int> OnSelectOption)
            {
                ShowMultipleChoice(title, options, OnSelectOption);
            }

            void OnNodeUrgent(string title, float initialDelay, List<UrgentChoiceManager.UrgentChoiceData> options, Action<int> OnSelectOption)
            {
                ShowUrgentChoice(title, options, OnSelectOption, initialDelay);
            }

            #endregion
        }

        /// <summary>
        /// Run the flow of the dialogue
        /// </summary>
        public IEnumerator RunDSyntaxDialogue(
            DSyntaxData.Tree tree, 
            Action OnDialogueFinished, 
            Action<IStatement,string, Action> OnNodeSay, 
            Action<string, Dictionary<string, int>, Action<int>> OnNodeChoice, 
            Action<string, float, List<UrgentChoiceManager.UrgentChoiceData>, Action<int>> OnNodeUrgent)
        {
            var settings = dSyntaxSettings;
            var currentBranch = tree.branches.Find(b => b.name == settings.START);

            while (currentBranch != null)
            {
                int index = 0;
                foreach (var node in currentBranch.nodes)
                {
                    if (node is NodeSay)
                    {
                        var nodeSay = node as NodeSay;
                        bool isContinued = false;
                        OnNodeSay(
                            new Statement(
                                text: nodeSay.text.text,
                                audio: null,
                                meta: null,
                                expression: (Expression)nodeSay.expression,
                                duration: nodeSay.duration,
                                gesture: (Gesture)nodeSay.gesture,
                                textIndex: nodeSay.text.id),
                            nodeSay.name, 
                            () => { isContinued = true; });
                        while (!isContinued) { yield return null;}
                    }

                    else if (node is NodeGoTo)
                    {
                        var nodeGoTo = node as NodeGoTo;
                        currentBranch = tree.branches.Find(b=>b.name == nodeGoTo.toBranchName);
                        break;
                    }

                    else if (node is NodeChoices)
                    {
                        var nodeChoice = node as NodeChoices;

                        var title = GetLocalisedText(nodeChoice.title);

                        // Filter options which don't fultill their conditions
                        var options = new Dictionary<string, int>();
                        int choiceIndex = 0;
                        foreach (var choice in nodeChoice.choices)
                        {
                            // NOTE: July 6, 2022; Conditions only consists of one condition
                            if (EvaluateCondition(choice.conditions[0], tree))
                            {
                                var text = GetLocalisedText(choice.text);
                                options.Add(text,choiceIndex);
                            }
                            choiceIndex++;
                        }

                        bool isContinued = false;
                        OnNodeChoice(
                            title, 
                            options,
                            (selectedIndex) =>
                            {
                                currentBranch = tree.branches.Find(b => b.name == nodeChoice.choices[selectedIndex].toBranchName);
                                isContinued = true;
                            });

                        while (!isContinued) { yield return null; }
                        break;
                    }

                    else if (node is NodeUrgent)
                    {
                        var nodeUrgent = node as NodeUrgent;

                        var title = GetLocalisedText(nodeUrgent.title);

                        // Filter options which don't fultill their conditions
                        var options = new List<UrgentChoiceManager.UrgentChoiceData>();
                        int choiceIndex = 0;
                        foreach (var choice in nodeUrgent.choices)
                        {
                            // NOTE: July 6, 2022; Conditions only consists of one condition
                            if (EvaluateCondition(choice.conditions[0], tree))
                            {
                                var text = GetLocalisedText(choice.text);
                                if(!int.TryParse(choice.speed, out int speed)) speed = UrgentChoiceManager.UrgentChoiceData.DefaultSpeed;
                                if(!Utility.ColorUtility.ToColor(choice.color, out Color color)) color = UrgentChoiceManager.UrgentChoiceData.DefaultColor;
                                options.Add(new UrgentChoiceManager.UrgentChoiceData(text,choiceIndex,speed,color));
                            }
                            choiceIndex++;
                        }

                        bool isContinued = false;
                        OnNodeUrgent(
                            title,
                            float.Parse(nodeUrgent.initialDelay),
                            options,
                            (selectedIndex) =>
                            {
                                currentBranch = tree.branches.Find(b => b.name == nodeUrgent.choices[selectedIndex].toBranchName);
                                isContinued = true;
                            });

                        while (!isContinued) { yield return null; }
                        break;
                    }

                    else if (node is NodeConditions)
                    {
                        var nodeConditions = node as NodeConditions;
                        var nextBranchName = GetNextBranchName();

                        if (!string.IsNullOrEmpty(nextBranchName))
                        {
                            currentBranch = tree.branches.Find(b => b.name == nextBranchName);
                            break;
                        }

                        string GetNextBranchName()
                        {
                            // Unprioritize the unassigned conditions to be checked last
                            for (int i = nodeConditions.conditions.Count - 1; i >= 0; i--)
                            {
                                var condition = nodeConditions.conditions[i];
                                if (condition.checkKey == Condition.CheckKey.Unassigned)
                                {
                                    nodeConditions.conditions.RemoveAt(i);
                                    nodeConditions.conditions.Add(condition);
                                }
                            }

                            foreach (var condition in nodeConditions.conditions)
                            {
                                // Condition with Pass command or wrong checkKeys progresses the dialogue to the next node
                                if (condition.checkKey == Condition.CheckKey.Unassigned) return null;

                                if (EvaluateCondition(condition, tree)) return condition.toBranchName;
                            }

                            return null;
                        }
                    }

                    else if (node is NodeSet)
                    {
                        var nodeSet = node as NodeSet;
                        var foundVar = tree.variables.Find(v => v.varName == nodeSet.variable.varName);
                        var newValue = nodeSet.variable.varValue;

                        var newValueIsVariable = tree.variables.Find(v => v.varName == newValue);
                        if (newValueIsVariable != null)
                            newValue = newValueIsVariable.varValue;

                        var isNumberOperation = foundVar.varType == "int";
                        int newValueInt = 0;
                        if (isNumberOperation)
                            int.TryParse(newValue, out newValueInt);

                        switch (nodeSet.operationType)
                        {
                            case NodeSet.OperationType.Invalid:
                                break;
                            case NodeSet.OperationType.Equal:
                                foundVar.varValue = nodeSet.variable.varValue;
                                break;
                            case NodeSet.OperationType.Increment:
                                Debug.Log(foundVar.varName + " : " + foundVar.varValue);
                                if (isNumberOperation)
                                    foundVar.varValue = (int.Parse(foundVar.varValue) + newValueInt).ToString();
                                Debug.Log(foundVar.varName + " : " + foundVar.varValue);
                                break;
                            case NodeSet.OperationType.Decrement:
                                if (isNumberOperation)
                                    foundVar.varValue = (int.Parse(foundVar.varValue) - newValueInt).ToString();
                                break;
                            case NodeSet.OperationType.Multiplication:
                                if (isNumberOperation)
                                    foundVar.varValue = (int.Parse(foundVar.varValue) * newValueInt).ToString();
                                break;
                            case NodeSet.OperationType.Division:
                                if (isNumberOperation)
                                    foundVar.varValue = (int.Parse(foundVar.varValue) / newValueInt).ToString();
                                break;
                            default:
                                break;
                        }

                    }

                    // Stop the dialogue if this is the last node
                    if (index == currentBranch.nodes.Count - 1)
                    {
                        currentBranch = null;
                        break;
                    }

                    index++;
                    yield return null;
                }

                yield return null;
            }

            OnDialogueFinished();
            corRunDSyntaxDialogue = null;

            bool EvaluateCondition(Condition condition, DSyntaxData.Tree tree)
            {
                if (condition.variable == null) return true;

                var left = condition.variable.varValue;
                var right = condition.variable.varValue;

                var leftVar = tree.variables.Find(v => v.varName == condition.variable.varName);
                if (leftVar != null) left = leftVar.varValue;
                var rightVar = tree.variables.Find(v => v.varName == condition.variable.varValue);
                if (rightVar != null) right = rightVar.varValue;

                var isNumberOperation = condition.variable.varType == "int";
                var leftInt = 0;
                var rightInt = 0;
                if (isNumberOperation)
                {
                    int.TryParse(left, out leftInt);
                    int.TryParse(right, out rightInt);
                }

                switch (condition.checkKey)
                {
                    case Condition.CheckKey.Unassigned:
                        return true;
                    case Condition.CheckKey.NotEqual:
                        if (isNumberOperation) return leftInt != rightInt;
                        else return left != right;
                    case Condition.CheckKey.Equal:
                        if (isNumberOperation) return leftInt == rightInt;
                        else return left == right;
                    case Condition.CheckKey.GreaterThan:
                        if (isNumberOperation) return leftInt > rightInt;
                        else return false;
                    case Condition.CheckKey.LessThan:
                        if (isNumberOperation) return leftInt < rightInt;
                        else return false;
                    case Condition.CheckKey.GreaterOrEqual:
                        if (isNumberOperation) return leftInt >= rightInt;
                        else return false;
                    case Condition.CheckKey.LessOrEqual:
                        if (isNumberOperation) return leftInt <= rightInt;
                        else return false;
                }
                return false;
            }
        }

        #endregion

        #region [Methods: Dialogue Processes]

        bool CanBeginDialogue(ISpeakingBundle bundle, DialogueSettings settings)
        {
            if (dialoguingActors.Count > 0) return false ; // DialoguingActors are reset OnDialogueFinsihed, but then also accessed and added when clicking new dialogue
            if (IsInSpeaking)
            {
                Debug.Log(nameof(IsInSpeaking) + " : " + IsInSpeaking);
                return false; // TODO: support multiple dialogues at the same time
            }

            // Prevent starting dialogue if an actor doesn't exist
            foreach (var actorName in bundle.ActorNames)
            {
                ActorContainer foundActorContainer = allActorsInScene.Find(a => a.Actor.ActorName == actorName);
                if (foundActorContainer == null)
                {
                    Debug.Log("Cannot find actor: [" + actorName + "] ");
                    return false;
                }
            }

            if (bundle.CSV != null)
                localisedDialogues = CSVUtility.GetColumn(bundle.CSV, GameManager.Instance.LanguageCode);
            currentDialogueSettings = settings;

            return true;
        }

        void PrepareDialoguingActors(List<string> actorNames)
        {
            OnSpeaking(true);

            // Set existing actors to the dialogue tree
            foreach (var actorName in actorNames)
            {
                ActorContainer foundActorContainer = allActorsInScene.Find(a => a.Actor.ActorName == actorName);
                if (foundActorContainer != null)
                {
                    #region [Make UIContainer]

                    DialogueUIContainer dialogueUIContainer;
                    if (foundActorContainer.Actor.ActorName == PlayerBrain.PLAYER_NAME)
                    {
                        dialogueUIContainer = dialogueUIContainerPlayer;
                    }
                    else
                    {
                        dialogueUIContainer = foundActorContainer.InstantiateDialogueUIContainer(dialogueUIContainer_NPC_Prefab);
                    }

                    #endregion

                    #region [Setup SpeakingActor]

                    SpeakingActor speakingActor = new SpeakingActor(
                        foundActorContainer,
                        foundActorContainer.transform.GetComponent<AnimationController>(),
                        dialogueUIContainer);
                    speakingActor.animationController?.SetCanIdleSpecial(true);
                    if (currentDialogueSettings.ActorsAreFacingPlayer)
                        speakingActor.actorContainer.FaceAt(GameManager.Instance.Player.gameObject);
                    dialoguingActors.Add(speakingActor);
                    OnActorSpeaking(foundActorContainer.name, true);

                    #endregion
                }
                else
                {
                    Debug.Log("Cannot find actor: [" + actorName + "]");
                }
            }

            GameManager.Instance.Player.SetCanInteract(false);
            GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.DialogueNext);
            (GameManager.Instance.Player.Controller as PlayerController).CameraZoomIn(true);
        }

        void PrepareDialoguingActors(DSyntaxData.Tree tree)
        {
            var actorNames = new List<string>();
            foreach (var actor in tree.actors)
                actorNames.Add(actor.Key);
            PrepareDialoguingActors(actorNames);
        }

        void PrepareDialoguingActors(DialogueTree tree)
        {
            var actorNames = new List<string>();
            foreach (var actor in tree.actorParameters)
            {
                ActorContainer foundActorContainer = allActorsInScene.Find(a => a.Actor.ActorName == actor.name);
                actor.actor = foundActorContainer;
                actorNames.Add(actor.name);
            }
            PrepareDialoguingActors(actorNames);
        }

        void ResetDialoguingActors()
        {
            StartCoroutine(WaitUntilSpeakingFalse());
            IEnumerator WaitUntilSpeakingFalse()
            {
                // Wait until bubble animation has finished
                while (!canTriggerNext)
                    yield return null;

                foreach (var speakingActor in dialoguingActors)
                    if (speakingActor.actorContainer.name != PlayerBrain.PLAYER_NAME) Destroy(speakingActor.uiContainer.gameObject);

                dialoguingActors.Clear();
            }
        }

        void SyncDialogueVariable(string varName, string varValue, Action<string> SyncValue)
        {
            // Dialogue tree's local variables shouldn't be modified or saved to save data 
            if (varName.StartsWith("local_", StringComparison.CurrentCultureIgnoreCase)) return;

            // Update variable inside the dialogue tree based on save data
            var foundVar = GameManager.Instance.GetGlobalVariable(varName);
            if (foundVar != null)
            {
                SyncValue(foundVar.VarValue); //varValue = foundVar.VarValue;
                Debug.Log("Dialogue variable <b>[" + varName + " : " + varValue + "]</b> updated for dialogue tree");
            }

            // Add new GlobalVariable to save data based on the dialogue tree's variable
            else
            {
                GameManager.Instance.AddGlobalVariable(new Serializables.GlobalVariableData(Guid.NewGuid().ToString(), varName, varValue.ToString()));
            }
        }

        void SyncDialogueVariables(DSyntaxData.Tree tree)
        {
            foreach (var variable in tree.variables)
            {
                SyncDialogueVariable(variable.varName, variable.varValue,
                    SyncValue: (newVarValue) =>
                    {
                        variable.varValue = newVarValue;
                    });
            }


            GetCurrentDialogueVariables = () =>
            {
                var currentVariables = new Dictionary<string, string>();
                foreach (var dialogueVariable in tree.variables)
                    currentVariables.Add(dialogueVariable.varName, dialogueVariable.varValue);
                return currentVariables;
            };
        }

        void SyncDialogueVariables(DialogueTree tree)
        {
            foreach (var dialogueVariable in tree.blackboard.variables)
            {
                SyncDialogueVariable(dialogueVariable.Key, dialogueVariable.Value.value.ToString(),
                    SyncValue: (newVarValue) =>
                    {
                        if (dialogueVariable.Value.varType == typeof(string))
                            dialogueVariable.Value.value = newVarValue;
                        else if (dialogueVariable.Value.varType == typeof(int))
                            if (int.TryParse(newVarValue, out int varValueInt))
                                dialogueVariable.Value.value = varValueInt;
                    });
            }

            GetCurrentDialogueVariables = () =>
            {
                var currentVariables = new Dictionary<string, string>();
                foreach (var dialogueVariable in tree.blackboard.variables)
                    currentVariables.Add(dialogueVariable.Key, dialogueVariable.Value.value.ToString());
                return currentVariables;
            };
        }

        /// <summary> Called by choice buttons that's been set in OnEnable </summary>
        public void SetPlayerChoice(int choice)
        {
            triggerChoice = choice;
        }

        public void NextBubble()
        {
            if (!isInMultipleChoice)
            {
                if (canTriggerNext)
                {
                    canTriggerNext = false;
                    triggerNextBubble = true; // Affects coroutine WaitForInput() inside ShowSpeech()
                    GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.Disabled);
                }
            }
        }

        #endregion

        #region [Methods: Dialogue UI]

        void ShowSpeech(IStatement statement, string actorName, Action OnContinue)
        {
            if (this == null) return;
            isInMultipleChoice = false;
            var actor = dialoguingActors.Find(speakingActor => speakingActor.actorContainer.name == actorName);
            AnimationController ac = actor?.animationController;
            if (ac != null)
            {
                ac.PlayExpression(statement.expression);
                ac.PlayGesture(statement.gesture);
            }

            string text = 
                currentDialogueSettings.OverrideMaxCharacters == -1 
                    ? WrapText(GetLocalisedText(statement), maxCharacters) 
                : currentDialogueSettings.OverrideMaxCharacters == 0 
                    ? GetLocalisedText(statement) 
                    : WrapText(GetLocalisedText(statement), currentDialogueSettings.OverrideMaxCharacters);

            var localisedActorName = GetLocalisedText(actorName, actorName, localisedDialogues);
            if (actorName == PlayerBrain.PLAYER_NAME)
                dialogueUIContainerPlayer.ShowSpeechBubble(text, localisedActorName);
            else
                dialoguingActors.Find(speakingActor => speakingActor.actorContainer.name == actorName).uiContainer.
                    ShowSpeechBubble(text, localisedActorName);

            // Wait for Next to be called
            StartCoroutine(WaitForInput());
            IEnumerator WaitForInput()
            {
                while (true)
                {
                    if (triggerNextBubble)
                    {
                        triggerNextBubble = false;
                        CloseAllBubbles(OnContinue: OnContinue);
                        break;
                    }
                    yield return null;
                }
            }
        }

        void ShowSpeech(SubtitlesRequestInfo info)
        {
            ShowSpeech(info.statement, info.actor.name, info.Continue);
        }

        void ShowMultipleChoice(string title, Dictionary<string, int> options, Action<int> OnSelectOption)
        {
            if (this == null) return;

            isInMultipleChoice = true;

            var choices = new List<DialogueUIContainer_Player.MultipleChoiceData>();
            foreach (var option in options)
            {
                choices.Add(new DialogueUIContainer_Player.MultipleChoiceData(option.Key,option.Value));
            }

            dialogueUIContainerPlayer.CreateMultipleChoiceCanvas(
                title: title, 
                choices: choices, 
                onSetPlayerChoice: SetPlayerChoice);

            // Wait for SetPlayerChoice to be called
            StartCoroutine(WaitForSelection());
            IEnumerator WaitForSelection()
            {
                while (true)
                {
                    if (triggerChoice > -1)
                    {
                        CloseAllBubbles(OnSelectOption: OnSelectOption, selectedChoice: triggerChoice);
                        triggerChoice = -1;
                        break;
                    }
                    yield return null;
                }
            }

        }

        void ShowUrgentChoice(string title, List<UrgentChoiceManager.UrgentChoiceData> options, Action<int> OnSelectOption, float initialDelay)
        {
            if (this == null) return;

            isInMultipleChoice = true;

            GameManager.Instance.CreateUrgentChoice(new UrgentChoiceManager.UrgentChoiceParameters(
                title: title,
                choices: options,
                initialDelay: initialDelay,
                onSetPlayerChoice: SetPlayerChoice
                ));

            // Wait for SetPlayerChoice to be called
            StartCoroutine(WaitForSelection());
            IEnumerator WaitForSelection()
            {
                while (true)
                {
                    if (triggerChoice > -1)
                    {
                        CloseAllBubbles(OnSelectOption: OnSelectOption, selectedChoice: triggerChoice);
                        triggerChoice = -1;
                        break;
                    }
                    yield return null;
                }
            }
        }

        void ShowMultipleChoice(MultipleChoiceRequestInfo info)
        {
            var options = new Dictionary<string, int>();
            foreach (var option in info.options)
                options.Add(GetLocalisedText(option.Key), option.Value);
            var title = GetLocalisedText(info.title, info.textIndex, localisedDialogues);

            ShowMultipleChoice(title, options, info.SelectOption);
        }

        void ShowUrgentChoice(MultipleChoiceRequestInfo info)
        {
            var options = new List<UrgentChoiceManager.UrgentChoiceData>();
            foreach (var option in info.options)
            {
                var text = GetLocalisedText(option.Key);
                var speed = UrgentChoiceManager.UrgentChoiceData.DefaultSpeed;
                var color = UrgentChoiceManager.UrgentChoiceData.DefaultColor;

                var meta = option.Key.meta.ExtractAll("{", "}", suppressWarning: true);
                if (meta.Count >= 2)
                {
                    // Only extract parameter of the 1st and 2nd of meta if exists
                    if (!int.TryParse(meta[0], out speed)) speed = 0; ;
                    if (!Utility.ColorUtility.ToColor(meta[1], out color)) color = UrgentChoiceManager.UrgentChoiceData.DefaultColor;
                }

                //options.Add(new DSyntaxData.Text(option.Key.text, option.Key.textIndex), option.Value);
                options.Add(new UrgentChoiceManager.UrgentChoiceData(text, option.Value, speed, color));
            }

            var title = GetLocalisedText(info.title, info.textIndex, localisedDialogues);
            ShowUrgentChoice(title, options, info.SelectOption, info.initialDelay);
        }

        /// <summary> Play hide animation, delay, then continue the dialogue</summary>
        void CloseAllBubbles(Action OnContinue = null, Action<int> OnSelectOption = null, int selectedChoice = -1)
        {
            foreach (var speakingActor in dialoguingActors)
            {
                speakingActor.uiContainer.HideSpeechBubble();
                speakingActor.animationController?.PlayExpression(Expression.None);
                speakingActor.animationController?.PlayGesture(Gesture.None);
            }

            StartCoroutine(WaitBubbleAnimation(showBubbleDuration));
            IEnumerator WaitBubbleAnimation(float delay)
            {
                // Animation hiding
                yield return new WaitForSeconds(delay);
                if (OnContinue != null) OnContinue();
                else if (OnSelectOption != null) OnSelectOption(selectedChoice);

                // Animation showing
                yield return new WaitForSeconds(delay);
                canTriggerNext = true;
                if (!isInMultipleChoice && IsInSpeaking) GameManager.Instance.Player.MouseManager.ChangeCursorSprite(CursorImageManager.CursorImage.DialogueNext);

            }
        }

        #endregion

        #region [Methods: Monologue & MultiMonologue]

        public bool BeginMonologue(string statementText, Expression expression, Gesture gesture, Actor actor, int overrideMaxCharacters)
        {
            var actorName = actor.ActorName;

            // Check availability and dependencies
            if (monologuingActors.Find(speakingActor => speakingActor.actorContainer.name == actorName) != null) return false;
            if (dialoguingActors.Find(speakingActor => speakingActor.actorContainer.name == actorName) != null) return false;

            ActorContainer actorContainer = allActorsInScene.Find(a => actorName == a.Actor.ActorName);
            if (actorContainer == null) { Debug.LogWarningFormat("Actor {0} couldn't be found", actorName); return false; }

            // Set dialogueUI
            DialogueUIContainer dialogueUIContainer;
            if (actorName == PlayerBrain.PLAYER_NAME)
                dialogueUIContainer = dialogueUIContainerPlayer;
            else
                dialogueUIContainer = actorContainer.InstantiateDialogueUIContainer(dialogueUIContainer_NPC_Prefab);

            // Set properties
            string text = overrideMaxCharacters ==
                -1 ? WrapText(statementText, maxCharacters) : overrideMaxCharacters ==
                0 ? statementText :
                        WrapText(statementText, overrideMaxCharacters);
            float readingDuration = text.Split(' ').Length / 230f * 60f + 2;

            // Show monologue
            if (dialogueUIContainer.ShowSpeechBubble(text, actorName: "", readingDuration))
            {
                // Call delegates when starting
                SpeakingActor speakingActor = new SpeakingActor(
                    actorContainer,
                    actorContainer.transform.GetComponent<AnimationController>(),
                    dialogueUIContainer);
                speakingActor.animationController?.PlayExpression(expression);
                speakingActor.animationController?.PlayGesture(gesture);

                OnSpeaking(true);
                OnActorSpeaking(speakingActor.actorContainer.name, true);
                monologuingActors.Add(speakingActor);

                StartCoroutine(Delay(readingDuration));
                IEnumerator Delay(float delay)
                {
                    yield return new WaitForSeconds(delay);

                    OnActorSpeaking(speakingActor.actorContainer.name, false);
                    OnSpeaking(false);
                    speakingActor.animationController?.PlayExpression(Expression.None);
                    speakingActor.animationController?.PlayGesture(Gesture.None);


                    yield return new WaitForSeconds(1.33f); // delay for closing animation

                    if (speakingActor.actorContainer.name != PlayerBrain.PLAYER_NAME) Destroy(speakingActor.uiContainer.gameObject);
                    monologuingActors.Remove(speakingActor);
                }

                return true;
            }
            else
            {
                if (actorName != PlayerBrain.PLAYER_NAME) Destroy(dialogueUIContainer.gameObject);
                return false;
            }
        }

        public bool BeginMonologue(MonologueBundle bundle, int index)
        {
            if (bundle.CSV != null)
            {
                var localisedMonologues = CSVUtility.GetColumn(bundle.CSV, GameManager.Instance.LanguageCode);
                var statement = bundle.Monologues[index].Statement;
                var actor = bundle.Monologues[index].Actor;
                var settings = bundle.Monologues[index].Settings;

                return BeginMonologue(GetLocalisedText(statement), statement.expression, statement.gesture, actor, settings.OverrideMaxCharacters);
            }
            else
            {
                return BeginMonologue(bundle.Monologues[index]);
            }
        }

        public bool BeginMonologue(Monologue monologue)
        {
            return BeginMonologue(monologue.Statement.text, monologue.Statement.expression, monologue.Statement.gesture, monologue.Actor, monologue.Settings.OverrideMaxCharacters);
        }

        Coroutine corMonologuing;
        public bool BeginMultiMonologue(MonologuesList dialogue)
        {
            if (corMonologuing != null) return false;
            #region [Check If Can MultiMonologue]

            foreach (var actorName in dialogue.GetActorNames())
            {
                if (allActorsInScene.Find(actor => actor.Actor.ActorName == actorName) == null)
                    return false;
                else if (monologuingActors.Find(speakingActor => speakingActor.actorContainer.name == actorName) != null)
                    return false;
            }

            #endregion

            corMonologuing = StartCoroutine(MultiMonologuing());
            IEnumerator MultiMonologuing()
            {
                GameManager.Instance.Player.SetCanInteract(false);
                OnSpeaking(true);

                // Show monologues one by one
                int index = 0;
                foreach (var monologue in dialogue.monologues)
                {
                    string actorName = monologue.Actor.ActorName;
                    Statement statement = monologue.Statement;

                    // Check availabbility and dependencies
                    ActorContainer actor = allActorsInScene.Find(a => actorName == a.Actor.ActorName);
                    if (actor == null) { Debug.LogWarningFormat("Actor {0} couldn't be found", actorName); }

                    DialogueUIContainer dialogueUIContainer;

                    if (actorName == PlayerBrain.PLAYER_NAME)
                    {
                        dialogueUIContainer = dialogueUIContainerPlayer;
                    }
                    else
                    {
                        ActorContainer actorContainer = actor.transform.GetComponent<ActorContainer>();
                        if (actorContainer == null) { Debug.LogWarningFormat("{0} has no ActorContainer", actor.name); }
                        dialogueUIContainer = actorContainer.InstantiateDialogueUIContainer(dialogueUIContainer_NPC_Prefab);
                    }

                    SpeakingActor speakingActor = new SpeakingActor(
                        actor,
                        actor.transform.GetComponent<AnimationController>(),
                        dialogueUIContainer);
                    monologuingActors.Add(speakingActor);

                    // Set properties
                    string text = monologue.Settings.OverrideMaxCharacters ==
                        -1 ? WrapText(GetLocalisedText(statement), maxCharacters) : monologue.Settings.OverrideMaxCharacters ==
                        0 ? GetLocalisedText(statement) :
                                WrapText(GetLocalisedText(statement), monologue.Settings.OverrideMaxCharacters);
                    float readingDuration = text.Split(' ').Length / 230f * 60f + 2;

                    dialogueUIContainer.ShowSpeechBubble(text, actorName, readingDuration);

                    // Call delegates when starting
                    OnActorSpeaking(actorName, true);
                    speakingActor.animationController?.PlayExpression(statement.expression);
                    speakingActor.animationController?.PlayGesture(statement.gesture);

                    // Wait for reading duration
                    yield return new WaitForSeconds(readingDuration);
                    OnActorSpeaking(actorName, false);
                    speakingActor.animationController?.PlayExpression(Expression.None);
                    speakingActor.animationController?.PlayGesture(Gesture.None);

                    // Wait bubble closes if the same person speak in sequence
                    if (index < dialogue.monologues.Count - 1 && dialogue.monologues[index].Actor.ActorName == actorName)
                        yield return new WaitForSeconds(1.33f);

                    index++;
                }

                yield return new WaitForSeconds(1.33f); // delay for final animation

                foreach (var actorName in dialogue.GetActorNames())
                {
                    SpeakingActor speakingActor = monologuingActors.Find(speakingActor => speakingActor.actorContainer.name == actorName);
                    if (speakingActor.actorContainer.name != PlayerBrain.PLAYER_NAME) Destroy(speakingActor.uiContainer.gameObject);
                    monologuingActors.Remove(speakingActor);
                }

                OnSpeaking(false);
                GameManager.Instance.Player.SetCanInteract(true);
                corMonologuing = null;
            }

            return true;
        }

        public bool BeginMultiMonologue(MultiMonologueBundle bundle, int index)
        {
            localisedDialogues = CSVUtility.GetColumn(bundle.CSV, GameManager.Instance.LanguageCode);
            return BeginMultiMonologue(bundle.Dialogues[index]);
        }

        #endregion  

        #region [Methods: Utility]

        string GetLocalisedText(IStatement statement)
        {
            return GetLocalisedText(statement.text, statement.textIndex, localisedDialogues);
        }

        string GetLocalisedText(DSyntaxData.Text text)
        {
            return GetLocalisedText(text.text, text.id, localisedDialogues);
        }

        string GetLocalisedText(string text, string id, Dictionary<string, string> localisedTexts)
        {
            localisedTexts.TryGetValue(id, out string localisedText);
            
            // Localised text is not found
            if (string.IsNullOrEmpty(localisedText))
            {
                Debug.Log("Localised text is empty : " + text);
                localisedText = text;
            }

            // Swap variables with its value for the localised text; Make sure the translated text includes the variable name
            var currentVariables = GetCurrentDialogueVariables?.Invoke();
            if (currentVariables != null)
            {
                foreach (var variable in currentVariables)
                   localisedText = localisedText.Replace("[" + variable.Key + "]", variable.Value);
            }

            return localisedText;
        }

        public static string WrapText(string input, int maxCharacters)
        {
            if (string.IsNullOrEmpty(input)) return "";

            // Split string by char " "         
            string[] words = input.Split(" "[0]);

            // Prepare result
            string result = "";

            // Temp line string
            string line = "";

            for (int i = 0; i < words.Length; i++)
            {
                if (words.Length == 1)
                {
                    result = words[i];
                    break;
                }

                if (i == 0)
                {
                    line = words[i];
                }
                else
                {
                    if (string.Concat(new string[] { line, " ", words[i] }).Length > maxCharacters)
                    {
                        result += line + "\n";
                        line = "";
                    }

                    if (i == words.Length - 1)
                    {
                        if (line.Length > 0)
                        {
                            result += line + " " + words[i];
                        }
                        else
                        {
                            result += words[i];
                        }
                    }
                    else
                    {
                        if (line.Length > 0)
                        {
                            line += " " + words[i];
                        }
                        else
                        {
                            line = words[i];
                        }
                    }
                }
            }

            return result.Trim();
        }

        public void AddDetectedActor(ActorContainer actor)
        {
            if (!allActorsInScene.Contains(actor))
                allActorsInScene.Add(actor);
        }

        public void RemoveActor(ActorContainer actor)
        {
            if (!allActorsInScene.Contains(actor))
                allActorsInScene.Remove(actor);
        }

        #endregion

    }
}