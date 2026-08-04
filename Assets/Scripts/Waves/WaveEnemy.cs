using System;

namespace Waves
{
	/// <summary>
	/// Wave entry pairing an enemy prefab with its spawn chance.
	/// </summary>
	[Serializable]
	public struct WaveEnemy
	{
		/// <summary>
		/// Enemy prefab spawned by this entry.
		/// </summary>
		public Enemy.Enemy EnemyPrefab;
		/// <summary>
		/// Spawn chance in the 0-100 range.
		/// </summary>
		public int EnemySpawnChance;
	}
}
