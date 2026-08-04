using System;
using UnityEngine;
using Waves;

namespace Enemy
{
	/// <summary>
	/// Runtime enemy controller.
	/// </summary>
	/// <remarks>
	/// Put this on the enemy prefab, assign an <see cref="EnemyConfigSO"/>, and let it manage movement,
	/// contact registration, XP drops, and enable/disable notifications.
	/// </remarks>
	public class Enemy : MonoBehaviour
	{
		/// <summary>
		/// Fired when an enemy becomes active.
		/// </summary>
		public static event Action<Enemy> enemyEnabled;
		/// <summary>
		/// Fired when an enemy is disabled.
		/// </summary>
		public static event Action<Enemy> enemyDisabled;

		private static Player.Player _cachedPlayer;

		[Header("Configuration")]
		[SerializeField] private EnemyConfigSO Config;

		private EnemyMovement _movement;
		private EnemyContact _contact;
		private int _currentHealth;

		private bool _hasAssignedSpawnAreaType;
		private Player.Player _player;
		private WaveSpawnAreaType _spawnAreaType;

		public Vector2 Position => this.transform.position;
		public int CurrentHealth => _currentHealth;
		public int MaxHealth => Config ? Mathf.Max(1, Config.Health) : 1;
		public bool IsSimulationActive { get; private set; }

		/// <summary>
		/// Initializes movement/contact helpers and registers the enemy with runtime systems.
		/// </summary>
		private void OnEnable()
		{
			_player = _cachedPlayer;
			if (!_player)
			{
				_player = UnityEngine.Object.FindAnyObjectByType<Player.Player>();
				_cachedPlayer = _player;
			}

			_movement ??= new EnemyMovement();
			_movement.Setup(this, Config);

			_contact ??= new EnemyContact();
			_contact.Setup(this, Config);

			_currentHealth = MaxHealth;

			Registry.Register(this);
			SetSimulationActive(true);
			enemyEnabled?.Invoke(this);

			if (!_player)
				return;

			_movement.SetTargetPos(_player.transform.position);
		}

		/// <summary>
		/// Unregisters the enemy from runtime systems.
		/// </summary>
		private void OnDisable()
		{
			enemyDisabled?.Invoke(this);
			SetSimulationActive(false);
			Registry.Unregister(this);
		}

		/// <summary>
		/// Refreshes the cached target position while the enemy is active.
		/// </summary>
		private void FixedUpdate()
		{
			if (!IsSimulationActive || !_player)
				return;

			_movement.SetTargetPos(_player.transform.position);
		}

		/// <summary>
		/// Advances movement while the simulation is active.
		/// </summary>
		private void Update()
		{
			if (!IsSimulationActive)
				return;

			_movement.MoveTowardsTarget();
		}

		/// <summary>
		/// Enables or disables enemy simulation and contact registration.
		/// </summary>
		/// <param name="isActive">True to simulate; false to sleep.</param>
		public void SetSimulationActive(bool isActive)
		{
			if (IsSimulationActive == isActive)
				return;

			IsSimulationActive = isActive;
			if (IsSimulationActive)
			{
				_contact.Register();
				if (_player)
					_movement.SetTargetPos(_player.transform.position);

				return;
			}

			_contact.Unregister();
		}

		/// <summary>
		/// Moves the enemy to a new world position while preserving Z.
		/// </summary>
		/// <param name="position">New world-space position.</param>
		public void Teleport(Vector2 position)
		{
			Vector3 current = this.transform.position;
			this.transform.position = new Vector3(position.x, position.y, current.z);
		}

		/// <summary>
		/// Disables the enemy GameObject.
		/// </summary>
		public void Kill()
		{
			if (!this.gameObject.activeSelf)
				return;

			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// Applies damage and kills the enemy when health reaches zero.
		/// </summary>
		/// <param name="amount">Damage amount.</param>
		/// <returns>True when the enemy died.</returns>
		public bool TakeDamage(int amount)
		{
			if (amount <= 0 || !this.gameObject.activeSelf)
				return false;

			_currentHealth = Mathf.Max(0, _currentHealth - amount);
			if (_currentHealth > 0)
				return false;

			DropXpOrb();
			Kill();
			return true;
		}

		/// <summary>
		/// Spawns the configured XP orb prefab when the enemy dies.
		/// </summary>
		private void DropXpOrb()
		{
			if (Config == null || Config.XpReward <= 0)
				return;

			XpOrbPickup.Spawn(this.transform.position, Config.XpReward, _player);
		}

		/// <summary>
		/// Records which spawn area type created this enemy.
		/// </summary>
		/// <param name="spawnAreaType">Assigned spawn area type.</param>
		public void SetSpawnAreaType(WaveSpawnAreaType spawnAreaType)
		{
			_spawnAreaType = spawnAreaType;
			_hasAssignedSpawnAreaType = true;
		}

		/// <summary>
		/// Attempts to read the spawn area type assigned at spawn time.
		/// </summary>
		/// <param name="spawnAreaType">Outputs the type when assigned.</param>
		/// <returns>True when a type was assigned.</returns>
		public bool TryGetSpawnAreaType(out WaveSpawnAreaType spawnAreaType)
		{
			spawnAreaType = _spawnAreaType;
			return _hasAssignedSpawnAreaType;
		}

		public static class Registry
		{
			/// <summary>
			/// Active enemy instances.
			/// </summary>
			public static readonly System.Collections.Generic.List<Enemy> Active = new(128);

			/// <summary>
			/// Adds an enemy to the active registry.
			/// </summary>
			/// <param name="enemy">Enemy to register.</param>
			public static void Register(Enemy enemy)
			{
				if (!enemy || Active.Contains(enemy))
					return;

				Active.Add(enemy);
			}

			/// <summary>
			/// Removes an enemy from the active registry.
			/// </summary>
			/// <param name="enemy">Enemy to remove.</param>
			public static void Unregister(Enemy enemy)
			{
				int index = Active.IndexOf(enemy);
				if (index < 0)
					return;

				int last = Active.Count - 1;
				Active[index] = Active[last];
				Active.RemoveAt(last);
			}
		}

		[RequireComponent(typeof(SpriteRenderer))]
		private sealed class XpOrbPickup : MonoBehaviour
		{
			/// <summary>
			/// Pickup radius before the orb is collected.
			/// </summary>
			private const float PICKUP_RADIUS = 0.25f;
			/// <summary>
			/// Distance at which the orb starts moving toward the player.
			/// </summary>
			private const float ATTRACTION_RADIUS = 2.5f;
			/// <summary>
			/// Orb movement speed while attracted.
			/// </summary>
			private const float MOVE_SPEED = 8f;
			/// <summary>
			/// Lifetime before the orb despawns.
			/// </summary>
			private const float LIFETIME_SECONDS = 12f;

			private static Sprite _sprite;
			private static GameObject _cachedPrefab;

			[SerializeField] private SpriteRenderer SpriteRenderer;

			private Player.Player _player;
			private float _remainingLifetime;
			private float _xpAmount;
			private bool _isInitialized;

			/// <summary>
			/// Spawns an XP orb from the Resources prefab.
			/// </summary>
			/// <param name="position">Spawn position.</param>
			/// <param name="xpAmount">XP amount awarded on pickup.</param>
			/// <param name="player">Optional player target to skip the first lookup.</param>
			public static void Spawn(Vector2 position, float xpAmount, Player.Player player = null)
			{
				GameObject prefab = GetPrefab();
				if (!prefab)
					return;

				GameObject orbObject = Instantiate(prefab, position, Quaternion.identity);
				XpOrbPickup orb = orbObject.GetComponent<XpOrbPickup>();
				if (!orb)
				{
					Debug.LogError("[XpOrbPickup] Prefab is missing XpOrbPickup component.");
					Destroy(orbObject);
					return;
				}

				orb.Initialize(xpAmount, player);
			}

			private static GameObject GetPrefab()
			{
				if (_cachedPrefab)
					return _cachedPrefab;

				_cachedPrefab = Resources.Load<GameObject>("Projectiles/XpOrb");
				if (!_cachedPrefab)
					Debug.LogError("[XpOrbPickup] Missing prefab at Resources/Projectiles/XpOrb.prefab.");

				return _cachedPrefab;
			}

			/// <summary>
			/// Ensures the sprite renderer is configured after instantiation.
			/// </summary>
			private void Awake()
			{
				if (SpriteRenderer == null)
					SpriteRenderer = GetComponent<SpriteRenderer>();

				if (SpriteRenderer != null)
				{
					SpriteRenderer.sprite = GetSprite();
					SpriteRenderer.color = new Color(0.4f, 0.9f, 1f, 1f);
					SpriteRenderer.sortingOrder = 5;
				}
			}

			/// <summary>
			/// Stores the runtime pickup state.
			/// </summary>
			/// <param name="xpAmount">XP amount to grant.</param>
			/// <param name="player">Player that should receive the pickup.</param>
			private void Initialize(float xpAmount, Player.Player player)
			{
				_player = player;
				_xpAmount = Mathf.Max(0.01f, xpAmount);
				_remainingLifetime = LIFETIME_SECONDS;
				_isInitialized = true;
			}

			/// <summary>
			/// Counts down lifetime, homes toward the player, and awards XP on contact.
			/// </summary>
			private void Update()
			{
				if (!_isInitialized)
					return;

				_remainingLifetime -= Time.deltaTime;
				if (_remainingLifetime <= 0f)
				{
					Destroy(gameObject);
					return;
				}

				if (!_player)
					_player = UnityEngine.Object.FindAnyObjectByType<Player.Player>();

				if (!_player)
					return;

				Vector2 playerPosition = _player.transform.position;
				Vector2 currentPosition = transform.position;
				float distance = Vector2.Distance(currentPosition, playerPosition);
				if (distance > ATTRACTION_RADIUS)
					return;

				if (distance <= PICKUP_RADIUS)
				{
					_player.XP.AddXp(_xpAmount);
					Destroy(gameObject);
					return;
				}

				transform.position = Vector2.MoveTowards(currentPosition, playerPosition, MOVE_SPEED * Time.deltaTime);
			}

			private static Sprite GetSprite()
			{
				if (_sprite)
					return _sprite;

				Texture2D texture = Texture2D.whiteTexture;
				Rect rect = new(0f, 0f, texture.width, texture.height);
				_sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), texture.width);
				return _sprite;
			}
		}
	}
}
