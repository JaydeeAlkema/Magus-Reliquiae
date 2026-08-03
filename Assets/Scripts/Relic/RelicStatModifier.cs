using System;
using PlayerStats;

namespace Relic
{
	[Serializable]
	public struct RelicStatModifier
	{
		public PlayerStatType Stat;
		public StatModifierOperation Operation;
		public float Value;
	}
}
