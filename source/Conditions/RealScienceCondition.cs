using System;

namespace RealScience.Conditions
{
    public enum EvalState
    {
        UNKNOWN = -1,
        VALID,
        INVALID,
        RESET,
        FAILED
    }

    public class RealScienceCondition : IScienceCondition
    {
        public virtual float DataRateModifier
        {
            get { return 1f; }
        }
        public virtual float MaximumDataModifier
        {
            get { return 1f; }
        }
        public virtual float MaximumDataBonus
        {
            get { return 0f; }
        }
        
        public virtual bool IsRestriction
        {
            get { return false; }
        }
        public virtual string Exclusion
        {
            get { return ""; }
        }
        public virtual string Tooltip
        {
            get { return ""; }
        }
        public virtual string Name
        {
            get { return ""; }
        }


		public virtual EvalState Evaluate(Part part, float deltaTime, ExperimentState state)
        {
            return EvalState.VALID;
        }
        public virtual void Load(ConfigNode node)
        {

        }
        public virtual void Save(ConfigNode node)
        {
        }
    }
}
