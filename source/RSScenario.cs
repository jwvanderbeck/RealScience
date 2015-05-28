using System;
using UnityEngine;
using KSPPluginFramework;

namespace RealScience
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, 
		new GameScenes[] 
		{ 
			GameScenes.FLIGHT,
			GameScenes.EDITOR,
			GameScenes.SPACECENTER,
			GameScenes.TRACKSTATION
		}
	)]
	public class RSScenario : ScenarioModule
	{
		public static RSScenario Instance { get; private set; }
		public bool isReady = false;

		internal void Log(string message)
		{
			bool debug = true;
			if (debug)
				Debug.Log("[RealScience] " + message);
		}


		// KSP Methods
		// ScenarioModule Methods
		public override void OnAwake ()
		{
			base.OnAwake ();

			Instance = this;
		}
		public void Start()
		{
			Log("Scenario Start");
			isReady = true; 
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
		}

		public override void OnSave(ConfigNode node)
		{
			base.OnSave(node);
		}
	}
}

