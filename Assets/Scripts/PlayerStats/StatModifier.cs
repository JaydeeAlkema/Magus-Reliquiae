using System;

namespace PlayerStats
{
	/// <summary>
	///     Operation used by a stat modifier.
	/// </summary>
	public enum StatModifierOperation
	{
		/// <summary>
		///     Adds directly to the base value.
		/// </summary>
		Add = 0,
		/// <summary>
		///     Multiplies the current total.
		/// </summary>
		Multiply = 1,
	}

	/// <summary>
	///     Immutable stat modifier record.
	/// </summary>
	[Serializable]
	public readonly struct StatModifier
	{
		/// <summary>
		///     Target stat.
		/// </summary>
		public readonly PlayerStatType Stat;
		/// <summary>
		///     Modification operation.
		/// </summary>
		public readonly StatModifierOperation Operation;
		/// <summary>
		///     Modification amount.
		/// </summary>
		public readonly float Value;
		/// <summary>
		///     Identifier used to remove all modifiers from a source.
		/// </summary>
		public readonly string SourceId;

		/// <summary>
		///     Creates a new modifier.
		/// </summary>
		/// <param name="stat">Target stat.</param>
		/// <param name="operation">Modifier operation.</param>
		/// <param name="value">Modifier amount.</param>
		/// <param name="sourceId">Source identifier.</param>
		public StatModifier(PlayerStatType stat, StatModifierOperation operation, float value, string sourceId)
		{
			Stat = stat;
			Operation = operation;
			Value = value;
			SourceId = sourceId ?? string.Empty;
		}
	}
}
