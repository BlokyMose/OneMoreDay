using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DialogueSyntax;
using Encore.Utility;

namespace Encore.MiniGames.Viewers
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Encore/MiniGames/Viewers/Text Viewer")]
    public class TextViewer : Viewer
    {
        #region [Classes]

        [Serializable]
        public class SectionForInspector
        {
            public enum SectionType { Title, Header, Body, Image, Space }

            [OnValueChanged(nameof(OnTypeChanged)), EnumToggleButtons, HideLabel, GUIColor(nameof(typeColor))]
            public SectionType type = SectionType.Body;

            [ShowIf(nameof(showText)), TextArea(1, 5), HideLabel]
            public string stringValue;

            [ShowIf(nameof(showSprite)), HideLabel, PreviewField]
            public Sprite sprite;

            [ShowIf((nameof(showInt))), LabelText("@" + nameof(intValueName))]
            public int intValue;

            #region [Methods: Inspector]

            Color typeColor;
            bool showText, showSprite, showInt;
            string intValueName;

            public void OnTypeChanged()
            {
                showText = false;
                showSprite = false;
                showInt = false;

                switch (type)
                {
                    case SectionType.Title:
                        typeColor = new Color(1, 0.3f, 0.6f);
                        showText = true;
                        break;
                    case SectionType.Header:
                        typeColor = new Color(1, 0.7f, 0.85f);
                        showText = true;
                        break;
                    case SectionType.Body:
                        typeColor = new Color(0.75f, 0.75f, 1f);
                        showText = true;
                        break;
                    case SectionType.Image:
                        typeColor = new Color(1f, 1f, 0.5f);
                        showSprite = true;
                        showInt = true;
                        intValueName = "Height";
                        if (intValue <= 0)
                            intValue = 320;
                        break;
                    case SectionType.Space:
                        typeColor = new Color(0.6f, 0.6f, 0.1f);
                        showInt = true;
                        intValueName = "Height";
                        if (intValue <= 0)
                            intValue = 320;
                        break;
                }
            }

            #endregion
        }

        [Serializable]
        public abstract class Section
        {
            public GameObject go;

            protected Section(GameObject go)
            {
                this.go = go;
            }
        }

        public class SectionTitle : Section
        {
            public string title;
            public TextMeshProUGUI text;

            public SectionTitle(GameObject go, string title, TextMeshProUGUI text) : base(go)
            {
                this.title = title;
                this.text = text;
            }
        }

        public class SectionHeader : Section
        {
            public string header;
            public TextMeshProUGUI text;

            public SectionHeader(GameObject go, string header, TextMeshProUGUI text) : base(go)
            {
                this.header = header;
                this.text = text;
            }
        }

        public class SectionBody : Section
        {
            public string body;
            public TextMeshProUGUI text;

            public SectionBody(GameObject go, string body, TextMeshProUGUI text) : base(go)
            {
                this.body = body;
                this.text = text;
            }
        }

        public class SectionImage : Section
        {
            public Image image;
            public int height;

            public SectionImage(GameObject go, Image image, int height) : base(go)
            {
                this.image = image;
                this.height = height;
            }
        }

        public class SectionSpace : Section
        {
            public int height;

            public SectionSpace(GameObject go, int height) : base(go)
            {
                this.height = height;
            }
        }

        #endregion

        #region [Vars: Components]

        #region [Components]

        [FoldoutGroup("Components"), SerializeField]
        Image bg;

        [FoldoutGroup("Components"), SerializeField]
        Transform sectionsParent;

        [FoldoutGroup("Components"), SerializeField]
        Scrollbar scrollbar;

        [FoldoutGroup("Components"), SerializeField]
        Transform pageSkipsParent;

        #endregion

        #region [Settings Components]

        [FoldoutGroup("Settings Components"), SerializeField]
        Animator settingsAnimator;

        [FoldoutGroup("Settings Components"), SerializeField]
        CanvasGroup hitShowSettingsPanel;

        [FoldoutGroup("Settings Components"), SerializeField]
        CanvasGroup hitMaskSettingsPanel;

        [FoldoutGroup("Settings Components"), SerializeField]
        CanvasGroup hitHideSettingsPanel;

        [FoldoutGroup("Settings Components"), SerializeField]
        CanvasGroup settingsPanel;

        [FoldoutGroup("Settings Components"), SerializeField]
        TMP_Dropdown fontDropDown;

        [FoldoutGroup("Settings Components"), SerializeField]
        Slider fontSizeSlider;

        [FoldoutGroup("Settings Components"), SerializeField]
        Slider bgOpacitySlider;

        #endregion

        #endregion

        #region [Vars: Properties]

        [FoldoutGroup("Properties"), SerializeField]
        TextViewerFontsPack defaultFonts;

        [FoldoutGroup("Properties"), SerializeField]
        List<TextViewerFontsPack> otherFonts;

        [FoldoutGroup("Properties"), SerializeField]
        GameObject pageSkipButPrefab;

        [Header("Sections Prefab")]
        [FoldoutGroup("Properties"), SerializeField]
        GameObject titlePrefab;

        [FoldoutGroup("Properties"), SerializeField]
        GameObject headerPrefab;

        [FoldoutGroup("Properties"), SerializeField]
        GameObject bodyPrefab;

        [FoldoutGroup("Properties"), SerializeField]
        GameObject imagePrefab;

        readonly int defaultFontSize = 32;

        #endregion

        #region [Vars: Sections]


        [FoldoutGroup("Sections"), SerializeField, LabelText("Sections")]
        List<SectionForInspector> sectionsForInspector;

#if UNITY_EDITOR

        [FoldoutGroup("Sections"), SerializeField, TextArea(5, 10), LabelText("Syntax")]
        string sectionSyntax;

        [FoldoutGroup("Sections"), SerializeField, InlineButton(nameof(FindAndAssignDefaultDSyantaxSettings), "Default", ShowIf = "@!" + nameof(dSyntaxSettings))]
        DSyntaxSettings dSyntaxSettings;

        void FindAndAssignDefaultDSyantaxSettings()
        {
            var dataArray = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(DSyntaxSettings));
            if (dataArray != null)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(dataArray[0]);
                dSyntaxSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<DSyntaxSettings>(path);
            }
        }

        [HorizontalGroup("Sections/Buttons"), Button("DSyntax to Sections", ButtonSizes.Large)]
        void OnConvertFromDSyntax() { ConvertFromDSyntax(); }


        [HorizontalGroup("Sections/Buttons"), Button("Sections to DSyntax", ButtonSizes.Large)]
        void OnConvertToDSyntax() { ConvertToDSyntax(); }

        void OnValidate()
        {
            foreach (var section in sectionsForInspector)
            {
                section.OnTypeChanged();
            }
        }

        void ConvertFromDSyntax()
        {
            sectionsForInspector = new List<SectionForInspector>();

            var commands = DSyntaxUtility.ReadCommands(dSyntaxSettings, sectionSyntax);

            foreach (var command in commands)
            {
                if (command.name.Equals(COMMAND_TITLE, StringComparison.CurrentCultureIgnoreCase))
                {
                    sectionsForInspector.Add(new SectionForInspector()
                    {
                        type = SectionForInspector.SectionType.Title,
                        stringValue = command.parameters.GetAt(0)
                    });
                }
                else if (command.name.Equals(COMMAND_HEADER, StringComparison.CurrentCultureIgnoreCase))
                {
                    sectionsForInspector.Add(new SectionForInspector()
                    {
                        type = SectionForInspector.SectionType.Header,
                        stringValue = command.parameters.GetAt(0)
                    });
                }
                else if (command.name.Equals(COMMAND_BODY, StringComparison.CurrentCultureIgnoreCase))
                {
                    sectionsForInspector.Add(new SectionForInspector()
                    {
                        type = SectionForInspector.SectionType.Body,
                        stringValue = command.parameters.GetAt(0)
                    });
                }
                else if (command.name.Equals(COMMAND_IMAGE, StringComparison.CurrentCultureIgnoreCase))
                {
                    Sprite spriteImage = null;

                    var spriteImageGUID = UnityEditor.AssetDatabase.FindAssets("t:sprite " + command.parameters.GetAt(1))[0];
                    if (!string.IsNullOrEmpty(spriteImageGUID))
                        spriteImage = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(spriteImageGUID));
                    else
                        Debug.Log("Cannot find sprite: " + command.parameters.GetAt(1));

                    sectionsForInspector.Add(new SectionForInspector()
                    {
                        type = SectionForInspector.SectionType.Image,
                        intValue = int.Parse(command.parameters.GetAt(0)),
                        sprite = spriteImage
                    });
                }
                else if (command.name.Equals(COMMAND_SPACE, StringComparison.CurrentCultureIgnoreCase))
                {
                    sectionsForInspector.Add(new SectionForInspector()
                    {
                        type = SectionForInspector.SectionType.Space,
                        intValue = int.Parse(command.parameters.GetAt(0))
                    });
                }
                else
                {
                    Debug.Log("Unknown command name: " + command.name);
                }

                if (sectionsForInspector.Count > 0) sectionsForInspector[sectionsForInspector.Count - 1].OnTypeChanged();
            }
        }

        void ConvertToDSyntax()
        {
            sectionSyntax = "";
            foreach (var section in sectionsForInspector)
            {
                switch (section.type)
                {
                    case SectionForInspector.SectionType.Title:
                        sectionSyntax += DSyntaxUtility.WriteCommand(dSyntaxSettings, COMMAND_TITLE, section.stringValue);
                        break;
                    case SectionForInspector.SectionType.Header:
                        sectionSyntax += DSyntaxUtility.WriteCommand(dSyntaxSettings, COMMAND_HEADER, section.stringValue);
                        break;
                    case SectionForInspector.SectionType.Body:
                        sectionSyntax += DSyntaxUtility.WriteCommand(dSyntaxSettings, COMMAND_BODY, section.stringValue);
                        break;
                    case SectionForInspector.SectionType.Image:
                        sectionSyntax += DSyntaxUtility.WriteCommand(dSyntaxSettings, COMMAND_OPENING_TOKEN, new List<string>() { section.intValue.ToString(), section.sprite.name });
                        break;
                    case SectionForInspector.SectionType.Space:
                        sectionSyntax += DSyntaxUtility.WriteCommand(dSyntaxSettings, COMMAND_SPACE, section.intValue.ToString());
                        break;
                    default:
                        break;
                }
            }
        }


#endif

        #region [Keys]

        const string COMMAND_OPENING_TOKEN = "[";
        const string COMMAND_CLOSING_TOKEN = "]";
        const string PARAMETER_OPENING_TOKEN = "{";
        const string PARAMETER_CLOSING_TOKEN = "}";

        const string COMMAND_TITLE = "TITLE";
        const string COMMAND_HEADER = "HEADER";
        const string COMMAND_BODY = "BODY";
        const string COMMAND_IMAGE = "IMAGE";
        const string COMMAND_SPACE = "SPACE";

        #endregion

        #endregion

        #region [Vars: Data Handlers]

        List<Section> sections = new List<Section>();
        List<Button> pageSkips = new List<Button>();

        #endregion

        #region [Methods: Unity]

        protected override void Awake()
        {
            base.Awake();

            #region [Setup: Settings]

            #region [EventTriggers: Hit Show/Hide]

            EventTrigger hitShowSettingsPanel_et = hitShowSettingsPanel.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry hitShowSettingsPanel_entry_enter = new EventTrigger.Entry();
            hitShowSettingsPanel_entry_enter.eventID = EventTriggerType.PointerEnter;
            hitShowSettingsPanel_entry_enter.callback.AddListener((data) =>
            {
                ShowSettings(true);
            });
            hitShowSettingsPanel_et.triggers.Add(hitShowSettingsPanel_entry_enter);

            EventTrigger hitHideSettingsPanel_et = hitHideSettingsPanel.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry hitHideSettingsPanel_entry_enter = new EventTrigger.Entry();
            hitHideSettingsPanel_entry_enter.eventID = EventTriggerType.PointerEnter;
            hitHideSettingsPanel_entry_enter.callback.AddListener((data) =>
            {
                ShowSettings(false);
            });
            hitHideSettingsPanel_et.triggers.Add(hitHideSettingsPanel_entry_enter);

            #endregion

            #region [Settings Menus]

            bgOpacitySlider.value = bg.color.a;
            bgOpacitySlider.onValueChanged.AddListener(OnValueChangedBGOpacitySlider);

            fontSizeSlider.onValueChanged.AddListener(OnValueChangedFontSizeSlider);

            fontDropDown.onValueChanged.AddListener(OnValueChangedFontDropDown);
            fontDropDown.options = new List<TMP_Dropdown.OptionData>();
            fontDropDown.options.Add(new TMP_Dropdown.OptionData("Default"));
            foreach (var fontsPack in otherFonts)
                fontDropDown.options.Add(new TMP_Dropdown.OptionData(fontsPack.fontsPackName));

            fontDropDown.value = 0;


            void OnValueChangedBGOpacitySlider(float value)
            {
                bg.color = new Color(0, 0, 0, value);
            }

            void OnValueChangedFontSizeSlider(float value)
            {
                foreach (var section in sections)
                {
                    if (section is SectionTitle)
                    {
                        var _section = (SectionTitle)section;
                        _section.text.fontSize = defaultFontSize * 2 + value;
                    }
                    else if (section is SectionHeader)
                    {
                        var _section = (SectionHeader)section;
                        _section.text.fontSize = defaultFontSize * 1.5f + value;
                    }
                    else if (section is SectionBody)
                    {
                        var _section = (SectionBody)section;
                        _section.text.fontSize = defaultFontSize + value;
                    }
                }
            }

            void OnValueChangedFontDropDown(int value)
            {
                TextViewerFontsPack selectedFontsPack = null;
                if (value == 0)
                    selectedFontsPack = defaultFonts;
                else
                    selectedFontsPack = otherFonts[value - 1];

                foreach (var section in sections)
                {
                    if (section is SectionTitle)
                    {
                        var _section = (SectionTitle)section;
                        _section.text.font = selectedFontsPack.titleFont;
                    }
                    else if (section is SectionHeader)
                    {
                        var _section = (SectionHeader)section;
                        _section.text.font = selectedFontsPack.headerFont;
                    }
                    else if (section is SectionBody)
                    {
                        var _section = (SectionBody)section;
                        _section.text.font = selectedFontsPack.bodyFont;
                    }
                }
            }

            #endregion

            #endregion

            // Just in case, clean the child
            for (int i = sectionsParent.childCount - 1; i >= 0; i--)
                Destroy(sectionsParent.GetChild(i).gameObject);

            // Just in case, clean the child
            for (int i = pageSkipsParent.childCount - 1; i >= 0; i--)
                Destroy(pageSkipsParent.GetChild(i).gameObject);

            InstantiateSections();

        }

        protected override void Start()
        {
            base.Start();
            ShowSettings(false);
        }

        #endregion

        #region [Methods: Main]

        void InstantiateSections()
        {
            foreach (var sectionFI in sectionsForInspector)
            {
                GameObject go = null;

                switch (sectionFI.type)
                {
                    case SectionForInspector.SectionType.Title:
                        go = Instantiate(titlePrefab, sectionsParent);
                        var textTitle = go.GetComponentInChildren<TextMeshProUGUI>();
                        textTitle.text = sectionFI.stringValue;
                        textTitle.font = defaultFonts.titleFont;
                        sections.Add(new SectionTitle(go, sectionFI.stringValue, textTitle));
                        break;

                    case SectionForInspector.SectionType.Header:
                        go = Instantiate(headerPrefab, sectionsParent);
                        var textHeader = go.GetComponentInChildren<TextMeshProUGUI>();
                        textHeader.text = sectionFI.stringValue;
                        textHeader.font = defaultFonts.headerFont;
                        sections.Add(new SectionHeader(go, sectionFI.stringValue, textHeader));

                        var index = sections.Count - 1;
                        var pageSkipGO = Instantiate(pageSkipButPrefab, pageSkipsParent);
                        var pageSkip = pageSkipGO.GetComponent<Button>();
                        pageSkip.onClick.AddListener(() =>
                        {
                            sectionsParent.transform.localPosition = new Vector2(sectionsParent.transform.localPosition.x, sections[index].go.transform.localPosition.y * -1);
                        });
                        pageSkips.Add(pageSkip);
                        break;

                    case SectionForInspector.SectionType.Body:
                        go = Instantiate(bodyPrefab, sectionsParent);
                        var textBody = go.GetComponentInChildren<TextMeshProUGUI>();
                        textBody.text = sectionFI.stringValue;
                        textBody.font = defaultFonts.bodyFont;
                        sections.Add(new SectionBody(go, sectionFI.stringValue, textBody));
                        break;

                    case SectionForInspector.SectionType.Image:
                        go = Instantiate(imagePrefab, sectionsParent);
                        var image = go.GetComponentInChildren<Image>();
                        image.sprite = sectionFI.sprite;
                        var rectTranformImage = image.GetComponent<RectTransform>();
                        rectTranformImage.sizeDelta = new Vector2(rectTranformImage.sizeDelta.x, sectionFI.intValue);
                        sections.Add(new SectionImage(go, image, sectionFI.intValue));
                        break;

                    case SectionForInspector.SectionType.Space:
                        go = new GameObject("Space");
                        go.transform.parent = sectionsParent;
                        var rectTranformSpace = go.AddComponent<RectTransform>();
                        rectTranformSpace.sizeDelta = new Vector2(rectTranformSpace.sizeDelta.x, sectionFI.intValue);
                        sections.Add(new SectionSpace(go, sectionFI.intValue));
                        break;
                }
            }

            scrollbar.value = 1;

            sectionsForInspector = null;
        }

        #endregion

        #region [Methods: Utilities]

        void ShowSettings(bool isShowing)
        {
            settingsPanel.interactable = isShowing;
            settingsPanel.blocksRaycasts = isShowing;
            hitHideSettingsPanel.interactable = isShowing;
            hitHideSettingsPanel.blocksRaycasts = isShowing;
            hitMaskSettingsPanel.interactable = isShowing;
            hitMaskSettingsPanel.blocksRaycasts = isShowing;

            settingsAnimator.SetBool(boo_show, isShowing);
        }



        #endregion

    }
}
