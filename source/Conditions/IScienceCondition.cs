using System;


using UnityEngine;

namespace RealScience.Conditions
{
    public interface IScienceCondition : IConfigNode
    {
        float DataRateModifier { get; }
        float MaximumDataModifier { get; }
        float MaximumDataBonus { get; }
        bool IsRestriction { get; }
        string Exclusion { get; }

        bool Evaluate(Part part, float deltaTime);
    }
}
