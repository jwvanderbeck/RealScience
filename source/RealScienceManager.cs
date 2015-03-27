using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using KSPPluginFramework;

using UnityEngine;

namespace RealScience
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class RealScienceManager : MonoBehaviourWindowPlus
    {
        internal bool stickyWindow = false;
        internal bool isReady = false;
        private ApplicationLauncherButton appLauncherButton;
        public static RealScienceManager Instance { get; private set; }
        public UserSettings userSettings = null;

        internal override void Awake()
        {
            base.Awake();
            Instance = this;
            if (userSettings == null)
                userSettings = new UserSettings("../settings.cfg");

            if (userSettings.FileExists)
                userSettings.Load();
            else
                userSettings.Save();
        }
      
        internal override void Start()
        {
            base.Start();
            StartCoroutine("AddToToolbar");
            RealScience.Resources.LoadTextures();
            DragEnabled = false;
            ClampToScreen = true;
            TooltipsEnabled = true;
            TooltipMouseOffset = new Vector2d(10, 10);
            TooltipStatic = true;
            WindowCaption = "";
        }

        IEnumerator AddToToolbar()
        {
            while (!ApplicationLauncher.Ready)
            {
                yield return null;
            }
            try
            {
                // Load the icon for the button
                Texture iconTexture = GameDatabase.Instance.GetTexture("RealScience/Resources/AppLauncherIcon", false);
                if (iconTexture == null)
                {
                    throw new Exception("RealScience KSC Window: Failed to load icon texture");
                }
                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    OpenWindow,
                    CloseWindow,
                    HoverInButton,
                    HoverOutButton,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    iconTexture);
                ApplicationLauncher.Instance.AddOnHideCallback(HideButton);
                ApplicationLauncher.Instance.AddOnRepositionCallback(RepostionWindow);
            }
            catch (Exception e)
            {
                LogFormatted_DebugOnly("Unable to add button to application launcher: " + e.Message);
                throw e;
            }
        }
        void OpenWindow()
        {
            CalculateWindowBounds();
            Visible = true;
            stickyWindow = true;
        }
        void CloseWindow()
        {
            Visible = false;
            stickyWindow = false;
        }
        void HideButton()
        {
            ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
        }
        void RepostionWindow()
        {
            CalculateWindowBounds();
        }
        void HoverInButton()
        {
            CalculateWindowBounds();
            Visible = true;
        }
        void HoverOutButton()
        {
            if (!stickyWindow)
                Visible = false;
        }
        internal void CalculateWindowBounds()
        {
            if (appLauncherButton == null)
                return;

            float windowWidth = 500f;
            float left = Screen.width - windowWidth - 75f;
            float windowHeight = 200f;
            float top = 40f;
            WindowRect = new Rect(left, top, windowWidth, windowHeight);
        }
        internal override void OnGUIOnceOnly()
        {
            Styles.InitStyles();
            Styles.InitSkins();
            SkinsLibrary.SetCurrent("SolarizedDark");
            SkinsLibrary.CurrentSkin.label.wordWrap = true;
        }
        internal override void DrawWindow(int id)
        {
            GUILayout.BeginVertical();
            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>Experiment</b>", GUILayout.Width(200));
            GUILayout.Label("<b>Data</b>", GUILayout.Width(75));
            GUILayout.Label("<b>Status</b>", GUILayout.Width(100));
            GUILayout.Label("<b>Actions</b>", GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.Parts == null)
                return;

            // Experiments
            userSettings.currentResearchScrollPosition = GUILayout.BeginScrollView(userSettings.currentResearchScrollPosition);
            foreach (Part part in FlightGlobals.ActiveVessel.Parts)
            {
                if (part.Modules.Contains("RealScienceExperiment"))
                {
                    foreach(PartModule pm in part.Modules)
                    {
                        RealScienceExperiment experiment = pm as RealScienceExperiment;
                        if (experiment != null)
                        {
                            GUILayout.BeginHorizontal();
                            // Title and Description
                            GUILayout.Label(new GUIContent(experiment.experimentTitle, experiment.description), GUILayout.Width(200));
                            // Current Data/Total Data
                            float currentData = experiment.currentData;
                            float totalData = experiment.requiredData;
                            string dataString = String.Format("{0:F2}/{1:F2}", currentData, totalData);
                            GUILayout.Label(dataString, GUILayout.Width(75));
                            // Status
                            string statusString = "";
                            switch (experiment.state.CurrentState)
                            {
                                case ExperimentState.StateEnum.IDLE:
                                    statusString = String.Format("<color=#b58900ff>{0,-30}</color>", "Idle");
                                    break;
                                case ExperimentState.StateEnum.RESEARCHING:
                                    statusString = String.Format("<color=#859900ff>{0,-30}</color>", "Researching");
                                    break;
                                case ExperimentState.StateEnum.RESEARCH_COMPLETE:
                                    statusString = String.Format("<color=#b58900ff>{0,-30}</color>", "Research Complete");
                                    break;
                                case ExperimentState.StateEnum.PAUSED:
                                    statusString = String.Format("<color=#859900ff>{0,-30}</color>", "Research Paused");
                                    break;
                                case ExperimentState.StateEnum.PAUSED_CONNECTION:
                                    statusString = String.Format("<color=#dc322fff>{0,-30}</color>", "Lost Connection");
                                    break;
                                case ExperimentState.StateEnum.ANALYZING:
                                    statusString = String.Format("<color=#b58900ff>{0,-30}</color>", "Analyzing");
                                    break;
                                case ExperimentState.StateEnum.COMPLETED:
                                    statusString = String.Format("<color=#859900ff>{0,-30}</color>", "Complete");
                                    break;
                                case ExperimentState.StateEnum.READY_TO_TRANSMIT:
                                    statusString = String.Format("<color=#b58900ff>{0,-30}</color>", "Ready to Transmit");
                                    break;
                                case ExperimentState.StateEnum.FAILED:
                                    statusString = String.Format("<color=#dc322fff>{0,-30}</color>", "Failed");
                                    break;
                            }
                            GUILayout.Label(statusString, GUILayout.Width(100));
                            // Action Button
                            switch (experiment.state.CurrentState)
                            {
                                case ExperimentState.StateEnum.IDLE:
                                    if (GUILayout.Button(new GUIContent(RealScience.Resources.btnStartResearch, "Start Research"), GUILayout.Width(32)))
                                    {
                                        // Start research
                                        experiment.state.CurrentState = ExperimentState.StateEnum.RESEARCHING;
                                    }
                                    break;
                                case ExperimentState.StateEnum.RESEARCHING:
                                    if (GUILayout.Button(new GUIContent(RealScience.Resources.btnPauseResearch, "Pause Research"), GUILayout.Width(32)))
                                    {
                                        // Pause research
                                        experiment.state.CurrentState = ExperimentState.StateEnum.PAUSED;
                                    }
                                    break;
                                case ExperimentState.StateEnum.RESEARCH_COMPLETE:
                                    if (GUILayout.Button(new GUIContent(RealScience.Resources.btnStartAnalysis, "Analyze Research Data"), GUILayout.Width(32)))
                                    {
                                        // Start Analysis
                                        experiment.state.CurrentState = ExperimentState.StateEnum.ANALYZING;
                                    }
                                    break;
                                case ExperimentState.StateEnum.PAUSED:
                                    if (GUILayout.Button(new GUIContent(RealScience.Resources.btnStartResearch, "Start Research"), GUILayout.Width(32)))
                                    {
                                        // Start research
                                        experiment.state.CurrentState = ExperimentState.StateEnum.RESEARCHING;
                                    }
                                    break;
                                case ExperimentState.StateEnum.READY_TO_TRANSMIT:
                                    if (GUILayout.Button(new GUIContent(RealScience.Resources.btnTransmitResults, "Transmit Results"), GUILayout.Width(32)))
                                    {
                                        // Transmit results
                                        experiment.state.CurrentState = ExperimentState.StateEnum.START_TRANSMIT;
                                    }
                                    break;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
