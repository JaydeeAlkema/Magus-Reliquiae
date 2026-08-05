using System;
using UnityEngine;

namespace Relic
{
	/// <summary>
	///     Runtime instance of a relic.
	/// </summary>
	/// <remarks>
	///     Created from a <see cref="RelicSO" /> and tracked by inventory and board systems for level, placement, and cooldown
	///     state.
	/// </remarks>
	public sealed class RelicInstance
	{
		private static int _nextInstanceId;

		/// <summary>
		///     Definition this instance was created from.
		/// </summary>
		public RelicSO Definition { get; }
		/// <summary>
		///     Current relic level.
		/// </summary>
		public int Level { get; private set; }
		/// <summary>
		///     True when the relic is at its maximum level.
		/// </summary>
		public bool IsMaxLevel => Level >= Definition.MaxLevel;

		/// <summary>
		///     Unique runtime identifier.
		/// </summary>
		public int InstanceId { get; } = ++_nextInstanceId;

		/// <summary>
		///     Current board anchor position.
		/// </summary>
		public Vector2Int AnchorPosition { get; private set; }

		/// <summary>
		///     True when the relic is placed on the board.
		/// </summary>
		public bool IsPlaced { get; private set; }

		/// <summary>
		///     Cooldown remaining for the primary behavior slot.
		/// </summary>
		public float CooldownRemaining
		{
			get => BehaviorTimers[0];
			set => BehaviorTimers[0] = value;
		}

		/// <summary>
		///     Behavior timer storage.
		/// </summary>
		public float[] BehaviorTimers { get; } = new float[4];

		/// <summary>
		///     Creates a new relic instance.
		/// </summary>
		/// <param name="definition">Relic definition.</param>
		public RelicInstance(RelicSO definition)
		{
			Definition = definition ?? throw new ArgumentNullException(nameof(definition));
			Level = 1;
		}

		/// <summary>
		///     Attempts to level the relic up by one.
		/// </summary>
		/// <returns>True if the level increased.</returns>
		public bool TryLevelUp()
		{
			if (IsMaxLevel)
				return false;

			Level++;
			return true;
		}

		/// <summary>
		///     Marks the instance as placed at the given anchor.
		/// </summary>
		/// <param name="anchor">Board anchor position.</param>
		public void SetPlaced(Vector2Int anchor)
		{
			AnchorPosition = anchor;
			IsPlaced = true;
		}

		/// <summary>
		///     Clears the placed flag.
		/// </summary>
		public void ClearPlaced()
		{
			IsPlaced = false;
		}
	}
}
