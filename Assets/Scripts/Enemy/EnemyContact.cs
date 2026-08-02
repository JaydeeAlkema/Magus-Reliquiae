using UnityEngine;

namespace Enemy
{
	public class EnemyContact
	{
		public Vector3 Position => _transform.position;
		public float Radius { get; private set; }

		private bool _isRegistered;
		private Transform _transform;

		public void Register()
		{
			if (_isRegistered)
				return;

			_isRegistered = true;
			EnemyPushRegistry.Register(this);
		}

		public void Unregister()
		{
			if (!_isRegistered)
				return;

			_isRegistered = false;
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
