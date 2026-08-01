using UnityEngine;

namespace Enemy
{
	[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ScriptableObjects/Enemy/Config", order = 0)]
	public class EnemyConfigSO : ScriptableObject
	{
		[Header("Movement")]
		public float MoveSpeed = 1f;
		
		[Header("Collision")]
		public float CollisionRadius = 1f;
		
		[Header("Combat")]
		public int Health = 10;
		public int Damage = 1;
	}
}
