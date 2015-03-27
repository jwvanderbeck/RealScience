using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealScience.Conditions
{
    public class RealScienceCondition_Altitude : RealScienceCondition
    {
        // common properties
        public string conditionType = "Altitude";
        public bool restriction = false;
        public string exclusion = "";
        public bool optional = false;
        public float dataRateModifier = 1f;
        // specific properties
        public float altitudeMin = 0f;
        public float altitudeMax = float.MaxValue;

        public override float DataRateModifier
        {
            get { return dataRateModifier; }
        }
        public override bool IsRestriction
        {
            get { return restriction; }
        }
        public override string Exclusion
        {
            get { return exclusion; }
        }
        public override bool Evaluate(Part part)
        {
            float altitude = FlightGlobals.getAltitudeAtPos(part.transform.position);
            if (altitude >= altitudeMin && altitude <= altitudeMax)
                return true;
            return false;
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
            if (node.HasValue("altitudeMin"))
            {
                try
                {
                    altitudeMin = float.Parse(node.GetValue("altitudeMin"));
                }
                catch (FormatException)
                {
                    altitudeMin = 0f;
                }

            }
            if (node.HasValue("altitudeMax"))
            {
                try
                {
                    altitudeMax = float.Parse(node.GetValue("altitudeMax"));
                }
                catch (FormatException)
                {
                    altitudeMax = float.MaxValue;
                }

            }
        }
        public override void Save(ConfigNode node)
        {
            // Save common properties
            node.AddValue("conditionType", conditionType);
            node.AddValue("restriction", restriction);
            node.AddValue("optional", optional);
            node.AddValue("dataRateModifier", dataRateModifier);
            // Save specific properties
            node.AddValue("altitudeMin", altitudeMin);
            node.AddValue("altitudeMax", altitudeMax);
        }
    }
}
