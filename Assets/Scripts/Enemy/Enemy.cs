using Player;
using UnityEngine;

namespace Enemy
{
	public class Enemy : MonoBehaviour
	{
		[Header("Configuration")]
		[SerializeField] private EnemyConfigSO Config;

		private EnemyMovement _movement;
		private EnemyContact _contact;

		private Player.Player _player;

		private void OnEnable()
		{
			// Testing purposes.
			_player = FindAnyObjectByType<Player.Player>();

			_movement ??= new EnemyMovement();
			_movement.Setup(this, Config);
			_movement.SetTargetPos(_player.transform.position);

			_contact ??= new EnemyContact();
			_contact.Setup(this, Config);
			_contact.Register();
		}

		private void OnDisable()
		{
			_contact.Unregister();
		}

		private void FixedUpdate()
		{
			_movement.SetTargetPos(_player.transform.position);
		}

		private void Update()
		{
			_movement.MoveTowardsTarget();
		}
	}
}
