namespace PlayerStats
{
	/// <summary>
	/// Supported player stat identifiers.
	/// </summary>
	public enum PlayerStatType
	{
		/// <summary>
		/// Walking speed.
		/// </summary>
		MoveSpeed = 0,
		/// <summary>
		/// Push interaction radius.
		/// </summary>
		PushRadius = 1,
		/// <summary>
		/// Maximum push speed against enemies.
		/// </summary>
		MaxEnemyPushSpeed = 2,
		/// <summary>
		/// Maximum health.
		/// </summary>
		MaxHealth = 3,
		/// <summary>
		/// Damage output.
		/// </summary>
		Damage = 4,
	}
}
