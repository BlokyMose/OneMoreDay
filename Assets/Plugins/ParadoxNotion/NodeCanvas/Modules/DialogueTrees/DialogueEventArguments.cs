using System;
using System.Collections.Generic;


namespace NodeCanvas.DialogueTrees
{

    ///<summary>Send along with a OnSubtitlesRequest event. Holds info about the actor speaking, the statement that being said as well as a callback to be called when dialogue is done showing</summary>
    public class SubtitlesRequestInfo
    {

        ///<summary>The actor speaking</summary>
        public IDialogueActor actor;
        ///<summary>The statement said</summary>
        public IStatement statement;
        ///<summary>Call this to Continue the DialogueTree</summary>
        public Action Continue;

        public SubtitlesRequestInfo(IDialogueActor actor, IStatement statement, Action callback) {
            this.actor = actor;
            this.statement = statement;
            this.Continue = callback;
        }
    }

    ///<summary>Send along with a OnMultipleChoiceRequest event. Holds information of the options, time available as well as a callback to be called providing the selected option</summary>
    public class MultipleChoiceRequestInfo
    {

        ///<summary>The actor related. This is usually the actor that will also say the options</summary>
        public IDialogueActor actor;
        public enum Mode { Normal, Urgent, Chat }
        public Mode mode = Mode.Normal;

        ///<summary>Time for player to read the texts</summary>
        public float initialDelay; // SAVE: custom property
        ///<summary>Title/Question of multiple choice</summary>
        public string title;
        ///<summary>For localization</summary>
        public string textIndex;
        ///<summary>The available choice option. Key: The statement, Value: the child index of the option</summary>
        public Dictionary<IStatement, int> options;

        ///<summary>Call this with to select the option to continue with in the DialogueTree</summary>
        public Action<int> SelectOption;

        ///<summary>The available time for a choice</summary>
        public float availableTime;
        ///<summary>Should the previous statement be shown along the options?</summary>
        public bool showLastStatement;

        public MultipleChoiceRequestInfo(IDialogueActor actor, Dictionary<IStatement, int> options, float availableTime, bool showLastStatement, Action<int> callback) {
            this.actor = actor;
            this.options = options;
            this.availableTime = availableTime;
            this.showLastStatement = showLastStatement;
            this.SelectOption = callback;
        }

        public MultipleChoiceRequestInfo(IDialogueActor actor, Dictionary<IStatement, int> options, Action<int> callback, Mode mode, float initialDelay, string title, float availableTime, string textIndex)
        {
            this.actor = actor;
            this.options = options;
            this.SelectOption = callback;

            this.mode = mode;
            this.title = title;
            this.textIndex = textIndex;
            this.initialDelay = initialDelay;
            this.availableTime=availableTime;   
        }
    }
}