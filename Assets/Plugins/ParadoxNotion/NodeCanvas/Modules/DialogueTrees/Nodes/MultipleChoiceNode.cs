using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.DialogueTrees
{

    [ParadoxNotion.Design.Icon("List")]
    [Name("Multiple Choice")]
    [Category("Branch")]
    [Description("Prompt a Dialogue Multiple Choice. A choice will be available if the choice condition(s) are true or there is no choice conditions. The Actor selected is used for the condition checks and will also Say the selection if the option is checked.")]
    [Color("b3ff7f")]
    public class MultipleChoiceNode : DTNode
    {

        [System.Serializable]
        public class Choice
        {
            public bool isUnfolded = true;
            public Statement statement;
            public ConditionTask condition;
            public Choice() { }
            public Choice(Statement statement) {
                this.statement = statement;
            }
        }

        ///----------------------------------------------------------------------------------------------

        //SAVE: Custom property
        public MultipleChoiceRequestInfo.Mode mode;
        [ShowIfs(nameof(mode), new int [] { 0, 1,} )]
        public string title;
        [ShowIfs(nameof(mode), new int [] { 0, 1,} )]
        public string textIndex;
        [SliderField(0f, 8f), ShowIf(nameof(mode), 1)]
        public float initialDelay;
        [SliderField(0f, 60f), ShowIf(nameof(mode), 2)]
        public float availableTime;


        // Unneeded variables
        //public bool saySelection;

        [SerializeField, AutoSortWithChildrenConnections]
        private List<Choice> availableChoices = new List<Choice>();
        public List<Choice> GetAvailableChoices() { return availableChoices; }

        public override int maxOutConnections { get { return availableChoices.Count; } }
        public override bool requireActorSelection { get { return true; } }

        protected override Status OnExecute(Component agent, IBlackboard bb) {

            if ( outConnections.Count == 0 ) {
                return Error("There are no connections to the Multiple Choice Node!");
            }

            var finalOptions = new Dictionary<IStatement, int>();

            for ( var i = 0; i < availableChoices.Count; i++ ) {
                var condition = availableChoices[i].condition;
                if ( condition == null || condition.CheckOnce(finalActor.transform, bb) ) {
                    var tempStatement = availableChoices[i].statement.BlackboardReplace(bb);
                    finalOptions[tempStatement] = i;
                }
            }

            if ( finalOptions.Count == 0 ) {
                ParadoxNotion.Services.Logger.Log("Multiple Choice Node has no available options. Dialogue Ends.", LogTag.EXECUTION, this);
                DLGTree.Stop(false);
                return Status.Failure;
            }

            var optionsInfo = new MultipleChoiceRequestInfo(finalActor, finalOptions, OnOptionSelected, mode, initialDelay, title, availableTime, textIndex);
            optionsInfo.showLastStatement = inConnections.Count > 0 && inConnections[0].sourceNode is StatementNode;
            DialogueTree.RequestMultipleChoices(optionsInfo);
            return Status.Running;
        }

        void OnOptionSelected(int index) {

            status = Status.Success;
            DLGTree.Continue(index);



            // Unneeded code
            //status = Status.Success;
            //System.Action Finalize = () => { DLGTree.Continue(index); };

            //if ( saySelection ) {
            //    var tempStatement = availableChoices[index].statement.BlackboardReplace(graphBlackboard);
            //    var speechInfo = new SubtitlesRequestInfo(finalActor, tempStatement, Finalize);
            //    DialogueTree.RequestSubtitles(speechInfo);
            //} else {
            //    Finalize();
            //}
        }

        ///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
#if UNITY_EDITOR

        public override void OnConnectionInspectorGUI(int i) {
            DoChoiceGUI(availableChoices[i]);
        }

        public override string GetConnectionInfo(int i) {
            if ( i >= availableChoices.Count ) {
                return "NOT SET";
            }
            var text = string.Format("'{0}'", availableChoices[i].statement.text);
            if ( availableChoices[i].condition == null ) {
                return text;
            }
            return string.Format("{0}\n{1}", text, availableChoices[i].condition.summaryInfo);
        }

        protected override void OnNodeGUI() {

            if (availableChoices.Count == 0)
            {
                GUILayout.Label("No Options Available");
                return;
            }

            if (mode == MultipleChoiceRequestInfo.Mode.Normal)
            {
                if (!string.IsNullOrEmpty(title))
                    GUILayout.Label(title);
            }

            else if (mode == MultipleChoiceRequestInfo.Mode.Urgent)
            {
                if (!string.IsNullOrEmpty(title))
                    GUILayout.Label(title);
                if (initialDelay > 0)
                    GUILayout.Label("Delay: " + initialDelay);
            }
            else if (mode == MultipleChoiceRequestInfo.Mode.Chat)
            {
                if (availableTime > 0)
                {
                    GUILayout.Label(availableTime + "' Seconds");
                }
            }

            for ( var i = 0; i < availableChoices.Count; i++ ) {
                var choice = availableChoices[i];
                var connection = i < outConnections.Count ? outConnections[i] : null;
                GUILayout.BeginHorizontal(Styles.roundedBox);
                GUILayout.Label(string.Format("{0} {1}", connection != null ? "■" : "□", choice.statement.text.CapLength(30)), Styles.leftLabel);
                GUILayout.EndHorizontal();
            }

            // Uneeded codes
            //GUILayout.BeginHorizontal();
            //if ( availableTime > 0 ) {
            //    GUILayout.Label(availableTime + "' Seconds");
            //}
            //if ( saySelection ) {
            //    GUILayout.Label("Say Selection");
            //}
            //GUILayout.EndHorizontal();
        }

        protected override void OnNodeInspectorGUI() {

            base.OnNodeInspectorGUI();

            if ( GUILayout.Button("Add Choice") ) {
                availableChoices.Add(new Choice(new Statement("I am a choice...")));
            }

            if ( availableChoices.Count == 0 ) {
                return;
            }

            EditorUtils.ReorderableList(availableChoices, (i, picked) =>
            {
                var choice = availableChoices[i];
                GUILayout.BeginHorizontal("box");

                var text = string.Format("{0} {1}", choice.isUnfolded ? "▼ " : "► ", choice.statement.text);
                if ( GUILayout.Button(text, (GUIStyle)"label", GUILayout.Width(0), GUILayout.ExpandWidth(true)) ) {
                    choice.isUnfolded = !choice.isUnfolded;
                }

                if ( GUILayout.Button("X", GUILayout.Width(20)) ) {
                    availableChoices.RemoveAt(i);
                    if ( i < outConnections.Count ) {
                        graph.RemoveConnection(outConnections[i]);
                    }
                }

                GUILayout.EndHorizontal();

                if ( choice.isUnfolded ) {
                    DoChoiceGUI(choice);
                }
            });

        }

        void DoChoiceGUI(Choice choice) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");

            choice.statement.text = UnityEditor.EditorGUILayout.TextField(choice.statement.text);
            choice.statement.textIndex = UnityEditor.EditorGUILayout.TextField("Text Index", choice.statement.textIndex);
            if (mode == MultipleChoiceRequestInfo.Mode.Urgent)
                choice.statement.meta = UnityEditor.EditorGUILayout.TextField("Meta", choice.statement.meta);

            // Commented out because not needed in Encore Project
            //choice.statement.audio = UnityEditor.EditorGUILayout.ObjectField("Audio File", choice.statement.audio, typeof(AudioClip), false) as AudioClip;
            //choice.statement.meta = UnityEditor.EditorGUILayout.TextField("Meta Data", choice.statement.meta);

            NodeCanvas.Editor.TaskEditor.TaskFieldMulti<ConditionTask>(choice.condition, graph, (c) => { choice.condition = c; });

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

#endif
    }
}