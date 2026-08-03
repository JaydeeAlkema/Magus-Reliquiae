using System;
using System.Collections.Generic;
using PlayerStats;

namespace Relic
{
	public class RelicInventory
	{
		private readonly Dictionary<string, RelicInstance> _ownedById = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, IRelicBehavior> _behaviorById = new(StringComparer.OrdinalIgnoreCase);
		private readonly RelicRuntimeContext _runtimeContext;

		public RelicInventory(PlayerStatsModel playerStats)
		{
			_runtimeContext = new RelicRuntimeContext(playerStats);
		}

		public IReadOnlyCollection<RelicInstance> OwnedRelics => _ownedById.Values;

		public bool TryAcquire(RelicSO relic, IRelicBehavior behavior = null)
		{
			if (relic == null || string.IsNullOrWhiteSpace(relic.Id) || _ownedById.ContainsKey(relic.Id))
				return false;

			RelicInstance instance = new(relic);
			_ownedById.Add(relic.Id, instance);
			if (behavior != null)
				_behaviorById[relic.Id] = behavior;

			ApplyLevelModifiers(instance, instance.Level);
			behavior?.OnAcquired(instance, _runtimeContext);
			return true;
		}

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

		public bool TryRemove(string relicId)
		{
			if (!_ownedById.TryGetValue(relicId, out RelicInstance instance))
				return false;

			RemoveLevelModifiers(instance, instance.Level);

			if (_behaviorById.TryGetValue(relicId, out IRelicBehavior behavior))
			{
				behavior.OnRemoved(instance, _runtimeContext);
				_behaviorById.Remove(relicId);
			}

			return _ownedById.Remove(relicId);
		}

		public void Tick(float deltaTime)
		{
			foreach (KeyValuePair<string, IRelicBehavior> kvp in _behaviorById)
			{
				if (_ownedById.TryGetValue(kvp.Key, out RelicInstance instance))
					kvp.Value.OnTick(instance, deltaTime, _runtimeContext);
			}
		}

		public void Publish(RelicTrigger trigger)
		{
			foreach (KeyValuePair<string, IRelicBehavior> kvp in _behaviorById)
			{
				if (_ownedById.TryGetValue(kvp.Key, out RelicInstance instance))
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
				_runtimeContext.PlayerStats.AddModifier(
					new StatModifier(modifier.Stat, modifier.Operation, modifier.Value, sourceId));
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
