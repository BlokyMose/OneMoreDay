using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Encore.Localisations;
using UnityEngine.UI;

namespace Encore.Menus
{
    [AddComponentMenu("Encore/Menu/Game Settings Menu")]
    public class GameSettingsMenu : MenuUI
    {
        #region [Components]

        [Title("Main")]
        [SerializeField]
        TextMeshProUGUI languageText;

        [SerializeField]
        TMP_Dropdown languageDropdown;

        #endregion

        protected override void Setup()
        {
            base.Setup();

            #region [Main Panel]

            #region [Language]

            languageDropdown.onValueChanged.AddListener(OnValueChangedLanguage);
            languageDropdown.options = new List<TMP_Dropdown.OptionData>();
            var languages = (LocalisationSystem.Language[])Enum.GetValues(typeof(LocalisationSystem.Language));
            int languagesIndex = 0;
            foreach (var language in languages)
            {
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(LocalisationSystem.GetLanguageNativeName(language)));
                if (language == GameManager.Instance.Language)
                    languageDropdown.value = languagesIndex;
                languagesIndex++;
            }


            void OnValueChangedLanguage(int value)
            {
                GameManager.Instance.ChangeLanguage((LocalisationSystem.Language)value);
            }

            #endregion

            #endregion
        }

    }
}
