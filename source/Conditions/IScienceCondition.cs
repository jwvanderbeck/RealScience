using System;


using UnityEngine;

namespace RealScience.Conditions
{
    public interface IScienceCondition : IConfigNode
    {
        float DataRateModifier { get; }
        bool IsRestriction { get; }
        string Exclusion { get; }

        bool Evaluate(Part part);
    }
}
