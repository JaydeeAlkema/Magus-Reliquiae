using System;
using System.Collections.Generic;
using PlayerStats;
using UnityEngine;

namespace Relic
{
	public enum MergeResult
	{
		Success = 0,
		SuccessButShapeConflict = 1,
		InvalidInput = 2,
		BagRelicNotInBag = 3,
		BoardRelicNotPlaced = 4,
		DifferentDefinitions = 5,
		AlreadyMaxLevel = 6,
	}

	public sealed class PlayerRelicManager
	{
		private readonly RelicRuntimeContext _runtimeContext;
		private readonly Dictionary<RelicInstance, IRelicBehavior> _behaviors = new();

		private readonly List<RelicInstance> _tickBuffer = new();

		public RelicBag Bag { get; }
		public RelicBoard Board { get; }

		public bool IsInteractionLocked { get; set; } = true;

		public event Action<RelicInstance> onRelicAcquired;
		public event Action<RelicInstance> onRelicPlaced;
		public event Action<RelicInstance> onRelicUnequipped;
		public event Action<RelicInstance, int> onRelicMerged;

		public PlayerRelicManager(PlayerStatsModel playerStats, int boardColumns, int boardRows)
		{
			Bag = new RelicBag();
			Board = new RelicBoard(boardColumns, boardRows);
			_runtimeContext = new RelicRuntimeContext(playerStats);
		}

		public void AcquireToBag(RelicSO definition, IRelicBehavior behavior = null)
		{
			if (definition == null) throw new ArgumentNullException(nameof(definition));

			RelicInstance instance = new(definition);
			if (behavior != null)
				_behaviors[instance] = behavior;

			Bag.Add(instance);
			behavior?.OnAcquired(instance, _runtimeContext);
			onRelicAcquired?.Invoke(instance);
		}

		public bool PlaceOnBoard(RelicInstance instance, Vector2Int anchor)
		{
			if (IsInteractionLocked) return false;
			if (instance == null || instance.IsPlaced) return false;
			if (!Bag.Contains(instance)) return false;

			RelicShape shape = instance.Definition.GetShape(instance.Level);
			if (!Board.TryPlace(instance, shape, anchor)) return false;

			Bag.Remove(instance);
			ApplyModifiers(instance);
			onRelicPlaced?.Invoke(instance);
			return true;
		}

		public bool RemoveFromBoard(RelicInstance instance)
		{
			if (IsInteractionLocked) return false;
			if (instance == null || !instance.IsPlaced) return false;

			RemoveModifiers(instance);

			if (_behaviors.TryGetValue(instance, out IRelicBehavior behavior))
				behavior.OnRemoved(instance, _runtimeContext);

			Board.Remove(instance);
			Bag.Add(instance);
			onRelicUnequipped?.Invoke(instance);
			return true;
		}

		public MergeResult TryMergeOnBoard(RelicInstance bagRelic, RelicInstance boardRelic)
		{
			if (IsInteractionLocked) return MergeResult.InvalidInput;
			if (bagRelic == null || boardRelic == null) return MergeResult.InvalidInput;
			if (!Bag.Contains(bagRelic)) return MergeResult.BagRelicNotInBag;
			if (!boardRelic.IsPlaced) return MergeResult.BoardRelicNotPlaced;
			if (bagRelic.Definition != boardRelic.Definition) return MergeResult.DifferentDefinitions;
			if (boardRelic.IsMaxLevel) return MergeResult.AlreadyMaxLevel;

			RemoveModifiers(boardRelic);
			int previousLevel = boardRelic.Level;
			boardRelic.TryLevelUp();

			RelicShape newShape = boardRelic.Definition.GetShape(boardRelic.Level);
			Vector2Int anchor = boardRelic.AnchorPosition;

			bool shapeConflict = !Board.CanPlaceIgnoring(newShape, anchor, boardRelic);

			Board.Remove(boardRelic); // clears cells and calls ClearPlaced()
			Bag.Remove(bagRelic); // consume the bag relic

			if (shapeConflict)
			{
				Bag.Add(boardRelic);
			}
			else
			{
				Board.TryPlace(boardRelic, newShape, anchor);
				ApplyModifiers(boardRelic);
			}

			if (_behaviors.TryGetValue(boardRelic, out IRelicBehavior behavior))
				behavior.OnLevelChanged(boardRelic, previousLevel, _runtimeContext);

			onRelicMerged?.Invoke(boardRelic, previousLevel);
			return shapeConflict ? MergeResult.SuccessButShapeConflict : MergeResult.Success;
		}


		public void Tick(float deltaTime)
		{
			_tickBuffer.Clear();
			_tickBuffer.AddRange(Board.PlacedRelics);
			foreach (RelicInstance instance in _tickBuffer)
			{
				if (_behaviors.TryGetValue(instance, out IRelicBehavior behavior))
					behavior.OnTick(instance, deltaTime, _runtimeContext);
			}
		}

		public void Publish(RelicTrigger trigger)
		{
			_tickBuffer.Clear();
			_tickBuffer.AddRange(Board.PlacedRelics);
			foreach (RelicInstance instance in _tickBuffer)
			{
				if (_behaviors.TryGetValue(instance, out IRelicBehavior behavior))
					behavior.OnTrigger(instance, trigger, _runtimeContext);
			}
		}

		public void UnlockBoardSize(int newColumns, int newRows)
		{
			Board.Resize(newColumns, newRows);
		}

		private void ApplyModifiers(RelicInstance instance)
		{
			RelicLevelData levelData = instance.Definition.GetLevelData(instance.Level);
			if (levelData == null) return;

			string sourceId = BuildModifierSource(instance);
			foreach (RelicStatModifier modifier in levelData.StatModifiers)
			{
				_runtimeContext.PlayerStats.AddModifier(
					new StatModifier(modifier.Stat, modifier.Operation, modifier.Value, sourceId));
			}
		}

		private void RemoveModifiers(RelicInstance instance)
		{
			_runtimeContext.PlayerStats.RemoveModifiersBySource(BuildModifierSource(instance));
		}

		private static string BuildModifierSource(RelicInstance instance)
		{
			return $"{instance.Definition.Id}@{instance.InstanceId}@L{instance.Level}";
		}
	}
}
