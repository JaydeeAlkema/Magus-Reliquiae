using System;
using UnityEngine;
using Waves;

namespace Enemy
{
	public class Enemy : MonoBehaviour
	{
		public static event Action<Enemy> EnemyEnabled;
		public static event Action<Enemy> EnemyDisabled;

		private static Player.Player _cachedPlayer;

		[Header("Configuration")]
		[SerializeField] private EnemyConfigSO Config;

		private EnemyMovement _movement;
		private EnemyContact _contact;

		private bool _hasAssignedSpawnAreaType;
		private Player.Player _player;
		private WaveSpawnAreaType _spawnAreaType;

		public Vector2 Position => this.transform.position;
		public bool IsSimulationActive { get; private set; }

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

			SetSimulationActive(true);
			EnemyEnabled?.Invoke(this);

			if (!_player)
				return;

			_movement.SetTargetPos(_player.transform.position);
		}

		private void OnDisable()
		{
			EnemyDisabled?.Invoke(this);
			SetSimulationActive(false);
		}

		private void FixedUpdate()
		{
			if (!IsSimulationActive || !_player)
				return;

			_movement.SetTargetPos(_player.transform.position);
		}

		private void Update()
		{
			if (!IsSimulationActive)
				return;

			_movement.MoveTowardsTarget();
		}

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

		public void Teleport(Vector2 position)
		{
			Vector3 current = this.transform.position;
			this.transform.position = new Vector3(position.x, position.y, current.z);
		}

		public void SetSpawnAreaType(WaveSpawnAreaType spawnAreaType)
		{
			_spawnAreaType = spawnAreaType;
			_hasAssignedSpawnAreaType = true;
		}

		public bool TryGetSpawnAreaType(out WaveSpawnAreaType spawnAreaType)
		{
			spawnAreaType = _spawnAreaType;
			return _hasAssignedSpawnAreaType;
		}
	}
}
