using UnityEngine;

namespace Enemy
{
	/// <summary>
	///     Enemy tuning asset.
	/// </summary>
	/// <remarks>
	///     Create one asset per enemy type and assign it to the <see cref="Enemy" /> prefab.
	/// </remarks>
	[CreateAssetMenu(fileName = "EnemyConfig", menuName = "ScriptableObjects/Enemy/Config", order = 0)]
	public class EnemyConfigSO : ScriptableObject
	{
		/// <summary>
		///     Movement speed in world units per second.
		/// </summary>
		[Header("Movement")]
		public float MoveSpeed = 1f;

		/// <summary>
		///     Collision radius used for push and separation logic.
		/// </summary>
		[Header("Collision")]
		public float CollisionRadius = 1f;

		/// <summary>
		///     Combat values used by the enemy runtime.
		/// </summary>
		[Header("Combat")]
		public int Health = 10;
		public int Damage = 1;
		public int XpReward = 10;
	}
}
