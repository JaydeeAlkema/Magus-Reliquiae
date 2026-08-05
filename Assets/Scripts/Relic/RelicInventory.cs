using System;
using System.Collections.Generic;
using PlayerStats;

namespace Relic
{
	/// <summary>
	///     Runtime collection of owned relics.
	/// </summary>
	/// <remarks>
	///     Managed by <see cref="PlayerRelicManager" />; keep it as gameplay data, not a scene component.
	/// </remarks>
	public class RelicInventory
	{
		private readonly Dictionary<string, RelicInstance> _ownedById = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, IRelicBehavior> _behaviorById = new(StringComparer.OrdinalIgnoreCase);
		private readonly RelicRuntimeContext _runtimeContext;

		public RelicInventory(PlayerStatsModel playerStats)
		{
			_runtimeContext = new RelicRuntimeContext(playerStats);
		}

		/// <summary>
		///     Owned relic instances.
		/// </summary>
		public IReadOnlyCollection<RelicInstance> OwnedRelics => _ownedById.Values;

		/// <summary>
		///     Acquires a relic into the inventory.
		/// </summary>
		/// <param name="relic">Relic definition.</param>
		/// <param name="behavior">Optional behavior override.</param>
		/// <returns>True when the relic was acquired.</returns>
		public bool TryAcquire(RelicSO relic, IRelicBehavior behavior = null)
		{
			if (relic == null || string.IsNullOrWhiteSpace(relic.Id) || _ownedById.ContainsKey(relic.Id))
				return false;

			behavior ??= relic.CreateBehavior();
			RelicInstance instance = new(relic);
			_ownedById.Add(relic.Id, instance);
			if (behavior != null)
				_behaviorById[relic.Id] = behavior;

			ApplyLevelModifiers(instance, instance.Level);
			behavior?.OnAcquired(instance, _runtimeContext);
			return true;
		}

		/// <summary>
		///     Upgrades an owned relic by ID.
		/// </summary>
		/// <param name="relicId">Relic identifier.</param>
		/// <returns>True when the upgrade succeeded.</returns>
		public bool TryUpgrade(string relicId)
		{
			if (!_ownedById.TryGetValue(relicId, out RelicInstance instance))
				return false;

			int previousLevel = instance.Level;
			if (!instance.TryLevelUp())
				return false;

			RemoveLevelModifiers(instance, previousLevel);
			ApplyLevelModifiers(instance, instance.Level);

			if (_behaviorById.TryGetValue(relicId, out IRelicBehavior behavior))
				behavior.OnLevelChanged(instance, previousLevel, _runtimeContext);

			return true;
		}

		/// <summary>
		///     Removes an owned relic by ID.
		/// </summary>
		/// <param name="relicId">Relic identifier.</param>
		/// <returns>True when the relic was removed.</returns>
		public bool TryRemove(string relicId)
		{
			if (!_ownedById.TryGetValue(relicId, out RelicInstance instance))
				return false;

			RemoveLevelModifiers(instance, instance.Level);

			if (!_behaviorById.TryGetValue(relicId, out IRelicBehavior behavior))
				return _ownedById.Remove(relicId);

			behavior.OnRemoved(instance, _runtimeContext);
			_behaviorById.Remove(relicId);

			return _ownedById.Remove(relicId);
		}

		/// <summary>
		///     Ticks all active relic behaviors.
		/// </summary>
		/// <param name="deltaTime">Frame delta time.</param>
		public void Tick(float deltaTime)
		{
			foreach (KeyValuePair<string, IRelicBehavior> kvp in _behaviorById)
			{
				if (!_ownedById.TryGetValue(kvp.Key, out RelicInstance instance))
					continue;

				kvp.Value.OnTick(instance, deltaTime, _runtimeContext);
			}
		}

		/// <summary>
		///     Publishes a trigger to all active relic behaviors.
		/// </summary>
		/// <param name="trigger">Trigger payload.</param>
		public void Publish(RelicTrigger trigger)
		{
			foreach (KeyValuePair<string, IRelicBehavior> kvp in _behaviorById)
			{
				if (!_ownedById.TryGetValue(kvp.Key, out RelicInstance instance))
					continue;

				kvp.Value.OnTrigger(instance, trigger, _runtimeContext);
			}
		}

		private void ApplyLevelModifiers(RelicInstance instance, int level)
		{
			RelicLevelData levelData = instance.Definition.GetLevelData(level);
			if (levelData == null)
				return;

			string sourceId = BuildModifierSource(instance.Definition.Id, level);
			foreach (RelicStatModifier modifier in levelData.StatModifiers)
			{
				_runtimeContext.PlayerStats.AddModifier(new StatModifier(modifier.Stat, modifier.Operation, modifier.Value, sourceId));
			}
		}

		private void RemoveLevelModifiers(RelicInstance instance, int level)
		{
			string sourceId = BuildModifierSource(instance.Definition.Id, level);
			_runtimeContext.PlayerStats.RemoveModifiersBySource(sourceId);
		}

		private static string BuildModifierSource(string relicId, int level)
		{
			return $"{relicId}@L{level}";
		}
	}
}
