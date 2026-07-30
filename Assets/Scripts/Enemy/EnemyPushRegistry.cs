using System.Collections.Generic;

namespace Enemy
{
	public static class EnemyPushRegistry
	{
		public static readonly List<EnemyContact> Active = new();

		public static void Register(EnemyContact enemy)
		{
			Active.Add(enemy);
		}

		public static void Unregister(EnemyContact enemy)
		{
			int index = Active.IndexOf(enemy);
			if (index < 0) return;

			int last = Active.Count - 1;
			Active[index] = Active[last];
			Active.RemoveAt(last);
		}
	}
}
