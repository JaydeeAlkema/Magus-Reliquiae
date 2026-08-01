using System;

namespace Waves
{
	[Serializable]
	public struct WaveEnemy
	{
		public Enemy.Enemy EnemyPrefab;
		public int EnemySpawnChance;
	}
}
