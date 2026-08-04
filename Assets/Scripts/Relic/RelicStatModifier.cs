using System;
using PlayerStats;

namespace Relic
{
	/// <summary>
	/// Serializable stat modifier used by relic level data.
	/// </summary>
	[Serializable]
	public struct RelicStatModifier
	{
		/// <summary>
		/// Target stat.
		/// </summary>
		public PlayerStatType Stat;
		/// <summary>
		/// Modifier operation.
		/// </summary>
		public StatModifierOperation Operation;
		/// <summary>
		/// Modifier amount.
		/// </summary>
		public float Value;
	}
}
