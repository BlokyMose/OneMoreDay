using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using static DialogueConverterWindow;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using DialogueSyntax;

public class DialogueAnalysisWindow : OdinMenuEditorWindow
{
    #region [Editor]

    public static void OpenWindow(List<DSyntaxData.Branch> _branches)
    {
        GetWindow<DialogueAnalysisWindow>("Dialogue Analysis").Show();
        branches = _branches;
    }

    #endregion

    #region [Data Classes]

    public class Route
    {
        public string name;
        public List<DSyntaxData.Branch> routeBranches;
        public List<string> chosenChoices;

        // Keys
        const string START = "START";
        const string ARROW_ICON = " \u2192 ";

        public class DialogueSnippets
        {
            public DSyntaxData.Branch fromBranch { get ;private set;}
            public DSyntaxData.Branch toBranch { get; private set; }

            public DialogueSnippets(DSyntaxData.Branch fr, DSyntaxData.Branch to)
            {
                fromBranch = fr;
                toBranch = to;
            }

            public (DSyntaxData.NodeSay, DSyntaxData.NodeSay) GetSnippet()
            {
                (DSyntaxData.NodeSay, DSyntaxData.NodeSay) snippet = (fromBranch.nodes.FindLast(n => n is DSyntaxData.NodeSay) as DSyntaxData.NodeSay, toBranch.nodes.Find(n => n is DSyntaxData.NodeSay) as DSyntaxData.NodeSay);
                return snippet;
            }
        }

        public Route()
        {
            routeBranches = new List<DSyntaxData.Branch>();
            chosenChoices = new List<string>();
        }

        public Route DeepCopy()
        {
            Route r = (Route)this.MemberwiseClone();
            DSyntaxData.Branch[] b_arr = new DSyntaxData.Branch[this.routeBranches.Count];
            this.routeBranches.CopyTo(b_arr);
            r.routeBranches = b_arr.ToList();

            return r;
        }

        public List<string> DeepCopyChosenChoices(List<string> oldChosenChoices)
        {
            List<string> newList = new List<string>();
            foreach (string chosenChoice in oldChosenChoices)
            {
                newList.Add(chosenChoice);
            }

            this.chosenChoices = newList;

            return newList;
        }

        public string GetRoute()
        {
            string s = "";

            foreach (DSyntaxData.Branch b in routeBranches)
            {
                s += b.name + ARROW_ICON;
            }
            s = s.Substring(0, s.Length - ARROW_ICON.Length);
            return s;
        }

        public string GetRouteWithoutStart()
        {
            string s = GetRoute();
            if (s == START) return s;
            s = s.Substring(START.Length + ARROW_ICON.Length, s.Length - (START.Length+ARROW_ICON.Length));
            return s;
        }

        public DSyntaxData.Branch GetFinalBranch()
        {
            return routeBranches.Last();
        }

        public int GetNodeCount()
        {
            int nodeCount = 0;
            foreach (DSyntaxData.Branch b in routeBranches)
            {
                foreach (DSyntaxData.Node n in b.nodes)
                {
                    if (n is DSyntaxData.NodeSay)
                        nodeCount++;
                }
            }
            return nodeCount;
        }

        public int GetWordCount()
        {
            int wordCount = 0;
            foreach (DSyntaxData.Branch b in routeBranches)
            {
                foreach (DSyntaxData.Node n in b.nodes)
                {
                    if (n is DSyntaxData.NodeSay)
                    {
                        DSyntaxData.NodeSay ns = n as DSyntaxData.NodeSay;
                        wordCount += ns.parameter.Split(' ').Length;
                    }
                }
            }
            return wordCount;
        }

        public float GetReadingDuration()
        {
            return GetWordCount() / 200f * 60f;
        }

        public List<DialogueSnippets> GetDialogueSnippets()
        {
            List<DialogueSnippets> snippets = new List<DialogueSnippets>();
            for (int i = 0; i < routeBranches.Count; i++)
            {
                if (i == routeBranches.Count - 1) continue;

                snippets.Add(new DialogueSnippets(routeBranches[i], routeBranches[i + 1]));
            }
            return snippets;
        }
    }

    #endregion

    #region [Methods: DialogueSyntax Analysis]

    List<Route> GenerateRoutes(List<DSyntaxData.Branch> branches)
    {
        List<Route> routes = new List<Route>();
        MapRoutes(branches, routes, branches[0]);

        return routes;

        // Get First Snippet of First Route (Start > 1)
        /*
        routeResult += routes[0].GetRoute() + '\n';
        routeResult += routes[0].GetDialogueSnippets()[0].ShowSnippets();
        */
    }

    string GetAllRoutes(List<Route> routes)
    {
        string allRoutes = "";
        foreach (Route r in routes)
        {
            allRoutes += r.GetRoute() + '\n';
        }

        return allRoutes;
    }

    void MapRoutes(List<DSyntaxData.Branch> branches, List<Route> routes, DSyntaxData.Branch startBranch, Route route = null, int iteration = 0)
    {
        if (iteration > 100)
        {
            Debug.LogError("DialogueConverterWindow: MapRoutes has encountered an infinite loop!");
            return;
        }
        if (branches != null && routes != null && startBranch != null)
        {
            bool endBranch = true;

            foreach (DSyntaxData.Node node in startBranch.nodes)
            {
                if (node is DSyntaxData.NodeSay)
                {
                    continue;
                }
                else if (node is DSyntaxData.NodeChoices)
                {
                    DSyntaxData.NodeChoices nc = node as DSyntaxData.NodeChoices;

                    for (int f = 0; f < nc.choices.Count; f++)
                    {
                        DSyntaxData.Branch _b = branches.Find(b => b.name == nc.choices[f].toBranchName);

                        if (route == null)
                        {
                            Route r = new Route();
                            r.routeBranches.Add(startBranch);
                            r.routeBranches.Add(_b);
                            r.chosenChoices.Add(nc.choices[f].text.text);
                            MapRoutes(branches, routes, _b, r, iteration++);
                        }
                        else
                        {
                            Route r = route.DeepCopy();
                            r.routeBranches.Add(_b);
                            r.DeepCopyChosenChoices(r.chosenChoices);
                            r.chosenChoices.Add(nc.choices[f].text.text);
                            MapRoutes(branches, routes, _b, r, iteration++);
                        }
                    }

                    endBranch = false;
                }
                else if (node is DSyntaxData.NodeGoTo)
                {
                    DSyntaxData.NodeGoTo ngt = node as DSyntaxData.NodeGoTo;
                    DSyntaxData.Branch _b = branches.Find(b => b.name == ngt.toBranchName);

                    if (route == null)
                    {
                        Route r = new Route();
                        r.routeBranches.Add(startBranch);
                        r.routeBranches.Add(_b);
                        MapRoutes(branches, routes, _b, r, iteration++);
                    }
                    else
                    {
                        Route r = route.DeepCopy();
                        r.routeBranches.Add(_b);
                        MapRoutes(branches, routes, _b, r, iteration++);
                    }
                    endBranch = false;
                }
                else
                {
                    continue;
                }
            }

            // Route end
            if (endBranch == true)
            {
                if (route == null)
                {
                    route = new Route();
                    route.routeBranches.Add(startBranch);
                }

                route.name = GenerateRouteName(routes.Count);
                routes.Add(route);
            }
            return;
        }
    }

    string GenerateRouteName(int index)
    {
        string routeName = string.Empty;
        int division = index / 26;
        if(division!=0)
        {
            routeName += Convert.ToChar(65 + division -1).ToString();
        }

        index -= division * 26;
        routeName += Convert.ToChar(65 + (index)).ToString();

        return routeName;
    }

    #endregion

    #region [UI]

    static List<DSyntaxData.Branch> branches;

    protected override OdinMenuTree BuildMenuTree()
    {
        if (branches == null || branches.Count == 0) Close();

        var tree = new OdinMenuTree();

        List<Route> routes = GenerateRoutes(branches);
        tree.Add("Overview", new OverviewAnalysis(routes));
        foreach (Route route in routes)
        {
            tree.Add(route.name + " : " + route.GetRouteWithoutStart(), new RouteFullAnalysis(route));
        }

        return tree;
    }

    public class OverviewAnalysis
    {
        #region [Sorting]
        enum SortMode
        {
            None,
            Descending,
            Ascending
        }

        SortMode nameSort = SortMode.Ascending;
        SortMode readSort = SortMode.None;
        SortMode choiceSort = SortMode.None;
        SortMode nodeSort = SortMode.None;

        void ResetSort()
        {
            nameSort = SortMode.None;
            readSort = SortMode.None;
            choiceSort = SortMode.None;
            nodeSort = SortMode.None;
        }
        string GetNameSortTitle()
        {
            switch (nameSort)
            {
                case SortMode.Descending:
                    return "Sort: Name \u25BC";
                case SortMode.Ascending:
                    return "Sort: Name \u25B2";
                default:
                    return "Sort: Name";
            }
        }
        string GetReadSortTitle()
        {
            switch (readSort)
            {
                case SortMode.Descending:
                    return "ReadingDuration \u25BC";
                case SortMode.Ascending:
                    return "ReadingDuration \u25B2";
                default:
                    return "ReadingDuration";
            }
        }
        string GetChoiceSortTitle()
        {
            switch (choiceSort)
            {
                case SortMode.Descending:
                    return "ChoiceCount \u25BC";
                case SortMode.Ascending:
                    return "ChoiceCount \u25B2";
                default:
                    return "ChoiceCount";
            }
        }
        string GetNodeSortTitle()
        {
            switch (nodeSort)
            {
                case SortMode.Descending:
                    return "NodeCount \u25BC";
                case SortMode.Ascending:
                    return "NodeCount \u25B2";
                default:
                    return "NodeCount";
            }
        }
        class NameComparer : IComparer<string>
        {
            private readonly IComparer<string> _baseComparer = StringComparer.CurrentCulture;

            public int Compare(string x, string y)
            {
                if (_baseComparer.Compare(x, y) == 0)
                    return 0;

                if (x.Length < y.Length)
                    return -1;
                else if (x.Length > y.Length)
                    return 1;

                return _baseComparer.Compare(x, y);
            }
        } 
        #endregion

        public OverviewAnalysis(List<Route> routes)
        {
            List<DSyntaxData.Branch> finalBranches = new List<DSyntaxData.Branch>();
            List<(DSyntaxData.NodeSay, DSyntaxData.NodeSay)> snippetCombinations = new List<(DSyntaxData.NodeSay, DSyntaxData.NodeSay)>();

            totalRoutes = routes.Count;
            float totalReadingDuration = 0;

            foreach (Route route in routes)
            {
                #region [Reading Stats]

                float readingDuration = route.GetReadingDuration();
                totalReadingDuration += readingDuration;
                if (readingDuration > maxReadingDuration)
                {
                    maxReadingDuration = readingDuration;
                    maxReadingBranch = route.name;
                }
                else if (readingDuration < minReadingDuration)
                {
                    minReadingDuration = readingDuration;
                    minReadingBranch = route.name;
                }

                #endregion

                #region [Most Route & Most DSyntaxData.Node]

                if (mostChoiceRouteCount < route.chosenChoices.Count)
                {
                    mostChoiceRouteCount = route.chosenChoices.Count;
                    mostChoiceRouteName = route.name;
                }

                if (mostNodeRouteCount < route.GetNodeCount())
                {
                    mostNodeRouteCount = route.GetNodeCount();
                    mostNodeRouteName = route.name;
                }

                #endregion

                #region [Snippet Combinations Stats]

                foreach (var snippet in route.GetDialogueSnippets())
                {
                    var snippetTuple = snippet.GetSnippet();
                    if (!snippetCombinations.Contains(snippetTuple))
                    {
                        snippetCombinations.Add(snippetTuple);

                        snippetCombinationsString +=    
                            "\n<" + snippet.fromBranch.name + "> <" + snippet.toBranch.name + ">" +
                            "\n" + snippetTuple.Item1.name + ": " + snippetTuple.Item1.text +
                            "\n" + snippetTuple.Item2.name + ": " + snippetTuple.Item2.text + "\n";
                    }
                }



                #endregion

                #region [Final Branches Stats]

                if (!finalBranches.Contains(route.GetFinalBranch()))
                {
                    finalBranches.Add(route.GetFinalBranch());

                    finalBranchesString += "\n<" + route.GetFinalBranch().name + ">\n";
                    foreach (DSyntaxData.Node node in route.GetFinalBranch().nodes)
                    {
                        finalBranchesString += node.name + ": " + node.parameter + "\n";
                    }
                }

                #endregion

                #region [Overview Routes]

                RouteMinimalAnalysis routeMinimal = new RouteMinimalAnalysis(
                    routeName: route.name,
                    readingDuration: route.GetReadingDuration(),
                    choiceCount: route.chosenChoices.Count,
                    nodeCount: route.GetNodeCount(),
                    finalBranch: route.GetFinalBranch().name
                    );

                overviewAllRoutes.Add(routeMinimal);

                #endregion

            }

            meanReadingDuration = (float)System.Math.Round((totalReadingDuration / routes.Count), 2);
            snippetCombinationsCount = snippetCombinations.Count;
            finalBranchesCount = finalBranches.Count;
        }

        #region [Reading Section]
        [Header("Reading")]
        [HorizontalGroup("Reading")]
        [VerticalGroup("Reading/Stats")]
        [DisplayAsString] [SuffixLabel("sec", true)] public float minReadingDuration = 100000;
        [VerticalGroup("Reading/Stats")]
        [DisplayAsString] [SuffixLabel("sec", true)] public float maxReadingDuration = 0;
        [VerticalGroup("Reading/Stats")]
        [DisplayAsString] [SuffixLabel("sec", true)] public float meanReadingDuration;

        [Header("DSyntaxData.Branch")]
        [VerticalGroup("Reading/DSyntaxData.Branch")]
        [LabelText("DSyntaxData.Branch Name: ")]
        [DisplayAsString] public string minReadingBranch = "";
        [VerticalGroup("Reading/DSyntaxData.Branch")]
        [LabelText("DSyntaxData.Branch Name: ")]
        [DisplayAsString] public string maxReadingBranch = "";
        #endregion

        #region [Routes Section]
        [Header("Routes")]
        [DisplayAsString] public int totalRoutes;
        [HorizontalGroup("MostChoice")]
        [VerticalGroup("MostChoice/Count")]
        [DisplayAsString] public int mostChoiceRouteCount = 0;
        [VerticalGroup("MostChoice/Route")]
        [LabelText("Route Name: ")]
        [DisplayAsString] public string mostChoiceRouteName;

        [HorizontalGroup("MostNode")]
        [VerticalGroup("MostNode/Count")]
        [DisplayAsString] public int mostNodeRouteCount = 0;
        [VerticalGroup("MostNode/Route")]
        [LabelText("Route Name: ")]
        [DisplayAsString] public string mostNodeRouteName;
        #endregion

        #region [Final Branches]

        [Title(" ")]
        [DisplayAsString] public int finalBranchesCount;
        [FoldoutGroup("Final Branches:")][HideLabel][TextArea(2, 35)]
        public string finalBranchesString;

        #endregion

        #region [Snippets Combinations]

        [Title(" ")]
        [DisplayAsString] public int snippetCombinationsCount;
        [FoldoutGroup("Snippet Combinations:")] [HideLabel] [TextArea(2, 35)]
        public string snippetCombinationsString;

        #endregion

        #region [Overview]

        #region [Sorting Buttons]
        [FoldoutGroup("Overview All Routes")]
        [HorizontalGroup("Overview All Routes/Sort")]
        [VerticalGroup("Overview All Routes/Sort/Name")]
        [Button("$GetNameSortTitle")]
        public void SortByName()
        {
            if (nameSort == SortMode.None)
            {
                ResetSort();
                nameSort = SortMode.Ascending;
                overviewAllRoutes = overviewAllRoutes.OrderBy(r => r.routeName, new NameComparer()).ToList();
            }
            else if (nameSort == SortMode.Descending)
            {
                nameSort = SortMode.Ascending;
                overviewAllRoutes = overviewAllRoutes.OrderBy(r => r.routeName, new NameComparer()).ToList();
            }
            else
            {
                nameSort = SortMode.Descending;
                overviewAllRoutes = overviewAllRoutes.OrderByDescending(r => r.routeName, new NameComparer()).ToList();
            }
        }

        [VerticalGroup("Overview All Routes/Sort/ReadingDuration")]
        [Button("$GetReadSortTitle")]
        public void SortByReadingDuration()
        {
            if (readSort == SortMode.None)
            {
                ResetSort();
                readSort = SortMode.Descending;
                overviewAllRoutes = overviewAllRoutes.OrderByDescending(r => r.readingDuration).ToList();
            }
            else if (readSort == SortMode.Descending)
            {
                readSort = SortMode.Ascending;
                overviewAllRoutes = overviewAllRoutes.OrderBy(r => r.readingDuration).ToList();
            }
            else
            {
                readSort = SortMode.Descending;
                overviewAllRoutes = overviewAllRoutes.OrderByDescending(r => r.readingDuration).ToList();
            }
        }

        [VerticalGroup("Overview All Routes/Sort/ChoiceCount")]
        [Button("$GetChoiceSortTitle")]
        public void SortByChoiceCount()
        {
            if (choiceSort == SortMode.None)
            {
                ResetSort();
                choiceSort = SortMode.Descending;
                overviewAllRoutes = overviewAllRoutes.OrderByDescending(r => r.choiceCount).ToList();
            }
            else if (choiceSort == SortMode.Descending)
            {
                choiceSort = SortMode.Ascending;
                overviewAllRoutes = overviewAllRoutes.OrderBy(r => r.choiceCount).ToList();
            }
            else
            {
                choiceSort = SortMode.Descending;
                overviewAllRoutes = overviewAllRoutes.OrderByDescending(r => r.choiceCount).ToList();
            }
        }

        [VerticalGroup("Overview All Routes/Sort/NodeCount")]
        [Button("$GetNodeSortTitle")]
        public void SortByNodeCount()
        {
            if (nodeSort == SortMode.None)
            {
                ResetSort();
                nodeSort = SortMode.Descending;
                overviewAllRoutes = overviewAllRoutes.OrderByDescending(r => r.nodeCount).ToList();
            }
            else if (nodeSort == SortMode.Descending)
            {
                nodeSort = SortMode.Ascending;
                overviewAllRoutes = overviewAllRoutes.OrderBy(r => r.nodeCount).ToList();
            }
            else
            {
                nodeSort = SortMode.Descending;
                overviewAllRoutes = overviewAllRoutes.OrderByDescending(r => r.nodeCount).ToList();
            }
        }
        #endregion

        [FoldoutGroup("Overview All Routes")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [HideLabel]
        [PropertyOrder(2)]
        public List<RouteMinimalAnalysis> overviewAllRoutes = new List<RouteMinimalAnalysis>(); 

        #endregion
    }

    [Serializable]
    public class RouteMinimalAnalysis
    {
        public RouteMinimalAnalysis(string routeName, float readingDuration, int choiceCount, int nodeCount, string finalBranch)
        {
            this.routeName = routeName;
            this.readingDuration = readingDuration;
            this.choiceCount = choiceCount;
            this.nodeCount = nodeCount;
            this.finalBranch = finalBranch;
        }

        public string routeName;
        public float readingDuration;
        public int choiceCount;
        public int nodeCount;
        public string finalBranch;
    }

    public class RouteFullAnalysis
    {
        public RouteFullAnalysis(Route route)
        {
            // Data for route's path
            routePath = route.GetRouteWithoutStart();

            // Data for Minimal Analysis
            overview = new RouteMinimalAnalysis(
                    routeName: route.name,
                    readingDuration: route.GetReadingDuration(),
                    choiceCount: route.chosenChoices.Count,
                    nodeCount: route.GetNodeCount(),
                    finalBranch: route.GetFinalBranch().name
                );

            // Data for dialogue between branches (snippets)
            foreach (var snippet in route.GetDialogueSnippets())
            {
                var data = snippet.GetSnippet();
                snippets += "\n" + data.Item1.name + ": " + data.Item1.text +
                            "\n" + data.Item2.name + ": " + data.Item2.text + "\n";
            }

            // Data for dialogue
            int _index = 0;
            foreach (DSyntaxData.Branch branch in route.routeBranches)
            {
                dialogue += "\n<" + branch.name + ">\n";
                foreach (DSyntaxData.Node node in branch.nodes)
                {
                    if (node is DSyntaxData.NodeSay)
                    {
                        dialogue += node.name + ": " + node.parameter + "\n";
                    }
                    else if (node is DSyntaxData.NodeChoices)
                    {
                        int __index = _index;
                        dialogue += "CHOICE: " + route.chosenChoices[__index] + "\n";
                        _index++;
                    }
                }
            }
        }

        [DisplayAsString]
        public string routePath;

        public RouteMinimalAnalysis overview;

        [FoldoutGroup("Snippets Between Branches")]
        [HideLabel]
        [TextArea(2, 35)]
        public string snippets = string.Empty;

        [FoldoutGroup("Dialogue:")]
        [HideLabel]
        [TextArea(2, 35)]
        public string dialogue = string.Empty;
    } 

    #endregion
}
