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
        public string exclusion = "";
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
            if (part.vessel.orbit.eccentricity < eccentricityMin)
                return false;
            if (part.vessel.orbit.eccentricity > eccentricityMax)
                return false;
            if (part.vessel.orbit.inclination < inclinationMin)
                return false;
            if (part.vessel.orbit.inclination > inclinationMax)
                return false;
            double r_ap = part.vessel.orbit.semiMajorAxis * (1 + part.vessel.orbit.eccentricity);
            double r_pe = part.vessel.orbit.semiMajorAxis * (1 - part.vessel.orbit.eccentricity);
            double radius = part.vessel.orbit.referenceBody.Radius;
            double ap = r_ap - radius;
            double pe = r_pe - radius;
            Debug.Log(String.Format("RealScience: Orbit: Evaluate: Calculated AP, radius={0:F2}, altitude={1:F2}.  Reference body={2}, radius={3:F2}", r_ap, ap, part.vessel.orbit.referenceBody.name, radius));
            Debug.Log(String.Format("RealScience: Orbit: Evaluate: Calculated PE, radius={0:F2}, altitude={1:F2}.  Reference body={2}, radius={3:F2}", r_pe, pe, part.vessel.orbit.referenceBody.name, radius));
            if ((float)ap < apoapsisMin)
                return false;
            if ((float)ap > apoapsisMax)
                return false;
            if ((float)pe < periapsisMin)
                return false;
            if ((float)pe > periapsisMax)
                return false;

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
            node.AddValue("dataRateModifier", dataRateModifier);

        }
    }
}
