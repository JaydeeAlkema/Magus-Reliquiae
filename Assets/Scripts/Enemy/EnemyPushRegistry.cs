using System.Collections.Generic;

namespace Enemy
{
	/// <summary>
	///     Shared registry of active enemy contacts for push and separation logic.
	/// </summary>
	/// <remarks>
	///     Call <see cref="Register" /> and <see cref="Unregister" /> from enemy contact helpers; do not add entries manually
	///     from scene objects.
	/// </remarks>
	public static class EnemyPushRegistry
	{
		/// <summary>
		///     Backing list of active contacts.
		/// </summary>
		public static readonly List<EnemyContact> Active = new(128);

		/// <summary>
		///     Reserves capacity for the contact list.
		/// </summary>
		/// <param name="capacity">Desired minimum capacity.</param>
		public static void EnsureCapacity(int capacity)
		{
			if (capacity <= 0)
				return;

			if (Active.Capacity < capacity)
				Active.Capacity = capacity;
		}

		/// <summary>
		///     Adds a contact to the registry.
		/// </summary>
		/// <param name="enemy">Contact to register.</param>
		public static void Register(EnemyContact enemy)
		{
			Active.Add(enemy);
		}

		/// <summary>
		///     Removes a contact from the registry.
		/// </summary>
		/// <param name="enemy">Contact to remove.</param>
		public static void Unregister(EnemyContact enemy)
		{
			int index = Active.IndexOf(enemy);
			if (index < 0)
				return;

			int last = Active.Count - 1;
			Active[index] = Active[last];
			Active.RemoveAt(last);
		}
	}
}
