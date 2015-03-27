using System;

using UnityEngine;

namespace RealScience.Conditions
{
    class RealScienceCondition_Biome : RealScienceCondition
    {
        // common properties
        public string conditionType = "Biome";
        public bool restriction = false;
        public string exclusion = "";
        public float dataRateModifier = 1f;
        // specific properties
        public string biome;

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
            string currentBiome = ScienceUtil.GetExperimentBiome(part.vessel.mainBody, part.vessel.latitude, part.vessel.longitude);
            if (biome.ToLower() == currentBiome.ToLower())
                return true;
            return true;
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
            if (node.HasValue("biome"))
                biome = node.GetValue("biome");
        }
        public override void Save(ConfigNode node)
        {
            // Save common properties
            node.AddValue("conditionType", conditionType);
            node.AddValue("restriction", restriction);
            node.AddValue("exclusion", exclusion);
            node.AddValue("dataRateModifier", dataRateModifier);
            node.AddValue("biome", biome);
        }
    }
}
