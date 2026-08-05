using System;
using UnityEngine;
using Waves;

namespace Enemy
{
	/// <summary>
	///     Runtime enemy controller.
	/// </summary>
	/// <remarks>
	///     Put this on the enemy prefab, assign an <see cref="EnemyConfigSO" />, and let it manage movement,
	///     contact registration, XP drops, and enable/disable notifications.
	/// </remarks>
	public class Enemy : MonoBehaviour
	{
		/// <summary>
		///     Fired when an enemy becomes active.
		/// </summary>
		public static event Action<Enemy> enemyEnabled;
		/// <summary>
		///     Fired when an enemy is disabled.
		/// </summary>
		public static event Action<Enemy> enemyDisabled;

		private static Player.Player _cachedPlayer;

		[Header("Configuration")]
		[SerializeField] private EnemyConfigSO Config;

		private EnemyMovement _movement;
		private EnemyContact _contact;

		private bool _hasAssignedSpawnAreaType;
		private Player.Player _player;
		private WaveSpawnAreaType _spawnAreaType;

		public Vector2 Position => this.transform.position;
		public int CurrentHealth { get; private set; }
		public int MaxHealth => Config ? Mathf.Max(1, Config.Health) : 1;
		public bool IsSimulationActive { get; private set; }

		/// <summary>
		///     Initializes movement/contact helpers and registers the enemy with runtime systems.
		/// </summary>
		private void OnEnable()
		{
			_player = _cachedPlayer;
			if (!_player)
			{
				_player = FindAnyObjectByType<Player.Player>();
				_cachedPlayer = _player;
			}

			_movement ??= new EnemyMovement();
			_movement.Setup(this, Config);

			_contact ??= new EnemyContact();
			_contact.Setup(this, Config);

			CurrentHealth = MaxHealth;

			EnemyRegistry.Register(this);
			SetSimulationActive(true);
			enemyEnabled?.Invoke(this);

			if (!_player)
				return;

			_movement.SetTargetPos(_player.transform.position);
		}

		/// <summary>
		///     Unregisters the enemy from runtime systems.
		/// </summary>
		private void OnDisable()
		{
			enemyDisabled?.Invoke(this);
			SetSimulationActive(false);
			EnemyRegistry.Unregister(this);
		}

		/// <summary>
		///     Refreshes the cached target position while the enemy is active.
		/// </summary>
		private void FixedUpdate()
		{
			if (!IsSimulationActive || !_player)
				return;

			_movement.SetTargetPos(_player.transform.position);
		}

		/// <summary>
		///     Advances movement while the simulation is active.
		/// </summary>
		private void Update()
		{
			if (!IsSimulationActive)
				return;

			_movement.MoveTowardsTarget();
		}

		/// <summary>
		///     Enables or disables enemy simulation and contact registration.
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
		///     Moves the enemy to a new world position while preserving Z.
		/// </summary>
		/// <param name="position">New world-space position.</param>
		public void Teleport(Vector2 position)
		{
			Vector3 current = this.transform.position;
			this.transform.position = new Vector3(position.x, position.y, current.z);
		}

		/// <summary>
		///     Disables the enemy GameObject.
		/// </summary>
		public void Kill()
		{
			if (!this.gameObject.activeSelf)
				return;

			this.gameObject.SetActive(false);
		}

		/// <summary>
		///     Applies damage and kills the enemy when health reaches zero.
		/// </summary>
		/// <param name="amount">Damage amount.</param>
		/// <returns>True when the enemy died.</returns>
		public bool TakeDamage(int amount)
		{
			if (amount <= 0 || !this.gameObject.activeSelf)
				return false;

			CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
			if (CurrentHealth > 0)
				return false;

			Kill();
			return true;
		}

		/// <summary>
		///     Records which spawn area type created this enemy.
		/// </summary>
		/// <param name="spawnAreaType">Assigned spawn area type.</param>
		public void SetSpawnAreaType(WaveSpawnAreaType spawnAreaType)
		{
			_spawnAreaType = spawnAreaType;
			_hasAssignedSpawnAreaType = true;
		}

		/// <summary>
		///     Attempts to read the spawn area type assigned at spawn time.
		/// </summary>
		/// <param name="spawnAreaType">Outputs the type when assigned.</param>
		/// <returns>True when a type was assigned.</returns>
		public bool TryGetSpawnAreaType(out WaveSpawnAreaType spawnAreaType)
		{
			spawnAreaType = _spawnAreaType;
			return _hasAssignedSpawnAreaType;
		}
	}
}
