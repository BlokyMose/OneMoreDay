using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;
using System;
using Sirenix.OdinInspector;
using Encore.Saves;

namespace Encore.Dialogues
{
    [AddComponentMenu("Encore/Dialogues/Actor Container")]
    public class ActorContainer : MonoBehaviour, IDialogueActor
    {
        #region [Vars: Components]

        [SerializeField]
        Actor actor;

        [SerializeField, Required]
        GameObject dialogueCanvas;

        #endregion

        #region [Vars: Data Handlers]

        protected DialogueUIContainer dialogueUIContainer;
        public virtual DialogueUIContainer GetDialogueUIContainer()
        {
            return dialogueUIContainer;
        }

        Coroutine corFacing;

        #endregion
        public Actor Actor { get { return actor; } }

        #region [Interface: IDialogueActor]

        public new string name => actor.ActorName;

        public Texture2D portrait => null;

        public Sprite portraitSprite => null;

        public Color dialogueColor => new Color(0, 0, 0, 0);

        public Vector3 dialoguePosition => Vector3.zero;

        #endregion

        void OnDisable()
        {
            if (GameManager.Instance != null && GameManager.Instance.DialogueManager)
                GameManager.Instance.DialogueManager.RemoveActor(this);
        }

        public void FaceAt(GameObject target)
        {
            StopFacing();
            corFacing = StartCoroutine(Facing());
            IEnumerator Facing()
            {
                while (true)
                {
                    if (target.transform.position.x > transform.position.x)
                    {
                        transform.localEulerAngles = Vector3.zero;
                    }
                    else if (target.transform.position.x < transform.position.x)
                    {
                        transform.localEulerAngles = new Vector3(0, 180, 0);
                    }

                    if (dialogueUIContainer != null)
                        dialogueUIContainer.Flip(transform.localEulerAngles);

                    yield return null;
                }
            }
        }

        public void StopFacing()
        {
            if (corFacing != null) StopCoroutine(corFacing);
        }

        public void Say(MonologueBundle bundle, int index)
        {
            GameManager.Instance.DialogueManager.BeginMonologue(bundle, index);
        }

        public void Say(Monologue monologue)
        {
            GameManager.Instance.DialogueManager.BeginMonologue(monologue);
        }

        public DialogueUIContainer InstantiateDialogueUIContainer(GameObject dialogueUIContainerPrefab)
        {
            GameObject dialogueUIContainerGO = Instantiate(dialogueUIContainerPrefab, dialogueCanvas.transform, false);
            dialogueUIContainer = dialogueUIContainerGO.GetComponent<DialogueUIContainer>();
            dialogueUIContainer.actorName = actor.name;
            dialogueUIContainer.Flip(transform.localEulerAngles);
            return dialogueUIContainer;
        }
    }
}