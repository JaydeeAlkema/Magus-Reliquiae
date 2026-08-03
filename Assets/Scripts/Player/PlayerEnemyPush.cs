using Enemy;
using UnityEngine;

namespace Player
{
	public class PlayerEnemyPush
	{
		private float _pushRadius;
		private float _maxEnemyPushSpeed;

		public void Setup(float pushRadius, float maxEnemyPushSpeed)
		{
			_pushRadius = pushRadius;
			_maxEnemyPushSpeed = maxEnemyPushSpeed;
		}

		public void SetTuning(float pushRadius, float maxEnemyPushSpeed)
		{
			_pushRadius = pushRadius;
			_maxEnemyPushSpeed = maxEnemyPushSpeed;
		}

		public Vector2 Compute(Vector2 playerPos)
		{
			Vector2 push = Vector2.zero;

			foreach (EnemyContact enemy in EnemyPushRegistry.Active)
			{
				Vector2 toPlayer = playerPos - (Vector2)enemy.Position;
				float combinedRadius = _pushRadius + enemy.Radius;
				float sqrDist = toPlayer.sqrMagnitude;

				if (sqrDist >= combinedRadius * combinedRadius || sqrDist < 1e-6f)
					continue;

				float dist = Mathf.Sqrt(sqrDist);
				float overlap = combinedRadius - dist;
				push += toPlayer / dist * overlap;
			}

			float maxStep = _maxEnemyPushSpeed * Time.fixedDeltaTime;
			if (push.sqrMagnitude > maxStep * maxStep)
				push = push.normalized * maxStep;

			return push;
		}
	}
}
