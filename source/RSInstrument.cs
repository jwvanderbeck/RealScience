using System;
using System.Collections.Generic;
using UnityEngine;
using KSPPluginFramework;

namespace RealScience
{
	public class RSInstrument : PartModuleExtended, IPartCostModifier
	{
		// Basic Properties
		// These properties define what type of scientific data the instrument collects, how fast it can collect it,
		// how much it can store, etc.
		[KSPField(isPersistant=true)]
		public string sampleType = "undefined";
		[KSPField(isPersistant=true)]
		public float sampleRate = 1.0f;
		[KSPField(isPersistant=true)]
		public float bufferSize = 60.0f;
		[KSPField(isPersistant=true)]
		public float transferRate = 5.0f;
		[KSPField(isPersistant=true)]
		public string resourceUsed = "ElectricCharge";
		[KSPField(isPersistant=true)]
		public float resourceCost = 1.0f;
		[KSPField(isPersistant=true)]
		public bool alwaysEnabled = false;
		[KSPField(isPersistant=true)]
		public float baseCost = 1.0f;
		// Multicast instruments can send one packet to multiple experiments
		[KSPField(isPersistant=false)]
		public bool multicast = false;
		//
		// GUI Properties
		// These properties define how much flexability the player has in tweaking the basic properties in the GUI
		// TODO Add properties to disable the individual controls if desired
		[KSPField(isPersistant=false)]
		public bool enableSampleRateGUI = true;
		[KSPField(isPersistant=false)]
		public float sampleRateMin = 0.5f;
		[KSPField(isPersistant=false)]
		public float sampleRateMax = 1.5f;
		public FloatCurve sampleRateCost = null;
		public FloatCurve sampleRateResourceCost = null;

		[KSPField(isPersistant=false)]
		public bool enableBufferSizeGUI = true;
		[KSPField(isPersistant=false)]
		public float bufferSizeMin = 10.0f;
		[KSPField(isPersistant=false)]
		public float bufferSizeMax = 120.0f;
		public FloatCurve bufferSizeCost = null;
		public FloatCurve bufferSizeResourceCost = null;

		[KSPField(isPersistant=false)]
		public bool enableTransferRateGUI = true;
		[KSPField(isPersistant=false)]
		public float transferRateMin = 1.0f;
		[KSPField(isPersistant=false)]
		public float transferRateMax = 10.0f;
		public FloatCurve transferRateCost = null;
		public FloatCurve transferRateResourceCost = null;


		// Private properties
		private float sampleBuffer = 0f;
		private bool instrumentEnabled = false;
		private float currentCost = 1.0f;
		private float currentResourseCost = 1.0f;
		private double lastTime = 0d;
		private 

		// Public Properties
		public float Buffer {
			get { return sampleBuffer; }
			private set { sampleBuffer = value; }
		}

		// Public Methods
		public void Activate()
		{
			this.SetActive (true);
		}

		public void Deactivate()
		{
			this.SetActive(false);
		}

		public bool SetActive(bool state)
		{
			bool lastState = instrumentEnabled;
			instrumentEnabled = state;
			return lastState;
		}


		//  IPartCostModifier Interface
		public float GetModuleCost (float defaultCost)
		{
			return baseCost * (sampleRateCost + bufferSizeCost + transferRateCost);
		}

		// KSP PartModule methods
		public override void OnAwake ()
		{
			base.OnAwake ();
		}

		public override void OnStart (StartState state)
		{
			base.OnStart (state);
			Part prefab = this.part.partInfo.partPrefab;
			foreach (PartModule pm in prefab.Modules)
			{
				RSInstrument modulePrefab = pm as RSInstrument;
				// Read in non-persistent data from the prefab
				if (modulePrefab != null)
				{
					// Sample Rate
					enableSampleRateGUI = modulePrefab.enableSampleRateGUI;
					sampleRateMin = modulePrefab.sampleRateMin;
					sampleRateMax = modulePrefab.sampleRateMax;
					if ((object)modulePrefab.sampleRateCost != null)
						sampleRateCost = modulePrefab.sampleRateCost;
					if ((object)modulePrefab.sampleRateResourceCost != null)
						sampleRateResourceCost = modulePrefab.sampleRateResourceCost;

					// Buffer Size
					enableBufferSizeGUI = modulePrefab.enableBufferSizeGUI;
					bufferSizeMin = modulePrefab.bufferSizeMin;
					bufferSizeMax = modulePrefab.bufferSizeMax;
					if ((object)modulePrefab.bufferSizeCost != null)
						bufferSizeCost = modulePrefab.bufferSizeCost;
					if ((object)modulePrefab.bufferSizeResourceCost != null)
						bufferSizeResourceCost = modulePrefab.bufferSizeResourceCost;

					// Transfer Rate
					enableTransferRateGUI = modulePrefab.enableTransferRateGUI;
					transferRateMin = modulePrefab.transferRateMin;
					transferRateMax = modulePrefab.transferRateMax;
					if ((object)modulePrefab.transferRateCost != null)
						transferRateCost = modulePrefab.transferRateCost;
					if ((object)modulePrefab.transferRateResourceCost != null)
						transferRateResourceCost = modulePrefab.transferRateResourceCost;
				}
			}
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();

			double currentTime = Planetarium.GetUniversalTime ();
			if (currentTime > lastTime + 1) 
			{
				// generate a sample and send it to attached experiments
			}
			lastTime = currentTime;
		}
			
		public override void OnLoad (ConfigNode node)
		{
			base.OnLoad (node);
			if (node.HasNode ("sampleRateCost")) {
				sampleRateCost = new FloatCurve ();
				sampleRateCost.Load(node.GetNode("sampleRateCost"));
			}
			if (node.HasNode ("sampleRateResourceCost")) {
				sampleRateResourceCost = new FloatCurve ();
				sampleRateResourceCost.Load(node.GetNode("sampleRateResourceCost"));
			}

			if (node.HasNode ("bufferSizeCost")) {
				bufferSizeCost = new FloatCurve ();
				bufferSizeCost.Load(node.GetNode("bufferSizeCost"));
			}
			if (node.HasNode ("bufferSizeResourceCost")) {
				bufferSizeResourceCost = new FloatCurve ();
				bufferSizeResourceCost.Load(node.GetNode("bufferSizeResourceCost"));
			}

			if (node.HasNode ("transferRateCost")) {
				transferRateCost = new FloatCurve ();
				transferRateCost.Load(node.GetNode("transferRateCost"));
			}
			if (node.HasNode ("transferRateResourceCost")) {
				transferRateResourceCost = new FloatCurve ();
				transferRateResourceCost.Load(node.GetNode("transferRateResourceCost"));
			}
		}

		public override void OnSave (ConfigNode node)
		{
			base.OnSave (node);
		}
	}
}

