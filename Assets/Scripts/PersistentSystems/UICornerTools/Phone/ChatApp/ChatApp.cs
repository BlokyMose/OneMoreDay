using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using NodeCanvas.DialogueTrees;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using NodeCanvas.Framework;
using Encore.Dialogues;
using Encore.CharacterControllers;
using static DialogueSyntax.DSyntaxData;
using DialogueSyntax;
using Encore.MiniGames.UrgentChoice;

namespace Encore.Phone.Chat
{
    [Serializable]
    public class ChatData
    {
        public enum ChatActorType { Player, NPC, System }
        [SerializeField, EnumToggleButtons, HideLabel]
        ChatActorType actorType;
        public ChatActorType ActorType { get { return actorType; } }

        [SerializeField, TextArea(minLines: 1, maxLines: 3), HideLabel]
        string say;
        public string Say { get { return say; } }

        public ChatData(ChatActorType actorType, string say)
        {
            this.actorType = actorType;
            this.say = say;
        }
    }

    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Encore/Phone/Chat App")]
    public class ChatApp : PhoneApp
    {
        #region [Classes]

        class ChatBubble
        {
            ChatData _data;
            Image _bubble;
            TextMeshProUGUI _text;

            public ChatData data { get { return _data; } }
            public Image bubble { get { return _bubble; } }
            public TextMeshProUGUI text { get { return _text; } }

            public ChatBubble(ChatData data, Image bubble, TextMeshProUGUI text)
            {
                _data = data;
                _bubble = bubble;
                _text = text;
            }
        }

        class ChatContactBubble
        {
            ChatContact contact;
            Image bubble;
            Image pointer;
            Image alert;


            public ChatContact Contact { get { return contact; } }
            public Image Bubble { get { return bubble; } }
            public Image Pointer { get { return pointer; } }
            public Image Alert { get { return alert; } }

            public ChatContactBubble(ChatContact contact, Image bubble, Image pointer, Image alert)
            {
                this.contact = contact;
                this.bubble = bubble;
                this.pointer = pointer;
                this.alert = alert;
            }
        }

        public class CachedMultipleChoice
        {
            public struct Choice
            {
                public string text;
                public int index;

                public Choice(string text, int choiceIndex)
                {
                    this.text = text;
                    this.index = choiceIndex;
                }
            }
            public List<Choice> Choices { get; private set; }
            public float AvailableTimeLeft { get; set; }

            public CachedMultipleChoice(List<Choice> choices, float availableTimeLeft)
            {
                this.Choices = choices;
                this.AvailableTimeLeft = availableTimeLeft;
            }

            public CachedMultipleChoice(MultipleChoiceNode node, float availableTimeLeft)
            {
                var choices  = new List<Choice>();

                // TODO: Conditions has not been checked, so all choices will be included
                // Consider not using NodeCanvas for Chat?

                int index = 0; 
                foreach (var choice in node.GetAvailableChoices())
                {
                    choices.Add(new Choice(choice.statement.text,index));
                    index++;
                }

                this.Choices = choices;
                this.AvailableTimeLeft = availableTimeLeft;
            }
        }

        #endregion

        #region [Vars: Components]

        [Header("Hierarchy Components")]
        [SerializeField] Transform chatList;
        [SerializeField] Transform contactList;
        [SerializeField] TextMeshProUGUI talkingWithName;
        [SerializeField] TextMeshProUGUI talkingWithStatus;

        [Header("Contact Components")]
        [SerializeField] Button butShowAddContactScreen;
        [SerializeField] Button exitAddContactScreen;
        [SerializeField] KeyboardPhoneNumber keyboardPhoneNumber;
        [SerializeField] UnityEngine.CanvasGroup addContactResultScreen;
        [SerializeField] TextMeshProUGUI addContactResultText;

        [Header("External Components")]
        [SerializeField] GameObject prefabBubblePlayer;
        [SerializeField] GameObject prefabBubbleNPC;
        [SerializeField] GameObject prefabBubbleSystem;
        [SerializeField] GameObject prefabBubbleChoice;
        [SerializeField] GameObject prefabContact;

        #endregion

        #region [Vars: Properties]

        [Header("Properties")]
        [SerializeField] int maxCharacters = 25;
        [SerializeField] List<ChatContact> initialContacts = new List<ChatContact>();

        #endregion

        #region [Vars: Data Handlers]

        List<ChatData> currentDialogue = new List<ChatData>();
        ChatContactBubble currentlyTalkingWith = null;

        public static readonly string CHAT_SAVE_KEY = "Chat";
        const string UNREAD_MESSAGES = "Unread messages:";
        const string SYSTEM_ACTOR_NAME = "System";
        const string IGNORE_MESSAGE = "<ignore>";

        Color colorContactUnselected = new Color(1, 1, 1, 0.33f);
        Color colorContactSelected = new Color(1, 1, 1, 1f);
        Color colorContactHighligthed = new Color(1, 1, 1, 0.66f);

        Animator animator;
        int boo_contactAdded, tri_showAddContactResult, boo_showAddContact, boo_showChoice;
        List<ChatContactBubble> contactBubbles = new List<ChatContactBubble>();

        /// <summary>Called by <see cref="ChatManager.BeginDialogue(DialogueTree)"/></summary>
        Dictionary<string, CachedMultipleChoice> choicesKept = new Dictionary<string, CachedMultipleChoice>();

        /// <summary>Always set to default value once being set in <see cref="BeginDialogue(DialogueTree)"/></summary> 
        struct SelectedChoice
        {
            public string actorName;
            public CachedMultipleChoice.Choice choice; // -1: waiting for selection; 0: means ignore; index that is higher refers to the choices

            public SelectedChoice(string actorName, CachedMultipleChoice.Choice choice)
            {
                this.actorName = actorName;
                this.choice = choice;
            }
        }
        SelectedChoice selectedChoiceIndex = new SelectedChoice("actorName", new CachedMultipleChoice.Choice("",-1));

        #endregion

        #region [Methods: Unity]

        protected override void Awake()
        {
            animator = GetComponent<Animator>();
            base.Awake();
            boo_contactAdded = Animator.StringToHash(nameof(boo_contactAdded));
            tri_showAddContactResult = Animator.StringToHash(nameof(tri_showAddContactResult));
            boo_showAddContact = Animator.StringToHash(nameof(boo_showAddContact));
            boo_showChoice = Animator.StringToHash("boo_show");
        }

        private void OnEnable()
        {
            butShowAddContactScreen.onClick.AddListener(() =>
            {
                animator.SetBool(boo_showAddContact, true);
                exitAddContactScreen.transform.parent.gameObject.SetActive(true);
            });
            exitAddContactScreen.onClick.AddListener(() =>
            {
                animator.SetBool(boo_showAddContact, false);
                exitAddContactScreen.transform.parent.gameObject.SetActive(false);
                keyboardPhoneNumber.Clear();
            });

            keyboardPhoneNumber.OnSend += AddContact;

            exitAddContactScreen.transform.parent.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            butShowAddContactScreen.onClick.RemoveAllListeners();
            exitAddContactScreen.onClick.RemoveAllListeners();
            keyboardPhoneNumber.OnSend -= AddContact;
        }

        #endregion

        #region [Methods: UI Corner Tool]

        public override void Show(bool isShow)
        {
            base.Show(isShow);

            if (isShow)
            {
                addContactResultScreen.gameObject.SetActive(false);
            }
            else
            {
                exitAddContactScreen.transform.parent.gameObject.SetActive(false);
                keyboardPhoneNumber.Clear();
                SaveChat();
                currentlyTalkingWith = null;
            }
        }


        #endregion

        #region [Methods: Dialogue]

        /// <summary> Called by <see cref="ChatTrigger.Interact"/> </summary>
        public void BeginDialogue(DialogueTree tree)
        {
            // Find contact data
            string npcName = tree.actorParameters.Find(a => a.name != PlayerBrain.PLAYER_NAME).name;

            var contact = GameManager.Instance.GetGameAssets().resources.ChatContacts.Find(c => c.ContactName == npcName);
            if (contact == null) { Debug.LogError("ChatApp: Could not find contact with name: " + npcName); return; }

            // Remove the last "Unread messages:", then add a new "Unread messages:"
            for (int i = contact.savedDialogue.Count - 1; i >= 0; i--)
            {
                if (contact.savedDialogue[i].ActorType == ChatData.ChatActorType.System &&
                    contact.savedDialogue[i].Say == UNREAD_MESSAGES)
                {
                    contact.savedDialogue.RemoveAt(i);
                    break;
                }
            }
            contact.savedDialogue.Add(new ChatData(ChatData.ChatActorType.System, UNREAD_MESSAGES));

            StartCoroutine(RunDialogue());
            IEnumerator RunDialogue()
            {
                NodeCanvas.Framework.Node currentNode = tree.primeNode;
                while (true)
                {
                    if (currentNode is StatementNode)
                    {
                        ShowSpeech((StatementNode)currentNode, contact);
                        if (currentNode.outConnections.Count == 0) break;
                        yield return new WaitForSeconds((currentNode as StatementNode).statement.duration);
                        currentNode = currentNode.outConnections[0].targetNode;
                    }

                    else if (currentNode is MultipleChoiceNode)
                    {
                        // HARDCODE:    1. MultipleChoiceNode must have "<Ignore>" choice as the first choice;
                        //              2. Every choice needs to be followed by a statement that repeats the choice's statement or so

                        MultipleChoiceNode multipleChoice = currentNode as MultipleChoiceNode;

                        // Record the choices in case player close the phone or player open the open while the choices are available
                        if (!choicesKept.ContainsKey(npcName))
                            choicesKept.Add(npcName, new CachedMultipleChoice(multipleChoice, multipleChoice.availableTime));

                        ShowMultipleChoice(multipleChoice, contact, multipleChoice.availableTime);

                        while (true)
                        {
                            choicesKept[npcName].AvailableTimeLeft -= Time.deltaTime;

                            // Auto choose "0" when running out of time
                            if (choicesKept[npcName].AvailableTimeLeft <= 0)
                                selectedChoiceIndex = new SelectedChoice(npcName, new CachedMultipleChoice.Choice(IGNORE_MESSAGE, 0));

                            // Stop waiting
                            if (selectedChoiceIndex.actorName == npcName && selectedChoiceIndex.choice.index != -1)
                            {
                                currentNode = currentNode.outConnections[selectedChoiceIndex.choice.index].targetNode;
                                choicesKept.Remove(npcName);
                                selectedChoiceIndex = new SelectedChoice("actorName", new CachedMultipleChoice.Choice("",-1));
                                break;
                            }
                            yield return null;
                        }


                    }

                    yield return null;
                }

            }
        }

        public bool BeginDialogue(DSyntaxBundle bundle)
        {
            var tree = DSyntaxUtility.GetTree(bundle.DSyntax.dSyntax, GameManager.Instance.DialogueManager.GetDSyntaxSettings);

            // Find contact data
            string npcName = "";

            foreach (var actor in tree.actors)
            {
                if (actor.Key != PlayerBrain.PLAYER_NAME)
                {
                    npcName = actor.Key;
                    break;
                }
            }

            var contact = GameManager.Instance.GetGameAssets().resources.ChatContacts.Find(c => c.ContactName == npcName); ;
            if (contact == null) { Debug.LogError("ChatApp: Could not find contact with name: " + npcName); return false; }

            // Remove the last "Unread messages:", then add a new "Unread messages:"
            for (int i = contact.savedDialogue.Count - 1; i >= 0; i--)
            {
                if (contact.savedDialogue[i].ActorType == ChatData.ChatActorType.System &&
                    contact.savedDialogue[i].Say == UNREAD_MESSAGES)
                {
                    contact.savedDialogue.RemoveAt(i);
                    break;
                }
            }
            contact.savedDialogue.Add(new ChatData(ChatData.ChatActorType.System, UNREAD_MESSAGES));

            StartCoroutine(GameManager.Instance.DialogueManager.RunDSyntaxDialogue(tree, OnDialogueFinished, OnNodeSay, OnNodeChoice, OnNodeUrgent));


            void OnDialogueFinished()
            {

            }

            void OnNodeSay(IStatement statement, string actorName, Action OnContinue)
            {
                ShowSpeech(statement, actorName, contact);

                StartCoroutine(Delay());
                IEnumerator Delay()
                {
                    yield return new WaitForSeconds(statement.duration);
                    OnContinue();
                }
            }

            void OnNodeChoice(string title, Dictionary<string, int> options, Action<int> OnSelectOption)
            {
                // HARDCODE:    1. MultipleChoiceNode must have "<Ignore>" choice as the first choice;
                //              2. Every choice needs to be followed by a statement that repeats the choice's statement or so


                // TODO: there's no architecture to support availableTime in NodeChoice because the nature of conventional dialogue and Chat is different
                int randomAvailableTime = UnityEngine.Random.Range(2, 10);
                var choices = new List<CachedMultipleChoice.Choice>();
                foreach (var choice in options)
                {
                    choices.Add(new CachedMultipleChoice.Choice(choice.Key, choice.Value));
                }

                // Record the choices in case player close the phone or player open the open while the choices are available
                if (!choicesKept.ContainsKey(npcName))
                    choicesKept.Add(npcName, new CachedMultipleChoice(choices, randomAvailableTime));


                ShowMultipleChoice(choices, contact, randomAvailableTime);


                StartCoroutine(WaitChoiceSelection());
                IEnumerator WaitChoiceSelection()
                {
                    while (true)
                    {
                        choicesKept[npcName].AvailableTimeLeft -= Time.deltaTime;

                        // Auto choose "0" when running out of time
                        if (choicesKept[npcName].AvailableTimeLeft <= 0)
                        {
                            selectedChoiceIndex = new SelectedChoice(npcName, new CachedMultipleChoice.Choice(IGNORE_MESSAGE, 0));
                        }

                        // Stop waiting
                        if (selectedChoiceIndex.actorName == npcName && selectedChoiceIndex.choice.index != -1)
                        {
                            OnSelectOption(selectedChoiceIndex.choice.index);
                            choicesKept.Remove(npcName);
                            selectedChoiceIndex = new SelectedChoice("actorName", new CachedMultipleChoice.Choice("", -1));
                            break;
                        }
                        yield return null;
                    }


                }
            }

            void OnNodeUrgent(string title, float initialDelay, List<UrgentChoiceManager.UrgentChoiceData> options, Action<int> OnSelectOption)
            {
                // TODO: create ShowUrgent instead of using ShowMultipleChoice

                var choices = new Dictionary<string, int>();
                foreach (var choice in options)
                    choices.Add(choice.text, choice.choiceIndex);

                OnNodeChoice(title, choices, OnSelectOption); 
            }


            return true;
        }

        public void ShowSpeech(IStatement statement, string actorName, ChatContact chatContactData)
        {
            string wrappedText = DialogueManagerNC.WrapText(statement.text, maxCharacters);

            // Record te dialogue when ChatApp is closed or Player is not viewing the same chat
            if (!isShow || currentlyTalkingWith.Contact.ContactName != chatContactData.ContactName)
            {
                // Player
                if (actorName == PlayerBrain.PLAYER_NAME)
                {
                    if (statement.text == IGNORE_MESSAGE) return;
                    chatContactData.savedDialogue.Add(new ChatData(ChatData.ChatActorType.Player, wrappedText));
                }
                // System
                else if (actorName == SYSTEM_ACTOR_NAME)
                {
                    chatContactData.savedDialogue.Add(new ChatData(ChatData.ChatActorType.System, wrappedText));
                }
                // NPC
                else
                {
                    chatContactData.savedDialogue.Add(new ChatData(ChatData.ChatActorType.NPC, wrappedText));

                    SetContactAlert(actorName, true);

                    // Add notification if player is not opening phone
                    if (!isShow)
                    {
                        PhoneManager.NotificationData notif = new PhoneManager.NotificationData(
                            appName: PhoneManager.PhoneAppName.Chat,
                            time: GameManager.Instance.TimeManager.clock.GetHour().ToString() + ":" + GameManager.Instance.TimeManager.clock.GetMinute().ToString(),
                            title: chatContactData.ContactName,
                            desc: statement.text,
                            image: chatContactData.Photo,
                            sound: notificationSound
                            );
                        OnAddNotification(notif);
                    }

                }
                chatContactData.HasAlert = true;

            }

            // Add bubbles when ChatApp is being opened
            else
            {
                AddSpeechBubble(actorName, wrappedText);
            }
        }

        public void ShowSpeech(StatementNode node, ChatContact chatContact)
        {
            ShowSpeech(node.statement, node.actorName, chatContact);
        }

        public void ShowMultipleChoice(List<CachedMultipleChoice.Choice> choices, ChatContact contactData, float delayToDelete)
        {
            if (currentlyTalkingWith?.Contact != contactData) return;

            List<GameObject> choicePanels = new List<GameObject>();

            // TODO: Prevent the same contact deliver two different choices at the same time

            // When ChatApp is being opened
            bool hasSelected = false;

            foreach (var choice in choices)
            {
                GameObject panel = Instantiate(prefabBubbleChoice, chatList);
                UnityEngine.CanvasGroup canvasGroup = panel.transform.GetComponent<UnityEngine.CanvasGroup>();
                Image bubble = panel.transform.GetChild(0).GetComponent<Image>();
                canvasGroup.alpha = 0.66f;
                TextMeshProUGUI text = canvasGroup.transform.GetChild(0).transform.Find("Text").GetComponent<TextMeshProUGUI>();
                text.text = choice.text;
                choicePanels.Add(panel);

                EventTrigger et = bubble.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry et_entry_enter = new EventTrigger.Entry();
                et_entry_enter.eventID = EventTriggerType.PointerEnter;
                et_entry_enter.callback.AddListener((data) => { canvasGroup.alpha = 1; });
                et.triggers.Add(et_entry_enter);

                EventTrigger.Entry et_entry_exit = new EventTrigger.Entry();
                et_entry_exit.eventID = EventTriggerType.PointerExit;
                et_entry_exit.callback.AddListener((data) => { canvasGroup.alpha = 0.66f; });
                et.triggers.Add(et_entry_exit);

                EventTrigger.Entry et_entry_click = new EventTrigger.Entry();
                et_entry_click.eventID = EventTriggerType.PointerClick;
                et_entry_click.callback.AddListener((data) => { 
                    hasSelected = true; 
                    selectedChoiceIndex = new SelectedChoice(contactData.ContactName, choice); 
                    DeleteChoicePanels(choicePanels);
                });
                et.triggers.Add(et_entry_click);

                Canvas.ForceUpdateCanvases();
                chatList.GetComponent<VerticalLayoutGroup>().enabled = false;
                chatList.GetComponent<VerticalLayoutGroup>().enabled = true;
                Canvas.ForceUpdateCanvases();
            }


            StartCoroutine(DelayToDelete(delayToDelete));
            IEnumerator DelayToDelete(float delay)
            {
                yield return new WaitForSeconds(delay);
                if (!hasSelected && choicePanels[0] != null)
                    DeleteChoicePanels(choicePanels);
            }

            void DeleteChoicePanels(List<GameObject> targetedPanels)
            {
                // Move to temp list
                List<GameObject> choicePanels = new List<GameObject>();
                foreach (GameObject panel in targetedPanels)
                {
                    choicePanels.Add(panel);
                }

                // Play animation, then delete
                for (int i = choicePanels.Count - 1; i >= 0; i--)
                {
                    int index = i;
                    Animator animator = choicePanels[index].GetComponent<Animator>();
                    animator.SetBool(boo_showChoice, false);
                    Destroy(choicePanels[index], 0.75f);
                }

                StartCoroutine(Delay(0.825f));
                IEnumerator Delay(float delay)
                {
                    Canvas.ForceUpdateCanvases();
                    yield return new WaitForSeconds(delay);
                    chatList.GetComponent<VerticalLayoutGroup>().enabled = false;
                    chatList.GetComponent<VerticalLayoutGroup>().enabled = true;
                    Canvas.ForceUpdateCanvases();

                }
            }
        }

        public void ShowMultipleChoice(MultipleChoiceNode multipleChoiceNode, ChatContact contactData, float availableTime)
        {
            var cachedMulipleChoice  = new CachedMultipleChoice(multipleChoiceNode, availableTime);
            ShowMultipleChoice(cachedMulipleChoice.Choices, contactData, availableTime);
        }

        void AddSpeechBubble(string actorName, string say)
        {
            // HARDCODE: player name's in dialogueTree must be PlayerBrain.PLAYER_NAME
            if (actorName == PlayerBrain.PLAYER_NAME)
            {
                if (say == IGNORE_MESSAGE) return;

                GameObject newBubble = Instantiate(prefabBubblePlayer, chatList);
                TextMeshProUGUI text = newBubble.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                text.text = say;
                currentDialogue.Add(new ChatData(ChatData.ChatActorType.Player, say));
            }
            else if (actorName == SYSTEM_ACTOR_NAME)
            {
                GameObject newBubble = Instantiate(prefabBubbleSystem, chatList);
                TextMeshProUGUI text = newBubble.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                text.text = say;
                currentDialogue.Add(new ChatData(ChatData.ChatActorType.System, say));
            }
            else
            {
                GameObject newBubble = Instantiate(prefabBubbleNPC, chatList);
                TextMeshProUGUI text = newBubble.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                text.text = say;
                currentDialogue.Add(new ChatData(ChatData.ChatActorType.NPC, say));
            }


            Canvas.ForceUpdateCanvases();
            chatList.GetComponent<VerticalLayoutGroup>().enabled = false;
            chatList.GetComponent<VerticalLayoutGroup>().enabled = true;
            Canvas.ForceUpdateCanvases();

        }

        void SaveChat()
        {
            if (currentlyTalkingWith == null) return;

            List<ChatData> saveDialogue = new List<ChatData>();
            foreach (ChatData bubble in currentDialogue)
            {
                // Don't save "Unread messages:"
                if (bubble.ActorType == ChatData.ChatActorType.System && bubble.Say == UNREAD_MESSAGES) continue;
                saveDialogue.Add(new ChatData(bubble.ActorType, bubble.Say));
            }

            currentlyTalkingWith.Contact.savedDialogue = saveDialogue;
        }

        #endregion

        #region [Methods: Contact]

        void AddContact(ChatContact _contactData)
        {
            var contactData = _contactData;

            GameObject newContact = Instantiate(prefabContact, contactList);
            Image bubbleImage = newContact.GetComponent<Image>();
            bubbleImage.sprite = contactData.Photo;
            Image pointer = bubbleImage.transform.Find("Pointer").GetComponent<Image>();
            Image alert = bubbleImage.transform.Find("Alert").GetComponent<Image>();


            ChatContactBubble contactBubble = new ChatContactBubble(contactData, bubbleImage, pointer, alert);
            contactBubbles.Add(contactBubble);
            SetContactAlert(contactData.ContactName, true);

            EventTrigger et = bubbleImage.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry et_entry_enter = new EventTrigger.Entry();
            et_entry_enter.eventID = EventTriggerType.PointerEnter;
            et_entry_enter.callback.AddListener((data) => { if (currentlyTalkingWith != contactBubble) bubbleImage.color = colorContactHighligthed; });
            et.triggers.Add(et_entry_enter);

            EventTrigger.Entry et_entry_exit = new EventTrigger.Entry();
            et_entry_exit.eventID = EventTriggerType.PointerExit;
            et_entry_exit.callback.AddListener((data) => { if (currentlyTalkingWith != contactBubble) bubbleImage.color = colorContactUnselected; });
            et.triggers.Add(et_entry_exit);

            EventTrigger.Entry et_entry_click = new EventTrigger.Entry();
            et_entry_click.eventID = EventTriggerType.PointerClick;
            et_entry_click.callback.AddListener((data) => { if (currentlyTalkingWith != contactBubble) SetTalkingWith(contactBubble); });
            et.triggers.Add(et_entry_click);

            bubbleImage.color = colorContactUnselected;
            Canvas.ForceUpdateCanvases();
        }

        void AddContact(string telNumber)
        {
            int rawTelNumber = int.Parse(telNumber.Replace("-", ""));

            var chatContacts = GameManager.Instance.GetGameAssets().resources.ChatContacts;
            ChatContact foundContact = null;
            foreach (var contact in chatContacts)
                if (contact.PhoneNumber == rawTelNumber)
                    foundContact = contact;

            // Contact exists
            if (foundContact != null)
            {
                // Contact is already in current contact
                if (contactBubbles.Find(contactBubble => contactBubble.Contact == foundContact) != null)
                {
                    addContactResultText.text = "<b>" + telNumber + "</b>\nAlready Added";
                    animator.SetBool(boo_contactAdded, true);
                    animator.SetTrigger(tri_showAddContactResult);
                    addContactResultScreen.gameObject.SetActive(true);
                }

                // Adding new contact to current contact
                else
                {
                    AddContact(foundContact);
                    addContactResultText.text = "<b>" + telNumber + "</b>\nAdded";
                    animator.SetBool(boo_contactAdded, true);
                    animator.SetTrigger(tri_showAddContactResult);
                    addContactResultScreen.gameObject.SetActive(true);
                }
            }

            // Contact doesn't exist
            else
            {
                addContactResultText.text = "<b>" + telNumber + "</b>\nNot Found";
                animator.SetBool(boo_contactAdded, false);
                animator.SetTrigger(tri_showAddContactResult);
                addContactResultScreen.gameObject.SetActive(true);
            }

            StartCoroutine(DelayForAnimation(5));
            IEnumerator DelayForAnimation(float delay)
            {
                yield return new WaitForSeconds(delay);
                addContactResultScreen.gameObject.SetActive(false);
            }

        }

        void SetTalkingWith(ChatContactBubble contactBubble)
        {
            // Save the previous dialogue befoer changing chat
            SaveChat();

            // UI
            if (currentlyTalkingWith != null)
            {
                currentlyTalkingWith.Bubble.color = colorContactUnselected;
                currentlyTalkingWith.Pointer.color = new Color(0, 0, 0, 0);
            }
            contactBubble.Bubble.color = colorContactSelected;
            contactBubble.Pointer.color = new Color(1, 1, 1, 1);
            contactBubble.Alert.color = new Color(1, 0.33f, 0.33f, 0);

            // Current variables
            currentlyTalkingWith = contactBubble;
            currentDialogue = contactBubble.Contact.savedDialogue;
            talkingWithName.text = contactBubble.Contact.ContactName;
            talkingWithStatus.text = contactBubble.Contact.Status;

            // Clear the chatList
            for (int i = chatList.childCount - 1; i >= 0; i--)
            {
                Destroy(chatList.GetChild(i).gameObject);
            }

            // Populate chatList with saved dialogue
            foreach (ChatData line in currentDialogue)
            {
                if (line.ActorType == ChatData.ChatActorType.Player)
                {
                    GameObject newBubble = Instantiate(prefabBubblePlayer, chatList);
                    TextMeshProUGUI text = newBubble.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                    text.text = line.Say;
                }
                else if (line.ActorType == ChatData.ChatActorType.NPC)
                {
                    GameObject newBubble = Instantiate(prefabBubbleNPC, chatList);
                    TextMeshProUGUI text = newBubble.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                    text.text = line.Say;
                }
                else if (line.ActorType == ChatData.ChatActorType.System)
                {
                    GameObject newBubble = Instantiate(prefabBubbleSystem, chatList);
                    TextMeshProUGUI text = newBubble.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                    text.text = line.Say;
                }
            }

            // Check if choices are currently shown
            if (choicesKept.ContainsKey(contactBubble.Contact.ContactName))
            {
                ShowMultipleChoice(choicesKept[contactBubble.Contact.ContactName].Choices, currentlyTalkingWith.Contact, choicesKept[contactBubble.Contact.ContactName].AvailableTimeLeft);
            }

            SetContactAlert(contactBubble.Contact.ContactName, false);

            chatList.GetComponent<VerticalLayoutGroup>().enabled = false;
            chatList.GetComponent<VerticalLayoutGroup>().enabled = true;
            Canvas.ForceUpdateCanvases();

            // TODO: set alpha to zero until everything is ready to be shown
        }

        void SetContactAlert(string name, bool isAlert)
        {
            ChatContactBubble _tempBubble = null;
            foreach (ChatContactBubble bubble in contactBubbles)
            {
                if (bubble.Contact.ContactName == name)
                {
                    if (isAlert)
                    {
                        bubble.Alert.color = new Color(1, 0.34f, 0.34f, 1);
                        _tempBubble = bubble;
                    }
                    else
                    {
                        bubble.Alert.color = new Color(1, 0.34f, 0.34f, 0);
                    }

                    break;

                }
            }

            if (_tempBubble != null)
            {
                contactBubbles.Remove(_tempBubble);
                contactBubbles.Insert(0, _tempBubble);
                _tempBubble.Bubble.transform.SetSiblingIndex(2);
            }
        }

        #endregion

        #region [Methods: SaveSystem]

        public void Save(GameManager.GameAssets gameAssets)
        {
            var chatContactData = new List<Serializables.ChatContactData>();
            foreach (var contactBubble in contactBubbles)
                chatContactData.Add(new Serializables.ChatContactData(contactBubble.Contact));

            var saveData = new Serializables.ChatAppData(chatContactData);
            gameAssets.systemData.chatAppData = saveData;
        }

        public void Load(GameManager.GameAssets gameAssets)
        {
            // TODO: calling AddContact too many times will slow down the loading 

            for (int i = contactBubbles.Count - 1; i >= 0; i--)
            {
                Destroy(contactBubbles[i].Bubble.gameObject);
                contactBubbles.RemoveAt(i);
            }

            var saveData = gameAssets.systemData.chatAppData;
            if (saveData == null || saveData.contactsData.Count == 0)
            {
                foreach (var contact in initialContacts)
                    AddContact(contact);
                return;
            }
            else
            {
                foreach (var contactData in saveData.contactsData)
                {
                    var foundContact = gameAssets.resources.ChatContacts.Find(contact => contact.ContactName == contactData.ContactName);
                    if (foundContact != null) AddContact(foundContact);
                }
            }

            currentlyTalkingWith = null;
            SetTalkingWith(contactBubbles[0]);
        }

        #endregion

    }
}