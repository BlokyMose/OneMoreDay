using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using TMPro;
using Encore.Serializables;
using Encore.Phone;
using Encore.Locations;
using Encore.Utility;

namespace Encore.Phone.ToDo
{

    /// <summary>
    /// [TODO]
    /// - Optimize UI if ToDoPanels become too many 
    /// </summary>
    [AddComponentMenu("Encore/Phone/ToDo App")]
    public class ToDoApp : PhoneApp
    {
        public static readonly string TODO_SAVE_KEY = "ToDo";

        #region [Classes]

        enum ToDoFilter { ToDo, Done, Aborted, All }

        public class ToDoPanel
        {
            Serializables.ToDoData _data;
            Image _panel;
            TextMeshProUGUI _text;
            Image _checkBox;

            public Serializables.ToDoData data { get { return _data; } set { _data = value; } }
            public Image panel { get { return _panel; } }
            public TextMeshProUGUI text { get { return _text; } }
            public Image checkBox { get { return _checkBox; } }


            public ToDoPanel(Serializables.ToDoData data, Image panel, TextMeshProUGUI text, Image checkBox)
            {
                _data = data;
                _panel = panel;
                _text = text;
                _checkBox = checkBox;
            }
        }

        class ToDoTagPanel
        {
            public ToDoTag tag;
            public GameObject panel;
            public List<ToDoPanel> toDoPanels;
        }


        #endregion

        #region [Vars: Components]

        [Header("Hierarchy Components")]
        [SerializeField] Image filterToDo;
        [SerializeField] Image filterDone;
        [SerializeField] Image filterAborted;
        [SerializeField] Image filterAll;
        [SerializeField] Transform toDoList;

        [Header("External Components")]
        [SerializeField] GameObject prefabToDoPanel;
        [SerializeField] GameObject prefabToDoTag;
        [SerializeField] ToDoTag untagged;

        [Header("Icons: ToDoPanel")]
        [SerializeField] Sprite uncheckedIcon;
        [SerializeField] Sprite checkedIcon;
        [SerializeField] Sprite abortedIcon;

        [Header("Icons: Notif")]
        [SerializeField] Sprite addedNotifIcon;
        [SerializeField] Sprite doneNotifIcon;
        [SerializeField] Sprite abortedNotifIcon;
        [SerializeField] Sprite editedNotifIcon;

        #endregion

        #region [Vars: Data Handlers]

        List<ToDoPanel> toDoPanels = new List<ToDoPanel>();
        List<ToDoTagPanel> toDoTagPanels = new List<ToDoTagPanel>();
        public List<ToDoTag> AllTags { get { return GameManager.Instance.GetGameAssets().resources.ToDoTags; } }

        ToDoFilter currentFilter = ToDoFilter.All; // Default must not be ToDo

        Color colorFilterUnselected = new Color(1, 1, 1, 0.5f);
        Color colorFilterHighlighted = new Color(1, 1, 1, 0.11f);
        Color colorFilterSelected = new Color(0.1f, 0.671f, 0.8f, 1f);
        Color colorFilterSelectedHighlighted = new Color(0.15f, 0.81f, 1f, 1f);

        #endregion

        protected override void Awake()
        {
            base.Awake();

            #region [EventTriggers: Filter]

            #region [ToDo]

            EventTrigger filterToDo_et = filterToDo.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry filterToDo_et_entry_enter = new EventTrigger.Entry();
            filterToDo_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            filterToDo_et_entry_enter.callback.AddListener((data) => { HighlightPanel(ToDoFilter.ToDo, true); });
            filterToDo_et.triggers.Add(filterToDo_et_entry_enter);

            EventTrigger.Entry filterToDo_et_entry_exit = new EventTrigger.Entry();
            filterToDo_et_entry_exit.eventID = EventTriggerType.PointerExit;
            filterToDo_et_entry_exit.callback.AddListener((data) => { HighlightPanel(ToDoFilter.ToDo, false); });
            filterToDo_et.triggers.Add(filterToDo_et_entry_exit);


            EventTrigger.Entry filterToDo_et_entry_click = new EventTrigger.Entry();
            filterToDo_et_entry_click.eventID = EventTriggerType.PointerClick;
            filterToDo_et_entry_click.callback.AddListener((data) => { SelectPanelAndFilter(ToDoFilter.ToDo); });
            filterToDo_et.triggers.Add(filterToDo_et_entry_click);

            #endregion

            #region [Done]

            EventTrigger filterDone_et = filterDone.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry filterDone_et_entry_enter = new EventTrigger.Entry();
            filterDone_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            filterDone_et_entry_enter.callback.AddListener((data) => { HighlightPanel(ToDoFilter.Done, true); });
            filterDone_et.triggers.Add(filterDone_et_entry_enter);

            EventTrigger.Entry filterDone_et_entry_exit = new EventTrigger.Entry();
            filterDone_et_entry_exit.eventID = EventTriggerType.PointerExit;
            filterDone_et_entry_exit.callback.AddListener((data) => { HighlightPanel(ToDoFilter.Done, false); });
            filterDone_et.triggers.Add(filterDone_et_entry_exit);


            EventTrigger.Entry filterDone_et_entry_click = new EventTrigger.Entry();
            filterDone_et_entry_click.eventID = EventTriggerType.PointerClick;
            filterDone_et_entry_click.callback.AddListener((data) => { SelectPanelAndFilter(ToDoFilter.Done); });
            filterDone_et.triggers.Add(filterDone_et_entry_click);

            #endregion

            #region [Aborted]

            EventTrigger filterAborted_et = filterAborted.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry filterAborted_et_entry_enter = new EventTrigger.Entry();
            filterAborted_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            filterAborted_et_entry_enter.callback.AddListener((data) => { HighlightPanel(ToDoFilter.Aborted, true); });
            filterAborted_et.triggers.Add(filterAborted_et_entry_enter);

            EventTrigger.Entry filterAborted_et_entry_exit = new EventTrigger.Entry();
            filterAborted_et_entry_exit.eventID = EventTriggerType.PointerExit;
            filterAborted_et_entry_exit.callback.AddListener((data) => { HighlightPanel(ToDoFilter.Aborted, false); });
            filterAborted_et.triggers.Add(filterAborted_et_entry_exit);


            EventTrigger.Entry filterAborted_et_entry_click = new EventTrigger.Entry();
            filterAborted_et_entry_click.eventID = EventTriggerType.PointerClick;
            filterAborted_et_entry_click.callback.AddListener((data) => { SelectPanelAndFilter(ToDoFilter.Aborted); });
            filterAborted_et.triggers.Add(filterAborted_et_entry_click);

            #endregion

            #region [All]

            EventTrigger filterAll_et = filterAll.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry filterAll_et_entry_enter = new EventTrigger.Entry();
            filterAll_et_entry_enter.eventID = EventTriggerType.PointerEnter;
            filterAll_et_entry_enter.callback.AddListener((data) => { HighlightPanel(ToDoFilter.All, true); });
            filterAll_et.triggers.Add(filterAll_et_entry_enter);

            EventTrigger.Entry filterAll_et_entry_exit = new EventTrigger.Entry();
            filterAll_et_entry_exit.eventID = EventTriggerType.PointerExit;
            filterAll_et_entry_exit.callback.AddListener((data) => { HighlightPanel(ToDoFilter.All, false); });
            filterAll_et.triggers.Add(filterAll_et_entry_exit);


            EventTrigger.Entry filterAll_et_entry_click = new EventTrigger.Entry();
            filterAll_et_entry_click.eventID = EventTriggerType.PointerClick;
            filterAll_et_entry_click.callback.AddListener((data) => { SelectPanelAndFilter(ToDoFilter.All); });
            filterAll_et.triggers.Add(filterAll_et_entry_click);

            #endregion

            #endregion
        }

        #region [Methods: Public Main]

        public override void Show(bool isShowing)
        {
            base.Show(isShowing);

            if (isShowing)
            {
                RefreshToDoList();
                SelectPanelAndFilter(ToDoFilter.ToDo);
            }
        }

        public void AddToDo(ToDoData data, bool ringNotification = true)
        {
            Serializables.ToDoData todoData = new Serializables.ToDoData(data,true);
            AddToDo(todoData, ringNotification);
        }

        public ToDoPanel AddToDo(Serializables.ToDoData todoData, bool ringNotification = true)
        {
            if (string.IsNullOrEmpty(todoData.Tag)) todoData.Tag = untagged.TagName;

            // Prevent adding the same ToDo
            if (toDoPanels.Find(panel => panel.data.ID == todoData.ID) != null) return null;

            // Create ToDoPanel and Record it
            GameObject newPanelGO = Instantiate(prefabToDoPanel, toDoList);
            Image panel = newPanelGO.GetComponent<Image>();
            TextMeshProUGUI text = newPanelGO.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            text.text = todoData.Text;
            Image checkBox = newPanelGO.transform.Find("CheckBox").GetComponent<Image>();
            var toDoPanel = new ToDoPanel(todoData, panel, text, checkBox);
            toDoPanels.Add(toDoPanel);

            if (todoData.IsKnownToPlayer)
            {
                newPanelGO.SetActive(MatchStatusAndFilter((ToDoData.ToDoStatus)todoData.Status, currentFilter));

                if (ringNotification) RingNotification(todoData, true);
                ChangeLocationTag(todoData);
            }
            else
            {
                newPanelGO.SetActive(false);
            }

            return toDoPanel;
        }

        public void EditToDo(Serializables.ToDoData newData, BoolStatus isKnownToPlayer)
        {
            if (string.IsNullOrEmpty(newData.Tag)) newData.Tag = untagged.TagName;

            var toDoPanel = toDoPanels.Find(panel => panel.data.ID == newData.ID);

            if (toDoPanel != null)
            {
                var foundToDoIsKnownToPlayer = toDoPanel.data.IsKnownToPlayer;
                toDoPanel.data = newData;
                toDoPanel.data.IsKnownToPlayer = isKnownToPlayer.GetBool(foundToDoIsKnownToPlayer);
                RefreshToDoList();
            }
            else
            {
                newData.IsKnownToPlayer = isKnownToPlayer.GetBool(false);
                toDoPanel = AddToDo(newData, false);
            }

            if (toDoPanel.data.IsKnownToPlayer)
            {
                RingNotification(newData, false);
                ChangeLocationTag(newData);
            }
        }

        void RingNotification(Serializables.ToDoData data, bool newToDo)
        {
            // Notifiying an edit in ToDo
            if (!newToDo)
            {
                PhoneManager.NotificationData notif = new PhoneManager.NotificationData(
                    appName: PhoneManager.PhoneAppName.ToDo,
                    time: GameManager.Instance.TimeManager.clock.GetHour().ToString() + ":" + GameManager.Instance.TimeManager.clock.GetMinute().ToString(),
                    title: data.Status == (int)ToDoData.ToDoStatus.Done
                        ? "Task Completed"
                        : data.Status == (int)ToDoData.ToDoStatus.Aborted
                        ? "Task Aborted"
                        : "Task Edited",
                    desc: data.Text,
                    image: data.Status == (int)ToDoData.ToDoStatus.Done
                        ? doneNotifIcon
                        : data.Status == (int)ToDoData.ToDoStatus.Aborted
                        ? abortedNotifIcon
                        : editedNotifIcon,
                    sound: notificationSound
                    );
                OnAddNotification(notif);
            }

            // Notifiying a new ToDo
            else
            {
                PhoneManager.NotificationData notif = new PhoneManager.NotificationData(
                    appName: PhoneManager.PhoneAppName.ToDo,
                    time: GameManager.Instance.TimeManager.clock.GetHour().ToString() + ":" + GameManager.Instance.TimeManager.clock.GetMinute().ToString(),
                    title: "Task Added",
                    desc: data.Text,
                    image: addedNotifIcon,
                    sound: notificationSound
                    );

                OnAddNotification(notif);
            }
        }

        void ChangeLocationTag(Serializables.ToDoData data)
        {
            if (!string.IsNullOrEmpty(data.Location))
            {
                var locData = GameManager.Instance.GetLocationData(data.Location);
                GameManager.Instance.ChangeLocationData(
                    data.Status == (int)ToDoData.ToDoStatus.Done ? locData.RemoveTag("ToDo") :
                    data.Status == (int)ToDoData.ToDoStatus.Aborted ? locData.RemoveTag("ToDo") :
                    locData.AddTag("ToDo")
                    );
            }
        }

        /// <summary> Update UI panel in the list to match its data </summary>
        public void RefreshToDoList()
        {
            foreach (var panel in toDoPanels)
            {
                panel.text.text = panel.data.Text;
            }

            #region [Find active tags]

            // Clear TagPanels
            foreach (var tag in toDoTagPanels)
            {
                Destroy(tag.panel);
            }
            toDoTagPanels.Clear();

            // Get tags which have active tasks
            List<string> activeTagsString = new List<string>();
            foreach (var panel in toDoPanels)
            {
                if (!activeTagsString.Contains(panel.data.Tag))
                {
                    activeTagsString.Add(panel.data.Tag);
                }
            }

            List<ToDoTag> activeTags = new List<ToDoTag>();
            foreach (var tagName in activeTagsString)
            {
                ToDoTag tag = AllTags.Find(x => x.TagName == tagName);
                if (tag != null) activeTags.Add(tag);
                else Debug.LogWarning("ToDoTag: " + tagName + " has no SO");
            }

            #endregion

            #region [Reposition panels]

            // Cache ToDoPanels for looping
            List<ToDoPanel> _toDoPanels = new List<ToDoPanel>();
            foreach (var toDo in toDoPanels) _toDoPanels.Add(toDo);

            // Reposition todo panels and make tag panels
            foreach (var tag in activeTags)
            {
                // Cache todoPanels which has this tag
                List<ToDoPanel> _toDoPanelsForStatusSorting = new List<ToDoPanel>();
                List<ToDoPanel> _toDoPanelWithThisTag = new List<ToDoPanel>();

                // Refreseh UI and reposition to top
                for (int i = _toDoPanels.Count - 1; i >= 0; i--)
                {
                    var todo = _toDoPanels[i];
                    if (todo.data.Tag == tag.TagName)
                    {
                        _toDoPanels.Remove(todo);
                        todo.panel.transform.SetAsFirstSibling();
                        _toDoPanelsForStatusSorting.Add(todo);
                        _toDoPanelWithThisTag.Add(todo);

                        // Set todo based on its own data
                        todo.text.text = todo.data.Text;
                        switch ((ToDoData.ToDoStatus)todo.data.Status)
                        {
                            case ToDoData.ToDoStatus.ToDo:
                                todo.checkBox.sprite = uncheckedIcon;
                                break;
                            case ToDoData.ToDoStatus.Done:
                                todo.checkBox.sprite = checkedIcon;
                                todo.panel.color = new Color(1, 1, 1, 0.75f);
                                break;
                            case ToDoData.ToDoStatus.Aborted:
                                todo.checkBox.sprite = abortedIcon;
                                todo.panel.color = new Color(1, 1, 1, 0.75f);
                                break;
                        }
                    }
                }

                // Reposition panels with this tag according to status
                SortByStatus((int)ToDoData.ToDoStatus.Aborted);
                SortByStatus((int)ToDoData.ToDoStatus.Done);
                SortByStatus((int)ToDoData.ToDoStatus.ToDo);
                void SortByStatus(int status)
                {
                    for (int i = _toDoPanelsForStatusSorting.Count - 1; i >= 0; i--)
                    {
                        var toDo = _toDoPanelsForStatusSorting[i];
                        if (toDo.data.Status == status)
                        {
                            toDo.panel.transform.SetAsFirstSibling();
                            _toDoPanelsForStatusSorting.Remove(toDo);
                        }
                    }
                }

                // Make tag panel
                var tagPanelGO = Instantiate(prefabToDoTag, toDoList);
                tagPanelGO.transform.Find("Image").GetComponent<Image>().sprite = tag.Image;
                tagPanelGO.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = tag.TagName;
                tagPanelGO.transform.SetAsFirstSibling();
                toDoTagPanels.Add(new ToDoTagPanel()
                {
                    tag = tag,
                    panel = tagPanelGO,
                    toDoPanels = _toDoPanelWithThisTag
                });
            }

            #endregion
        }

        public void Save(GameManager.GameAssets gameAssets)
        {
            var todoDataList = new List<Serializables.ToDoData>();
            foreach (var todo in toDoPanels) todoDataList.Add(todo.data);
            var saveData = new ToDoAppData(todoDataList);

            gameAssets.systemData.toDoAppData = saveData;
        }

        public void Load(GameManager.GameAssets gameAssets)
        {
            var saveData = gameAssets.systemData.toDoAppData;
            if (saveData == null) return;
            foreach (var todo in saveData.todoDataList)
                AddToDo(todo, false);
        }
        #endregion

        #region [Methods: Utility]

        void SelectPanelAndFilter(ToDoFilter filter)
        {
            Image panel = null;
            switch (filter)
            {
                case ToDoFilter.ToDo:
                    panel = filterToDo;
                    break;
                case ToDoFilter.Done:
                    panel = filterDone;
                    break;
                case ToDoFilter.Aborted:
                    panel = filterAborted;
                    break;
                case ToDoFilter.All:
                    panel = filterAll;
                    break;
                default:
                    break;
            }

            if (currentFilter != filter)
            {
                filterToDo.color = colorFilterUnselected;
                filterDone.color = colorFilterUnselected;
                filterAborted.color = colorFilterUnselected;
                filterAll.color = colorFilterUnselected;

                panel.color = colorFilterSelected;

                SetFilter(filter);
            }

            void SetFilter(ToDoFilter toDoFilter)
            {
                currentFilter = toDoFilter;

                // Show all panels
                foreach (ToDoPanel panel in toDoPanels)
                {
                    if (panel.data.IsKnownToPlayer)
                        panel.panel.gameObject.SetActive(true);
                }

                // Hide filtered panels & Show tagPanels
                foreach (var tagPanel in toDoTagPanels)
                {
                    int toDoWithSameFilter = 0;
                    foreach (var toDoPanel in tagPanel.toDoPanels)
                    {
                        if (!toDoPanel.data.IsKnownToPlayer || !MatchStatusAndFilter((ToDoData.ToDoStatus)toDoPanel.data.Status, currentFilter))
                        {
                            toDoPanel.panel.gameObject.SetActive(false);
                        }
                        else
                        {
                            toDoWithSameFilter++;
                        }
                    }

                    tagPanel.panel.SetActive(toDoWithSameFilter > 0 ? true : false);
                }
            }
        }

        void HighlightPanel(ToDoFilter filter, bool isHighlighting)
        {
            Image panel = null;
            switch (filter)
            {
                case ToDoFilter.ToDo:
                    panel = filterToDo;
                    break;
                case ToDoFilter.Done:
                    panel = filterDone;
                    break;
                case ToDoFilter.Aborted:
                    panel = filterAborted;
                    break;
                case ToDoFilter.All:
                    panel = filterAll;
                    break;
                default:
                    break;
            }

            if (currentFilter == filter)
            {
                panel.color = isHighlighting ? colorFilterSelectedHighlighted : colorFilterSelected;
            }
            else
            {
                panel.color = isHighlighting ? colorFilterHighlighted : colorFilterUnselected;
            }
        }

        bool MatchStatusAndFilter(ToDoData.ToDoStatus status, ToDoFilter filter)
        {
            if (filter == ToDoFilter.All) return true;
            else if (filter == ToDoFilter.ToDo && status == ToDoData.ToDoStatus.ToDo) return true;
            else if (filter == ToDoFilter.Done && status == ToDoData.ToDoStatus.Done) return true;
            else if (filter == ToDoFilter.Aborted && status == ToDoData.ToDoStatus.Aborted) return true;
            else return false;
        }

        public List<Serializables.ToDoData> GetToDos()
        {
            var todos = new List<Serializables.ToDoData>();
            foreach (var todoPanel in toDoPanels)
            {
                if((ToDoData.ToDoStatus)todoPanel.data.Status == ToDoData.ToDoStatus.ToDo)
                {
                    todos.Add(todoPanel.data);
                }
            }

            return todos;
        }

        #endregion
    }
}