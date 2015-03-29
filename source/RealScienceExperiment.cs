using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using KSPPluginFramework;

using UnityEngine;

using RealScience.Conditions;

namespace RealScience
{
    public class ExperimentState
    {
        public enum StateEnum
        {
            UNKNOWN = -1,
            IDLE,
            CONDITIONS_NOT_MET,
            PAUSED,
            PAUSED_CONNECTION,
            RESEARCHING,
            RESEARCH_COMPLETE,
            RESEARCH_PAUSED_CONDITIONS_NOT_MET,
            ANALYZING,
            ANALYSIS_COMPLETE,
            READY_TO_TRANSMIT,
            START_TRANSMIT,
            TRANSMITTING,
            TRANSMIT_COMPLETE,
            FAILED,
            COMPLETED
        }

        private StateEnum currentState = StateEnum.UNKNOWN;
        public StateEnum CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }

        public ExperimentState()
        {
            currentState = StateEnum.UNKNOWN;
        }

        public override string ToString()
        {
            return string.Format("{0:D}", (int)currentState);
        }

        public static ExperimentState FromStrimg(string stateString)
        {
            ExperimentState newState = new ExperimentState();
            StateEnum state = (StateEnum)int.Parse(stateString);
            newState.CurrentState = state;
            return newState;
        }
    }

    public class RealScienceExperiment : PartModuleExtended
    {

        [KSPField(isPersistant = false)]
        public string experimentName;
        [KSPField(isPersistant = false)]
        public string experimentTitle;
        [KSPField(isPersistant = false)]
        public string description;
        [KSPField(isPersistant = false)]
        public string discipline = "science";
        [KSPField(isPersistant = false)]
        public float requiredData = 0f;
        [KSPField(isPersistant = false)]
        public float maximumData = -1f;
        [KSPField(isPersistant = false)]
        public float analysisTime = 0f;
        [KSPField(isPersistant = false)]
        public float scienceValue = 0f;
        [KSPField(isPersistant = false)]
        public float scienceValuePerData = -1f;
        [KSPField(isPersistant = false)]
        public float researchDataRate = 1f;
        [KSPField(isPersistant = false)]
        public bool multiFlight = false;
        [KSPField(isPersistant = false)]
        public bool onRails = false;
        [KSPField(isPersistant = false)]
        public bool autoAnalyze = true;
        [KSPField(isPersistant = false)]
        public bool autoTransmit = true;
        [KSPField(isPersistant = false)]
        public float dataSize = 0f;
        [KSPField(isPersistant = false)]
        public bool canFailAtAnyTime = false;
        [KSPField(isPersistant = false)]
        public float transmitValue = 1f;


        List<IScienceCondition> conditions;
        List<RealScienceConditionGroup> conditionGroups;
        public ExperimentState state;
        bool loaded = false;
        float totalDataRateModifier = 1f;
        float totalDataCapModifier = 1f;
        float currentDataCap = -1f;


        [KSPField(isPersistant = true)]
        public float currentData = 0f;
        [KSPField(isPersistant = true)]
        public float analysisTimeRemaining = 0f;
        [KSPField(isPersistant = true)]
        public float lastMET = 0f;
        [KSPField(isPersistant = true)]
        public float transmittedPackets = 0f;
        // DataRate is how many packets per second
        [KSPField(isPersistant = true)]
        public float transmissionDataRate = 1f;
        // DataResourceCost is how much Electric Charge per packet
        [KSPField(isPersistant = true)]
        public float transmissionDataResourceCost = 1f;
        [KSPField(isPersistant = true)]
        public float quedPackets = 0f;
        [KSPField(isPersistant = true)]
        public float recoveryValue = 0f;


        public override void Start()
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
                    scienceValue = modulePrefab.scienceValue;
                    researchDataRate = modulePrefab.researchDataRate;
                    conditions = modulePrefab.conditions;
                    conditionGroups = modulePrefab.conditionGroups;
                }
            }
            // In case OnLoad never happened, we need to properly intialize our values
            if (!loaded)
            {
                analysisTimeRemaining = analysisTime;
                lastMET = (float)this.vessel.missionTime;
                state = new ExperimentState();
                state.CurrentState = ExperimentState.StateEnum.IDLE;
            }
            if (transmitValue < 1f)
                GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
        }

        protected void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> vs)
        {
            if (vs.to == Vessel.Situations.LANDED || vs.to == Vessel.Situations.SPLASHED)
            {
                if (vs.from == Vessel.Situations.FLYING)
                {
                    if (vs.host.mainBody.isHomeWorld)
                    {
                        if (state.CurrentState != ExperimentState.StateEnum.FAILED)
                        {
                            ResearchAndDevelopment.Instance.AddScience(recoveryValue, TransactionReasons.ScienceTransmission);
                            ScreenMessage statusMessage = new ScreenMessage(String.Format("[{0}] Experiment Recovered.", experimentTitle), 5.0f, ScreenMessageStyle.UPPER_LEFT);
                            ScreenMessages.PostScreenMessage(statusMessage, true);
                        }
                    }
                }
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            float currentMET = (float)this.vessel.missionTime;
            float deltaTime = currentMET - lastMET;
            EvalState eval = ValidateConditions(deltaTime);

            switch (state.CurrentState)
            {
                case ExperimentState.StateEnum.ANALYZING:
                    analysisTimeRemaining -= (currentMET - lastMET);
                    if (analysisTimeRemaining <= 0)
                        state.CurrentState = ExperimentState.StateEnum.ANALYSIS_COMPLETE;
                    break;
                case ExperimentState.StateEnum.ANALYSIS_COMPLETE:
                    state.CurrentState = ExperimentState.StateEnum.READY_TO_TRANSMIT;
                    break;
                case ExperimentState.StateEnum.READY_TO_TRANSMIT:
                    if (autoTransmit)
                    {
                        state.CurrentState = ExperimentState.StateEnum.START_TRANSMIT;
                    }
                    break;
                case ExperimentState.StateEnum.START_TRANSMIT:
                    // find an antenna
                    List<IScienceDataTransmitter> antennas = part.FindModulesImplementing<IScienceDataTransmitter>();
                    float dataRate = 0f;
                    double resourceCost = double.MaxValue;
                    bool favorLowPowerAntenna = true;
                    IScienceDataTransmitter chosenTransmitter = null;
                    if (RealScienceManager.Instance != null)
                        favorLowPowerAntenna = RealScienceManager.Instance.userSettings.favorLowPowerAntenna;
                    foreach(IScienceDataTransmitter transmitter in antennas)
                    {
                        if (favorLowPowerAntenna)
                        {
                            if (transmitter.DataResourceCost < resourceCost)
                            {
                                resourceCost = transmitter.DataResourceCost;
                                chosenTransmitter = transmitter;
                            }
                        }
                        else
                        {
                            if (transmitter.DataRate > dataRate)
                            {
                                dataRate = transmitter.DataRate;
                                chosenTransmitter = transmitter;
                            }
                        }
                    }
                    if (chosenTransmitter == null)
                        break;
                    if (chosenTransmitter.IsBusy())
                        break;
                    // Shouldn't be needed, but as a last safety measure we find all experiments on the craft and check that none are transmitting
                    foreach(Part vPart in this.vessel.Parts)
                    {
                        foreach(PartModule pm in vPart.Modules)
                        {
                            RealScienceExperiment vExperiment = pm as RealScienceExperiment;
                            if (vExperiment != null)
                            {
                                if (vExperiment.state.CurrentState == ExperimentState.StateEnum.TRANSMITTING)
                                    break;
                            }
                        }
                    }
                    transmissionDataRate = chosenTransmitter.DataRate;
                    transmissionDataResourceCost = (float)chosenTransmitter.DataResourceCost;
                    transmittedPackets = 0f;

                    ScreenMessages.PostScreenMessage(String.Format("[{0}]: Starting Transmission...", experimentTitle), 5f, ScreenMessageStyle.UPPER_LEFT);
                    state.CurrentState = ExperimentState.StateEnum.TRANSMITTING;
                    break;
                case ExperimentState.StateEnum.TRANSMITTING:
                    float transmitScienceValue;
                    float sciencePerPacket = scienceValue / dataSize;
                    LogFormatted_DebugOnly(String.Format("RealScience: OnUpdate: TRANSMITTING: transmittedPackets={0:F2}", transmittedPackets));
                    if (transmittedPackets >= dataSize)
                    {
                        LogFormatted_DebugOnly(String.Format("RealScience: OnUpdate: TRANSMITTING: Transmission Complete.  Changing state."));
                        state.CurrentState = ExperimentState.StateEnum.TRANSMIT_COMPLETE;
                        break;
                    }
                    quedPackets += transmissionDataRate * deltaTime;
                    LogFormatted_DebugOnly(String.Format("RealScience: OnUpdate: TRANSMITTING: deltaTime={0:F2}, quedPackets={1:F2}", deltaTime, quedPackets));
                    // This just ensures we don't transmit too much
                    if (transmittedPackets + quedPackets > dataSize)
                    {
                        quedPackets = dataSize - transmittedPackets;
                        LogFormatted_DebugOnly(String.Format("RealScience: OnUpdate: TRANSMITTING: adjusted quedPackets={0:F2}", quedPackets));
                    }
                    if (quedPackets < 1f && transmittedPackets + quedPackets < dataSize)
                    {
                        LogFormatted_DebugOnly(String.Format("RealScience: OnUpdate: TRANSMITTING: Waiting until we can send a whole packet."));
                        break;
                    }
                    transmittedPackets += quedPackets;
                    // Use up charge
                    if (part.RequestResource("ElectricCharge", transmissionDataResourceCost * deltaTime) == transmissionDataResourceCost * deltaTime)
                    {
                        float transmitScience = (sciencePerPacket * quedPackets) * transmitValue;
                        float recoveryScience = (sciencePerPacket * quedPackets) * 1f - transmitValue;
                        ResearchAndDevelopment.Instance.AddScience(transmitScience, TransactionReasons.ScienceTransmission);
                        recoveryValue += recoveryScience;
                        LogFormatted_DebugOnly(String.Format("RealScience: OnUpdate: TRANSMITTING: transmittedPackets={0:F2}, add {1:F2} science", transmittedPackets, sciencePerPacket * quedPackets));
                        ScreenMessage statusMessage = new ScreenMessage(String.Format("{0:F2}/{1:F2} Packets Transmitted...", transmittedPackets, dataSize), 5.0f, ScreenMessageStyle.UPPER_LEFT);
                        ScreenMessages.PostScreenMessage(statusMessage, true);
                        quedPackets = 0f;
                    }
                    break;
                case ExperimentState.StateEnum.TRANSMIT_COMPLETE:
                    // science is awarded by the transmission, so we don't need to do it here
                    // ResearchAndDevelopment.Instance.AddScience(scienceValue, TransactionReasons.ScienceTransmission);
                    ScreenMessage completionMessage = new ScreenMessage(String.Format("[{0}]: Transmission Completed", experimentTitle), 5.0f, ScreenMessageStyle.UPPER_LEFT);
                    ScreenMessages.PostScreenMessage(completionMessage, true);
                    state.CurrentState = ExperimentState.StateEnum.COMPLETED;
                    break;
                case ExperimentState.StateEnum.FAILED:
                    break;
                case ExperimentState.StateEnum.IDLE:
                    switch (eval)
                    {
                        case EvalState.VALID:
                            break;
                        case EvalState.FAILED:
                            state.CurrentState = ExperimentState.StateEnum.FAILED;
                            break;
                        case EvalState.RESET:
                            currentData = 0f;
                            break;
                        case EvalState.INVALID:
                            state.CurrentState = ExperimentState.StateEnum.CONDITIONS_NOT_MET;
                            break;
                    }
                    break;
                case ExperimentState.StateEnum.PAUSED:
                    switch (eval)
                    {
                        case EvalState.VALID:
                            break;
                        case EvalState.FAILED:
                            state.CurrentState = ExperimentState.StateEnum.FAILED;
                            break;
                        case EvalState.RESET:
                            currentData = 0f;
                            state.CurrentState = ExperimentState.StateEnum.IDLE;
                            break;
                    }
                    break;
                case ExperimentState.StateEnum.CONDITIONS_NOT_MET:
                    switch (eval)
                    {
                        case EvalState.VALID:
                            state.CurrentState = ExperimentState.StateEnum.IDLE;
                            break;
                        case EvalState.FAILED:
                            state.CurrentState = ExperimentState.StateEnum.FAILED;
                            break;
                        case EvalState.RESET:
                            currentData = 0f;
                            break;
                    }
                    break;
                case ExperimentState.StateEnum.RESEARCHING:
                    // check if research data >= required data and change state to RESEARCH_COMPLETE if so
                    if (currentData >= requiredData)
                    {
                        state.CurrentState = ExperimentState.StateEnum.RESEARCH_COMPLETE;
                        break;
                    }
                    switch (eval)
                    {
                        case EvalState.VALID:
                            float currentDataRate = researchDataRate * totalDataRateModifier;
                            currentData = currentData + (currentDataRate * (currentMET - lastMET));
                            break;
                        case EvalState.FAILED:
                            state.CurrentState = ExperimentState.StateEnum.FAILED;
                            break;
                        case EvalState.RESET:
                            currentData = 0f;
                            state.CurrentState = ExperimentState.StateEnum.IDLE;
                            break;
                        case EvalState.INVALID:
                            state.CurrentState = ExperimentState.StateEnum.RESEARCH_PAUSED_CONDITIONS_NOT_MET;
                            break;
                    }
                    break;
                case ExperimentState.StateEnum.RESEARCH_PAUSED_CONDITIONS_NOT_MET:
                    switch (eval)
                    {
                        case EvalState.VALID:
                            state.CurrentState = ExperimentState.StateEnum.RESEARCHING;
                            break;
                        case EvalState.FAILED:
                            state.CurrentState = ExperimentState.StateEnum.FAILED;
                            break;
                        case EvalState.RESET:
                            currentData = 0f;
                            state.CurrentState = ExperimentState.StateEnum.IDLE;
                            break;
                    }
                    break;
                case ExperimentState.StateEnum.RESEARCH_COMPLETE:
                    if (autoAnalyze)
                        state.CurrentState = ExperimentState.StateEnum.ANALYZING;
                    break;
                case ExperimentState.StateEnum.UNKNOWN:
                    break;
            }
            lastMET = currentMET;
        }

        public EvalState ValidateConditions(float deltaTime)
        {
            totalDataRateModifier = 1f;
            totalDataCapModifier = 1f;
            currentDataCap = maximumData;

            if (conditionGroups == null || conditionGroups.Count == 0)
            {
                // No valid groups so we evaluate each condition instead
                foreach (IScienceCondition condition in conditions)
                {
                    EvalState eval = condition.Evaluate(part, deltaTime);
                    if (eval != EvalState.VALID)
                        return eval;

                    totalDataRateModifier *= condition.DataRateModifier;
                    totalDataCapModifier *= condition.MaximumDataModifier;
                    currentDataCap += condition.MaximumDataBonus;
                }
                return EvalState.VALID;
            }
            else
            {
                // We have groups, so instead of evaluating the conditions, we evaluate the groups
                foreach (RealScienceConditionGroup group in conditionGroups)
                {
                    EvalState eval = group.Evaluate(part, deltaTime);
                    if (eval != EvalState.VALID)
                        return eval;

                    totalDataRateModifier *= group.DataRateModifer;
                    totalDataCapModifier *= group.MaximumDataModifier;
                    currentDataCap += group.MaximumDataBonus;
                }
                return EvalState.VALID;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (node.HasValue("state"))
            {
                state = ExperimentState.FromStrimg(node.GetValue("state"));
            }
            else
            {
                state = new ExperimentState();
                state.CurrentState = ExperimentState.StateEnum.IDLE;
            }

            if (!node.HasValue("analysisTimeRemaining"))
            {
                analysisTimeRemaining = analysisTime;
            }

            if (!node.HasValue("lastMET"))
            {
                if (this.vessel != null)
                    lastMET = (float)this.vessel.missionTime;
            }

            // Load bare conditions if present
            if (node.HasNode("condition"))
            {
                LogFormatted_DebugOnly("RealScience: OnLoad: Loading conditions...");
                if (conditions == null)
                    conditions = new List<IScienceCondition>();
                conditions.Clear();
                foreach(ConfigNode conditionNode in node.GetNodes("condition"))
                {
                    if (conditionNode.HasValue("conditionType"))
                    {
                        string conditionType = conditionNode.GetValue("conditionType");
                        LogFormatted_DebugOnly("RealScience: OnLoad: Creating condition of type: " + conditionType);
                        RealScienceCondition newCondition = null;
                        System.Runtime.Remoting.ObjectHandle conditionObj = null;
                        conditionObj = Activator.CreateInstance(null, "RealScience.Conditions.RealScienceCondition_" + conditionType);
                        if (conditionObj == null)
                            LogFormatted_DebugOnly("RealScience: OnLoad: Failed to create Condition ObjectHandle");
                        else
                            newCondition = (RealScienceCondition)conditionObj.Unwrap();
                        if (newCondition != null)
                        {
                            newCondition.Load(conditionNode);
                            conditions.Add(newCondition);
                        }
                        else
                            LogFormatted_DebugOnly("RealScience: OnLoad: Failed to create Condition instance");
                    }
                }
            }
            // otherwise load groups
            else if (node.HasNode("conditionGroup"))
            {
                if (conditionGroups == null)
                    conditionGroups = new List<RealScienceConditionGroup>();
                conditionGroups.Clear();
                foreach(ConfigNode groupNode in node.GetNodes("conditionGroup"))
                {
                    RealScienceConditionGroup group = new RealScienceConditionGroup();
                    group.Load(groupNode);
                    conditionGroups.Add(group);
                }
            }
            loaded = true;
        }
    }

    public class RealScienceConditionGroup : IConfigNode
    {
        [KSPField(isPersistant = false)]
        public string groupType = "or";

        protected List<IScienceCondition> conditions;
        protected float dataRateModifier = 1f;
        protected float maximumDataModifier = 1f;
        protected float maximumDataBonus = 0f;

        public float DataRateModifer
        {
            get { return dataRateModifier; }
        }
        public float MaximumDataModifier
        {
            get { return maximumDataModifier; }
        }
        public float MaximumDataBonus
        {
            get { return maximumDataBonus; }
        }

        public EvalState Evaluate(Part part, float deltaTime)
        {
            if (groupType.ToLower() == "or")
            {
                foreach (IScienceCondition condition in conditions)
                {
                    EvalState eval = condition.Evaluate(part, deltaTime);
                    if (eval == EvalState.INVALID)
                        continue;

                    if (eval == EvalState.VALID)
                    {
                        dataRateModifier *= condition.DataRateModifier;
                        maximumDataModifier *= condition.MaximumDataModifier;
                        maximumDataBonus += condition.MaximumDataBonus;
                        return EvalState.VALID;
                    }

                    return eval;
                }
                return EvalState.INVALID;
            }
            else
            {
                foreach (IScienceCondition condition in conditions)
                {
                    EvalState eval = condition.Evaluate(part, deltaTime);
                    if (eval == EvalState.VALID)
                    {
                        dataRateModifier *= condition.DataRateModifier;
                        maximumDataModifier *= condition.MaximumDataModifier;
                        maximumDataBonus += condition.MaximumDataBonus;
                    }
                    else
                        return eval;
                }
                return EvalState.VALID;
            }
        }

        public void Load(ConfigNode node)
        {
            if (node.HasNode("groupType"))
                groupType = node.GetValue("groupType");
            if (node.HasNode("condition"))
            {
                foreach (ConfigNode conditionNode in node.GetNodes("condition"))
                {
                    if (conditionNode.HasValue("conditionType"))
                    {
                        string conditionType = conditionNode.GetValue("conditionType");
                        IScienceCondition newCondition = (IScienceCondition)Activator.CreateInstance(null, "RealScience.RealScienceCondition_" + conditionType);
                        newCondition.Load(conditionNode);
                        conditions.Add(newCondition);
                    }
                }
            }
        }
        public void Save(ConfigNode node)
        {

        }
    }
}
