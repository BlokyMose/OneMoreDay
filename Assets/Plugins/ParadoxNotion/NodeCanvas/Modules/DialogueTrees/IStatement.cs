using ParadoxNotion;
using NodeCanvas.Framework;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace NodeCanvas.DialogueTrees
{
    public enum Expression { None = -1, Listening, Sad, HappyBit, Happy, Confused, Surprised, Angry, Untrust  }
    public enum Gesture { None = -1, Speaking, Nod, Thinking, Pondering, This, NoIdea, LeanBack }


    ///<summary>An interface to use for whats being said by a dialogue actor</summary>
    public interface IStatement
    {
        string text { get; }

        AudioClip audio { get; }
        string meta { get; }

        Expression expression { get; } // SAVE: Custom property
        Gesture gesture { get; } // Custom property
        string textIndex { get; } // Custom property
        float duration { get; } // Custom property
    }

    ///<summary>Holds data of what's being said usualy by an actor</summary>
    [System.Serializable]
    public class Statement : IStatement
    {
        #region [Properties]

        [SerializeField,TextArea(2,5),HideLabel]
        private string _text = string.Empty;
        [SerializeField, HorizontalGroup, LabelText("Face:"), LabelWidth(50), PropertySpace(0,5)]
        private Expression _expression = Expression.None; // SAVE: Custom property
        [SerializeField,HorizontalGroup, LabelText("Gesture:"), LabelWidth(50), PropertySpace(0, 5)]
        private Gesture _gesture = Gesture.None; // Custom property
        [SerializeField]
        private string _textIndex; // Custom property

        [SerializeField,Range(1,10)]
        private float _duration = 2.2f;

        [HideInInspector,SerializeField]
        private AudioClip _audio;
        [HideInInspector, SerializeField]
        private string _meta = string.Empty;

        #endregion

        #region [Set-Get]

        public string text {
            get { return _text; }
            set { _text = value; }
        }

        public AudioClip audio {
            get { return _audio; }
            set { _audio = value; }
        }

        public string meta {
            get { return _meta; }
            set { _meta = value; }
        }

        public Expression expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        public Gesture gesture
        {
            get { return _gesture; }
            set { _gesture = value; }
        }

        public string textIndex
        {
            get { return _textIndex; }
            set { _textIndex = value; }
        }

        public float duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        #endregion

        #region [Constructors]

        public Statement() { }
        public Statement(string text) {
            this.text = text;
        }

        public Statement(string text, AudioClip audio) {
            this.text = text;
            this.audio = audio;
        }

        public Statement(string text, AudioClip audio, string meta) {
            this.text = text;
            this.audio = audio;
            this.meta = meta;
        }

        public Statement(string text, AudioClip audio, string meta, Expression expression)
        {
            this.text = text;
            this.audio = audio;
            this.meta = meta;
            this.expression = expression;
        }

        public Statement(string text, AudioClip audio, string meta, Expression expression, float duration)
        {
            this.text = text;
            this.audio = audio;
            this.meta = meta;
            this.expression = expression;
            this.duration = duration;
        }

        public Statement(string text, AudioClip audio, string meta, Expression expression, float duration, Gesture gesture)
        {
            this.text = text;
            this.audio = audio;
            this.meta = meta;
            this.expression = expression;
            this.duration = duration;
            this.gesture = gesture;
        }

        public Statement(string text, AudioClip audio, string meta, Expression expression, float duration, Gesture gesture, string textIndex)
        {
            this.text = text;
            this.audio = audio;
            this.meta = meta;
            this.expression = expression;
            this.duration = duration;
            this.gesture = gesture;
            this.textIndex = textIndex;
        }

        #endregion

        ///<summary>Replace the text of the statement found in brackets, with blackboard variables ToString and returns a Statement copy</summary>
        public IStatement BlackboardReplace(IBlackboard bb) {
            var copy = ParadoxNotion.Serialization.JSONSerializer.Clone<Statement>(this);

            copy.text = copy.text.ReplaceWithin('[', ']', (input) =>
            {
                object o = null;
                if ( bb != null ) { //referenced blackboard replace
                    var v = bb.GetVariable(input, typeof(object));
                    if ( v != null ) { o = v.value; }
                }

                if ( input.Contains("/") ) { //global blackboard replace
                    var globalBB = GlobalBlackboard.Find(input.Split('/').First());
                    if ( globalBB != null ) {
                        var v = globalBB.GetVariable(input.Split('/').Last(), typeof(object));
                        if ( v != null ) { o = v.value; }
                    }
                }
                return o != null ? o.ToString() : input;
            });

            return copy;
        }

        public override string ToString() {
            return text;
        }
    }

    public static class NodeCanvasUtility
    {
        public static Expression ConvertToExpression(string text)
        {
            if (text == Expression.Listening.ToString()) return Expression.Listening;
            else if (text == Expression.Sad.ToString()) return Expression.Sad;
            else if (text == Expression.HappyBit.ToString()) return Expression.HappyBit;
            else if (text == Expression.Happy.ToString()) return Expression.Happy;
            else if (text == Expression.Confused.ToString()) return Expression.Confused;
            else if (text == Expression.Surprised.ToString()) return Expression.Surprised;
            else if (text == Expression.Angry.ToString()) return Expression.Angry;
            else if (text == Expression.Untrust.ToString()) return Expression.Untrust;
            else return Expression.None;
        }
    }
}