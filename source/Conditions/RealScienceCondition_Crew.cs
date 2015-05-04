using System;

using UnityEngine;

namespace RealScience.Conditions
{
	class RealScienceCondition_Crew : RealScienceCondition
	{
		// common properties
		public string conditionType = "Crew";
		public bool restriction = false;
		public string exclusion = "";
		public float dataRateModifier = 1f;
		public float maximumDataModifier = 1f;
		public float maximumDataBonus = 0f;

		// specific properties
		public int minimumCrew = 1;
		public int maximumCrew = 20;

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
		protected string tooltip;
		public override string Tooltip
		{
			get { return tooltip; }
		}
		public override string Name
		{
			get { return conditionType; }
		}

		public override EvalState Evaluate(Part part, float deltaTime, ExperimentState state)
		{
			bool valid = false;
			tooltip = "\nCrewed Condition";
			if (restriction)
			{
				if (exclusion.ToLower() == "reset")
					tooltip += "\nThe following condition must <b>not</b> be met.  If they are the experiment will be <b>reset</b>.";
				else if (exclusion.ToLower() == "fail")
					tooltip += "\nThe following condition must <b>not</b> be met.  If they are, the experiment will <b>fail</b>.";
				else
					tooltip += "\nThe following condition must <b>not</b> be met.";
			}
			else
				tooltip += "\nThe following condition must be met.";

			int currentCrew = part.vessel.GetCrewCount();
			if (currentCrew >= minimumCrew && currentCrew <= maximumCrew)
				valid = true;

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
			if (node.HasValue("minimumCrew"))
				minimumCrew = int.Parse(node.GetValue("minimumCrew"));
			if (node.HasValue("maximumCrew"))
				maximumCrew = int.Parse(node.GetValue("maximumCrew"));
		}
		public override void Save(ConfigNode node)
		{
			// Save common properties
			node.AddValue("conditionType", conditionType);
			node.AddValue("restriction", restriction);
			node.AddValue("exclusion", exclusion);
			node.AddValue("dataRateModifier", dataRateModifier);
			node.AddValue("minimumCrew", minimumCrew);
			node.AddValue("maximumCrew", maximumCrew);
		}
	}
}
