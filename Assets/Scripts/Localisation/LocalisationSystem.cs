using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encore.Localisations
{
    public class LocalisationSystem
    {

        #region [Language]
        public enum Language
        {
            [InspectorName("English")]
            ENG,

            [InspectorName("Japanese")]
            JPN,

            [InspectorName("Bahasa Indonesia")]
            IND
        }

        public static string GetLanguageNativeName(Language language)
        {
            switch (language)
            {
                case Language.ENG:
                    return "English";
                case Language.JPN:
                    return "日本語";
                case Language.IND:
                    return "Bahasa Indonesia";
                default:
                    return "";
            }

        }


        #endregion

        static Language language = Language.ENG;

        private static Dictionary<string, string> localisedENG;
        private static Dictionary<string, string> localisedJPN;

        public static bool IsInit { get; set; }

        public static CSVLoader csvLoader;

        public static string GetLocalisedValue(string key)
        {
            if (!IsInit) { Init(); }

            string value = key;

            switch (language)
            {
                case Language.ENG:
                    localisedENG.TryGetValue(key, out value);
                    break;
                case Language.JPN:
                    localisedJPN.TryGetValue(key, out value);
                    break;
            }

            return value;
        }

        public static void Init()
        {
            csvLoader = new CSVLoader();
            csvLoader.LoadCSV();

            UpdateDictionaries();

            IsInit = true;
        }

        public static Dictionary<string, string> GetDictionaryForEditor()
        {
            if (!IsInit) { Init(); }
            return localisedENG;
        }

        public static void UpdateDictionaries()
        {
            localisedENG = csvLoader.GetDictionaryValues(Language.ENG.ToString());
            localisedJPN = csvLoader.GetDictionaryValues(Language.JPN.ToString());
        }

#if UNITY_EDITOR


        public static void Add(string key, string value)
        {
            if (value.Contains("\""))
            {
                value.Replace('"', '\"');
            }

            if (csvLoader == null)
            {
                csvLoader = new CSVLoader();
            }

            csvLoader.LoadCSV();
            csvLoader.Add(key, value);
            csvLoader.LoadCSV();

            UpdateDictionaries();
        }

        public static void Replace(string key, string value)
        {
            if (value.Contains("\""))
            {
                value.Replace('"', '\"');
            }

            if (csvLoader == null)
            {
                csvLoader = new CSVLoader();
            }

            csvLoader.LoadCSV();
            csvLoader.Edit(key, value);
            csvLoader.LoadCSV();

            UpdateDictionaries();
        }

        public static void Remove(string key)
        {
            if (csvLoader == null)
            {
                csvLoader = new CSVLoader();
            }

            csvLoader.LoadCSV();
            csvLoader.Remove(key);
            csvLoader.LoadCSV();

            UpdateDictionaries();
        }

#endif

    }
}