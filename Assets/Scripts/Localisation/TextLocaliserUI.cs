using Encore.Localisations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Encore.Deprecated
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("Encore/Deprecated/ (Deprecated) Text Localiser")]
    public class TextLocaliserUI : MonoBehaviour
    {
        TextMeshProUGUI textField;

        public LocalisedString localisedString;

        private void Start()
        {
            textField = GetComponent<TextMeshProUGUI>();
            textField.text = localisedString.value;
        }
    }
}