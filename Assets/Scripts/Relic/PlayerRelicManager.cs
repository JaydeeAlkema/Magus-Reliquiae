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

	/// <summary>
	///     Owns the player's relic inventory and board model.
	/// </summary>
	/// <remarks>
	///     Construct it from <see cref="global::Player.Player" /> and use it for acquire, place, merge, and removal flows.
	/// </remarks>
	public sealed class PlayerRelicManager
	{
		private readonly RelicRuntimeContext _runtimeContext;
		private readonly Dictionary<RelicInstance, IRelicBehavior> _behaviors = new();

		private readonly List<RelicInstance> _tickBuffer = new();

		/// <summary>
		///     Inventory bag for unequipped relics.
		/// </summary>
		public RelicBag Bag { get; }
		/// <summary>
		///     Board used for equipped relics.
		/// </summary>
		public RelicBoard Board { get; }

		/// <summary>
		///     Blocks interaction when true.
		/// </summary>
		public bool IsInteractionLocked { get; set; } = true;

		/// <summary>
		///     Fired when a relic is acquired.
		/// </summary>
		public event Action<RelicInstance> onRelicAcquired;
		/// <summary>
		///     Fired when a relic is placed on the board.
		/// </summary>
		public event Action<RelicInstance> onRelicPlaced;
		/// <summary>
		///     Fired when a relic is removed from the board.
		/// </summary>
		public event Action<RelicInstance> onRelicUnequipped;
		/// <summary>
		///     Fired when a merge completes.
		/// </summary>
		public event Action<RelicInstance, int> onRelicMerged;

		/// <summary>
		///     Creates the player relic manager.
		/// </summary>
		/// <param name="playerStats">Player stats model.</param>
		/// <param name="boardColumns">Board width.</param>
		/// <param name="boardRows">Board height.</param>
		/// <param name="owner">Optional owning player.</param>
		public PlayerRelicManager(PlayerStatsModel playerStats, int boardColumns, int boardRows, Player.Player owner = null)
		{
			Bag = new RelicBag();
			Board = new RelicBoard(boardColumns, boardRows);
			_runtimeContext = new RelicRuntimeContext(playerStats, owner);
		}

		/// <summary>
		///     Acquires a relic into the bag.
		/// </summary>
		/// <param name="definition">Relic definition.</param>
		/// <param name="behavior">Optional behavior override.</param>
		/// <returns>The acquired instance.</returns>
		public RelicInstance AcquireToBag(RelicSO definition, IRelicBehavior behavior = null)
		{
			if (definition == null) throw new ArgumentNullException(nameof(definition));

			behavior ??= definition.CreateBehavior();
			RelicInstance instance = new(definition);
			if (behavior != null)
				_behaviors[instance] = behavior;

			Bag.Add(instance);
			behavior?.OnAcquired(instance, _runtimeContext);
			onRelicAcquired?.Invoke(instance);
			return instance;
		}

		/// <summary>
		///     Places a bag relic onto the board.
		/// </summary>
		/// <param name="instance">Relic instance to place.</param>
		/// <param name="anchor">Board anchor.</param>
		/// <returns>True when placement succeeded.</returns>
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

		/// <summary>
		///     Removes a placed relic back to the bag.
		/// </summary>
		/// <param name="instance">Relic instance to remove.</param>
		/// <returns>True when removal succeeded.</returns>
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

		/// <summary>
		///     Attempts to merge a bag relic into a board relic.
		/// </summary>
		/// <param name="bagRelic">Relic being consumed from the bag.</param>
		/// <param name="boardRelic">Relic already on the board.</param>
		/// <returns>The merge result.</returns>
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


		/// <summary>
		///     Ticks all board relic behaviors.
		/// </summary>
		/// <param name="deltaTime">Frame delta time.</param>
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

		/// <summary>
		///     Publishes a relic trigger to all active board relics.
		/// </summary>
		/// <param name="trigger">Trigger payload.</param>
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

		/// <summary>
		///     Expands the board size.
		/// </summary>
		/// <param name="newColumns">New width.</param>
		/// <param name="newRows">New height.</param>
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
