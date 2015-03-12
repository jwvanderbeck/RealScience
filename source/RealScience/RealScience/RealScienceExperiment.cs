using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace RealScience
{
    public enum ExperimentState
    {
        UNKNOWN = -1,
        IDLE,
        PAUSED,
        PAUSED_CONNECTION,
        RESEARCHING,
        RESEARCH_COMPLETE,
        ANALYZING,
        FAILED,
        COMPLETED
    }

    public class RealScienceExperiment : PartModule
    {

        [KSPField(isPersistant = false)]
        public string experimentName;
        [KSPField(isPersistant = false)]
        public string experimentTitle;
        [KSPField(isPersistant = false)]
        public string description;
        [KSPField(isPersistant = false)]
        public string discipline;
        [KSPField(isPersistant = false)]
        public float requiredData = 0f;
        [KSPField(isPersistant = false)]
        public float analysisTime = 0f;
        [KSPField(isPersistant = false)]
        public float value = 0f;
        [KSPField(isPersistant = false)]
        public float scienceValue = 0f;
        [KSPField(isPersistant = false)]
        public float dataRate = 1f;

        List<RealScienceCondition> conditions;
        List<RealScienceConditionGroup> conditionGroups;

        protected float currentData = 0f;
        protected double AnalysisTimeRemaining = 0f;
        protected ExperimentState currentState = ExperimentState.UNKNOWN;
        protected double lastMET = 0;

        public void Start()
        {
            // We need to re-load our data from the prefab
            Part prefab = this.part.partInfo.partPrefab;
            foreach (PartModule pm in prefab.Modules)
            {
                RealScienceExperiment modulePrefab = pm as RealScienceExperiment;
                if (modulePrefab != null)
                {
                    experimentName = modulePrefab.experimentName;
                    experimentTitle = modulePrefab.experimentTitle;
                    description = modulePrefab.description;
                    discipline = modulePrefab.discipline;
                    requiredData = modulePrefab.requiredData;
                    analysisTime = modulePrefab.analysisTime;
                    value = modulePrefab.value;
                    scienceValue = modulePrefab.scienceValue;
                    dataRate = modulePrefab.dataRate;
                    conditions = modulePrefab.conditions;
                    conditionGroups = modulePrefab.conditionGroups;
                }
            }
            currentData = 0f;
            AnalysisTimeRemaining = analysisTime;
            currentState = ExperimentState.IDLE;
            lastMET = this.vessel.missionTime;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            double currentMET = this.vessel.missionTime;

            switch (currentState)
            {
                case ExperimentState.ANALYZING:
                    AnalysisTimeRemaining -= (lastMET - currentMET);
                    if (AnalysisTimeRemaining <= 0)
                    {
                        currentState = ExperimentState.COMPLETED;
                        // TODO award science points
                    }
                    break;
                case ExperimentState.FAILED:
                    break;
                case ExperimentState.IDLE:
                    break;
                case ExperimentState.PAUSED:
                    break;
                case ExperimentState.PAUSED_CONNECTION:
                    break;
                case ExperimentState.RESEARCHING:
                    // check if research data >= required data and change state to RESEARCH_COMPLETE if so
                    if (currentData >= requiredData)
                    {
                        currentState = ExperimentState.RESEARCH_COMPLETE;
                    }
                    // Evaluate each group or condition and if they are all true, add research data
                    else
                    {
                        bool conditionsValid = true;
                        float totalDataRateModifier = 1f;
                        if (conditionGroups == null || conditionGroups.Count == 0)
                        {
                            // No valid groups so we evaluate each condition instead
                            foreach (RealScienceCondition condition in conditions)
                            {
                                if (!condition.Evaluate(this.part))
                                    conditionsValid = false;
                                else
                                    totalDataRateModifier *= condition.dataRateModifier;
                            }
                        }
                        else
                        {
                            // We have groups, so instead of evaluating the conditions, we evaluate the groups
                            foreach (RealScienceConditionGroup group in conditionGroups)
                            {
                                if (!group.Evaluate(this.part))
                                    conditionsValid = false;
                                else
                                    totalDataRateModifier *= group.DataRateModifer;
                            }
                        }

                        if (conditionsValid)
                        {
                            float currentDataRate = dataRate * totalDataRateModifier;
                            currentData = currentData + (currentDataRate * ((float)lastMET - (float)currentMET));
                        }
                    }
                    break;
                case ExperimentState.UNKNOWN:
                    break;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
        }
    }

    public class RealScienceConditionGroup : ConfigNode
    {
        [KSPField(isPersistant = false)]
        public string groupType = "or";

        protected List<RealScienceCondition> conditions;
        protected float dataRateModifier = 1f;

        public float DataRateModifer
        {
            get { return dataRateModifier; }
        }

        public bool Evaluate(Part part)
        {
            if (groupType.ToLower() == "or")
            {
                foreach (RealScienceCondition condition in conditions)
                {
                    if (condition.Evaluate(part))
                    {
                        dataRateModifier *= condition.dataRateModifier;
                        return true;
                    }
                }
                return false;
            }
            else
            {
                bool conditionsValid = true;
                foreach (RealScienceCondition condition in conditions)
                {
                    if (!condition.Evaluate(part))
                        conditionsValid = false;
                    dataRateModifier *= condition.dataRateModifier;
                }
                return conditionsValid;
            }
        }

    }

    public class RealScienceCondition : ConfigNode
    {
        [KSPField(isPersistant = false)]
        public string conditionName;
        [KSPField(isPersistant = false)]
        public string conditionType;
        [KSPField(isPersistant = false)]
        public bool restriction = false;
        [KSPField(isPersistant = false)]
        public bool optional = false;
        [KSPField(isPersistant = false)]
        public float dataRateModifier = 1f;

        public bool Evaluate(Part part)
        {
            return true;
        }
    }

    public class RealScienceCondition_Orbit : RealScienceCondition
    {
        [KSPField(isPersistant = false)]
        public float eccentricityMin = 0f;
        [KSPField(isPersistant = false)]
        public float eccentricityMax = 1f;
        [KSPField(isPersistant = false)]
        public float apoapsisMin = float.MinValue;
        [KSPField(isPersistant = false)]
        public float apoapsisMax = float.MaxValue;
        [KSPField(isPersistant = false)]
        public float periapsisMin = float.MinValue;
        [KSPField(isPersistant = false)]
        public float periapsisMax = float.MaxValue;
        [KSPField(isPersistant = false)]
        public float inclinationMin = 0f;
        [KSPField(isPersistant = false)]
        public float inclinationMax = 180f;
        [KSPField(isPersistant = false)]
        public float siderealMin = 0f;
        [KSPField(isPersistant = false)]
        public float siderealMax = float.MaxValue;
    }

    public class RealScienceCondition_Altitude : RealScienceCondition
    {
        [KSPField(isPersistant = false)]
        public float altitudeMin = 0f;
        [KSPField(isPersistant = false)]
        public float altitudeMax = float.MaxValue;

        public bool Evaluate(Part part)
        {
            float altitude = FlightGlobals.getAltitudeAtPos(part.transform.position);
            return altitude >= altitudeMin && altitude <= altitudeMax;
        }
    }
}
