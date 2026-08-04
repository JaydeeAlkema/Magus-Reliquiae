using System;

namespace Player
{
	/// <summary>
	/// XP progression helper owned by <see cref="Player"/>.
	/// </summary>
	/// <remarks>
	/// Construct it from the player prefab and use it to add XP, level up, and query thresholds.
	/// </remarks>
	public sealed class PlayerXpService
	{
		private readonly float[] _levelThresholds;

		/// <summary>
		/// Fired after the player levels up.
		/// </summary>
		public event Action<int> onLevelUp;

		/// <summary>
		/// Current accumulated XP.
		/// </summary>
		public float CurrentXP { get; private set; }
		/// <summary>
		/// Current level, starting at 1.
		/// </summary>
		public int CurrentLevel { get; private set; }
		/// <summary>
		/// True when all thresholds are cleared.
		/// </summary>
		public bool IsMaxLevel => CurrentLevel - 1 >= _levelThresholds.Length;

		/// <summary>
		/// XP threshold for the next level.
		/// </summary>
		public float NextLevelThreshold => IsMaxLevel
			? float.MaxValue
			: _levelThresholds[CurrentLevel - 1];

		/// <summary>
		/// Remaining XP until the next level.
		/// </summary>
		public float XpToNextLevel => IsMaxLevel ? 0f : NextLevelThreshold - CurrentXP;

		/// <summary>
		/// Normalized progress through the current level.
		/// </summary>
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

		/// <summary>
		/// Creates the progression service.
		/// </summary>
		/// <param name="levelThresholds">Ascending XP thresholds for levels beyond 1.</param>
		public PlayerXpService(float[] levelThresholds)
		{
			_levelThresholds = levelThresholds ?? throw new ArgumentNullException(nameof(levelThresholds));
			CurrentLevel = 1;
			CurrentXP = 0f;
		}

		/// <summary>
		/// Adds XP and applies any resulting level-ups.
		/// </summary>
		/// <param name="amount">XP amount to add.</param>
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
