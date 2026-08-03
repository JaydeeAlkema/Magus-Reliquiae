using System;

namespace PlayerStats
{
	public enum StatModifierOperation
	{
		Add = 0,
		Multiply = 1,
	}

	[Serializable]
	public readonly struct StatModifier
	{
		public readonly PlayerStatType Stat;
		public readonly StatModifierOperation Operation;
		public readonly float Value;
		public readonly string SourceId;

		public StatModifier(PlayerStatType stat, StatModifierOperation operation, float value, string sourceId)
		{
			Stat = stat;
			Operation = operation;
			Value = value;
			SourceId = sourceId ?? string.Empty;
		}
	}
}
