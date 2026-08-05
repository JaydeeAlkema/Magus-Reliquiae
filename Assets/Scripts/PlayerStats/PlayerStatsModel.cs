using System;
using System.Collections.Generic;

namespace PlayerStats
{
	/// <summary>
	///     Runtime stat container for base values and active modifiers.
	/// </summary>
	/// <remarks>
	///     Set base stats first, then add or remove modifiers by source ID to drive derived values.
	/// </remarks>
	public sealed class PlayerStatsModel
	{
		private readonly Dictionary<PlayerStatType, float> _baseValues = new();
		private readonly Dictionary<PlayerStatType, List<StatModifier>> _modifiers = new();

		/// <summary>
		///     Sets the unmodified value for a stat.
		/// </summary>
		/// <param name="stat">Stat to update.</param>
		/// <param name="value">Base value.</param>
		public void SetBaseValue(PlayerStatType stat, float value)
		{
			_baseValues[stat] = value;
		}

		/// <summary>
		///     Reads the stored base value for a stat.
		/// </summary>
		/// <param name="stat">Stat to query.</param>
		/// <returns>The stored base value, or zero if unset.</returns>
		public float GetBaseValue(PlayerStatType stat)
		{
			return _baseValues.GetValueOrDefault(stat, 0f);
		}

		/// <summary>
		///     Adds a modifier to the tracked list for its stat.
		/// </summary>
		/// <param name="modifier">Modifier to add.</param>
		public void AddModifier(StatModifier modifier)
		{
			if (!_modifiers.TryGetValue(modifier.Stat, out List<StatModifier> list))
			{
				list = new List<StatModifier>();
				_modifiers.Add(modifier.Stat, list);
			}

			list.Add(modifier);
		}

		/// <summary>
		///     Removes all modifiers from a source ID.
		/// </summary>
		/// <param name="sourceId">Source identifier to match.</param>
		/// <returns>Number of removed modifiers.</returns>
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

		/// <summary>
		///     Computes the final value for a stat.
		/// </summary>
		/// <param name="stat">Stat to compute.</param>
		/// <returns>Base value after additive and multiplicative modifiers.</returns>
		public float GetValue(PlayerStatType stat)
		{
			float baseValue = GetBaseValue(stat);
			if (!_modifiers.TryGetValue(stat, out List<StatModifier> list) || list.Count == 0)
				return baseValue;

			float additive = 0f;
			float multiplicative = 1f;
			foreach (StatModifier modifier in list)
			{
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
