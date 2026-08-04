namespace Relic
{
	/// <summary>
	/// Types of relic trigger events.
	/// </summary>
	public enum RelicTriggerEvent
	{
		/// <summary>
		/// Fired when an enemy is killed.
		/// </summary>
		EnemyKilled = 0,
		/// <summary>
		/// Fired when the player takes damage.
		/// </summary>
		PlayerHit = 1,
		/// <summary>
		/// Fired when a projectile impacts.
		/// </summary>
		ProjectileImpact = 2,
		/// <summary>
		/// Fired when the player levels up.
		/// </summary>
		LevelUp = 3,
	}
}
