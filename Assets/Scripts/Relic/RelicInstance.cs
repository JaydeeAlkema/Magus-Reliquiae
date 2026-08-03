using System;
using UnityEngine;

namespace Relic
{
	public sealed class RelicInstance
	{
		private static int _nextInstanceId;

		public RelicSO Definition { get; }
		public int Level { get; private set; }
		public bool IsMaxLevel => Level >= Definition.MaxLevel;

		public int InstanceId { get; } = ++_nextInstanceId;

		public Vector2Int AnchorPosition { get; private set; }

		public bool IsPlaced { get; private set; }

		public float CooldownRemaining
		{
			get => BehaviorTimers[0];
			set => BehaviorTimers[0] = value;
		}

		public float[] BehaviorTimers { get; } = new float[4];

		public RelicInstance(RelicSO definition)
		{
			Definition = definition ?? throw new ArgumentNullException(nameof(definition));
			Level = 1;
		}

		public bool TryLevelUp()
		{
			if (IsMaxLevel)
				return false;

			Level++;
			return true;
		}

		public void SetPlaced(Vector2Int anchor)
		{
			AnchorPosition = anchor;
			IsPlaced = true;
		}

		public void ClearPlaced()
		{
			IsPlaced = false;
		}
	}
}
