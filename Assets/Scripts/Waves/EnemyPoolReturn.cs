using UnityEngine;
using UnityEngine.Pool;

namespace Waves
{
	public class EnemyPoolReturn : MonoBehaviour
	{
		public bool IsReleasingToPool { get; set; }

		private Enemy.Enemy _enemy;
		private IObjectPool<Enemy.Enemy> _pool;

		public void Setup(Enemy.Enemy enemy, IObjectPool<Enemy.Enemy> pool)
		{
			_enemy = enemy;
			_pool = pool;
		}

		private void OnDisable()
		{
			if (IsReleasingToPool)
				return;

			if (!_enemy || _pool == null || !this.gameObject.scene.IsValid())
				return;

			_pool.Release(_enemy);
		}
	}
}
