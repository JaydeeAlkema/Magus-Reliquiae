using System;

namespace Player
{
	public sealed class PlayerXpService
	{
		private readonly float[] _levelThresholds;

		public event Action<int> onLevelUp;

		public float CurrentXP { get; private set; }
		public int CurrentLevel { get; private set; }
		public bool IsMaxLevel => CurrentLevel - 1 >= _levelThresholds.Length;

		public float NextLevelThreshold => IsMaxLevel
			? float.MaxValue
			: _levelThresholds[CurrentLevel - 1];

		public float XpToNextLevel => IsMaxLevel ? 0f : NextLevelThreshold - CurrentXP;

		public float LevelProgress
		{
			get
			{
				if (IsMaxLevel) return 1f;
				float prevThreshold = CurrentLevel >= 2 ? _levelThresholds[CurrentLevel - 2] : 0f;
				float nextThreshold = _levelThresholds[CurrentLevel - 1];
				float span = nextThreshold - prevThreshold;
				return span > 0f ? (CurrentXP - prevThreshold) / span : 0f;
			}
		}

		public PlayerXpService(float[] levelThresholds)
		{
			_levelThresholds = levelThresholds ?? throw new ArgumentNullException(nameof(levelThresholds));
			CurrentLevel = 1;
			CurrentXP = 0f;
		}

		public void AddXp(float amount)
		{
			if (amount <= 0f || IsMaxLevel) return;
			CurrentXP += amount;
			CheckLevelUps();
		}

		private void CheckLevelUps()
		{
			while (!IsMaxLevel && CurrentXP >= _levelThresholds[CurrentLevel - 1])
			{
				CurrentLevel++;
				onLevelUp?.Invoke(CurrentLevel);
			}
		}
	}
}
