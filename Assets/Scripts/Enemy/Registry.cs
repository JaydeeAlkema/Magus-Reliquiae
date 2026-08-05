using System.Collections.Generic;

namespace Enemy
{
	public static class EnemyRegistry
	{
		/// <summary>
		///     Active enemy instances.
		/// </summary>
		public static readonly List<Enemy> Active = new(128);

		/// <summary>
		///     Adds an enemy to the active registry.
		/// </summary>
		/// <param name="enemy">Enemy to register.</param>
		public static void Register(Enemy enemy)
		{
			if (!enemy || Active.Contains(enemy))
				return;

			Active.Add(enemy);
		}

		/// <summary>
		///     Removes an enemy from the active registry.
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
}
