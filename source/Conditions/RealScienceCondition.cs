using System;

namespace RealScience.Conditions
{
    public class RealScienceCondition : IScienceCondition
    {
        public virtual float DataRateModifier
        {
            get { return 1f; }
        }

        public virtual bool Evaluate(Part part)
        {
            return true;
        }
        public virtual void Load(ConfigNode node)
        {

        }
        public virtual void Save(ConfigNode node)
        {
        }
    }
}
