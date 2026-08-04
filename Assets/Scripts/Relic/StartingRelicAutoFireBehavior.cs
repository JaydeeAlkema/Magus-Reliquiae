using UnityEngine;

namespace Relic
{
	/// <summary>
	/// Auto-fire behavior for the starting relic.
	/// </summary>
	[CreateAssetMenu(fileName = "StartingRelicAutoFireBehavior", menuName = "ScriptableObjects/Relic/Behaviors/Starting Relic Auto Fire", order = 0)]
	public sealed class StartingRelicAutoFireBehavior : RelicBehaviorSO
	{
		/// <summary>
		/// Fire cooldown in seconds.
		/// </summary>
		[SerializeField][Min(0f)] private float CooldownSeconds = 2f;
		/// <summary>
		/// Projectile damage.
		/// </summary>
		[SerializeField][Min(1)] private int ProjectileDamage = 1;
		/// <summary>
		/// Projectile speed.
		/// </summary>
		[SerializeField][Min(0.1f)] private float ProjectileSpeed = 8f;
		/// <summary>
		/// Projectile lifetime.
		/// </summary>
		[SerializeField][Min(0.1f)] private float ProjectileLifetime = 3f;
		/// <summary>
		/// Spawn offset from the player.
		/// </summary>
		[SerializeField][Min(0f)] private float ProjectileSpawnOffset = 0.35f;

		/// <summary>
		/// Creates the runtime behavior.
		/// </summary>
		/// <returns>A behavior instance.</returns>
		public override IRelicBehavior CreateBehavior()
		{
			return new RuntimeBehavior(CooldownSeconds, ProjectileDamage, ProjectileSpeed, ProjectileLifetime, ProjectileSpawnOffset);
		}

		private sealed class RuntimeBehavior : IRelicBehavior
		{
			private readonly float _cooldownSeconds;
			private readonly int _projectileDamage;
			private readonly float _projectileSpeed;
			private readonly float _projectileLifetime;
			private readonly float _projectileSpawnOffset;

			public RuntimeBehavior(float cooldownSeconds, int projectileDamage, float projectileSpeed, float projectileLifetime, float projectileSpawnOffset)
			{
				_cooldownSeconds = cooldownSeconds;
				_projectileDamage = projectileDamage;
				_projectileSpeed = projectileSpeed;
				_projectileLifetime = projectileLifetime;
				_projectileSpawnOffset = projectileSpawnOffset;
			}

			public void OnAcquired(RelicInstance instance, RelicRuntimeContext context)
			{
				instance.CooldownRemaining = _cooldownSeconds;
			}

			public void OnRemoved(RelicInstance instance, RelicRuntimeContext context) { }

			public void OnLevelChanged(RelicInstance instance, int previousLevel, RelicRuntimeContext context)
			{
				instance.CooldownRemaining = _cooldownSeconds;
			}

			public void OnTick(RelicInstance instance, float deltaTime, RelicRuntimeContext context)
			{
				if (deltaTime <= 0f)
					return;

				instance.CooldownRemaining -= deltaTime;
				if (instance.CooldownRemaining > 0f)
					return;

				FireNearestEnemy(context);
				instance.CooldownRemaining = _cooldownSeconds;
			}

			public void OnTrigger(RelicInstance instance, RelicTrigger trigger, RelicRuntimeContext context) { }

			private void FireNearestEnemy(RelicRuntimeContext context)
			{
				Enemy.Enemy nearest = FindNearestEnemy(context);
				if (!nearest)
					return;

				Vector2 origin = Vector2.zero;
				if (context.Owner)
					origin = context.Owner.transform.position;
				Vector2 targetDirection = nearest.Position - origin;
				if (targetDirection.sqrMagnitude <= 1e-6f)
					targetDirection = Vector2.right;

				Vector2 spawnPosition = origin + targetDirection.normalized * _projectileSpawnOffset;
				RelicProjectile.Spawn(
					spawnPosition,
					targetDirection,
					_projectileSpeed,
					_projectileDamage,
					_projectileLifetime);
			}

			private static Enemy.Enemy FindNearestEnemy(RelicRuntimeContext context)
			{
				if (Enemy.Enemy.Registry.Active.Count == 0)
					return null;

				Vector2 origin = Vector2.zero;
				if (context.Owner)
					origin = context.Owner.transform.position;

				Enemy.Enemy nearest = null;
				float nearestDistanceSqr = float.MaxValue;
				foreach (Enemy.Enemy enemy in Enemy.Enemy.Registry.Active)
				{
					if (!enemy || !enemy.isActiveAndEnabled)
						continue;

					Vector2 enemyPosition = enemy.Position;
					float distanceSqr = (enemyPosition - origin).sqrMagnitude;
					if (distanceSqr >= nearestDistanceSqr)
						continue;

					nearestDistanceSqr = distanceSqr;
					nearest = enemy;
				}

				return nearest;
			}
		}
	}
}
