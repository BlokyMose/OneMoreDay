using Encore.SceneMasters;
using Encore.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Encore.Editor
{
    public class GlobalStateMonitor : EditorWindow
    {
        #region [Editor]

        [MenuItem("Tools/Global State Monitor")]
        public static void OpenWindow()
        {
            GetWindow<GlobalStateMonitor>("GS Monitor").Show();
        }

        #endregion

        #region [Classes]

        public class GlobalStateController
        {
            public GlobalState gs;
            public string currentStage;

            public GlobalStateController(GlobalState gs, string currentStage)
            {
                this.gs = gs;
                this.currentStage = currentStage;
            }

        }

        #endregion

        #region [Properties]

        Padding padding = new Padding(10, 5, 10, 10);
        const float rowHeight = 20f;
        enum MonitorMode { States, GSOContainer, GSSetter }
        MonitorMode monitorMode;

        #endregion


        #region [Data Handlers]

        List<GlobalStateController> gsControllers = new List<GlobalStateController>();
        List<GlobalStateObjectContainer> detectedGSOContainers = new List<GlobalStateObjectContainer>();
        List<GlobalStateSetter> detectedGSSetter = new List<GlobalStateSetter>();
        List<GlobalState> detectedGlobalState = new List<GlobalState>(); // detected in scene
        Dictionary<string,Serializables.GlobalStateData> runtimeGlobalStateData = new Dictionary<string,Serializables.GlobalStateData>();
        Vector2 scrollViewPos = new Vector2();
        Color defaultColor;
        string search;
        enum SearchMode { All, Detected }
        SearchMode searchMode = SearchMode.All;

        #endregion



        #region [Methods: Main]

        private void OnEnable()
        {
            gsControllers = new List<GlobalStateController>();
            detectedGSOContainers = new List<GlobalStateObjectContainer>();
            detectedGSSetter = new List<GlobalStateSetter>();
            detectedGlobalState = new List<GlobalState>();
            search = "";
            searchMode = SearchMode.All;

            // Get all global states in project
            var globalStatesInProject = new List<GlobalState>();
            var globalStatesGUID = AssetDatabase.FindAssets("t:" + nameof(GlobalState));
            foreach (var guid in globalStatesGUID)
            {
                globalStatesInProject.Add(AssetDatabase.LoadAssetAtPath<GlobalState>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            // Editor Mode
            
            if (!Application.isPlaying)
            {
                foreach (var gs in globalStatesInProject)
                {
                    var stage = gs.stages.GetAt(0, new GlobalState.Stage(""));
                    gsControllers.Add(new GlobalStateController(gs, stage.stageName));
                }
            }

            // Runtime Mode
            else
            {
                runtimeGlobalStateData = GameManager.Instance.GetAllGlobalStateData();

                foreach (var gsData in runtimeGlobalStateData)
                {
                    var gs = globalStatesInProject.Find(gsInProject => gsInProject.stateName == gsData.Key);
                    gsControllers.Add(new GlobalStateController(gs, gsData.Value.currentStage));
                }
            }

            detectedGSOContainers = new List<GlobalStateObjectContainer>(FindObjectsOfType<GlobalStateObjectContainer>(true));
            detectedGSSetter = new List<GlobalStateSetter>(FindObjectsOfType<GlobalStateSetter>(true));
            foreach (var container in detectedGSOContainers)
                foreach (var act in container.gso.activations)
                    detectedGlobalState.AddIfHasnt(act.gs);

            defaultColor = GUI.color;
        }

        private void OnFocus()
        {
            if (Application.isPlaying)
                runtimeGlobalStateData = GameManager.Instance.GetAllGlobalStateData();
        }

        private void OnGUI()
        {
            var currentY = padding.top;
            var currentX = padding.left;
            var availableWidth = position.width - padding.left - padding.right;

            MakeModesButtons();

            currentY += rowHeight + 5;
            currentX = padding.left;

            MakeSearchBar();

            currentY += rowHeight + 5;
            currentX = padding.left;

            #region [Begin ScrollView]

            var scrollViewRect = new Rect(0, currentY, position.width, position.height - currentY);
            var viewWidth = availableWidth;
            var viewHeight = GetViewHeight();
            var viewRect = new Rect(0, 0, viewWidth, viewHeight);
            scrollViewPos = GUI.BeginScrollView(scrollViewRect, scrollViewPos, viewRect);

            #endregion

            if (monitorMode == MonitorMode.States)
                MakeStatesList();

            else if (monitorMode == MonitorMode.GSOContainer)
                MakeGSOContainersList();

            else if (monitorMode == MonitorMode.GSSetter)
                MakeGSSettersList();

            GUI.EndScrollView();


            void MakeModesButtons()
            {
                int modesCount = 3;

                // Mode: States
                var monitorStatesRect = new Rect(currentX, currentY, availableWidth / modesCount, rowHeight);
                GUI.color = GetMonitorModeColor(MonitorMode.States);
                if (GUI.Button(monitorStatesRect, "States"))
                    monitorMode = MonitorMode.States;
                GUI.color = defaultColor;
                currentX += monitorStatesRect.width;

                // Mode: GSOContainer
                var monitorGSOContainerRect = new Rect(currentX, currentY, availableWidth / modesCount, rowHeight);
                GUI.color = GetMonitorModeColor(MonitorMode.GSOContainer);
                if (GUI.Button(monitorGSOContainerRect, "GSOContainer"))
                    monitorMode = MonitorMode.GSOContainer;
                GUI.color = defaultColor;
                currentX += monitorGSOContainerRect.width;

                // Mode: GSSetter
                var monitorGSSetterRect = new Rect(currentX, currentY, availableWidth / modesCount, rowHeight);
                GUI.color = GetMonitorModeColor(MonitorMode.GSSetter);
                if (GUI.Button(monitorGSSetterRect, "GSSetter"))
                    monitorMode = MonitorMode.GSSetter;
                GUI.color = defaultColor;
                currentX += monitorGSSetterRect.width;
            }

            void MakeSearchBar()
            {
                var searchBarWidth = 0f;
                var searchModeWith = 0f;
                switch (monitorMode)
                {
                    case MonitorMode.States:
                        searchBarWidth = availableWidth * 0.6f;
                        searchModeWith = availableWidth * 0.2f;
                        break;
                    case MonitorMode.GSOContainer:
                        searchBarWidth = availableWidth * 0.8f;
                        break;
                    case MonitorMode.GSSetter:
                        searchBarWidth = availableWidth * 0.8f;
                        break;
                }

                // Search text field
                var searchRect = new Rect(currentX, currentY, searchBarWidth, rowHeight);
                search = EditorGUI.TextField(searchRect, search);

                // PlaceHolder
                if (string.IsNullOrEmpty(search))
                {
                    var searchPlaceholderStyle = new GUIStyle(GUI.skin.label);
                    searchPlaceholderStyle.fontStyle = FontStyle.Italic;
                    GUI.color = Color.gray;
                    EditorGUI.LabelField(searchRect, "search", searchPlaceholderStyle);
                    GUI.color = defaultColor;
                }
                currentX += searchRect.width;

                // SearchMode button
                var searchModeRect = new Rect(currentX, currentY, searchModeWith, rowHeight);
                GUI.color = GetSearchModeColor();
                if (GUI.Button(searchModeRect, searchMode.ToString()))
                    searchMode = (SearchMode)((((int)searchMode) + 1) % Enum.GetNames(typeof(SearchMode)).Length);
                GUI.color = defaultColor;

                currentX += searchModeRect.width;

                // Refresh
                var refreshRect = new Rect(currentX, currentY, availableWidth * 0.2f, rowHeight);
                if (GUI.Button(refreshRect, "Refresh"))
                    Refresh();
            }

            void MakeStatesList()
            {
                currentY = 0;

                foreach (var controller in gsControllers)
                {
                    if (!controller.gs.stateName.Contains(search, StringComparison.CurrentCultureIgnoreCase)) 
                        continue;

                    if (searchMode == SearchMode.Detected && !detectedGlobalState.Contains(controller.gs))
                        continue;

                    // GlobalState button
                    var gsButtonRect = new Rect(currentX, currentY, availableWidth * 0.25f, rowHeight);
                    var gsButtonStyle = new GUIStyle(GUI.skin.label);
                    if (GUI.Button(gsButtonRect, controller.gs.stateName, gsButtonStyle))
                        EditorGUIUtility.PingObject(controller.gs);
                    currentX += gsButtonRect.width + 5;

                    // Runtime mode: CurrentStage
                    if (Application.isPlaying)
                    {
                        var currentStageRect = new Rect(currentX, currentY, availableWidth * 0.25f, rowHeight);
                        var currentStage = GetRuntimeCurrentStage(controller.gs);
                        EditorGUI.LabelField(currentStageRect,  currentStage +" \u2192");
                        currentX += currentStageRect.width + 5;
                    }

                    // Stages
                    var stagesRect = new Rect(currentX, currentY, availableWidth * 0.25f, rowHeight);
                    var stages = controller.gs.stages;
                    if (stages.Count > 0)
                    {
                        var stagesNames = new List<string>();
                        var currentStageIndex = 0;
                        var index = 0;
                        foreach (var stage in stages)
                        {
                            stagesNames.Add(stage.stageName);
                            if (stage.stageName == controller.currentStage)
                                currentStageIndex = index;
                            index++;
                        }

                        currentStageIndex = EditorGUI.Popup(stagesRect, currentStageIndex, stagesNames.ToArray());

                        controller.currentStage = stagesNames[currentStageIndex];
                    }
                    else
                    {
                        EditorGUI.LabelField(stagesRect, "No stage detected");
                    }
                    currentX += gsButtonRect.width + 5;

                    // Apply Button
                    if (detectedGlobalState.Contains(controller.gs))
                    {
                        var applyRect = new Rect(currentX, currentY, availableWidth * 0.15f, rowHeight);

                        if (GUI.Button(applyRect, "Apply"))
                        {
                            if (!Application.isPlaying)
                            {
                                ApplyStage(controller.gs.stateName, controller.currentStage);
                            }
                            else
                            {
                                GameManager.Instance.ModifyGlobalStateData(new GlobalStateSetter.GlobalStateModified(controller.gs, controller.currentStage));
                                SceneMaster.current.ApplyState(controller.gs, controller.currentStage);
                            }

                        }
                    }

                    currentY += rowHeight + 5;
                    currentX = padding.left;
                }
            }

            void MakeGSOContainersList()
            {
                currentY = 0;

                foreach (var container in detectedGSOContainers)
                {
                    if (!container.gameObject.name.Contains(search)) continue;

                    var containerRect = new Rect(currentX, currentY, availableWidth * 0.5f, rowHeight);
                    EditorGUI.ObjectField(containerRect, container.gameObject, typeof(GameObject), true);
                    currentX += containerRect.width;

                    var statesLabelRect = new Rect(currentX, currentY, 30f, rowHeight);
                    GUI.color = new Color(1, 1, 1, 0.5f);
                    EditorGUI.LabelField(statesLabelRect, " GS: ");
                    GUI.color = defaultColor;
                    currentX += statesLabelRect.width;

                    foreach (var act in container.gso.activations)
                    {
                        var stateRect = new Rect(currentX, currentY, 100, rowHeight);
                        var stateStyle = new GUIStyle(GUI.skin.label);
                        if (GUI.Button(stateRect, act.gs.stateName+", ", stateStyle))
                            EditorGUIUtility.PingObject(act.gs);
                        currentX += stateRect.width;
                    }

                    currentX = padding.left;
                    currentY += rowHeight + 5;
                }
            }

            void MakeGSSettersList()
            {
                currentY = 0;

                foreach (var setter in detectedGSSetter)
                {
                    if (!setter.gameObject.name.Contains(search)) continue;

                    var setterRect = new Rect(currentX, currentY, availableWidth * 0.5f, rowHeight);
                    EditorGUI.ObjectField(setterRect, setter.gameObject, typeof(GameObject), true);
                    currentX += setterRect.width;

                    var statesLabelRect = new Rect(currentX, currentY, 30f, rowHeight);
                    GUI.color = new Color(1, 1, 1, 0.5f);
                    EditorGUI.LabelField(statesLabelRect, " GS: ");
                    GUI.color = defaultColor;
                    currentX += statesLabelRect.width;

                    foreach (var state in setter.modifiedStates)
                    {
                        var stateRect = new Rect(currentX, currentY, 150, rowHeight);
                        var stateStyle = new GUIStyle(GUI.skin.label);
                        if (GUI.Button(stateRect, state.gs.stateName +" \u2192 "+ state.targetStage + ", ", stateStyle))
                            EditorGUIUtility.PingObject(state.gs);
                        currentX += stateRect.width;
                    }

                    currentX = padding.left;
                    currentY += rowHeight + 5;
                }
            }
        }

        Color GetMonitorModeColor(MonitorMode checkMonitorMode)
        {
            if (monitorMode == checkMonitorMode)
                return Color.green;
            else
                return Color.gray;
        }

        float GetViewHeight()
        {
            var height = 0f;
            switch (monitorMode)
            {
                case MonitorMode.States:
                    foreach (var controller in gsControllers)
                        height += rowHeight + 5;
                    break;
                case MonitorMode.GSOContainer:
                    foreach (var container in detectedGSOContainers)
                        height += rowHeight + 5;
                    break;

            }

            return height;
        }

        Color GetSearchModeColor()
        {
            switch (searchMode)
            {
                case SearchMode.All: return Encore.Utility.ColorUtility.paleGreen;
                case SearchMode.Detected: return Encore.Utility.ColorUtility.goldenRod;
            }
            return defaultColor;
        }

        #endregion

        public static void ApplyStage(string stateName, string stageName)
        {
            var allGSOContainers = FindObjectsOfType<GlobalStateObjectContainer>(true);

            foreach (var gso in allGSOContainers)
                gso.Evaluate(stateName, stageName);
        }

        public void Refresh()
        {
            OnEnable();
        }



        string GetRuntimeCurrentStage(GlobalState gs)
        {
            return runtimeGlobalStateData.Get(gs.stateName, new Serializables.GlobalStateData("Unknown state")).currentStage;
        }
    }

}
