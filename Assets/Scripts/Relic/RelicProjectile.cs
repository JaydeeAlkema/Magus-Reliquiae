using UnityEngine;

namespace Relic
{
	/// <summary>
	/// Runtime projectile for relic attacks.
	/// </summary>
	/// <remarks>
	/// Spawn it through the static factory; it loads the prefab, configures the sprite, and handles lifetime and hit logic.
	/// </remarks>
	public sealed class RelicProjectile : MonoBehaviour
	{
		private const float HIT_RADIUS = 0.2f;
		private const int DEFAULT_SORTING_ORDER = 10;

		private static GameObject _cachedPrefab;
		private static Sprite _cachedSprite;

		private Vector2 _direction;
		private float _speed;
		private int _damage;
		private float _remainingLifetime;
		private bool _isInitialized;

		/// <summary>
		/// Spawns a projectile instance.
		/// </summary>
		/// <param name="position">Spawn position.</param>
		/// <param name="direction">Travel direction.</param>
		/// <param name="speed">Projectile speed.</param>
		/// <param name="damage">Damage dealt on hit.</param>
		/// <param name="lifetime">Maximum lifetime.</param>
		/// <returns>The spawned projectile, or null.</returns>
		public static RelicProjectile Spawn(Vector2 position, Vector2 direction, float speed, int damage, float lifetime)
		{
			GameObject prefab = GetPrefab();
			if (!prefab)
				return null;

			GameObject projectileObject = Instantiate(prefab, position, Quaternion.identity);
			RelicProjectile projectile = projectileObject.GetComponent<RelicProjectile>();
			if (!projectile)
			{
				Debug.LogError("[RelicProjectile] Prefab is missing RelicProjectile component.");
				Destroy(projectileObject);
				return null;
			}

			SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				spriteRenderer.sprite = GetCachedSprite();
				spriteRenderer.color = Color.yellow;
				spriteRenderer.sortingOrder = DEFAULT_SORTING_ORDER;
			}

			projectile.Setup(direction, speed, damage, lifetime);
			return projectile;
		}

		private static GameObject GetPrefab()
		{
			if (_cachedPrefab)
				return _cachedPrefab;

			_cachedPrefab = Resources.Load<GameObject>("Projectiles/RelicProjectile");
			if (!_cachedPrefab)
				Debug.LogError("[RelicProjectile] Missing prefab at Resources/Projectiles/RelicProjectile.prefab.");

			return _cachedPrefab;
		}

		private void Setup(Vector2 direction, float speed, int damage, float lifetime)
		{
			_direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;
			_speed = Mathf.Max(0f, speed);
			_damage = Mathf.Max(1, damage);
			_remainingLifetime = Mathf.Max(0.01f, lifetime);
			_isInitialized = true;
		}

		private void Update()
		{
			if (!_isInitialized)
				return;

			float deltaTime = Time.deltaTime;
			_remainingLifetime -= deltaTime;
			if (_remainingLifetime <= 0f)
			{
				Destroy(this.gameObject);
				return;
			}

			Vector3 position = this.transform.position;
			position += (Vector3)(_direction * (_speed * deltaTime));
			this.transform.position = position;
			if (TryHitEnemy())
				Destroy(this.gameObject);
		}

		private bool TryHitEnemy()
		{
			if (Enemy.Enemy.Registry.Active.Count == 0)
				return false;

			Vector2 projectilePosition = this.transform.position;
			const float hitRadiusSqr = HIT_RADIUS * HIT_RADIUS;
			foreach (Enemy.Enemy enemy in Enemy.Enemy.Registry.Active)
			{
				if (!enemy || !enemy.isActiveAndEnabled)
					continue;

				Vector2 enemyPosition = enemy.Position;
				if ((enemyPosition - projectilePosition).sqrMagnitude > hitRadiusSqr)
					continue;

				enemy.TakeDamage(_damage);
				return true;
			}

			return false;
		}

		private static Sprite GetCachedSprite()
		{
			if (_cachedSprite)
				return _cachedSprite;

			Texture2D texture = Texture2D.whiteTexture;
			Rect rect = new(0f, 0f, texture.width, texture.height);
			_cachedSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), texture.width);
			return _cachedSprite;
		}
	}
}
