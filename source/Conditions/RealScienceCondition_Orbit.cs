using System;
using System.Collections.Generic;

using UnityEngine;

namespace RealScience.Conditions
{
    public class RealScienceCondition_Orbit : RealScienceCondition
    {
        // common properties
        public string conditionType = "Orbit";
        public bool restriction = false;
        public bool optional = false;
        public float dataRateModifier = 1f;
        // specific properties
        public float eccentricityMin = 0f;
        public float eccentricityMax = 1f;
        public float apoapsisMin = float.MinValue;
        public float apoapsisMax = float.MaxValue;
        public float periapsisMin = float.MinValue;
        public float periapsisMax = float.MaxValue;
        public float inclinationMin = 0f;
        public float inclinationMax = 180f;
        public float siderealMin = 0f;
        public float siderealMax = float.MaxValue;

        public override float DataRateModifier
        {
            get { return dataRateModifier; }
        }

        public override bool Evaluate(Part part)
        {
            return true;
        }
        public override void Load(ConfigNode node)
        {
            // Load common properties
            if (node.HasValue("conditionType"))
                conditionType = node.GetValue("conditionType");
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
            if (node.HasValue("optional"))
            {
                try
                {
                    optional = bool.Parse(node.GetValue("optional"));
                }
                catch (FormatException)
                {
                    optional = false;
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

        }
        public override void Save(ConfigNode node)
        {
            // Save common properties
            node.AddValue("conditionType", conditionType);
            node.AddValue("restriction", restriction);
            node.AddValue("optional", optional);
            node.AddValue("dataRateModifier", dataRateModifier);

        }
    }
}
