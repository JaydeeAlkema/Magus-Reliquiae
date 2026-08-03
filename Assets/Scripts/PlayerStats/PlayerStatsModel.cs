using System;
using System.Collections.Generic;

namespace PlayerStats
{
	public sealed class PlayerStatsModel
	{
		private readonly Dictionary<PlayerStatType, float> _baseValues = new();
		private readonly Dictionary<PlayerStatType, List<StatModifier>> _modifiers = new();

		public void SetBaseValue(PlayerStatType stat, float value)
		{
			_baseValues[stat] = value;
		}

		public float GetBaseValue(PlayerStatType stat)
		{
			return _baseValues.TryGetValue(stat, out float value) ? value : 0f;
		}

		public void AddModifier(StatModifier modifier)
		{
			if (!_modifiers.TryGetValue(modifier.Stat, out List<StatModifier> list))
			{
				list = new List<StatModifier>();
				_modifiers.Add(modifier.Stat, list);
			}

			list.Add(modifier);
		}

		public int RemoveModifiersBySource(string sourceId)
		{
			int removedCount = 0;
			foreach (KeyValuePair<PlayerStatType, List<StatModifier>> kvp in _modifiers)
			{
				List<StatModifier> list = kvp.Value;
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (!string.Equals(list[i].SourceId, sourceId, StringComparison.Ordinal))
						continue;

					list.RemoveAt(i);
					removedCount++;
				}
			}

			return removedCount;
		}

		public float GetValue(PlayerStatType stat)
		{
			float baseValue = GetBaseValue(stat);
			if (!_modifiers.TryGetValue(stat, out List<StatModifier> list) || list.Count == 0)
				return baseValue;

			float additive = 0f;
			float multiplicative = 1f;
			for (int i = 0; i < list.Count; i++)
			{
				StatModifier modifier = list[i];
				if (modifier.Operation == StatModifierOperation.Add)
				{
					additive += modifier.Value;
					continue;
				}

				multiplicative *= 1f + modifier.Value;
			}

			return (baseValue + additive) * multiplicative;
		}
	}
}
