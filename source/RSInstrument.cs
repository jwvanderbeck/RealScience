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
        [KSPField]
        public string sampleType = "undefined";
        [KSPField(isPersistant = true)]
        public float sampleRate = 1.0f;
        [KSPField(isPersistant = true)]
        public float bufferSize = 60.0f;
        [KSPField(isPersistant = true)]
        public float transferRate = 5.0f;
        [KSPField]
        public string resourceUsed = "ElectricCharge";
        [KSPField]
        public float resourceBaseCost = 1.0f;
        [KSPField]
        public bool alwaysEnabled = false;
        [KSPField]
        public float baseCost = 1.0f;
        // Multicast instruments can send one packet to multiple experiments
        [KSPField]
        public bool multicast = false;
        //
        // GUI Properties
        // These properties define how much flexability the player has in tweaking the basic properties in the GUI
        [KSPField]
        public bool enableSampleRateGUI = true;
        [KSPField]
        public float sampleRateMin = 0.5f;
        [KSPField]
        public float sampleRateMax = 1.5f;
        [KSPField]
        public FloatCurve sampleRateCost = null;
        [KSPField]
        public FloatCurve sampleRateResourceCost = null;

        [KSPField]
        public bool enableBufferSizeGUI = true;
        [KSPField]
        public float bufferSizeMin = 10.0f;
        [KSPField]
        public float bufferSizeMax = 120.0f;
        [KSPField]
        public FloatCurve bufferSizeCost = null;
        [KSPField]
        public FloatCurve bufferSizeResourceCost = null;

        [KSPField]
        public bool enableTransferRateGUI = true;
        [KSPField]
        public float transferRateMin = 1.0f;
        [KSPField]
        public float transferRateMax = 10.0f;
        [KSPField]
        public FloatCurve transferRateCost = null;
        [KSPField]
        public FloatCurve transferRateResourceCost = null;


        // Private properties
        private float sampleBuffer = 0f;
        private bool instrumentEnabled = false;
        private float currentCost = 1.0f;
        private float resourceCost = 1.0f;

        [KSPField(isPersistant = true)]
        private double lastTime = 0d;
        [KSPField(isPersistant = true)]
        private double lastBufferTransferTime = 0d;

        #region Public Interface

        public void Activate()
        {
            SetActive(true);
        }

        public void Deactivate()
        {
            SetActive(false);
        }

        public bool SetActive(bool state)
        {
            bool lastState = instrumentEnabled;
            instrumentEnabled = state;
            enabled = state;
            return lastState;
        }
            
        /// <summary>
        /// Gets samples from the buffer.  Samples can only be transfered in accordance with the transferRate of the instrument
        /// </summary>
        /// <returns>The actual number of samples removed from the buffer.  Due to storage or traansfer rate this may be less than requested.</returns>
        /// <param name="desiredSampleCount">Desired sample count.</param>
        public float GetSamplesFromBuffer(float desiredSampleCount)
        {
            if (!instrumentEnabled)
                return 0f;
            
            double currentTime = Planetarium.GetUniversalTime();
            double transferTime = currentTime - lastBufferTransferTime;
            float actualSamples = Mathf.Min(desiredSampleCount, sampleBuffer);
            actualSamples = Math.Min(actualSamples, (float)(transferTime * transferRate));
            sampleBuffer -= actualSamples;
            lastBufferTransferTime = currentTime;
            return actualSamples;
        }

        #endregion

        #region Internal Methods

        protected void GenerateSamples(double time)
        {
            // determine how many samples we can produce this tick
            float samplesGenerated = (float)(time * sampleRate);
            // determine the resource cost of that many samples
            float sampleCost = samplesGenerated * resourceCost;
            // are there enough resources to pay or this?
            float resourceAvailable = this.part.RequestResource(resourceUsed, sampleCost);
            if (resourceAvailable < sampleCost)
            {
                // If we got less resource than what was needed to pay for the samples, we need to scale back how many samples we generate
                samplesGenerated = resourceAvailable / resourceCost;
            }
            AddSamplesToBuffer(samplesGenerated);
        }

        /// <summary>
        /// Adds the samples to buffer and returns the actual amount added
        /// </summary>
        /// <returns>The amount of samples actually addeed to the buffer, which due to space might be less than requested.</returns>
        /// <param name="sampleCount">Amount of samples to add to the buffer.</param>
        protected float AddSamplesToBuffer(float sampleCount)
        {
            float bufferSpace = bufferSize - sampleBuffer;
            float samplesAdded = Mathf.Min(bufferSpace, sampleCount);
            sampleBuffer += samplesAdded;
            return samplesAdded;
        }

        #endregion


        #region IPartCostModifier implementation

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return currentCost;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        #endregion

        #region PartModule implementation

        public override void OnAwake()
        {
            base.OnAwake();
            // Create default FloatCurve objects
            if (sampleRateCost == null)
            {
                sampleRateCost = new FloatCurve();
                sampleRateCost.Add(0f, 1f);
            }
            if (sampleRateResourceCost == null)
            {
                sampleRateResourceCost = new FloatCurve();
                sampleRateResourceCost.Add(0f, 1f);
            }
            if (bufferSizeCost == null)
            {
                bufferSizeCost = new FloatCurve();
                bufferSizeCost.Add(0f, 1f);
            }
            if (bufferSizeResourceCost == null)
            {
                bufferSizeResourceCost = new FloatCurve();
                bufferSizeResourceCost.Add(0f, 1f);
            }
            if (transferRateCost == null)
            {
                transferRateCost = new FloatCurve();
                transferRateCost.Add(0f, 1f);
            }
            if (transferRateResourceCost == null)
            {
                transferRateResourceCost = new FloatCurve();
                transferRateResourceCost.Add(0f, 1f);
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }
            
        public override void OnUpdate()
        {
            base.OnUpdate();

            double currentTime = Planetarium.GetUniversalTime();
            if (currentTime > lastTime + 1)
            {
                // generate samples and buffer them
                double sampleTime = currentTime - lastTime;
                GenerateSamples(sampleTime);
                lastTime = currentTime;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
        }

        #endregion

    }
}

