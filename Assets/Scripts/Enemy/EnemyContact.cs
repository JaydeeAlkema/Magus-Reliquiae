using UnityEngine;

namespace Enemy
{
	public class EnemyContact
	{
		public Vector3 Position => _transform.position;
		public float Radius { get; private set; }

		private Transform _transform;

		public void Register()
		{
			EnemyPushRegistry.Register(this);
		}

		public void Unregister()
		{
			EnemyPushRegistry.Unregister(this);
		}

		public void Setup(Enemy enemy, EnemyConfigSO config)
		{
			_transform = enemy.transform;
			Radius = config.CollisionRadius;
		}

		public void Push(Vector2 offset)
		{
			_transform.position += (Vector3)offset;
		}
	}
}
