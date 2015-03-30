using System;

using UnityEngine;

namespace RealScience.Conditions
{
    class RealScienceCondition_Resource : RealScienceCondition
    {
        // common properties
        public string conditionType = "Resource";
        public bool restriction = false;
        public string exclusion = "";
        public float dataRateModifier = 1f;
        public float maximumDataModifier = 1f;
        public float maximumDataBonus = 0f;

        // specific properties
        public string resourceName = "ElectricCharge";
        public float initialConsumption = 1f;
        public float usagePerSecond = 1f;
        public bool initialOnly = false;
        public bool validIfEmpty = false;

        public bool initialUpdate = true;

        public override float DataRateModifier
        {
            get { return dataRateModifier; }
        }
        public override float MaximumDataModifier
        {
            get { return maximumDataModifier; }
        }
        public override float MaximumDataBonus
        {
            get { return maximumDataBonus; }
        }
        public override bool IsRestriction
        {
            get { return restriction; }
        }
        public override string Exclusion
        {
            get { return exclusion; }
        }

        public override EvalState Evaluate(Part part, float deltaTime)
        {
            bool valid;
            if (!initialUpdate && initialOnly)
                valid = true;
            float consumption;
            if (initialUpdate)
            {
                initialUpdate = false;
                consumption = part.RequestResource(resourceName, initialConsumption);
                if (consumption != initialConsumption && !validIfEmpty)
                    valid = false;
                else 
                    valid = true;
            }
            else
            {
                consumption = part.RequestResource(resourceName, usagePerSecond * deltaTime);
                if (consumption != (usagePerSecond * deltaTime) && !validIfEmpty)
                    valid = false;
                else 
                    valid = true;
            }
            if (!restriction)
            {
                if (valid)
                    return EvalState.VALID;
                else
                    return EvalState.INVALID;
            }
            else
            {
                if (!valid)
                    return EvalState.VALID;
                else
                {
                    if (exclusion.ToLower() == "reset")
                        return EvalState.RESET;
                    else if (exclusion.ToLower() == "fail")
                        return EvalState.FAILED;
                    else
                        return EvalState.INVALID;
                }
            }
        }
        public override void Load(ConfigNode node)
        {
            // Load common properties
            if (node.HasValue("conditionType"))
                conditionType = node.GetValue("conditionType");
            if (node.HasValue("exclusion"))
                exclusion = node.GetValue("exclusion");
            if (node.HasValue("restriction"))
            {
                try
                {
                    restriction = bool.Parse(node.GetValue("restriction"));
                }
                catch (FormatException)
                {
                    restriction = false;
                }
            }
            if (node.HasValue("dataRateModifier"))
            {
                try
                {
                    dataRateModifier = float.Parse(node.GetValue("dataRateModifier"));
                }
                catch (FormatException)
                {
                    dataRateModifier = 1f;
                }
            }
            // Load specific properties
            if (node.HasValue("resourceName"))
                resourceName = node.GetValue("resourceName");
            if (node.HasValue("initialConsumption"))
                initialConsumption = float.Parse(node.GetValue("initialConsumption"));
            if (node.HasValue("usagePerSecond"))
                usagePerSecond = float.Parse(node.GetValue("usagePerSecond"));
            if (node.HasValue("initialOnly"))
                initialOnly = bool.Parse(node.GetValue("initialOnly"));
            if (node.HasValue("validIfEmpty"))
                validIfEmpty = bool.Parse(node.GetValue("validIfEmpty"));
            if (node.HasValue("initialUpdate"))
                initialUpdate = bool.Parse(node.GetValue("initialUpdate"));
        }
        public override void Save(ConfigNode node)
        {
            // Save common properties
            node.AddValue("conditionType", conditionType);
            node.AddValue("restriction", restriction);
            node.AddValue("exclusion", exclusion);
            node.AddValue("dataRateModifier", dataRateModifier);
            node.AddValue("resourceName", resourceName);
            node.AddValue("initialConsumption", initialConsumption);
            node.AddValue("usagePerSecond", usagePerSecond);
            node.AddValue("initialOnly", initialOnly);
            node.AddValue("validIfEmpty", validIfEmpty);
            node.AddValue("initialUpdate", initialUpdate);
        }
    }
}
